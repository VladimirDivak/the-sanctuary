using System.Collections;
using UnityEngine;
using System.Linq;

enum SwipeDirection
{
    Left,
    Right,
    Up,
    Down
}

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject raycastPosition;
    [SerializeField]
    Camera ReflectionCamera;
    [SerializeField]
    RenderTexture PlanarReflectionTexture;

    public static Transform controllerTransform;
    public static float netDistance;

    private Transform _cameraTransform;
    private Transform _reflectionCameraTransform;

    private Vector3 _net = new Vector3(12.779f, 0, 0);

    private Gyroscope _gyroscope;

    private Vector2 _swipeStart;
    private Vector2 _swipeEnd;

    private Coroutine c_rotation;

    void Awake()
    {
        _gyroscope = Input.gyro;
        _gyroscope.enabled = true;
    }

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        controllerTransform = transform;
    }

    void Update()
    {
        if (controllerTransform.position.x < 0)
            _net *= -1;
        
        netDistance = (_net - new Vector3(
            controllerTransform.position.x,
            0,
            controllerTransform.position.z)).magnitude;

        _cameraTransform.localRotation = _gyroscope.attitude * new Quaternion(0, 0, 1, 0);

        if(Input.touches.First().phase == TouchPhase.Ended)
        {
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Input.touches.First().position);
            touchPosition = new Vector3(touchPosition.x, touchPosition.y, Camera.main.transform.position.z);

            Debug.DrawLine(touchPosition, touchPosition + Camera.main.transform.forward * 20);
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                _swipeStart = touch.position;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                _swipeEnd = touch.position;
                float horizontalSwipeMagnitude = _swipeEnd.x - _swipeStart.x;
                float verticalSwipeMagnitude = _swipeEnd.y - _swipeStart.y;

                if (Mathf.Abs(horizontalSwipeMagnitude) > Mathf.Abs(verticalSwipeMagnitude))
                {
                    if (Mathf.Abs(horizontalSwipeMagnitude) > 300)
                    {
                        if (_swipeEnd.x - _swipeStart.x < 0)
                        {
                            StartCoroutine(RotationRoutine(SwipeDirection.Right));
                        }
                        else
                        {
                            StartCoroutine(RotationRoutine(SwipeDirection.Left));
                        }
                    }
                }
                else
                {
                    if (Mathf.Abs(verticalSwipeMagnitude) > 300)
                    {
                        if(_swipeEnd.y - _swipeStart.y < 0)
                        {
                            StartCoroutine(MovingRoutine(SwipeDirection.Down));
                        }
                        else
                        {
                            StartCoroutine(MovingRoutine(SwipeDirection.Up));
                        }
                    }
                }
            }
        }
    }

    IEnumerator MovingRoutine(SwipeDirection dir)
    {
        float lerpProgress = 0;
        float lerpTime = 8f;
        float direction = 1f;

        if (dir == SwipeDirection.Down) direction *= -1;

        Vector3 startPosition = controllerTransform.position;
        Vector3 endPosition = startPosition + new Vector3(
            _cameraTransform.forward.x * direction,
            0,
            _cameraTransform.forward.z * direction
        ).normalized * 2;

        while(lerpProgress < 1)
        {
            controllerTransform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                lerpProgress
            );

            lerpProgress += lerpTime * Time.deltaTime;
            yield return null;
        }

        controllerTransform.position = endPosition;
        yield break;
    }

    IEnumerator RotationRoutine(SwipeDirection dir)
    {
        float lerpProgress = 0;
        float lerpTime = 8f;
        Vector3 rotationDir = new Vector3();
        Vector3 startRotation = controllerTransform.eulerAngles;

        if(dir == SwipeDirection.Left) rotationDir = new Vector3(0, -60, 0);
        else rotationDir = new Vector3(0, 60, 0);

        while(lerpProgress < 1)
        {
            controllerTransform.rotation = Quaternion.Lerp(
                Quaternion.Euler(startRotation),
                Quaternion.Euler(startRotation + rotationDir),
                lerpProgress
            );

            lerpProgress += lerpTime * Time.deltaTime;
            yield return null;
        }

        yield break;
    }
}
