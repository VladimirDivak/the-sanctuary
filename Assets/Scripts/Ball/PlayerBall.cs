using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSanctuary;

public class PlayerBall : Ball
{
    [SerializeField]
    Material _transparentMaterial;
    private Material _baseMaterial;

    Transform _cameraTransform;

    protected override void Start()
    {
        _cameraTransform = Camera.main.transform;
        _baseMaterial = GetComponent<Renderer>().material;

        base.Start();
    }

    public override void PhysicEnable()
    {
        GetComponent<Renderer>().material = _baseMaterial;
        base.PhysicEnable();
    }

    public override void PhysicDisable()
    {
        GetComponent<Renderer>().material = _transparentMaterial;
        base.PhysicDisable();
    }

    void Update()
    {
        if(isGrabed)
        {
            _transform.position = Vector3.Lerp(
                _transform.position,
                _cameraTransform.position + _cameraTransform.forward * .8f,
                Time.deltaTime * 15
            );

            _transform.position = new Vector3(
                Mathf.Clamp(_transform.position.x, -15.5f, 15.5f),
                _transform.position.y,
                Mathf.Clamp(_transform.position.z, -7.5f, 7.5f)
            );
        }
    }
}
