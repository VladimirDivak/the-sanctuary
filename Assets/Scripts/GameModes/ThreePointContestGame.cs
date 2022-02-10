using System.Linq;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

public class ThreePointContestGame : GameMode
{
    [SerializeField] BallsRack ballRack;
    BallsRack _currentBallRack;

    void Start()
    {
        useBlockBallGrabbing = true;
        UseDestroyBallAfterThrow = true;
    }

    public override void StartGame()
    {
        currentThreePointPosition = ThreePointPosition.Poisiton90;

        Fade.Instance.ShowAfter(() =>
        {
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

            sirenSoundHandler.PlayStartGameSounds(async ()=>
            {
                PlayerController.
                    Instance.
                    ableToRaycast = true;
                
                await StartTimer();
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
            Vector3 rackPosition = rack.transform.right * .5f - rack.transform.forward * .5f + rack.transform.position;
            rack.transform.position = rackPosition;
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
    
    public override void OnBallThrow()
    {
        Debug.Log(currentThreePointPosition);
        throwsCounter++;
    }

    public override void OnGetScore()
    {
        isScore = true;

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
        if(isScore)
        {
            isScore = false;
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
                _currentBallRack = null;
                currentThreePointPosition = ThreePointPosition.Poisiton90;
                throwsCounter = 0;

                GameManager.Instance.currentGameMode = null;
                GameManager.Instance.ResetGameState();
            });
        });
    }
}
