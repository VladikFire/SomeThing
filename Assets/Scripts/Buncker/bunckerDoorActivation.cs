using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bunckerDoorActivation : MonoBehaviour
{
    public Transform LeftDoor;
    public Transform rightDoor;
    public Vector3 opemLeftDoor = new Vector3(0,0,4);
    public Vector3 closeLeftDoor = new Vector3(0, 0, 0);
    public Vector3 openRightDoor = new Vector3(0, 0, 0);
    public Vector3 closeRightDoor = new Vector3(0, 0, 0);
    public float openspeed = 5;
    private bool openState = false;
    void Start()
    {
      
       
    }
    void OnTriggerEnter(Collider Collider)
    {
       if (Collider.gameObject.tag == "Player")
        {
            OpenDoor();
        }
    }
    private void OnTriggerExit(Collider Collider)
    {
        if (Collider.gameObject.tag == "Player")
        {
            CloseDoor();
        }
    }
    void Update()
    {
        if (openState)
        {
            LeftDoor.position = Vector3.Lerp(LeftDoor.position,  opemLeftDoor, Time.deltaTime * openspeed);
            rightDoor.position = Vector3.Lerp(rightDoor.position, openRightDoor, Time.deltaTime * openspeed);
        }
        else
        {
            LeftDoor.position = Vector3.Lerp(LeftDoor.position, closeLeftDoor, Time.deltaTime * openspeed);
            rightDoor.position = Vector3.Lerp(rightDoor.position, closeRightDoor, Time.deltaTime * openspeed);
        }
    }
    void OpenDoor()
    {
        openState = true;
    }
    void CloseDoor()
    {
        openState = false;
    }
}
