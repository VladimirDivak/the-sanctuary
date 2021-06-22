using System;
using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    private Transform TriggerTransform;
    private AudioSource NetSound;

    private NetworkBall OtherBallScript;
    private BallLogic BallScript;
    // private NetworkGameRules GameRulesScript;
    private Arcade ArcadeScript;

    void Start()
    {
        //GameRulesScript = GameObject.Find("CANDYSHOP").GetComponent<NetworkGameRules>();
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

            if(BallScript.C_ChekBallHigh != null)
            {
                BallScript.StopCoroutine(BallScript.C_ChekBallHigh);
                BallScript.C_ChekBallHigh = null;
            }

            if(BallScript.BallCorrectHigh == true)
            {
                // if(!BallScript.itsPoint && GameRulesScript.isGame)
                // {
                //     if(GameRulesScript.is3pt)
                //     {
                //         if(!BallScript.isZone3pt)
                //         {
                //             BallScript.itsPoint = false;
                //         }
                //         else
                //         {
                //             BallScript.itsPoint = true;
                //         }
                //     }
                //     else
                //     {
                //         BallScript.itsPoint = true;
                //     }
                // }
                if(ArcadeScript.ArcadeMode)
                {
                    ArcadeScript.OnGetScore();
                }
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
