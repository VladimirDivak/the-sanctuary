using System;
using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    private Transform TriggerTransform;
    private AudioSource NetSound;

    private NetworkBall OtherBallScript;
    private BallLogic BallScript;
    private Arcade ArcadeScript;

    void Start()
    {
        ArcadeScript = FindObjectOfType<Arcade>();
        NetSound = GetComponent<AudioSource>();
        TriggerTransform = transform;
    }

    public void OnBallInit()
    {
        BallScript = GameObject.FindObjectOfType<BallLogic>();
    }

    private void OnTriggerEnter(Collider other)
    {
        OtherBallScript = null;
        
        if(other.TryGetComponent<BallLogic>(out BallScript))
        {
            BallScript = other.GetComponent<BallLogic>();
            BallScript.StopCheckBallHight();

            if(BallScript.BallCorrectHigh == true)
            {
                if(ArcadeScript.ArcadeMode)
                    ArcadeScript.OnGetScore();

                NetSound.Play();
            }
        }
        else
        {
            OtherBallScript = other.GetComponent<NetworkBall>();

            if(OtherBallScript.C_ChekBallHigh != null)
            {
                OtherBallScript.StopCoroutine(OtherBallScript.C_ChekBallHigh);
                OtherBallScript.C_ChekBallHigh = null;
            }
            
            if(OtherBallScript.BallCorrectHigh == true)
            {
                OtherBallScript.itsPoint = true;
                NetSound.Play();
            }
        }
    }
}
