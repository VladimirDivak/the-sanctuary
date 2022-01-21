using System.Collections;
using System.Collections.Generic;
using  System.Threading.Tasks;

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

    [SerializeField]
    ScoreTrigger[] scoreTriggers;
    ScoreTrigger _selectedScoreTrigger;

    [SerializeField]
    SirenSound siren;

    [SerializeField]
    BallsRack ballRack;
    BallsRack _currentBallRack;

    ThreePointPosition _currentPosition = ThreePointPosition.Poisiton90;

    void Start()
    {
        siren.OnStartTimerEnded.AddListener(() => PlayerController.Instance.ableToRaycast = true);
        blockBallGrabbing = true;
        destroyBallAfter = true;
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

    void ChangeThrowPosition()
    {
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

        _currentBallRack = Instantiate(ballRack,
        PlayerController.Instance.transform.right * 0.5f + PlayerController.Instance.transform.up * 0.5f + PlayerController.Instance.position, Quaternion.Euler(new Vector3(0, rackAngle, 0)));
        _currentBallRack.OnRackIsEmpty.AddListener(ChangeThrowPosition);
    }

    public string GetGameDiscription()
    {
        return string.Empty;
    }

    public void StartGame()
    {
        int scoreTriggerIndex = Random.Range(0, scoreTriggers.Length);
        _selectedScoreTrigger = scoreTriggers[scoreTriggerIndex];

        PlayerController.Instance.ableToRaycast = false;
        PlayerController.Instance.ableToMoving = false;

        ChangeThrowPosition();
        siren.PlayStartGameSirenSound();

        GameManager.Instance.currentGameMode = this;
    }
    public void OnBallThrow()
    {
        if (_currentBallRack.ballsCounter == 0)
        {
            if(_currentPosition == ThreePointPosition.Position90Minus) EndGame();
            else
            {
                _currentPosition++;
                ChangeThrowPosition();
            }
        }
    }

    public void OnBallGetParket() {}

    public void OnGetScore()
    {

    }

    public void EndGame()
    {
        siren.PlayEndGameSirenSound();
        Destroy(_currentBallRack.gameObject);
        _currentBallRack = null;
        _currentPosition = ThreePointPosition.Poisiton90;

        PlayerController.Instance.ableToMoving = true;

        GameManager.Instance.currentGameMode = null;
        PlayerDataHandler.Save();
    }
}
