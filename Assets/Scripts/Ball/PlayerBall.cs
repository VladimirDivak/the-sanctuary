using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class PlayerBall : Ball {

    [SerializeField]
    Material _transparentMaterial;
    [SerializeField]
    ScoreTrigger leftScoreTrigger;
    [SerializeField]
    ScoreTrigger rightScoreTrigger;
    [SerializeField]
    GameObject leftNet;
    [SerializeField]
    GameObject rightNet;

    Material _baseMaterial;
    Material _currentNetMaterial;

    Transform _cameraTransform;
    float _shootForce;

    [HideInInspector]
    public bool shootingMode;

    [SerializeField]
    float _h = 6.5f;
    float _gravity = -18;
    float _distanceToTrigger;
    ScoreTrigger _currentScoreTrigger;

    [SerializeField]
    float _angleBetweenCameraAndScoreTrigger;
    float _accuracy;
    float _ballAccuracy;
    [SerializeField]
    float _minAngleValue;

    [SerializeField]
    float acc;
    [SerializeField]
    string accState;

    [SerializeField]
    UnityEvent<float, float> OnBallThrowing;

    string _materialPropertyName = "_accuracy";
    float _materialLerpValue;

    protected override void Start() {
        _cameraTransform = Camera.main.transform;
        _baseMaterial = GetComponent<Renderer>().material;
        _currentScoreTrigger = leftScoreTrigger;

        base.Start();
    }

    public override void PhysicEnable() {
        base.PhysicEnable();
        _transparentMaterial.SetFloat(_materialPropertyName, 0.55f);
        GetComponent<Renderer>().material = _baseMaterial;

        shootingMode = false;
        _rigidBody.useGravity = true;

        if(_angleBetweenCameraAndScoreTrigger <= _minAngleValue) {
            Physics.gravity = Vector3.up * _gravity;
            _rigidBody.velocity = GetTrowVelocity();

            OnBallThrowing?.Invoke(Mathf.RoundToInt(PlayerController.accuracy * _accuracy * 100), PlayerController.accuracy * _accuracy);
        }
        else {
            Physics.gravity = Vector3.up * -9.81f;
            _rigidBody.AddForce(_cameraTransform.forward * 600 * PlayerController.shootingTouchRange);
        }

        _currentNetMaterial?.SetFloat("_lerpValue", .55f);
        _currentNetMaterial?.SetInt("_useColorLerp", 0);
    }

    public override void PhysicDisable() {
        base.PhysicDisable();
        _rigidBody.useGravity = false;
        GetComponent<Renderer>().material = _transparentMaterial;
    }

    public void ShootingModeInit() {
        GameObject currentNet;
        shootingMode = true;
        currentNet = _currentScoreTrigger.Equals(leftScoreTrigger) ? leftNet : rightNet;
        _currentNetMaterial = currentNet.GetComponent<SkinnedMeshRenderer>().materials[0];
        _currentNetMaterial.SetInt("_useColorLerp", 1);
    }

    void Update() {
        if(_angleBetweenCameraAndScoreTrigger > 90) {
            if(_currentScoreTrigger == leftScoreTrigger) {
                _currentScoreTrigger = rightScoreTrigger;
            }
            else {
                _currentScoreTrigger = leftScoreTrigger;
            }
        }

        _distanceToTrigger = GetDistanceToTrigger(_transform.position).magnitude;
        _h = (6.5f * _distanceToTrigger) / 10f;
        _h = Mathf.Clamp(_h, 2f, 6.5f);

        Vector3 fromTriggerToPlayer = -(_cameraTransform.position - new Vector3(_currentScoreTrigger.transform.position.x, 3.2f, _currentScoreTrigger.transform.position.z)).normalized;
        Vector3 cameraForwardDir = _cameraTransform.forward.normalized;

        _angleBetweenCameraAndScoreTrigger = Vector3.Angle(fromTriggerToPlayer,
        cameraForwardDir);

        float coef = Mathf.Clamp(_angleBetweenCameraAndScoreTrigger, _minAngleValue, 90f);
        _accuracy = (1f - ((1f * coef) / _minAngleValue)) * -1;
        _accuracy = 1f - Mathf.Clamp(_accuracy, 0f, 1f);
        _accuracy = System.MathF.Round(_accuracy, 2);

        if (isGrabed) {
            if(!shootingMode)
            _transform.position = Vector3.Lerp (
                _transform.position,
                _cameraTransform.position + _cameraTransform.forward * .8f - _cameraTransform.up * .15f,
                Time.deltaTime * 30
            );

            _transform.position = new Vector3 (
                Mathf.Clamp(_transform.position.x, -15.5f, 15.5f),
                _transform.position.y,
                Mathf.Clamp(_transform.position.z, -7.5f, 7.5f)
            );

            if (shootingMode) {
                _transform.position = Vector3.Lerp(
                _cameraTransform.position + _cameraTransform.forward * .8f - _cameraTransform.up * .15f,
                _cameraTransform.position + _cameraTransform.forward * .6f - _cameraTransform.up * .4f,
                PlayerController.shootingTouchRange);

                if(_angleBetweenCameraAndScoreTrigger <= _minAngleValue) {
                    _transparentMaterial.SetFloat(_materialPropertyName, PlayerController.accuracy * _accuracy);
                }
                else _transparentMaterial.SetFloat(_materialPropertyName, PlayerController.shootingTouchRange);

                _currentNetMaterial?.SetFloat("_lerpValue", _accuracy);
            }
        }
    }

    Vector3 GetDistanceToTrigger(Vector3 position) {
        Vector3 playerPosition = new Vector3(position.x, 0, position.z);
        Vector3 triggerPosition = new Vector3(_currentScoreTrigger.transform.position.x, 0, _currentScoreTrigger.transform.position.z);
        acc = System.MathF.Round(PlayerController.accuracy * _accuracy, 1);

        return triggerPosition - playerPosition;
    }

    Vector3 GetTrowVelocity() {
        Vector3 triggerPositionOffset = new Vector3(_cameraTransform.forward.normalized.x, 0, _cameraTransform.forward.normalized.z) * 0.2f;
        Vector3 target;
        float coef;

        coef = System.MathF.Round(PlayerController.accuracy * _accuracy, 2);
        if(coef >= .95f) coef = 1;
        else if(coef >= .85f && coef < .95f) coef = Random.Range(.988f, 1f);
        else if(coef >= .6f && coef < .85f) coef = .98f;
        else coef = .8f;

        target = Vector3.Lerp(_transform.position, triggerPositionOffset + _currentScoreTrigger.transform.position, coef);

        float displacementY = target.y - _transform.position.y;
        Vector3 displacementXZ = new Vector3 (
            target.x - _transform.position.x,
            0,
            target.z - _transform.position.z
        );

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * _gravity * _h);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * _h / _gravity) + Mathf.Sqrt(2 * (displacementY - _h) / _gravity));

        return (velocityXZ + velocityY);
    }
}