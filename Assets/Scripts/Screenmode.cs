using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenmode : MonoBehaviour
{
    float MouseSensitivity = 2f;
    float xRotation = 0f;
    float yRotarion = 0f;
    Transform CameraTransform;

    void Start()
    {
        CameraTransform = transform;
    }

    void Update()
    {
        Rotation();
        Moving();
    }

    private void Rotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;

        xRotation -= mouseY;
        yRotarion += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        CameraTransform.localRotation = Quaternion.Euler(xRotation, yRotarion, 0f);
    }

    private void Moving()
    {
        Vector3 NewPos = new Vector3();
        float SpeedCoef = 4f;
        Vector3 Direction = new Vector3();

        if(Input.GetKey(KeyCode.W))
        {
            Direction = CameraTransform.forward;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            Direction = -CameraTransform.forward;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            Direction = -CameraTransform.right;
        }
        else if(Input.GetKey(KeyCode.D))
        {
            Direction = CameraTransform.right;
        }

        NewPos = CameraTransform.position + (Direction * SpeedCoef * Time.deltaTime);
        CameraTransform.position = NewPos;
    }
}
