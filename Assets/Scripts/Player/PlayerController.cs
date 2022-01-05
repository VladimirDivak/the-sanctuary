using System.Collections;
using UnityEngine;
using System.Linq;

enum SwipeDirection {
    Left,
    Right,
    Up,
    Down
}

public class PlayerController : MonoBehaviour {

    public static Vector2 firstTouchPosition { get; private set; }
    public static Vector2 lastTouchPosition { get; private set; }
	public static float shootingTouchRange {
		get {
			var distance = lastTouchPosition.y - firstTouchPosition.y;
			distance = Mathf.Clamp(distance, -Screen.height * 0.2f, 0);
			distance = Mathf.Abs(distance);

			return distance * .5f / (Screen.height * .2f / 2);
		}
	}
    public static float accuracy { get; private set; }

    public static Transform controllerTransform;
    public static float netDistance;

    [HideInInspector]
    public Transform cameraTransform { get; private set; }

	Vector3 _net = new Vector3(12.779f, 0, 0);
	Vector2 _correctTouchPosition;
    Gyroscope _gyroscope;
    Vector2 _swipeStart;
    Vector2 _swipeEnd;
    Coroutine c_rotation;
    PlayerBall _currentBall;

    void Awake() {
        _gyroscope = Input.gyro;
        _gyroscope.updateInterval = 0.05f;
        _gyroscope.enabled = true;
    }

    private void Start() {
        cameraTransform = Camera.main.transform;
        controllerTransform = transform;
    }

    void Update() {
        if (controllerTransform.position.x < 0) _net *= -1;
        
        netDistance = (
            _net - new Vector3 (
                controllerTransform.position.x,
                0,
                controllerTransform.position.z
            )
        ).magnitude;

        cameraTransform.localRotation = new Quaternion
        (
            _gyroscope.attitude.x,
            _gyroscope.attitude.y,
            -_gyroscope.attitude.z,
            -_gyroscope.attitude.w
        );

        if(Input.touches.Length > 0) {
            foreach (Touch touch in Input.touches) {
                if (touch.phase == TouchPhase.Began) {
                    _swipeStart = touch.position;
                    firstTouchPosition = touch.position;

                    Vector3 touchScreenPosition = touch.rawPosition;
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);

                    if(Physics.Raycast(ray, out RaycastHit hitData, Mathf.Infinity)) {
                        if (hitData.transform.TryGetComponent<PlayerBall>(out var myBall)) {
                            _currentBall = myBall;
                            if (!myBall.isGrabed) {
                                _currentBall.PhysicDisable();
                            }

                            else {
                                _currentBall.ShootingModeInit();

								float randomY = Random.Range(firstTouchPosition.y - Screen.height * 0.2f, firstTouchPosition.y);
								_correctTouchPosition = new Vector2(firstTouchPosition.x, randomY);
                            }
                        }

                        else if (hitData.transform.TryGetComponent<RaycastEventHandler>(out var item)) {
                            item.OnTriggerHasRaycst?.Invoke();
                        }
                    }

                    continue;
                }

                else if (touch.phase == TouchPhase.Ended) {
                    if (_currentBall != null && _currentBall.shootingMode) {
                        _currentBall.PhysicEnable();
                        _currentBall = null;
                        accuracy = 0;

                        break;
                    }

                    _swipeEnd = touch.position;
                    float horizontalSwipeMagnitude = _swipeEnd.x - _swipeStart.x;
                    float verticalSwipeMagnitude = _swipeEnd.y - _swipeStart.y;

                    if (Mathf.Abs(horizontalSwipeMagnitude) > Mathf.Abs(verticalSwipeMagnitude)) {
                        if (Mathf.Abs(horizontalSwipeMagnitude) > 150) {
                            if (_swipeEnd.x - _swipeStart.x < 0) {
                                StartCoroutine(RotationRoutine(SwipeDirection.Right));
                            }
                            else {
                                StartCoroutine(RotationRoutine(SwipeDirection.Left));
                            }
                        }
                    }
                    else {
                        if (Mathf.Abs(verticalSwipeMagnitude) > 150) {
                            if(_swipeEnd.y - _swipeStart.y < 0) {
                                StartCoroutine(MovingRoutine(SwipeDirection.Up));
                            }
                            else {
                                StartCoroutine(MovingRoutine(SwipeDirection.Down));
                            }
                        }
                    }
                }
            }

            lastTouchPosition = Input.touches.Last().position;

            if(_currentBall != null && _currentBall.shootingMode) {
				var touchPosition = Mathf.Clamp(lastTouchPosition.y, firstTouchPosition.y - Screen.height * 0.2f, firstTouchPosition.y);
                accuracy = Mathf.Abs(_correctTouchPosition.y - touchPosition);
                accuracy = 1 - (0.5f * accuracy / (Screen.height * 0.2f / 2));
				accuracy = System.MathF.Round(accuracy, 2);
            }
        }
    }

    IEnumerator MovingRoutine(SwipeDirection dir) {
        float lerpProgress = 0;
        float lerpTime = 8f;
        float direction = 1f;

        if (dir == SwipeDirection.Down) direction *= -1;

        Vector3 startPosition = controllerTransform.position;
        Vector3 endPosition = startPosition + new Vector3(
            cameraTransform.forward.x * direction,
            0,
            cameraTransform.forward.z * direction
        ).normalized * 2;

        endPosition = new Vector3(
            Mathf.Clamp(endPosition.x, -15.5f, 15.5f),
            endPosition.y,
            Mathf.Clamp(endPosition.z, -7.5f, 7.5f)
        );

        while(lerpProgress < 1) {
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

    IEnumerator RotationRoutine(SwipeDirection dir) {
        float lerpProgress = 0;
        float lerpTime = 8f;
        Vector3 rotationDir = new Vector3();
        Vector3 startRotation = controllerTransform.eulerAngles;

        if(dir == SwipeDirection.Left) rotationDir = new Vector3(0, -60, 0);
        else rotationDir = new Vector3(0, 60, 0);

        while(lerpProgress < 1) {
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
