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
    public uint scoreMultiplier { get; set; } = 1;
    protected uint scoreStandart { get; set; } = 100;
    protected uint scoreGoodAccuracy { get; set; } = 500;
    protected uint scorePerfectAccuracy { get; set; } = 1000;
    protected uint goodScoresCounter { get; set; }

    public bool useBlockBallGrabbing { get; set; }
    public bool UseDestroyBallAfterThrow { get; set; }
    public float currentGameTime { get; set; }
    public uint currentGameScores { get; set; }

    const float threePointLineDistance = 7f;
    const float freeThrowLineDistance = 4.6f;
    
    protected int throwsCounter;
    protected bool isScore;
    protected ThreePointPosition currentThreePointPosition { get; set; }

    protected Coroutine c_GameTotalTimeCounter;

    protected virtual Vector3 GetThrowPosition(ThreePointPosition position)
    {
        int angle = (int)position - 90;

        Vector3 distanceFromNet = selectedScoreTrigger.transform.forward * threePointLineDistance;
        distanceFromNet = new Vector3(distanceFromNet.x, 0, distanceFromNet.z);
        distanceFromNet = Quaternion.Euler(Vector3.up * angle) * distanceFromNet;

        var selectedTriggerPos = selectedScoreTrigger.transform.position;
        return distanceFromNet + new Vector3(selectedTriggerPos.x, 0, selectedTriggerPos.z);
    }

    protected virtual async Task ChangeThrowPosition()
    {
        await Task.Yield();
    }

    protected IEnumerator TotalGameTimeCounterRoutine()
    {
        while(true)
        {
            currentGameTime += Time.deltaTime;
            currentGameTime = System.MathF.Round(currentGameTime, 2);
            Debug.Log(currentGameTime);
            yield return null;
        }
    }

    public virtual string GetGameDiscription()
    {
        return string.Empty;
    }

    public virtual void StartGame() { }
    public virtual void OnGetScore()
    {
        uint score = 0;

        int accuracy = PlayerController.Instance.roundAccuracy;
        isScore = true;

        if(accuracy < 90)
        {
            scoreMultiplier = 1;
            score = scoreStandart;
        }
        else if(accuracy > 95 && accuracy < 100)
        {
            score = scoreGoodAccuracy;
            goodScoresCounter++;
            if(goodScoresCounter % 3 == 0) scoreMultiplier++;
        }
        else if(accuracy == 100)
        {
            goodScoresCounter++;
            if(goodScoresCounter % 3 == 0) scoreMultiplier++;

            score = scorePerfectAccuracy;
            scoreMultiplier++;
        }

        currentGameScores += score * scoreMultiplier;
        Debug.Log($"очков: {currentGameScores}. точность броска: {accuracy}. множитель: {scoreMultiplier}");
    }
    public virtual void OnBallGetParket()
    {
        if(!isScore)
        {
            scoreMultiplier = 1;
            goodScoresCounter = 0;
        }
    }
    public virtual void OnBallThrow()
    {
        throwsCounter++;
    }

    public virtual void EndGame()
    {
        StopCoroutine(c_GameTotalTimeCounter);
        c_GameTotalTimeCounter = null;

        if(currentGameTime < gameInformation.bestTime || gameInformation.bestScore == 0)
        {
            Debug.Log($"Новый рекорд - {currentGameTime} секунд!");
            gameInformation.bestTime = currentGameTime;
        }

        if(currentGameScores > gameInformation.bestScore)
        {
            Debug.Log($"Новый рекорд - {currentGameScores} очков!");
            gameInformation.bestScore = currentGameScores;
        }

        Debug.Log($"Время: {currentGameTime}. Очки: {currentGameScores}.");
    }

    protected void Reset()
    {
        throwsCounter = 0;
        scoreMultiplier = 1;
        currentGameScores = 0;
        currentGameTime = 0;

        GameManager.Instance.currentGameMode = null;
        GameManager.Instance.ResetGameState();
    }
}
