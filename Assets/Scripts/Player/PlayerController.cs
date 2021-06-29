using System.Collections;
using UnityEngine;

//  данный класс создан для описания логики перемещения игрока
//
//  до сих пор ищу решение по наиболее грамотному написанию
//  movement-компонента для игрока, т.к. описанный мною
//  вариант даёт рывки при вращении и перемещении,
//  что было заметно также и в VR-версии проекта

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public Camera ReflectionCamera;
    [SerializeField]
    public RenderTexture PlanarReflectionTexture;

    public static Transform ControllerTransform;
    public static float NetDistance;

    private Transform _cameraTransform;
    private Transform _reflectionCameraTransform;
    private Vector3 _cameraForward;
    private Vector3 _cameraRight;

    private float _mouseSensitivity = 10f;

    private float _cameraYRot;
    private float _cameraXRot;
    private float _movingSpeed = 0.18f;
    private float _lerpConst = 25;

    private Vector3 _net = new Vector3(12.779f, 0, 0);
    private Coroutine _movement;

    void Awake()
    {
        PlanarReflectionTexture.height = Screen.height;
        PlanarReflectionTexture.width = Screen.width;
    }

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        _reflectionCameraTransform = ReflectionCamera.transform;

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

        //  здесь присутствует дублирование кода, знаю...
        //  пока что просто оставлю как есть, но лучше, конечно,
        //  менять в этом цикле только знак вектора _net
        //
        //  и вообще я не уверен, что писать всё это внутри куротины
        //  верное решение, но вариантов было использовано несколько
        
        while(true)
        {
            _cameraForward = new Vector3(_cameraTransform.forward.x, 0, _cameraTransform.forward.z);
            _cameraRight = new Vector3(_cameraTransform.right.x, 0, _cameraTransform.right.z);


            if(ControllerTransform.position.x < 0)
                _net *= -1;
            
            NetDistance = (_net - new Vector3(ControllerTransform.position.x, 0, ControllerTransform.position.z)).magnitude;

            ControllerRotating();
            ControllerMoving();

            yield return null;
        }
    }

    private void ControllerRotating()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _cameraXRot -= mouseY;
        _cameraXRot = Mathf.Clamp(_cameraXRot, -90f, 90f);

        _cameraYRot += mouseX;

        var cameraRotation = new Vector3(_cameraXRot, _cameraYRot, 0);
        var reflectionCameraRotation = new Vector3(-cameraRotation.x, cameraRotation.y, -cameraRotation.z);

        _cameraTransform.localRotation = Quaternion.Slerp(_cameraTransform.localRotation, Quaternion.Euler(cameraRotation), _lerpConst * Time.deltaTime);
        _reflectionCameraTransform.localRotation = Quaternion.Slerp(_reflectionCameraTransform.localRotation, Quaternion.Euler(reflectionCameraRotation), _lerpConst * Time.deltaTime);
    }

    private void ControllerMoving()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 TranslateVector = (_cameraForward * z + _cameraRight * x) * _movingSpeed;

        Vector3 newPosition = ControllerTransform.position + TranslateVector;
        newPosition = new Vector3(Mathf.Clamp(newPosition.x, -15.5f, 15.5f), newPosition.y, Mathf.Clamp(newPosition.z, -7.5f, 7.5f));
        ControllerTransform.position = Vector3.Lerp(ControllerTransform.position, newPosition, _lerpConst * Time.deltaTime);

        _reflectionCameraTransform.localPosition = new Vector3(0, -_cameraTransform.localPosition.y, 0);
    }
}
