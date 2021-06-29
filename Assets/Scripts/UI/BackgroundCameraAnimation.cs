using System.Collections;
using UnityEngine;

//  данный скрипт описывает перемещение камеры, изображение
//  которой служит фоном для большого дисплея в зале

[RequireComponent(typeof(Camera))]
public class BackgroundCameraAnimation : MonoBehaviour
{
    private Transform _cameraTransform;
    private float _movingSpeed = 0.1f;

    private Vector3[] CameraStartPositions = new Vector3[3]
    {
        new Vector3(-1.202374f, -1.351f, -3.519f),
        new Vector3(-7.354f, -2.6847f, -6.614f),
        new Vector3(-16.27f, -7.24f, -3.68f)
    };
    private Quaternion[] CameraStartRotations = new Quaternion[3]
    {
        Quaternion.Euler(new Vector3(5.672f, -117.291f, 0)),
        Quaternion.Euler(new Vector3(0, 236.717f, 0)),
        Quaternion.Euler(new Vector3(-16.839f, 65.53001f, 0))
    };

    private Coroutine C_BGCameraLerp;

    void Start()
    {
        for (int i = 0; i < CameraStartPositions.Length; i++)
        {
            CameraStartPositions[i] += new Vector3(19.75381f, 8.451416f, 8.722995f);
        }
    }

    public void StartBackgroundCameraMoving()
    {
        if(C_BGCameraLerp != null)
        {
            StopCoroutine(C_BGCameraLerp);
            C_BGCameraLerp = null;
        }
        C_BGCameraLerp = StartCoroutine(BackgroundCameraLerp()); 
    }

    public void StopBackgroundCameraMoving()
    {
        if(C_BGCameraLerp != null)
        {
            StopCoroutine(C_BGCameraLerp);
            C_BGCameraLerp = null;
        }
    }

    private IEnumerator BackgroundCameraLerp()
    {
        yield return null;

        Vector3 Direction = new Vector3();
        float Distance = 0;

        while(true)
        {
            for(int i = 0; i < 3; i++)
            {
                transform.position = CameraStartPositions[i];
                transform.rotation = CameraStartRotations[i];
                float LerpProc = 0;

                switch(i)
                {
                    case 0:
                        Direction = Vector3.forward;
                        Distance = 2;
                        break;
                    case 1:
                        Direction = -Vector3.right;
                        Distance = 3;
                        break;
                    case 2:
                        Direction = Vector3.right;
                        Distance = 3;
                        break;
                }

                while( LerpProc < 1)
                {
                    transform.position = Vector3.Lerp(CameraStartPositions[i], CameraStartPositions[i] + Direction * Distance, LerpProc);
                    LerpProc += _movingSpeed * Time.deltaTime;

                    yield return null;
                }

                continue;
            }
            
            yield return null;
        }
    }
}
