using UnityEngine;


//  логика поведения при попадании мяча в кольцо.
//  явный минус - разделение логики локального мяча
//  и мячей сетевых игроков


public class ScoreTrigger : MonoBehaviour
{
    private Transform _triggerTransform;
    private AudioSource _netSound;

    private NetworkBall _otherBallScript;
    private BallLogic _ballScript;
    private Arcade _arcadeScript;

    void Start()
    {
        _netSound = GetComponent<AudioSource>();
        _triggerTransform = transform;
    }

    public void OnBallInit() =>
        _ballScript = GameObject.FindObjectOfType<BallLogic>();

    private void OnTriggerEnter(Collider other)
    {
        _otherBallScript = null;
        
        if(other.TryGetComponent<BallLogic>(out _ballScript))
        {
            _ballScript = other.GetComponent<BallLogic>();
            _ballScript.StopCheckBallHight();

            if(_ballScript.BallCorrectHigh == true)
            {
                if(_arcadeScript.ArcadeMode)
                    _arcadeScript.OnGetScore();

                _netSound.Play();
            }
        }
        else
        {
            _otherBallScript = other.GetComponent<NetworkBall>();

            if(_otherBallScript.C_ChekBallHigh != null)
            {
                _otherBallScript.StopCoroutine(_otherBallScript.C_ChekBallHigh);
                _otherBallScript.C_ChekBallHigh = null;
            }
            
            if(_otherBallScript.BallCorrectHigh == true)
            {
                _otherBallScript.itsPoint = true;
                _netSound.Play();
            }
        }
    }
}
