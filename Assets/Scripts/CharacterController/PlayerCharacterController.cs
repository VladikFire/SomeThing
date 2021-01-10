using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler))]
public class PlayerCharacterController : MonoBehaviour
{
    public float gravityDownForce = 20f;
    public float SphereRadius;
    public GameObject bullet;
    
    [Header("References")]
    [Tooltip("Reference to the main camera used for the player")]
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private GameObject BulletStartPos;
    [SerializeField]
    private Transform PlayerBottom;
    [SerializeField]
    private Transform camTarget;
    public LayerMask layerMask;

    [Header("Movement")]
    public float maxSpeedOnGround = 10f;
    public float movementSharpnessOnGround = 15;
    [Range(0, 1)]
    public float maxSpeedCrouchedRatio = 0.5f;
    public float maxSpeedInAir = 10f;
    public float accelerationSpeedInAir = 25f;
    public float sprintSpeedModifier = 10f;
    public float killHeight = -50f;

    public float rotationSpeed = 150.0f;
    [Range(0.1f, 1f)]
    public float aimingRotationMultiplier = 0.4f;
    [Header("Jump")]
    public float jumpForce = 9f;

    [Header("Stance")]
    public float cameraHeightRatio = 0.9f;
    public float capsuleHeightStanding = 1.8f;
    public float capsuleHeightCrouching = 0.9f;
    public float crouchingSharpness = 10f;

   public GameObject dsfg;
    RaycastHit raycastHit;
    public Vector3 characterVelocity { get; set; }
    public bool isGrounded { get; private set; }
    public bool hasJumpedThisFrame { get; private set; }
    public bool isDead { get; private set; }
    public bool isCrouching { get; private set; }
    public float RotationMultiplier
    {
        get
        {
            return 1f;
        }
    }
    protected Ray ray = new Ray();
    PlayerInputHandler m_InputHandler;
    CharacterController m_Controller;
    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    Parcur m_Parcur;
    Vector3 m_GroundNormal;
    float m_TargetCharacterHeight;
    public float cameraFildOfView = 10f;
    protected internal Vector3 _ray;
    void Start()
    {
        Application.targetFrameRate = 60;
        // fetch components on the same gameObject
        m_Controller = GetComponent<CharacterController>();
        m_Parcur = GetComponent<Parcur>();
        m_InputHandler = GetComponent<PlayerInputHandler>();
        m_Controller.enableOverlapRecovery = true;

    }

    void Update()
    {
        hasJumpedThisFrame = false;
        GroundCheck();
        HandleCharacterMovement();
      if(Input.GetMouseButtonDown(0))
            Fire();
        if (Input.GetMouseButtonDown(1))
        {
            virtualCamera.m_Lens.FieldOfView = cameraFildOfView;
            rotationSpeed = 15f;
        }
       else if (Input.GetMouseButtonUp(1))
        {
            virtualCamera.m_Lens.FieldOfView = 45;
            rotationSpeed = 100f;
        }

    }

    void GroundCheck()
    {
        isGrounded = false;
        isGrounded = m_Controller.isGrounded; //Physics.CheckSphere(PlayerBottom.position, SphereRadius, layerMask);
        m_GroundNormal = Vector3.up;
    }
    //  float chosenGroundCheckDistance = isGrounded ? (m_Controller.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;
    //  if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
    // {
    // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
    //   if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(),
    //      GetCapsuleTopHemisphere(m_Controller.height),
    //     m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance,
    //     groundCheckLayers, QueryTriggerInteraction.Ignore))
    // {
    // storing the upward direction for the surface found
    //    m_GroundNormal = hit.normal;

    //   if (Vector3.Dot(hit.normal, transform.up) > 0f && IsNormalUnderSlopeLimit(m_GroundNormal))
    //   {
    //       isGrounded = true;

    // handle snapping to the ground
    //        if (hit.distance > m_Controller.skinWidth)
    //     {
    //         m_Controller.Move(Vector3.down * hit.distance);
    //  }
    // }
    // }
    //}


    void HandleCharacterMovement()
    {
        {
            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime, 0));
            camTarget.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime, 0, 0));
        }


        float speedModifier = true ? sprintSpeedModifier : 1f;
        Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());

        if (isGrounded)
        {
            Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier;

            targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) * targetVelocity.magnitude;
            characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);
        }
        else
        {
            // add air acceleration
            characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

            float verticalVelocity = characterVelocity.y;
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
            characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

            characterVelocity += Vector3.down * gravityDownForce * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)//&& m_Parcur._canJump == true)
        {
            characterVelocity = characterVelocity + Vector3.up * jumpForce * Time.deltaTime;
        }

        m_Controller.Move(characterVelocity  * Time.deltaTime);


        // Gets a reoriented direction that is tangent to a given slope
        Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            //        direction = new Vector3(1f, 0, 1f);
            Vector3 directionRight = Vector3.Cross(direction, transform.up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }
    }
    private void Fire()
    {
        Ray ray1 = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        ray.origin = BulletStartPos.transform.position;
        if (Physics.Raycast(ray1, out raycastHit, 4000f))
        {
            _ray = raycastHit.point - BulletStartPos.transform.position;
             Instantiate(bullet, BulletStartPos.transform.position, Quaternion.identity);
            Debug.DrawLine(ray.origin, raycastHit.point, Color.blue, 1);
        }
        else
        {
            _ray = ray1.GetPoint(300)  - BulletStartPos.transform.position;

            Instantiate(bullet, BulletStartPos.transform.position, Quaternion.identity);
            Debug.DrawLine(ray.origin, ray1.GetPoint(300), Color.blue, 10);
            Debug.DrawRay(ray1.origin, ray1.direction * 10f, Color.red, 5);     
        }
    }

}

