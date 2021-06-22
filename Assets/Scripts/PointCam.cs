using UnityEngine;
using System.Linq;

public class PointCam : MonoBehaviour
{
    private Transform PointCamTransform;
    private Transform BallTransform;
    private Vector3 DefaultPos;
    private Vector3 DefaultRot;
    private Vector3 ConstOffset;

    private Vector3 Net1forAngle, Net2forAngle;

    private string CurrentBall;
    private BallLogic BallScript;

    private Vector3 Net1, Net2;

    private Vector3 CameraDirection;
    private Vector3 NetPosition;

    void Start()
    {
        Net1 = GameObject.FindGameObjectsWithTag("ScoreTrigger").Where(x => x.transform.name == "ScoreTriggerLeft").Last().transform.position;
        Net2 = new Vector3(-Net1.x, Net1.y, Net1.z);

        BallScript = GameObject.FindObjectOfType<BallLogic>();

        Net1forAngle = new Vector3(Net1.x, 0, 0);
        Net2forAngle = new Vector3(-Net1.x, 0, 0);

        PointCamTransform = transform;
        DefaultPos = PointCamTransform.position;
        DefaultRot = PointCamTransform.rotation.eulerAngles;
        ConstOffset = new Vector3(12.369f, 3.802f, 0) - PointCamTransform.position;

        BallTransform = GameObject.FindGameObjectWithTag("Ball").transform;
    }
    
    public void OnBallGrab(Transform _ball)
    {
        BallTransform = _ball;
        CurrentBall = BallTransform.transform.name;
        BallScript = BallTransform.GetComponent<BallLogic>();
    }

    private void Update()
    {
        float Angle = 0;
        float AngleModule = 1;
        float RotationOffset = 90;

        Vector3 NetAngle = new Vector3();
        Vector3 Offset = ConstOffset;

        Vector3 Test = new Vector3();

        if(CameraRaycast.IsBoard1)
        {
            NetPosition = Net1;

            NetAngle = Net1forAngle;
            if(BallScript.StartToFlyPosition.z < 0)
            {
                AngleModule = -1;
                Test = new Vector3(Test.x, Test.y, -Test.z);
            }
        }
        else if(CameraRaycast.IsBoard2)
        {
            NetPosition = Net2;

            Offset = new Vector3(ConstOffset.x - 0.438f, ConstOffset.y, ConstOffset.z);
            NetAngle = Net2forAngle;
            if(BallScript.StartToFlyPosition.z > 0)
            {
                AngleModule = -1;
                Test = new Vector3(Test.x, Test.y, -Test.z);
            }
            RotationOffset *= -1;
        }

        Angle = Vector3.Angle(NetAngle, NetAngle - BallScript.StartToFlyPosition) * AngleModule;
        PointCamTransform.rotation = Quaternion.Euler(new Vector3(30, Angle + RotationOffset, 0));

        PointCamTransform.position = CameraDirection * ConstOffset.magnitude + BallTransform.position - new Vector3(0, ConstOffset.y, 0);
    }

    public void OnBallThrow(string _newBall)
    {
        var GUIScript = GameManager.FindObjectOfType<GUI>();
        CameraDirection = -(new Vector3(NetPosition.x, 0, NetPosition.z) - new Vector3(BallTransform.position.x, 0, BallTransform.position.z)).normalized;
        if(_newBall == CurrentBall) GUIScript.SetActivePointCam(false, null);
    }
}
