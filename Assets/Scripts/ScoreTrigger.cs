using System.Linq;
using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    [SerializeField] GameObject net;

    Transform _triggerTransform;
    AudioSource _netSound;
    Material _netMaterial;

    public float angleBetweenCameraAndNet;
    public float angleForGames;
    public float angleBetweenPlayerAndNet;

    public bool correctAngle;
    public bool isEnable { get; set; }
    public bool isShootingMode => _netMaterial.GetInt("_useColorLerp") == 1 ? true : false;
    public void SetShootingMode(bool isTrue) => _netMaterial.SetInt("_useColorLerp", isTrue ? 1 : 0);

    void Start()
    {
        _netSound = GetComponent<AudioSource>();
        _triggerTransform = transform;
        _netMaterial = net.GetComponent<SkinnedMeshRenderer>().materials[0];
    }

    void Update()
    {
        Vector3 fromNetToCamera = -(PlayerController.Instance.cameraTransform.position - new Vector3(transform.position.x, 3.2f, transform.position.z)).normalized;
        Vector3 cameraForwardDir = PlayerController.Instance.cameraTransform.forward.normalized;
        angleBetweenCameraAndNet = Vector3.SignedAngle(fromNetToCamera, cameraForwardDir, Vector3.up);
        angleForGames = Vector3.SignedAngle(new Vector3(fromNetToCamera.x, 0, fromNetToCamera.z), new Vector3(cameraForwardDir.x, 0, cameraForwardDir.z), Vector3.up);
        
        var playerAngleA = new Vector3(
            fromNetToCamera.x,
            0,
            fromNetToCamera.z);

        var playerAngleB = new Vector3(
            PlayerController.Instance.transform.up.x, 
            0,
            PlayerController.Instance.transform.up.z);

        angleBetweenPlayerAndNet = Vector3.SignedAngle(playerAngleA, playerAngleB, Vector3.up);

        if (Mathf.Abs(angleBetweenCameraAndNet) < 30 && !isEnable)
        {
            isEnable = true;
            if(PlayerController.Instance.currentScoreTrigger.isShootingMode) SetShootingMode(true);
            PlayerController.Instance.currentScoreTrigger.SetShootingMode(false);
            PlayerController.Instance.currentScoreTrigger.isEnable = false;
            PlayerController.Instance.currentScoreTrigger = this;
        }

        if(isEnable && _netMaterial.GetInt("_useColorLerp") == 1) {
            _netMaterial.SetFloat("_lerpValue", System.MathF.Round(PlayerController.Instance.shootAccuracy, 1));
            correctAngle = Mathf.Abs(angleBetweenCameraAndNet) <= PlayerController.Instance.minAngleValue ? true : false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Ball>(out var ball))
        {
            ball.StopCheckBallHight();
            if(ball.ballCorrectHigh == true)
            {
                GameManager
                    .Instance
                    .currentGameMode
                    .OnGetScore();
                
                ball.itsPoint = true;
                _netSound.Play();
            }
        }
    }
}
