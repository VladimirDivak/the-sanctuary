using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionsShader : MonoBehaviour
{
    [SerializeField]
    public RenderTexture _reflectionTexture;
    private Camera _mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        _reflectionTexture.height = Screen.height / 4;
        _reflectionTexture.width = Screen.width / 4;
        
        _mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 CameraPosition = _mainCamera.transform.localPosition;
        Vector3 CameraRotation = _mainCamera.transform.localRotation.eulerAngles;

        transform.localPosition = new Vector3(CameraPosition.x, -CameraPosition.y, CameraPosition.z);
        transform.localRotation = Quaternion.Euler(new Vector3(-CameraRotation.x, CameraRotation.y, -CameraRotation.z));
    }
}
