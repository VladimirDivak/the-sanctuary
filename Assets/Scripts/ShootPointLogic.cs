using UnityEngine;

public class ShootPointLogic : MonoBehaviour
{
    public static Transform PointTransform;
    public static Transform Net1;
    public static Transform Net2;

    private float Y;
    private float DistanceK;

    Vector3 ToNet1FromPoint;
    Vector3 ToNet2FromPoint;
    Vector3 PositionConst;

    public static float ToNetDistance;

    public static Transform PlayerControllerTransform;
    private BallLogic BallScript;

    bool BackToConst;

    public void OnBallInit()
    {
        BallScript = GameObject.Find("Ball").GetComponent<BallLogic>();
    }

    void Start()
    {
        PositionConst = transform.position;

        Net1 = GameObject.Find("net").transform;
        Net2 = GameObject.Find("net (1)").transform;

        PointTransform = transform;
        Y = PointTransform.position.y;

        Vector3 NET1 = new Vector3(Net1.position.x, Y, Net1.position.z);
        Vector3 NET2 = new Vector3(Net2.position.x, Y, Net2.position.z);
        Vector3 SQUARE = new Vector3(PointTransform.position.x, Y, PointTransform.position.z);

        ToNet1FromPoint = NET1 - SQUARE;
        ToNet2FromPoint = NET2 - new Vector3(-SQUARE.x, SQUARE.y, SQUARE.z);
    }

    void Update()
    {
        Vector3 ToNetFromPoint = new Vector3();
        Vector3 NET = new Vector3();
        Vector3 CONTROLLER = new Vector3();

        try
        {
            CONTROLLER = new Vector3(PlayerControllerTransform.position.x, Y, PlayerControllerTransform.position.z);
        }
        catch
        {
            Debug.LogWarning("Возможно, котроллеры ещё не были загружены");
        }

        if (CameraRaycast.IsBoard1)
        {
            NET = new Vector3(Net1.position.x, Y, Net1.position.z);
            ToNetFromPoint = ToNet1FromPoint;
        }
        else if(CameraRaycast.IsBoard2)
        {
            NET = new Vector3(Net2.position.x, Y, Net2.position.z);
            ToNetFromPoint = ToNet2FromPoint;
            PointTransform.position = new Vector3(-PointTransform.position.x, PointTransform.position.y, PointTransform.position.z);
        }

        if ((CONTROLLER - NET).magnitude > 1.3f)
        {
            if (BackToConst)
            {
                PointTransform.position = PositionConst;
                BackToConst = false;
            }
            if(BallScript.isZone3pt)
            {
                DistanceK = 0.5f;
                Y = 4.8f;
            }
            else
            {
                DistanceK = 0.35f;
            }

            PointTransform.position = Vector3.Normalize(CONTROLLER - NET) * (ToNetFromPoint.magnitude + ((CONTROLLER - NET).magnitude * DistanceK)) + NET;
        }
        else
        {
            BackToConst = true;
            PointTransform.position = new Vector3(PlayerController.ControllerTransform.position.x, 3, PlayerController.ControllerTransform.position.z) + CameraRaycast.CameraTransform.forward;
        }
    }
}
