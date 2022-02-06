using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using TheSanctuary.Interfaces;

public class ThreePointContestGame : MonoBehaviour, IGameMode
{
    enum ThreePointPosition
    {
        Poisiton90,
        Position45,
        PositionCenter,
        Positon45Minus,
        Position90Minus
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

            siren.PlayStartGameSounds(()=>
            {
                PlayerController.
                    Instance.
                    ableToRaycast = true;
            });
        });
    }

    Vector3 GetThrowPosition(ThreePointPosition position)
    {
        int angle = 0;
        switch(position)
        {
            case ThreePointPosition.Poisiton90:
                _currentPosition = ThreePointPosition.Poisiton90;
                angle = 90;
            break;

            case ThreePointPosition.Position45:
                _currentPosition = ThreePointPosition.Position45;
                angle = 45;
            break;

            case ThreePointPosition.PositionCenter:
                _currentPosition = ThreePointPosition.PositionCenter;
                angle = 1;
            break;

            case ThreePointPosition.Positon45Minus:
                _currentPosition = ThreePointPosition.Positon45Minus;
                angle = -45;
            break;

            case ThreePointPosition.Position90Minus:
                _currentPosition = ThreePointPosition.Position90Minus;
                angle = -90;
            break;
        }

        Vector3 distanceFromNet = _selectedScoreTrigger.transform.forward * _distanceToNet;
        distanceFromNet = new Vector3(distanceFromNet.x, 0, distanceFromNet.z);
        distanceFromNet = Quaternion.Euler(Vector3.up * angle) * distanceFromNet;

        return distanceFromNet + new Vector3(_selectedScoreTrigger.transform.position.x, 0, _selectedScoreTrigger.transform.position.z);
    }

    async Task ChangeThrowPosition()
    {
        ballRack.activeBalls = 5;
        ballRack.useLastBall = true;

        if(_currentBallRack != null)
        {
            Destroy(_currentBallRack.gameObject);
            _currentBallRack = null;
        }

        float angle = 0;
        float rackAngle = 0;
        PlayerController.Instance.position = GetThrowPosition(_currentPosition);

        switch(_currentPosition)
        {
            case ThreePointPosition.Poisiton90:
            angle = 180;
            rackAngle = 90;
            break;

            case ThreePointPosition.Position45:
            angle = 180 + 45;
            rackAngle = 90 - 45;
            break;

            case ThreePointPosition.PositionCenter:
            angle = 180 + 90;
            rackAngle = 90 - 90;
            break;

            case ThreePointPosition.Positon45Minus:
            angle = 180 + 135;
            rackAngle = 90 - 135;
            break;

            case ThreePointPosition.Position90Minus:
            angle = 180 + 180;
            rackAngle = 90 - 180;
            break;
        }

        if (_selectedScoreTrigger.name.Contains("Right"))
        {
            angle -= 180;
            rackAngle -= 180;
        }

        Quaternion playerRotation = Quaternion.Euler(Vector3.forward * angle);
        playerRotation = Quaternion.Euler(90, playerRotation.eulerAngles.y, playerRotation.eulerAngles.z);
        PlayerController.Instance.rotation = playerRotation;

        await Task.Delay(10);
        
        Vector3 currentRotation = PlayerController.Instance.transform.eulerAngles;
        PlayerController.Instance.rotation = Quaternion.Euler(currentRotation.x,
            currentRotation.y,
            currentRotation.z + _selectedScoreTrigger.angleForGames);
        
        Vector3 rackPosition = PlayerController.Instance.cameraTransform.right * 0.5f +
            PlayerController.Instance.cameraTransform.forward * 0.5f +
            PlayerController.Instance.position;

        rackPosition = new Vector3(rackPosition.x, 0, rackPosition.z);

        _currentBallRack = Instantiate(
            ballRack,
            rackPosition,
            Quaternion.Euler(new Vector3(0, rackAngle, 0)));
    }

    public string GetGameDiscription()
    {
        return string.Empty;
    }
    
    public void OnBallThrow()
    {
        _throwsCounter++;
    }

    public void OnGetScore()
    {
        _isScore = true;

        if(_throwsCounter % 5 == 0)
        {
            if(_throwsCounter != 25 && _currentPosition != ThreePointPosition.Position90Minus)
            {
                _currentPosition++;
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
                _currentPosition++;
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

                Destroy(_currentBallRack.gameObject);
                _currentBallRack = null;
                _currentPosition = ThreePointPosition.Poisiton90;
                _throwsCounter = 0;

                GameManager.Instance.currentGameMode = null;
                GameManager.Instance.ResetGameState();
            });
        });
    }
}
