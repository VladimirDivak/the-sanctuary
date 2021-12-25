using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryRenderer : MonoBehaviour {
    LineRenderer _line;

    void Start() {
        _line = GetComponent<LineRenderer>();
    }

    public void SetPositions(Vector3[] points) {
        _line.positionCount = points.Length;
        _line.SetPositions(points);
    }
}