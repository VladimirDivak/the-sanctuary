using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerBall : Ball {

    [SerializeField]
    Material _transparentMaterial;

    Material _baseMaterial;
    Transform _cameraTransform;
    bool _shootingMode;

    protected override void Start() {
        _cameraTransform = Camera.main.transform;
        _baseMaterial = GetComponent<Renderer>().material;

        base.Start();
    }

    public override void PhysicEnable() {
        base.PhysicEnable();
        GetComponent<Renderer>().material = _baseMaterial;
    }

    public override void PhysicDisable() {
        base.PhysicDisable();
        GetComponent<Renderer>().material = _transparentMaterial;
    }

    void Update() {
        if (isGrabed) {
            if (_shootingMode) {
                Debug.Log(PlayerController.firstTouchPosition - PlayerController.lastTouchPosition);

                return;
            }

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
        }
    }

    public void StartShootingMode() {
        if (!isGrabed) return;

        _shootingMode = true;
    }
}
