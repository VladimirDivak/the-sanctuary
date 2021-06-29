using UnityEngine;

//  класс написан для того, чтобы управлять симметричной
//  камерой, которая создаёт эффект отражений в реальном
//  времени на поверхности паркета

public class PlanarReflectionsShader : MonoBehaviour
{
    [SerializeField]
    public RenderTexture ReflectionTexture;
    private Camera _mainCamera;

    void Start()
    {        
        _mainCamera = Camera.main;
        ReflectionTexture.height = Screen.height;
        ReflectionTexture.width = Screen.width;
    }

    void Update()
    {
        //  по моему мнению, здесь нужно всё-таки использовать Lerp,
        //  а лучше вообще менять вращение и положение камеры на ряду
        //  с основной
        // 
        //  оставлю как есть, потому что ошибки - это не плохо :)

        Vector3 CameraPosition = _mainCamera.transform.localPosition;
        Vector3 CameraRotation = _mainCamera.transform.localRotation.eulerAngles;

        transform.localPosition = new Vector3(CameraPosition.x, -CameraPosition.y, CameraPosition.z);
        transform.localRotation = Quaternion.Euler(new Vector3(-CameraRotation.x, CameraRotation.y, -CameraRotation.z));
    }
}
