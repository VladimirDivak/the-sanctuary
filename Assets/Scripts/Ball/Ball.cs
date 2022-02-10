using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public abstract class Ball : MonoBehaviour
{
    [SerializeField]
    protected List<AudioClip> BouncesSound = new List<AudioClip>();

    protected Transform transformCashe;
    protected Rigidbody rigidBody;
    protected AudioSource bouncesSource;

    [HideInInspector] public bool onAir;
    [HideInInspector] public bool itsPoint;
    [HideInInspector] public bool isGrabed;
    [HideInInspector] public bool ballCorrectHigh;

    protected bool ballOnParket;

    public int parketHitCount { get; protected set; }

    Coroutine c_ballOnAir;
    Coroutine c_chekBallHigh;
    Coroutine c_changeForceOffset;

    protected Vector3 lastPosition;
    protected Quaternion lastRotation;

    protected bool ableToGrabbing = true;
    protected bool destroyAfter = false;

    private Material _baseMaterial;

    public string propColor { get; private set; } = "_Color";
    public string propUseEmmision { get; private set; } = "_UseEmission";
    public string propEmissionColor { get; private set; } = "_EmissionColor";
    public string propThrowMode { get; private set; } = "_ThrowMode";
    public string propThrowPower { get; private set; } = "_ThrowPower";
    public string propIsLastBall { get; private set; } = "_IsLastBall";

    public void ChangeColor(Color color) => _baseMaterial.SetColor(propColor, color);
    public void SetEmission(int value) => _baseMaterial.SetInt(propUseEmmision, value);
    public void ChangeEmissionColor (Color color) => _baseMaterial.SetColor(propEmissionColor, color);
    public void SetThrowMode(int value) => _baseMaterial.SetInt(propThrowMode, value);
    public void ChangeThrowPower(float value) => _baseMaterial.SetFloat(propThrowPower, value);
    public void SetLastBallTexture(int value) => _baseMaterial.SetInt(propIsLastBall, value);

    protected virtual void Start()
    {
        _baseMaterial = GetComponent<MeshRenderer>().material;

        transformCashe = transform;
        rigidBody = GetComponent<Rigidbody>();
        bouncesSource = GetComponent<AudioSource>();

        GameManager.Instance.AddColliderForNet(GetComponent<SphereCollider>());

        if(GameManager.Instance.currentGameMode != null)
        {
            ableToGrabbing = GameManager.Instance.
                currentGameMode.
                useBlockBallGrabbing;

            destroyAfter = GameManager.Instance.
                currentGameMode.
                useBlockBallGrabbing;
        }
    }

    public void StopCheckBallHight()
    {
        if(c_chekBallHigh != null)
        {
            StopCoroutine(c_chekBallHigh);
            c_chekBallHigh = null;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 0.1f)
        {
            bouncesSource.clip = BouncesSound[Random.Range(0, BouncesSound.Count)];
            bouncesSource.Play();

            if (collision.transform.tag == "Parket")
            {
                if(parketHitCount == 0)
                {
                    if(GameManager.Instance.currentGameMode != null)
                    {
                        GameManager.Instance.currentGameMode.OnBallGetParket();
                    }
                    if(destroyAfter) DestroyBall();
                }

                ballOnParket = true;
                parketHitCount++;

                if (c_ballOnAir != null)
                {
                    StopCoroutine(c_ballOnAir);
                    c_ballOnAir = null;
                }
            }
        }
    }

    async void DestroyBall()
    {
        await Task.Delay(3000);
        GameManager.Instance.RemoveColliderForNet(GetComponent<SphereCollider>());
        Destroy(this.gameObject);
    }

    public virtual void PhysicEnable()
    {
        StopCheckBallHight();
        c_chekBallHigh = StartCoroutine(ChekHigh());
        if(c_ballOnAir != null)
        {
            StopCoroutine(c_ballOnAir);
            c_ballOnAir = null;
        }
        c_ballOnAir = StartCoroutine(WaitingBallOnAir());

        onAir = true;
        rigidBody.isKinematic = false;

        isGrabed = false;
    }
    public virtual void PhysicDisable()
    {
        if(!ableToGrabbing) return;

        StopCheckBallHight();
        if(c_ballOnAir != null)
        {
            StopCoroutine(c_ballOnAir);
            c_ballOnAir = null;
        }

        parketHitCount = 0;
        onAir = false;
        rigidBody.isKinematic = true;

        isGrabed = true;
    }

    protected IEnumerator ChekHigh()
    {
        while(true) {
            yield return null;
            if(transformCashe.position.y >= 2.8f) {
                ballCorrectHigh = true;
                break;
            }
        }
        yield break;
    }

    protected IEnumerator WaitingBallOnAir()
    {
        int count = 0;

        while(ballOnParket == false)
        {
            yield return new WaitForSeconds(1);
            count++;
            
            if(count == 10)
            {
                rigidBody.AddForce(new Vector3(2,2,0), ForceMode.Impulse);
                break;
            }
        }
        yield break;
    }
}
