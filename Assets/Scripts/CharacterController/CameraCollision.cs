using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{

    public float minDistance = 1.0f;
    public float maxDistance = 4.0f;
    public float smooth = 10.0f;
    Vector3 dollyDir;
 //  public GameObject Target;
    public Vector3 dollyDirAdjusted;
    public float distance;


    void Awake()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }
    private void Update()
    {
         transform.localPosition = dollyDir * distance;
    
    }
    // Update is called once per frame
    void FixedUpdate()
    {
    // Vector3 Targetg = Target.transform.position;
        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out hit))
        {
            distance = Mathf.Clamp((hit.distance * 0.87f), minDistance, maxDistance);
        }
        else
        {
            distance = maxDistance;
        }
   //     transform.LookAt(Targetg);
       // Vector3.Lerp (transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
           Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
    }
}
