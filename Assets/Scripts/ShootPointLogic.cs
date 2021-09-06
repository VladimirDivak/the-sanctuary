using UnityEngine;


//  данный скрипт описывает поведение
//  точки, в которую мяч стримится в случае,
//  если при броске игрок направил прицел
//  в область баскетбольного кольца
public class ShootPointLogic : MonoBehaviour
{
    public static Transform PointTransform;
    public static Transform Net1;
    public static Transform Net2;

    private float _y;
    private float _distanceK;

    Vector3 _toNet1FromPoint;
    Vector3 _toNet2FromPoint;
    Vector3 _positionConst;

    public static float ToNetDistance;

    public static Transform PlayerControllerTransform;
    private PlayerBall _ballScript;

    bool _backToConst;

    public void OnBallInit()
    {
        _ballScript = GameObject.Find("Ball").GetComponent<PlayerBall>();
    }

    void Start()
    {
        _positionConst = transform.position;

        Net1 = GameObject.Find("net").transform;
        Net2 = GameObject.Find("net (1)").transform;

        PointTransform = transform;
        _y = PointTransform.position.y;

        Vector3 NET1 = new Vector3(Net1.position.x, _y, Net1.position.z);
        Vector3 NET2 = new Vector3(Net2.position.x, _y, Net2.position.z);
        Vector3 SQUARE = new Vector3(PointTransform.position.x, _y, PointTransform.position.z);

        _toNet1FromPoint = NET1 - SQUARE;
        _toNet2FromPoint = NET2 - new Vector3(-SQUARE.x, SQUARE.y, SQUARE.z);
    }

    void Update()
    {
        Vector3 ToNetFromPoint = new Vector3();
        Vector3 NET = new Vector3();
        Vector3 CONTROLLER = new Vector3();

        try
        {
            CONTROLLER = new Vector3(PlayerControllerTransform.position.x, _y, PlayerControllerTransform.position.z);
        }
        catch
        {
            Debug.LogWarning("Возможно, котроллеры ещё не были загружены");
        }

        if (CameraRaycast.IsBoard1)
        {
            NET = new Vector3(Net1.position.x, _y, Net1.position.z);
            ToNetFromPoint = _toNet1FromPoint;
        }
        else if(CameraRaycast.IsBoard2)
        {
            NET = new Vector3(Net2.position.x, _y, Net2.position.z);
            ToNetFromPoint = _toNet2FromPoint;
            PointTransform.position = new Vector3(-PointTransform.position.x, PointTransform.position.y, PointTransform.position.z);
        }

        if ((CONTROLLER - NET).magnitude > 1.3f)
        {
            if (_backToConst)
            {
                PointTransform.position = _positionConst;
                _backToConst = false;
            }
            if(_ballScript.isZone3pt)
            {
                _distanceK = 0.5f;
                _y = 4.8f;
            }
            else
            {
                _distanceK = 0.35f;
            }

            PointTransform.position = Vector3.Normalize(CONTROLLER - NET) * (ToNetFromPoint.magnitude + ((CONTROLLER - NET).magnitude * _distanceK)) + NET;
        }
        else
        {
            _backToConst = true;
            PointTransform.position = new Vector3(PlayerController.ControllerTransform.position.x, 3, PlayerController.ControllerTransform.position.z) + CameraRaycast.CameraTransform.forward;
        }
    }
}
