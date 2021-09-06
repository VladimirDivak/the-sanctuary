using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private protected List<AudioClip> BouncesSound = new List<AudioClip>();

    private protected Transform _transform;

    private protected Rigidbody _rigidBody;
    private protected AudioSource _bouncesSource;

    public bool onAir;
    public bool itsPoint;
    public bool isGrabed;
    public bool ballCorrectHigh;

    private protected bool _ballOnParket;

    public int ParketHitCount { get; protected set; }

    private protected Coroutine c_ballOnAir;
    private protected Coroutine c_chekBallHigh;
    private protected Coroutine c_changeForceOffset;

    private protected Vector3 _lastPosition;
    private protected Quaternion _lastRotation;

    public void StopCheckBallHight()
    {
        if(c_chekBallHigh != null)
        {
            StopCoroutine(c_chekBallHigh);
            c_chekBallHigh = null;
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        
    }

    protected virtual IEnumerator ChekHigh()
    {
        return null;
    }

    protected virtual IEnumerator WaitingBallOnAir()
    {
        return null;
    }
}
