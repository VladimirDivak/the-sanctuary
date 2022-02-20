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
    public int scoreMultiplier { get; set; } = 1;
    protected int scoreStandart { get; set; } = 100;
    protected int scoreGoodAccuracy { get; set; } = 250;
    protected int scorePerfectAccuracy { get; set; } = 500;
    protected int goodScoresCounter { get; set; }

    public float timeToStartThrowing { get; set; }
    public float timeToReleaseThrowing { get; set; }
    float timeBetweenStartEndReleaseThrowing => timeToReleaseThrowing - timeToStartThrowing;

    public bool useBlockBallGrabbing { get; set; }
    public bool UseDestroyBallAfterThrow { get; set; }
    public float currentGameTime { get; set; }
    public int currentGameScores { get; set; }

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

    protected void SetBestIndicatorsData()
    {
        GUI.bestIndicators.SetData(GUI.timer.ConvertToTimeValue(gameInformation.bestTime), gameInformation.bestScore.ToString());
        GUI.bestIndicators.gameObject.SetActive(true);
    }

    protected IEnumerator TotalGameTimeCounterRoutine()
    {
        while(true)
        {
            currentGameTime += Time.deltaTime;
            GUI.timer.SetValue(currentGameTime);

            yield return null;
        }
    }

    public virtual string GetGameDiscription()
    {
        return string.Empty;
    }

    public virtual void StartGame()
    {

    }

    public virtual void OnGetScore()
    {
        int score = 0;

        int accuracy = PlayerController.Instance.roundAccuracy;
        isScore = true;

        if(accuracy < 95)
        {
            scoreMultiplier = 1;
            score = scoreStandart;
        }
        else if(accuracy >= 95 && accuracy < 100)
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

        var x1 = accuracy / 100f;
        var x2 = score - (timeBetweenStartEndReleaseThrowing * 10);
        var x3 = scoreMultiplier;

        int additionalPoints = Mathf.RoundToInt(x1 * x2 * x3);
        currentGameScores += additionalPoints;
        
        GUI.scoreboard.UpdateScores(currentGameScores, scoreMultiplier);
        GUI.points.ShowValue($"+{additionalPoints}");
    }
    public virtual void OnBallGetParket()
    {
        if(!isScore)
        {
            scoreMultiplier = 1;
            goodScoresCounter = 0;

            GUI.scoreboard.UpdateScores(currentGameScores, scoreMultiplier);
        }
    }
    public virtual void OnBallThrow()
    {
        GUI.accuracy.ShowValue
        (
            PlayerController.Instance.roundAccuracy,
            System.MathF.Round(PlayerController.Instance.sumAccuracy,
            1)
        );

        isScore = false;
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
        isScore = false;

        GUI.ShowScoreboard(false);
        GUI.ShowTimer(false);
        GUI.bestIndicators.gameObject.SetActive(false);

        GameManager.Instance.currentGameMode = null;
        GameManager.Instance.ResetGameState();
    }
}
