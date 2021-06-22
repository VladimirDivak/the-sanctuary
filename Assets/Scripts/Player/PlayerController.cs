using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Transform ControllerTransform;
    public static float NetDistance;

    private Transform _cameraTransform;

    private float _mouseSensitivity = 60f;

    private float _cameraYRot;
    private float _movingSpeed = 3f;

    private Vector3 _net = new Vector3(12.779f, 0, 0);
    private Coroutine _movement;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;

        Vector3 StartPosition = new Vector3(8.35f, 0, 0);
        Quaternion StartRotation = Quaternion.Euler(new Vector3(0, -270, 0));
        ControllerTransform = this.transform;
    }

    public void MovementInit(bool isTrue)
    {
        if(_movement != null)
        {
            StopCoroutine(_movement);
            _movement = null;
        }
        if(isTrue) _movement = StartCoroutine(Movement());
    }

    private IEnumerator Movement()
    {
        while(true)
        {
            if(ControllerTransform.position.x > 0)
            {
                NetDistance = (_net - new Vector3(ControllerTransform.position.x, 0, ControllerTransform.position.z)).magnitude;
            }
            else
            {
                NetDistance = (-_net - new Vector3(ControllerTransform.position.x, 0, ControllerTransform.position.z)).magnitude;
            }

            ControllerRotating();
            ControllerMoving();

            yield return null;
        }
    }

    private void ControllerRotating()
    {
        var ControllerRotation = ControllerTransform.rotation.eulerAngles;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _cameraYRot -= mouseY;
        _cameraYRot = Mathf.Clamp(_cameraYRot, -90f, 90f);

        ControllerRotation.y += mouseX;

        ControllerTransform.rotation = Quaternion.Slerp(ControllerTransform.rotation, Quaternion.Euler(ControllerRotation), _mouseSensitivity * Time.deltaTime);
        _cameraTransform.localRotation = Quaternion.Slerp(_cameraTransform.localRotation, Quaternion.Euler(_cameraYRot, 0, 0), _mouseSensitivity * Time.deltaTime);

        // ControllerTransform.rotation = Quaternion.Euler(ControllerRotation);
        // _cameraTransform.localRotation = Quaternion.Euler(_cameraYRot, 0, 0);
    }

    private void ControllerMoving()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 TranslateVector = (ControllerTransform.forward * z + ControllerTransform.right * x) * _movingSpeed;

        // Vector3 NewPosition = Vector3.Lerp(ControllerTransform.position, ControllerTransform.position + TranslateVector, 5f * Time.deltaTime);
        Vector3 NewPosition = ControllerTransform.position + TranslateVector * Time.deltaTime;
        ControllerTransform.position = new Vector3(Mathf.Clamp(NewPosition.x, -15.5f, 15.5f), NewPosition.y, Mathf.Clamp(NewPosition.z, -7.5f, 7.5f));
    }
}
