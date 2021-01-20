using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler))]
public class PlayerCharacterController : MonoBehaviour
{
    public float gravityDownForce = 20f;
    public float SphereRadius;
    public GameObject bullet;
    Quaternion m_MyQuaternion;
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
    float m_LastTimeJumped = 0f;
    public float rotationSpeed = 150.0f;
    [Range(0.1f, 1f)]
    public float _rotationMultiplier = 0.4f;
    [Header("Jump")]
    public float jumpForce = 9f;

    [Header("Stance")]
    public float cameraHeightRatio = 0.9f;
    public float capsuleHeightStanding = 1.8f;
    public float capsuleHeightCrouching = 0.9f;
    public float crouchingSharpness = 10f;

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
    private Quaternion quaternion = new Quaternion();
    private float groundCheckDistance;
    const float k_JumpGroundingPreventionTime = 0.2f;
    const float k_GroundCheckDistanceInAir = 0.07f;

    void Start()
    {
        m_MyQuaternion = new Quaternion();
        Application.targetFrameRate = 90;
        // fetch components on the same gameObject
        m_Controller = GetComponent<CharacterController>();
        m_Parcur = GetComponent<Parcur>();
        m_InputHandler = GetComponent<PlayerInputHandler>();

    }

    void Update()
    {
        hasJumpedThisFrame = false;
        GroundCheck();
        HandleCharacterMovement();
        if (Input.GetMouseButtonDown(0))
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
        // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
        float chosenGroundCheckDistance = isGrounded ? (m_Controller.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;

        // reset values before the ground check
        isGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
        {
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height), m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance,layerMask))
            {
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    isGrounded = true;

                    // handle snapping to the ground
                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }


    void HandleCharacterMovement()
    {
        {
            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime, 0));
            camTarget.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime, 0, 0));
        }



       

        {

            float speedModifier = 1f;

            // converts move input to a worldspace vector based on our character's transform orientation
            Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());

            Debug.Log(characterVelocity.magnitude + "    " + isGrounded);
            if (isGrounded)
            {
                // calculate the desired velocity from inputs, max speed, and current slope
                Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier;
                // reduce speed if crouching by crouch speed ratio
               
                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) * targetVelocity.magnitude;

                // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);

                // jumping
                if (isGrounded && Input.GetKey(KeyCode.Space))
                {
                    // force the crouch state to false

                    // start by canceling out the vertical component of our velocity
                    characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);

                    // then, add the jumpSpeed value upwards
                    characterVelocity += Vector3.up * jumpForce;

                    // play sound
              
                    // remember last time we jumped because we need to prevent snapping to ground for a short time
                    m_LastTimeJumped = Time.time;
                    hasJumpedThisFrame = true;

                    // Force grounding to false
                    isGrounded = false;
                    m_GroundNormal = Vector3.up;

                }

            }
            // handle air movement
            else
            {
                // add air acceleration
                characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

                // limit air speed to a maximum, but only horizontally
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                // apply the gravity to the velocity
                characterVelocity += Vector3.down * gravityDownForce * Time.deltaTime;
            }
        }

        // apply the final calculated velocity value as a character movement
        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
        
        m_Controller.Move(characterVelocity * Time.deltaTime);



        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius, characterVelocity.normalized, out RaycastHit hit, characterVelocity.magnitude * Time.deltaTime,layerMask))
         {
            characterVelocity = Vector3.ProjectOnPlane(characterVelocity, hit.normal);
           
            Debug.DrawRay(hit.point, hit.normal, Color.red, 4);
           
        }
    }
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_Controller.radius);
    }
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_Controller.radius));
    }
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, Vector3.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }
    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
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
            _ray = ray1.GetPoint(300) - BulletStartPos.transform.position;

            Instantiate(bullet, BulletStartPos.transform.position, Quaternion.identity);
            Debug.DrawLine(ray.origin, ray1.GetPoint(300), Color.blue, 10);
            Debug.DrawRay(ray1.origin, ray1.direction * 10f, Color.red, 5);
        }
    }

}

