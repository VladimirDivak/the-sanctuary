using UnityEngine;


//  логика поведения при попадании мяча в кольцо.
//  явный минус - разделение логики локального мяча
//  и мячей сетевых игроков


public class ScoreTrigger : MonoBehaviour
{
    private Transform _triggerTransform;
    private AudioSource _netSound;

    void Start()
    {
        _netSound = GetComponent<AudioSource>();
        _triggerTransform = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Ball>(out var ball))
        {
            ball.StopCheckBallHight();
            if(ball.ballCorrectHigh == true)
            {
                ball.itsPoint = true;
                _netSound.Play();
            }
        }
    }
}
