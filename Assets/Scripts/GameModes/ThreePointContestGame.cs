using System.Linq;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

public class ThreePointContestGame : GameMode
{
    [SerializeField] BallsRack ballRack;

    void Start()
    {
        useBlockBallGrabbing = true;
        UseDestroyBallAfterThrow = true;
    }

    public override void StartGame()
    {
        base.StartGame();
        gameInformation = PlayerDataHandler.GetNetworkGameData(nameof(ThreePointContestGame));
        currentThreePointPosition = ThreePointPosition.Poisiton90;

        Fade.Instance.ShowAfter(() =>
        {
            SetBestIndicatorsData();
            GUI.ShowScoreboard(true);
            GUI.ShowTimer(true);

            PlayerController.Instance.ableToRaycast = false;
            PlayerController.Instance.ableToMoving = false;

            var racks = FindObjectsOfType<BallsRack>();
            foreach(var rack in racks) Destroy(rack.gameObject);

            var balls = FindObjectsOfType<Ball>();
            foreach(var ball in balls) Destroy(ball.gameObject);

            int scoreTriggerIndex = Random.Range(0, scoreTriggers.Length);
            selectedScoreTrigger = scoreTriggers[scoreTriggerIndex];

            Fade.Instance.actionTask = ChangeThrowPosition();
            GameManager.Instance.currentGameMode = this;

            SpawnBallRacks();

            sirenSoundHandler.PlayStartGameSounds(()=>
            {
                PlayerController.
                    Instance.
                    ableToRaycast = true;
                
                c_GameTotalTimeCounter = StartCoroutine(TotalGameTimeCounterRoutine());
            });
        });
    }

    void SpawnBallRacks()
    {
        ballRack.activeBalls = 5;
        ballRack.useLastBall = true;

        for(int i = (int)ThreePointPosition.Poisiton90; i > -1; i -= 45)
        {
            int rackAngle = i - 90;
            if (selectedScoreTrigger.name.Contains("Right")) rackAngle -= 180;

            BallsRack rack = Instantiate(ballRack, GetThrowPosition((ThreePointPosition)i), Quaternion.Euler(Vector3.up * rackAngle));
            Transform rackTransform = rack.transform;
            Vector3 rackPosition = rackTransform.right * .5f - rackTransform.forward * .5f + rackTransform.position;
            rackTransform.position = rackPosition;
        }
    }

    protected override async Task ChangeThrowPosition()
    {
        float angle = 360 - (int)currentThreePointPosition;
        PlayerController.Instance.position = GetThrowPosition(currentThreePointPosition);

        if (selectedScoreTrigger.name.Contains("Right"))
        {
            angle -= 180;
        }

        Quaternion playerRotation = Quaternion.Euler(Vector3.forward * angle);
        playerRotation = Quaternion.Euler(90, playerRotation.eulerAngles.y, playerRotation.eulerAngles.z);
        PlayerController.Instance.rotation = playerRotation;

        await Task.Delay(10);
        
        Vector3 currentRotation = PlayerController.Instance.transform.eulerAngles;
        PlayerController.Instance.rotation = Quaternion.Euler(currentRotation.x,
            currentRotation.y,
            currentRotation.z + selectedScoreTrigger.angleForGames);
    }

    public override void OnGetScore()
    {
        base.OnGetScore();
        
        if(throwsCounter % 5 == 0)
        {
            if(throwsCounter != 25 && currentThreePointPosition != ThreePointPosition.Position90Minus)
            {
                currentThreePointPosition -= 45;
                Fade.Instance.ShowAfter(()=> Fade.Instance.actionTask = ChangeThrowPosition());
            }
            else EndGame();
        }
    }

    public override void OnBallGetParket()
    {
        base.OnBallGetParket();

        if(isScore)
        {
            return;
        }

        if(throwsCounter % 5 == 0)
        {
            if(throwsCounter != 25 && currentThreePointPosition != ThreePointPosition.Position90Minus)
            {
                currentThreePointPosition -= 45;
                Fade.Instance.ShowAfter(()=> Fade.Instance.actionTask = ChangeThrowPosition());
            }
            else EndGame();
        }
    }

    public override void EndGame()
    {
        base.EndGame();

        sirenSoundHandler.PlayEndGameSounds(()=>
        {
            Fade.Instance.ShowAfter(()=>
            {
                PlayerDataHandler.UpdateNetworkGameData(gameInformation, nameof(ThreePointContestGame));
                Fade.Instance.actionTask = PlayerDataHandler.SaveAsync();
                
                PlayerController.Instance.ableToMoving = true;
                foreach(BallsRack rack in FindObjectsOfType<BallsRack>())
                {
                    Destroy(rack.gameObject);
                }
                currentThreePointPosition = ThreePointPosition.Poisiton90;

                Reset();
            });
        });
    }
}
