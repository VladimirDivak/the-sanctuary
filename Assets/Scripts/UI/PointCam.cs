// using System.Linq;
// using System.Collections;
// using System.Collections.Generic;

// using UnityEngine;

// public class PointCam : MonoBehaviour
// {
//     [SerializeField]
//     private Transform _pointCameraTransform;
    
//     private Vector3 _defaultPos;
//     private Vector3 _defaultRot;
//     private Vector3 _constOffset;

//     private Vector3 _net1forAngle, _net2forAngle;

//     private Vector3 _net1, _net2;

//     private Vector3 _cameraDirection;
//     private Vector3 _netPosition;

//     void Start()
//     {
//         _net1 = GameObject.FindGameObjectsWithTag("ScoreTrigger").Last(x => x.transform.name == "ScoreTriggerLeft").transform.position;
//         _net2 = new Vector3(-_net1.x, _net1.y, _net1.z);

//         _net1forAngle = new Vector3(_net1.x, 0, 0);
//         _net2forAngle = new Vector3(-_net1.x, 0, 0);

//         _defaultPos = _pointCameraTransform.position;
//         _defaultRot = _pointCameraTransform.rotation.eulerAngles;
//         _constOffset = new Vector3(12.369f, 3.802f, 0) - _pointCameraTransform.position;
//     }
    
//     public void OnBallGrab()
//     {
//         _pointCameraTransform.gameObject.SetActive(true);
//     }

//     private void Update()
//     {
//         float Angle = 0;
//         float AngleModule = 1;
//         float RotationOffset = 90;

//         Vector3 NetAngle = new Vector3();
//         Vector3 Offset = _constOffset;

//         Vector3 Test = new Vector3();

//         if(CameraRaycast.isBoard1)
//         {
//             _netPosition = _net1;

//             NetAngle = _net1forAngle;
//             if(PlayerController.Instance.currentBall.transform.position.z < 0)
//             {
//                 AngleModule = -1;
//                 Test = new Vector3(Test.x, Test.y, -Test.z);
//             }
//         }
//         else if(CameraRaycast.isBoard2)
//         {
//             _netPosition = _net2;

//             Offset = new Vector3(_constOffset.x - 0.438f, _constOffset.y, _constOffset.z);
//             NetAngle = _net2forAngle;
//             if(_ballScript.startToFlyPosition.z > 0)
//             {
//                 AngleModule = -1;
//                 Test = new Vector3(Test.x, Test.y, -Test.z);
//             }
//             RotationOffset *= -1;
//         }

//         if(_ballScript != null)
//         {
//             Angle = Vector3.Angle(NetAngle, NetAngle - _ballScript.startToFlyPosition) * AngleModule;
//             _pointCameraTransform.rotation = Quaternion.Euler(new Vector3(30, Angle + RotationOffset, 0));

//             _pointCameraTransform.position = _cameraDirection * _constOffset.magnitude + _ballTransform.position - new Vector3(0, _constOffset.y, 0);
//         }
//     }

//     public void OnBallThrow(PlayerBall newBall)
//     {
//         var GUIScript = GameManager.FindObjectOfType<GUI>();
//         _cameraDirection = -(new Vector3(_netPosition.x, 0, _netPosition.z) - new Vector3(_ballTransform.position.x, 0, _ballTransform.position.z)).normalized;
//         if(newBall.Equals(_ballScript)) GUIScript.SetActivePointCam(false, null);
//     }
// }
