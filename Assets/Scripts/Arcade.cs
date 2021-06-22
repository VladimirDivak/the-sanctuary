using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class Arcade : MonoBehaviour
{
    private GUI GUIScript;
    // private GamemodeSelector ModeSelector;
    private BallLogic BallScript;

    private Rigidbody BallRB;

    private GameObject ArcadeText;
    public GameObject Scoreboard;
    private GameObject SirenLigts;

    public TextMesh ScoreboardText;
    private Text ArcadeTextField;

    private int Timer = 3;
    private int GlobalTimer = 0;
    private int Scores = 0;

    private Vector3[] ShootingPos = new Vector3[9];
    private List<Vector3> NewPositions = new List<Vector3>();

    private Coroutine C_ArcadeInit;
    private Coroutine C_TimerCounter;
    private Coroutine C_ArcadeEnd;

    private bool IsInit = true;

    private AudioSource SirenSource;

    [SerializeField] public AudioClip TimerSoundMiddle;
    [SerializeField] public AudioClip TimerSoundHigh;
    [SerializeField] public AudioClip EndgameSound;

    public bool ArcadeMode;

    void SetPositionsList()
    {
        NewPositions = ShootingPos.ToList();
        for(int x = 0; x < ShootingPos.Length; x++)
        {
            if(NewPositions[x].z != 0)
            {
                NewPositions.Add(new Vector3(NewPositions[x].x, NewPositions[x].y, -NewPositions[x].z));
            }
        }
    }

    void Start()
    {
        ShootingPos[0] = new Vector3(8.35f, 0, 0);
        ShootingPos[1] = new Vector3(5.65f, 0, 0);
        ShootingPos[2] = new Vector3(6.15f, 0, 3.275f);
        ShootingPos[3] = new Vector3(9.45f, 0, 2.95f);
        ShootingPos[4] = new Vector3(8.5f, 0, 6.3f);
        ShootingPos[5] = new Vector3(12.7f, 0, 2.7f);
        ShootingPos[6] = new Vector3(12.7f, 0, 4.95f);
        ShootingPos[7] = new Vector3(12.7f, 0, 7.45f);
        ShootingPos[8] = new Vector3(1.5f, 0, 0);

        SetPositionsList();

        GUIScript = FindObjectOfType<GUI>();
        // ModeSelector = FindObjectOfType<GamemodeSelector>();

        ArcadeText = GameObject.Find("ArcadeText");
        ArcadeTextField = ArcadeText.GetComponent<Text>();
        ArcadeText.SetActive(false);

        SirenSource = GameObject.Find("Siren").GetComponent<AudioSource>();

        SirenLigts = GameObject.Find("SirenLights");
        SirenLigts.SetActive(false);

        
    }

    public void ArcadeInitialization()
    {
        BallRB = GameObject.FindObjectOfType<BallLogic>().GetComponent<Rigidbody>();

        // ModeSelector.HideGamemodeSelector();
        C_ArcadeInit = StartCoroutine(ArcadeInit());
        if(C_ArcadeEnd != null)
        {
            StopCoroutine(C_ArcadeEnd);
            C_ArcadeEnd = null;

        }

        for(int x = 0; x < ShootingPos.Length; x++)
        {
            ShootingPos[x].y = -0.08000016f;
        }

        BallScript = FindObjectOfType<BallLogic>();
        IsInit = true;
    }

    public void AfterFade()
    {
        if(IsInit == true)
        {
            IsInit = false;
            if(C_ArcadeInit != null)
            {
                StopCoroutine(C_ArcadeInit);
                C_ArcadeInit = null;
            }
            ArcadeMode = true;
            ArcadeText.SetActive(true);
            GameManager.SetMovingControl = false;

            if(C_TimerCounter != null)
            {
                StopCoroutine(C_TimerCounter);
                C_TimerCounter = null;
            }
            C_TimerCounter = StartCoroutine(TimerCounter());

            PlayerController.ControllerTransform.position = ShootingPos[0];
        }
        else
        {
            if(NewPositions.Count == 0)
            {
                SetPositionsList();
            }

            var RandomInt = Random.Range(0, NewPositions.Count);
            PlayerController.ControllerTransform.position = NewPositions[RandomInt];
            NewPositions.Remove(NewPositions[RandomInt]);
        }

        BallScript.BallSetArcadePosition();
    }

    public void OnGetScore()
    {
        Scores++;
        Timer += 5; 
    }

    public void OnBallCollisionWithParket()
    {
        //GUIScript.SetFade();
    }

    private IEnumerator ArcadeInit()
    {
        yield return null;
        GUIScript.ShowPopUpMessage("arcade mode will be started after 5 seconds...", Color.green, PopUpMessageType.Message);

        yield return new WaitForSecondsRealtime(5);

        //GUIScript.SetFade();
    }

    private IEnumerator ArcadeEnd()
    {
        ArcadeTextField.text = $"{GlobalTimer} SECOND(S)\n{Scores} SCORE(S)";
        ScoreboardText.text = $"{GlobalTimer} SECOND(S)\n{Scores} SCORE(S)";

        ArcadeMode = false;
        Timer = 3;
        Scores = 0;
        GlobalTimer = 0;

        // ModeSelector.ShowGamemodeSelector();
        GameManager.SetMovingControl = true;

        BallRB.isKinematic = false;
        BallRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        BallRB.detectCollisions = true;

        yield return new WaitForSeconds(5);

        SirenLigts.SetActive(false);

        ArcadeText.SetActive(false);
        Scoreboard.SetActive(false);
        ScoreboardText.text = string.Empty;

        if(C_TimerCounter != null)
        {
            StopCoroutine(C_TimerCounter);
            C_TimerCounter = null;
        }

        yield return null;
    }

    private IEnumerator TimerCounter()
    {
        yield return null;
        BallRB.detectCollisions = false;
        while(Timer != 0)
        {
            if(SirenSource.clip.name != TimerSoundMiddle.name) SirenSource.clip = TimerSoundMiddle;
            SirenSource.Play();

            ArcadeTextField.text = Timer.ToString();
            yield return new WaitForSeconds(1);
            Timer--;
            yield return null;
        }

        SirenSource.clip = TimerSoundHigh;
        SirenSource.Play();

        Timer = 5;
        BallRB.detectCollisions = true;
        Scoreboard.SetActive(true);

        while(true)
        {
            yield return null;

            if(Timer <= 3 && Timer >= 1)
            {
                SirenSource.clip = TimerSoundMiddle;
                SirenSource.Play();
            }

            if(Timer == 0)
            {
                SirenLigts.SetActive(true);
                SirenSource.clip = EndgameSound;
                SirenSource.Play();

                C_ArcadeEnd = StartCoroutine(ArcadeEnd());
                break;
            }

            ScoreboardText.text = $"{GlobalTimer} SECOND(S)\n{Scores} SCORE(S)";
            ArcadeTextField.text = Timer.ToString();

            yield return new  WaitForSeconds(1);

            GlobalTimer++;
            Timer--;
        }
    }
}
