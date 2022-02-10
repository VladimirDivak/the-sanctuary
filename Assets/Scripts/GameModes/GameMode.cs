using System.Linq;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using TheSanctuary;
using TheSanctuary.Interfaces;

public enum ThreePointPosition
{
    Poisiton90 = 180,
    Position45 = 135,
    PositionCenter = 90,
    Positon45Minus = 45,
    Position90Minus = 0
}

public abstract class GameMode : MonoBehaviour, IGameMode
{
    [SerializeField] protected SirenSound sirenSoundHandler;
    [SerializeField] protected ScoreTrigger[] scoreTriggers;
    protected ScoreTrigger selectedScoreTrigger;

    public NetworkGame gameInformation { get; set; }
    public bool isMultiplayer { get; set; }
    public int scoreMultiplier { get; set; }
    public bool useBlockBallGrabbing { get; set; }
    public bool UseDestroyBallAfterThrow { get; set; }
    public ushort currentGameTime { get; set; }
    public uint currentGameScores { get; set; }

    protected const float threePointLineDistance = 7f;
    protected const float freeThrowLineDistance = 4.6f;
    
    protected int throwsCounter;
    protected bool isScore;
    protected ThreePointPosition currentThreePointPosition { get; set; }

    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private CancellationToken _cancellationToken = _cancellationTokenSource.Token;

    protected virtual Vector3 GetThrowPosition(ThreePointPosition position)
    {
        int angle = (int)position - 90;

        Vector3 distanceFromNet = selectedScoreTrigger.transform.forward * threePointLineDistance;
        distanceFromNet = new Vector3(distanceFromNet.x, 0, distanceFromNet.z);
        distanceFromNet = Quaternion.Euler(Vector3.up * angle) * distanceFromNet;

        return distanceFromNet + new Vector3(selectedScoreTrigger.transform.position.x, 0, selectedScoreTrigger.transform.position.z);
    }

    protected virtual async Task ChangeThrowPosition()
    {
        await Task.Yield();
    }

    protected async Task StartTimer()
    {
        while(!_cancellationToken.IsCancellationRequested)
        {
            currentGameTime++;
            await Task.Delay(1, _cancellationToken);
        }
    }

    public void SetNetworkGameData(NetworkGame gameData)
    {
        gameInformation = gameData;
    }

    public virtual string GetGameDiscription()
    {
        return string.Empty;
    }

    public virtual void StartGame() { }

    public virtual void EndGame()
    {
        _cancellationTokenSource.Cancel();

        if(currentGameTime > gameInformation.bestTime)
        {
            Debug.Log($"Новый рекорд - {currentGameTime / 1000} секунд!");
            gameInformation.bestTime = currentGameTime;
        }

        if(currentGameScores > gameInformation.bestScore)
        {
            Debug.Log($"Новый рекорд - {currentGameScores} очков!");
            gameInformation.bestScore = currentGameScores;
        }
    }

    public virtual void OnGetScore() { }
    public virtual void OnBallGetParket() { }
    public virtual void OnBallThrow() { }
}
