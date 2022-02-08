using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using TheSanctuary.Interfaces;

public class ThreePointContestGame : MonoBehaviour, IGameMode
{
    enum ThreePointPosition
    {
        Poisiton90 = 180,
        Position45 = 135,
        PositionCenter = 90,
        Positon45Minus = 45,
        Position90Minus = 0
    }

    public bool isMultiplayer { get; set; }
    public int scoreMultiplier { get; set; }
    public string gamemodeName { get; set; } = "Three-Point Contest";
    public bool blockBallGrabbing { get; set; }
    public bool destroyBallAfter { get; set; }

    private readonly float _distanceToNet = 7f;
    private int _throwsCounter;
    private bool _isScore;

    [SerializeField] ScoreTrigger[] scoreTriggers;
    ScoreTrigger _selectedScoreTrigger;

    [SerializeField] SirenSound siren;

    [SerializeField] BallsRack ballRack;
    BallsRack _currentBallRack;

    ThreePointPosition _currentPosition = ThreePointPosition.Poisiton90;

    void Start()
    {
        blockBallGrabbing = true;
        destroyBallAfter = true;
    }

    public void StartGame()
    {
        Fade.Instance.ShowAfter(() =>
        {
            PlayerController.Instance.ableToRaycast = false;
            PlayerController.Instance.ableToMoving = false;

            var racks = FindObjectsOfType<BallsRack>();
            foreach(var rack in racks) Destroy(rack.gameObject);

            var balls = FindObjectsOfType<Ball>();
            foreach(var ball in balls) Destroy(ball.gameObject);

            int scoreTriggerIndex = Random.Range(0, scoreTriggers.Length);
            _selectedScoreTrigger = scoreTriggers[scoreTriggerIndex];

            Fade.Instance.actionTask = ChangeThrowPosition();
            GameManager.Instance.currentGameMode = this;

            SpawnBallRacks();

            siren.PlayStartGameSounds(()=>
            {
                PlayerController.
                    Instance.
                    ableToRaycast = true;
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
            if (_selectedScoreTrigger.name.Contains("Right")) rackAngle -= 180;

            BallsRack rack = Instantiate(ballRack, GetThrowPosition((ThreePointPosition)i), Quaternion.Euler(Vector3.up * rackAngle));
            Vector3 rackPosition = rack.transform.right * .5f - rack.transform.forward * .5f + rack.transform.position;
            rack.transform.position = rackPosition;
        }
    }

    Vector3 GetThrowPosition(ThreePointPosition position)
    {
        int angle = (int)position - 90;

        Vector3 distanceFromNet = _selectedScoreTrigger.transform.forward * _distanceToNet;
        distanceFromNet = new Vector3(distanceFromNet.x, 0, distanceFromNet.z);
        distanceFromNet = Quaternion.Euler(Vector3.up * angle) * distanceFromNet;

        return distanceFromNet + new Vector3(_selectedScoreTrigger.transform.position.x, 0, _selectedScoreTrigger.transform.position.z);
    }

    async Task ChangeThrowPosition()
    {
        float angle = 360 - (int)_currentPosition;
        PlayerController.Instance.position = GetThrowPosition(_currentPosition);

        if (_selectedScoreTrigger.name.Contains("Right"))
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
            currentRotation.z + _selectedScoreTrigger.angleForGames);
    }

    public string GetGameDiscription()
    {
        return string.Empty;
    }
    
    public void OnBallThrow()
    {
        Debug.Log(_currentPosition);
        _throwsCounter++;
    }

    public void OnGetScore()
    {
        _isScore = true;

        if(_throwsCounter % 5 == 0)
        {
            if(_throwsCounter != 25 && _currentPosition != ThreePointPosition.Position90Minus)
            {
                _currentPosition -= 45;
                Fade.Instance.ShowAfter(()=> Fade.Instance.actionTask = ChangeThrowPosition());
            }
            else EndGame();
        }
    }

    public void OnBallGetParket()
    {
        if(_isScore)
        {
            _isScore = false;
            return;
        }

        if(_throwsCounter % 5 == 0)
        {
            if(_throwsCounter != 25 && _currentPosition != ThreePointPosition.Position90Minus)
            {
                _currentPosition -= 45;
                Fade.Instance.ShowAfter(()=> Fade.Instance.actionTask = ChangeThrowPosition());
            }
            else EndGame();
        }
    }

    public void EndGame()
    {
        PlayerDataHandler.Save();

        siren.PlayEndGameSounds(()=>
        {
            Fade.Instance.ShowAfter(()=>
            {
                PlayerController.Instance.ableToMoving = true;

                foreach(BallsRack rack in FindObjectsOfType<BallsRack>())
                {
                    Destroy(rack.gameObject);
                }
                _currentBallRack = null;
                _currentPosition = ThreePointPosition.Poisiton90;
                _throwsCounter = 0;

                GameManager.Instance.currentGameMode = null;
                GameManager.Instance.ResetGameState();
            });
        });
    }
}
