using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

public class PlayerBall : Ball {

    [SerializeField]
    Material _transparentMaterial;
    [SerializeField]
    TrajectoryRenderer trajectoryLine;
    TrajectoryRenderer _currentTrajectoryLine;

    Material _baseMaterial;
    Transform _cameraTransform;
    float _shootForce;

    [HideInInspector]
    public Vector3 shootForceVector;
    [HideInInspector]
    public bool shootingMode;
    [HideInInspector]
    public float shootingOffset;

    protected override void Start() {
        _cameraTransform = Camera.main.transform;
        _baseMaterial = GetComponent<Renderer>().material;

        base.Start();
    }

    public override void PhysicEnable() {
        base.PhysicEnable();

        GetComponent<Renderer>().material = _baseMaterial;
        Destroy(_currentTrajectoryLine.gameObject);
        _currentTrajectoryLine = null;

        shootingMode = false;
        shootingOffset = 0;

        _rigidBody.AddForce(shootForceVector, ForceMode.Impulse);
        _rigidBody.detectCollisions = true;
    }

    public override void PhysicDisable() {
        base.PhysicDisable();
        GetComponent<Renderer>().material = _transparentMaterial;
    }

    public void ShootingModeInit() {
        shootingOffset = 0.25f;
        _currentTrajectoryLine = Instantiate(trajectoryLine);
        shootingMode = true;
        _rigidBody.detectCollisions = false;
    }

    void Update() {
        if (isGrabed) {
            _transform.position = Vector3.Lerp (
                _transform.position,
                _cameraTransform.position + _cameraTransform.forward * .8f - _cameraTransform.up * .15f + _cameraTransform.right * shootingOffset,
                Time.deltaTime * 30
            );

            _transform.position = new Vector3 (
                Mathf.Clamp(_transform.position.x, -15.5f, 15.5f),
                _transform.position.y,
                Mathf.Clamp(_transform.position.z, -7.5f, 7.5f)
            );

            if (shootingMode) {

                _shootForce = PlayerController.firstTouchPosition.y - PlayerController.lastTouchPosition.y;
                _shootForce = Mathf.Clamp(_shootForce, 0, Mathf.Infinity) / 60;
                shootForceVector = (_cameraTransform.forward + Vector3.up).normalized * _shootForce;

                var positionsArray = GameManager.Instance.SimulateTrajectory(_transform, shootForceVector, ForceMode.Impulse);
                _currentTrajectoryLine.SetPositions(positionsArray);
            }
        }
    }
}
