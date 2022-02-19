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

    public static PlayerController Instance { get; private set; }
    public bool ableToMoving { get; set; } = true;
    public bool ableToRaycast { get; set; } = true;
    public PlayerBall currentBall;

    [SerializeField]
    public float netDistance;
    [SerializeField]
    public AccuracyValue uiAccValue;

    public ScoreTrigger currentScoreTrigger;

    [SerializeField]
    public float minAngleValue;

    public Vector2 firstTouchPosition { get; private set; }
    public Vector2 lastTouchPosition { get; private set; }
	public float shootingTouchRange
    {
		get
        {
			var distance = lastTouchPosition.y - firstTouchPosition.y;
			distance = Mathf.Clamp(distance, -Screen.height * 0.2f, 0);
			distance = Mathf.Abs(distance);

			return distance * .5f / (Screen.height * .2f / 2);
		}
	}
    public float touchAccuracy { get; private set; }
    public float shootAccuracy { get; private set; }
    public float sumAccuracy => touchAccuracy * shootAccuracy;
    public int roundAccuracy { get; private set; }

    private Transform _controllerTransform;

    [HideInInspector]
    public Transform cameraTransform { get; private set; }

	Vector2 _correctTouchPosition;
    Gyroscope _gyroscope;
    Vector2 _swipeStart;
    Vector2 _swipeEnd;
    Coroutine c_rotation;

    public Vector3 position
    {
        get => _controllerTransform.position;
        set => _controllerTransform.position = value;
    }

    public Quaternion rotation
    {
        get => _controllerTransform.rotation;
        set => _controllerTransform.rotation = value;
    }

    void Awake()
    {
        _gyroscope = Input.gyro;
        _gyroscope.updateInterval = 0.2f;
        _gyroscope.enabled = true;
    }

    private void Start()
    {
        Instance = this;
        cameraTransform = Camera.main.transform;
        _controllerTransform = transform;

        currentScoreTrigger.isEnable = true;
    }

    void Update()
    {
        float coef = Mathf.Clamp(currentScoreTrigger.angleBetweenCameraAndNet, minAngleValue, 90f);
        shootAccuracy = (1f - ((1f * coef) / minAngleValue)) * -1;
        shootAccuracy = 1f - Mathf.Clamp(shootAccuracy, 0f, 1f);
        shootAccuracy = System.MathF.Round(shootAccuracy, 2);

        Quaternion gyroRotation = new Quaternion
        (
            _gyroscope.attitude.x,
            _gyroscope.attitude.y,
            -_gyroscope.attitude.z,
            -_gyroscope.attitude.w
        );

        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, gyroRotation, 15 * Time.deltaTime);

        if(Input.touches.Length > 0)
        {
            if(!ableToRaycast) return;

            foreach (Touch touch in Input.touches) 
            {
                if (touch.phase == TouchPhase.Began) 
                {
                    _swipeStart = touch.position;
                    firstTouchPosition = touch.position;

                    Vector3 touchScreenPosition = touch.rawPosition;
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);

                    if(Physics.Raycast(ray, out RaycastHit hitData, Mathf.Infinity))
                    {
                        if (hitData.transform.TryGetComponent<PlayerBall>(out PlayerBall myBall))
                        {
                            if(currentBall == null)
                            {
                                currentBall = myBall;
                                currentBall.PhysicDisable();
                            }
                            else
                            {
                                if(!currentBall.Equals(myBall)) return;

                                currentBall.shootingMode = true;
								float randomY = Random.Range(firstTouchPosition.y - Screen.height * 0.2f,
                                    firstTouchPosition.y);
                                
								_correctTouchPosition = new Vector2(firstTouchPosition.x, randomY);
                                
                                currentScoreTrigger.SetShootingMode(true);
                                GUI.Instance.ShowCrosshair();
                            }
                        }

                        else if (hitData.transform.TryGetComponent<RaycastEventHandler>(out var item))
                        {
                            item.OnTriggerHasRaycst?.Invoke();
                        }
                    }

                    continue;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    if (currentBall != null && currentBall.shootingMode)
                    {
                        if(sumAccuracy != 0 && currentScoreTrigger.correctAngle)
                        {
                            roundAccuracy = Mathf.RoundToInt(sumAccuracy * 100);

                            uiAccValue.gameObject.SetActive(true);
                            uiAccValue.ShowAccuracyValue(
                                roundAccuracy,
                                System.MathF.Round(sumAccuracy, 1));
                        }

                        currentBall.PhysicEnable();
                        GUI.Instance.HideCrosshair();
                        
                        GameManager.Instance.currentGameMode?.OnBallThrow();
                        currentBall = null;
                        touchAccuracy = 0;
                        currentScoreTrigger.SetShootingMode(false);
                        
                        break;
                    }

                    _swipeEnd = touch.position;
                    float horizontalSwipeMagnitude = _swipeEnd.x - _swipeStart.x;
                    float verticalSwipeMagnitude = _swipeEnd.y - _swipeStart.y;
                    SwipeDirection swipeDirection = 0;

                    if (Mathf.Abs(horizontalSwipeMagnitude) > Mathf.Abs(verticalSwipeMagnitude))
                    {
                        if (Mathf.Abs(horizontalSwipeMagnitude) > 150)
                        {
                            if (_swipeEnd.x - _swipeStart.x < 0) swipeDirection = SwipeDirection.Right;
                            else swipeDirection = SwipeDirection.Left;
                            StartCoroutine(RotationRoutine(swipeDirection));
                        }
                    }
                    else {
                        if (Mathf.Abs(verticalSwipeMagnitude) > 150)
                        {
                            if(_swipeEnd.y - _swipeStart.y < 0) swipeDirection = SwipeDirection.Up;
                            else swipeDirection = SwipeDirection.Down;
                            StartCoroutine(MovingRoutine(swipeDirection));
                        }
                    }
                }
            }

            lastTouchPosition = Input.touches.Last().position;

            if(currentBall != null && currentBall.shootingMode)
            {
				var touchPosition = Mathf.Clamp(lastTouchPosition.y,
                firstTouchPosition.y - Screen.height * 0.2f,
                firstTouchPosition.y);

                touchAccuracy = Mathf.Abs(_correctTouchPosition.y - touchPosition);
                touchAccuracy = 1 - (0.5f * touchAccuracy / (Screen.height * 0.2f / 2));
				touchAccuracy = System.MathF.Round(touchAccuracy, 2);
            }
        }
    }

    IEnumerator MovingRoutine(SwipeDirection dir)
    {
        if(!ableToMoving) yield break;

        float lerpProgress = 0;
        float lerpTime = 8f;
        float direction = 1f;

        if (dir == SwipeDirection.Down) direction *= -1;

        Vector3 startPosition = _controllerTransform.position;
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

        if(currentBall != null) currentBall.lerpTime = 100;

        while(lerpProgress < 1)
        {
            _controllerTransform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                lerpProgress
            );

            lerpProgress += lerpTime * Time.deltaTime;
            yield return null;
        }

        _controllerTransform.position = endPosition;
        if(currentBall != null) currentBall.lerpTime = 15;
        
        yield break;
    }

    IEnumerator RotationRoutine(SwipeDirection dir)
    {
        if(!ableToMoving) yield break;
        
        float lerpProgress = 0;
        float lerpTime = 8f;
        Vector3 rotationDir = new Vector3(0, 30, 0);
        Vector3 startRotation = _controllerTransform.eulerAngles;

        if(dir == SwipeDirection.Left) rotationDir *= -1;

        while(lerpProgress < 1)
        {
            _controllerTransform.rotation = Quaternion.Lerp(
                Quaternion.Euler(startRotation),
                Quaternion.Euler(startRotation + rotationDir),
                lerpProgress
            );

            lerpProgress += lerpTime * Time.deltaTime;
            yield return null;
        }

        _controllerTransform.rotation = Quaternion.Euler(startRotation + rotationDir);
        yield break;
    }

    public Vector3 GetDistanceToTrigger()
    {
        Vector3 playerPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 triggerPosition = new Vector3(currentScoreTrigger.transform.position.x, 0, currentScoreTrigger.transform.position.z);

        return triggerPosition - playerPosition;
    }
}
