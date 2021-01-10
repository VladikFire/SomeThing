using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class ThirdPersonCameraControl : MonoBehaviour
{
    public float CameraMoveSpeed = 1f;
    public GameObject TargetCameraObject;
    Vector3 TarrgetPos;
    public float clampAngel = 90.0f;
    public float InputSensitivity = 20f;

    void Start()
    {
 

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        
    }
    

   

}