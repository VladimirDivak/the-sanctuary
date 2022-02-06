using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerBall : Ball {

    // [SerializeField] Material transparentMaterial;
    [HideInInspector] public bool shootingMode;
    [HideInInspector] public float lerpTime = 15;

    private float _shootForce;

    private float _h = 6.5f;
    private float _gravity = -18;
    private float _distanceToTrigger;

    public override void PhysicEnable() {
        base.PhysicEnable();
        SetThrowMode(0);

        shootingMode = false;
        rigidBody.useGravity = true;

        if(PlayerController.Instance.currentScoreTrigger.correctAngle) {
            Physics.gravity = Vector3.up * _gravity;
            rigidBody.velocity = GetTrowVelocity();
            
            if (GameManager.Instance.currentGameMode != null)
            PlayerDataHandler.AddAvgAccuracy(PlayerController.Instance.sumAccuracy * 100);
        }
        else {
            Physics.gravity = Vector3.up * -9.81f;
            rigidBody.AddForce(PlayerController.Instance.cameraTransform.forward * 600 * PlayerController.Instance.shootingTouchRange);
        }

        rigidBody.AddTorque(transform.right * 2, ForceMode.Impulse);
    }

    public override void PhysicDisable() {
        if(!ableToGrabbing) return;
        
        base.PhysicDisable();
        rigidBody.useGravity = false;
        SetThrowMode(1);
        ChangeThrowPower(.5f);
    }

    void Update() {
        _distanceToTrigger = PlayerController.Instance.GetDistanceToTrigger().magnitude;
        _h = (5.5f * _distanceToTrigger) / 10f;
        _h = Mathf.Clamp(_h, 2f, 5.5f);

        if (isGrabed) {
            if(!shootingMode) {
                transformCashe.position = Vector3.Lerp (
                    transformCashe.position,
                    PlayerController.Instance.cameraTransform.position + PlayerController.Instance.cameraTransform.forward * .8f - PlayerController.Instance.cameraTransform.up * .15f,
                    Time.deltaTime * lerpTime
                );

                transform.LookAt(PlayerController.Instance.cameraTransform);

                transformCashe.position = new Vector3 (
                    Mathf.Clamp(transformCashe.position.x, -15.5f, 15.5f),
                    transformCashe.position.y,
                    Mathf.Clamp(transformCashe.position.z, -7.5f, 7.5f)
                );
            }
            else {
                var position = PlayerController.Instance.cameraTransform.position;
                var forward = PlayerController.Instance.cameraTransform.forward;
                var up = PlayerController.Instance.cameraTransform.up;
                
                transformCashe.position = Vector3.Lerp(
                position + forward * .8f - up * .15f,
                position + forward * .6f - up * .4f,
                PlayerController.Instance.shootingTouchRange);

                float lerpValue = PlayerController.Instance.currentScoreTrigger.correctAngle
                        ? PlayerController.Instance.sumAccuracy
                        : PlayerController.Instance.shootingTouchRange;
                lerpValue = System.MathF.Round(lerpValue, 1);

                ChangeThrowPower(lerpValue);
            }
        }
    }

    Vector3 GetTrowVelocity() {
        Vector3 triggerPositionOffset = new Vector3(PlayerController.Instance.cameraTransform.forward.normalized.x, 0, PlayerController.Instance.cameraTransform.forward.normalized.z) * 0.2f;
        Vector3 target;
        float coef;

        coef = System.MathF.Round(PlayerController.Instance.sumAccuracy, 2);
        if(coef >= .95f) coef = 1;
        else if(coef >= .85f && coef < .95f) coef = Random.Range(.988f, 1f);
        else if(coef >= .6f && coef < .85f) coef = .98f;
        else coef = .8f;

        target = Vector3.Lerp(transformCashe.position, triggerPositionOffset + PlayerController.Instance.currentScoreTrigger.transform.position, coef);

        float displacementY = target.y - transformCashe.position.y;
        Vector3 displacementXZ = new Vector3 (
            target.x - transformCashe.position.x,
            0,
            target.z - transformCashe.position.z
        );

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * _gravity * _h);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * _h / _gravity) + Mathf.Sqrt(2 * (displacementY - _h) / _gravity));

        return (velocityXZ + velocityY);
    }
}