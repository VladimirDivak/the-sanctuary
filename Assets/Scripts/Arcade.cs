using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;


//  старый скрипт, описывающтй один из режимов игры
public class Arcade : MonoBehaviour
{
    private GUI _guiScript;
    // private GamemodeSelector ModeSelector;
    private PlayerBall _ballScript;

    private Rigidbody _ballRB;

    private GameObject _arcadeText;
    public GameObject Scoreboard;
    private GameObject _sirenLigts;

    public TextMesh ScoreboardText;
    private Text _arcadeTextField;

    private int _timer = 3;
    private int _globalTimer = 0;
    private int _scores = 0;

    private Vector3[] _shootingPos = new Vector3[9];
    private List<Vector3> _newPositions = new List<Vector3>();

    private Coroutine C_ArcadeInit;
    private Coroutine C_TimerCounter;
    private Coroutine C_ArcadeEnd;

    private bool _isInit = true;

    private AudioSource _sirenSource;

    [SerializeField] public AudioClip TimerSoundMiddle;
    [SerializeField] public AudioClip TimerSoundHigh;
    [SerializeField] public AudioClip EndgameSound;

    public bool ArcadeMode;

    void SetPositionsList()
    {
        _newPositions = _shootingPos.ToList();
        for(int x = 0; x < _shootingPos.Length; x++)
        {
            if(_newPositions[x].z != 0)
            {
                _newPositions.Add(new Vector3(_newPositions[x].x, _newPositions[x].y, -_newPositions[x].z));
            }
        }
    }

    void Start()
    {
        _shootingPos[0] = new Vector3(8.35f, 0, 0);
        _shootingPos[1] = new Vector3(5.65f, 0, 0);
        _shootingPos[2] = new Vector3(6.15f, 0, 3.275f);
        _shootingPos[3] = new Vector3(9.45f, 0, 2.95f);
        _shootingPos[4] = new Vector3(8.5f, 0, 6.3f);
        _shootingPos[5] = new Vector3(12.7f, 0, 2.7f);
        _shootingPos[6] = new Vector3(12.7f, 0, 4.95f);
        _shootingPos[7] = new Vector3(12.7f, 0, 7.45f);
        _shootingPos[8] = new Vector3(1.5f, 0, 0);

        SetPositionsList();

        _guiScript = FindObjectOfType<GUI>();
        // ModeSelector = FindObjectOfType<GamemodeSelector>();

        _arcadeText = GameObject.Find("ArcadeText");
        _arcadeTextField = _arcadeText.GetComponent<Text>();
        _arcadeText.SetActive(false);

        _sirenSource = GameObject.Find("Siren").GetComponent<AudioSource>();

        _sirenLigts = GameObject.Find("SirenLights");
        _sirenLigts.SetActive(false);

        
    }

    public void ArcadeInitialization()
    {
        _ballRB = GameObject.FindObjectOfType<PlayerBall>().GetComponent<Rigidbody>();

        // ModeSelector.HideGamemodeSelector();
        C_ArcadeInit = StartCoroutine(ArcadeInit());
        if(C_ArcadeEnd != null)
        {
            StopCoroutine(C_ArcadeEnd);
            C_ArcadeEnd = null;

        }

        for(int x = 0; x < _shootingPos.Length; x++)
        {
            _shootingPos[x].y = -0.08000016f;
        }

        _ballScript = FindObjectOfType<PlayerBall>();
        _isInit = true;
    }

    public void AfterFade()
    {
        if(_isInit == true)
        {
            _isInit = false;
            if(C_ArcadeInit != null)
            {
                StopCoroutine(C_ArcadeInit);
                C_ArcadeInit = null;
            }
            ArcadeMode = true;
            _arcadeText.SetActive(true);
            GameManager.SetMovingControl = false;

            if(C_TimerCounter != null)
            {
                StopCoroutine(C_TimerCounter);
                C_TimerCounter = null;
            }
            C_TimerCounter = StartCoroutine(TimerCounter());

            PlayerController.ControllerTransform.position = _shootingPos[0];
        }
        else
        {
            if(_newPositions.Count == 0)
            {
                SetPositionsList();
            }

            var RandomInt = Random.Range(0, _newPositions.Count);
            PlayerController.ControllerTransform.position = _newPositions[RandomInt];
            _newPositions.Remove(_newPositions[RandomInt]);
        }

        _ballScript.BallSetArcadePosition();
    }

    public void OnGetScore()
    {
        _scores++;
        _timer += 5; 
    }

    public void OnBallCollisionWithParket()
    {
        //GUIScript.SetFade();
    }

    private IEnumerator ArcadeInit()
    {
        yield return null;
        _guiScript.ShowPopUpMessage("arcade mode will be started after 5 seconds...", Color.green, PopUpMessageType.Message);

        yield return new WaitForSecondsRealtime(5);

        //GUIScript.SetFade();
    }

    private IEnumerator ArcadeEnd()
    {
        _arcadeTextField.text = $"{_globalTimer} SECOND(S)\n{_scores} SCORE(S)";
        ScoreboardText.text = $"{_globalTimer} SECOND(S)\n{_scores} SCORE(S)";

        ArcadeMode = false;
        _timer = 3;
        _scores = 0;
        _globalTimer = 0;

        // ModeSelector.ShowGamemodeSelector();
        GameManager.SetMovingControl = true;

        _ballRB.isKinematic = false;
        _ballRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _ballRB.detectCollisions = true;

        yield return new WaitForSeconds(5);

        _sirenLigts.SetActive(false);

        _arcadeText.SetActive(false);
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
        _ballRB.detectCollisions = false;
        while(_timer != 0)
        {
            if(_sirenSource.clip.name != TimerSoundMiddle.name) _sirenSource.clip = TimerSoundMiddle;
            _sirenSource.Play();

            _arcadeTextField.text = _timer.ToString();
            yield return new WaitForSeconds(1);
            _timer--;
            yield return null;
        }

        _sirenSource.clip = TimerSoundHigh;
        _sirenSource.Play();

        _timer = 5;
        _ballRB.detectCollisions = true;
        Scoreboard.SetActive(true);

        while(true)
        {
            yield return null;

            if(_timer <= 3 && _timer >= 1)
            {
                _sirenSource.clip = TimerSoundMiddle;
                _sirenSource.Play();
            }

            if(_timer == 0)
            {
                _sirenLigts.SetActive(true);
                _sirenSource.clip = EndgameSound;
                _sirenSource.Play();

                C_ArcadeEnd = StartCoroutine(ArcadeEnd());
                break;
            }

            ScoreboardText.text = $"{_globalTimer} SECOND(S)\n{_scores} SCORE(S)";
            _arcadeTextField.text = _timer.ToString();

            yield return new  WaitForSeconds(1);

            _globalTimer++;
            _timer--;
        }
    }
}
