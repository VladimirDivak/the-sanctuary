using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Ball : MonoBehaviour {
    [SerializeField]
    private protected List<AudioClip> BouncesSound = new List<AudioClip>();

    private protected Transform _transform;
    private protected Rigidbody _rigidBody;
    private protected AudioSource _bouncesSource;

    [HideInInspector]
    public bool onAir;
    [HideInInspector]
    public bool itsPoint;
    [HideInInspector]
    public bool isGrabed;
    [HideInInspector]
    public bool ballCorrectHigh;

    private protected bool _ballOnParket;

    public int parketHitCount { get; protected set; }

    private protected Coroutine c_ballOnAir;
    private protected Coroutine c_chekBallHigh;
    private protected Coroutine c_changeForceOffset;

    private protected Vector3 _lastPosition;
    private protected Quaternion _lastRotation;

    protected virtual void Start() {
        _transform = transform;
        _rigidBody = GetComponent<Rigidbody>();
        _bouncesSource = GetComponent<AudioSource>();
    }

    public void StopCheckBallHight() {
        if(c_chekBallHigh != null) {
            StopCoroutine(c_chekBallHigh);
            c_chekBallHigh = null;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (collision.relativeVelocity.magnitude > 0.1f) {
            _bouncesSource.clip = BouncesSound[Random.Range(0, BouncesSound.Count)];
            _bouncesSource.Play();

            if (collision.transform.tag == "Parket") {
                _ballOnParket = true;
                parketHitCount++;

                if (c_ballOnAir != null) {
                    StopCoroutine(c_ballOnAir);
                    c_ballOnAir = null;
                }
            }
        }
    }

    public virtual void PhysicEnable() {
        StopCheckBallHight();
        c_chekBallHigh = StartCoroutine(ChekHigh());

        onAir = true;
        _rigidBody.isKinematic = false;

        isGrabed = false;
    }
    public virtual void PhysicDisable() {
        StopCheckBallHight();

        parketHitCount = 0;
        onAir = false;
        _rigidBody.isKinematic = true;

        isGrabed = true;
    }

    protected IEnumerator ChekHigh() {
        while(true) {
            yield return null;
            if(_transform.position.y >= 2.8f) {
                ballCorrectHigh = true;
                break;
            }
        }
        yield break;
    }

    protected IEnumerator WaitingBallOnAir() {
        int count = 0;

        while(_ballOnParket == false) {
            yield return new WaitForSeconds(1);
            count++;
            
            if(count == 10) {
                _rigidBody.AddForce(new Vector3(2,2,0), ForceMode.Impulse);
                break;
            }
        }
        yield break;
    }
}
