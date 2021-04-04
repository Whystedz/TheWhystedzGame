using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneBounds : MonoBehaviour
{
    public Vector3 UpperLeftCorner { get; private set; }
    public Vector3 UpperRightCorner { get; private set; }
    public Vector3 LowerLeftCorner { get; private set; }
    public Vector3 LowerRightCorner { get; private set; }

    private Vector3[] meshVertices;

    private void Awake() => SetCorners();

    public void SetCorners()
    {
        this.meshVertices = GetComponent<MeshFilter>().sharedMesh.vertices;

        UpperLeftCorner = transform.TransformPoint(this.meshVertices[0]);
        UpperRightCorner = transform.TransformPoint(this.meshVertices[10]);
        LowerLeftCorner = transform.TransformPoint(this.meshVertices[110]);
        LowerRightCorner = transform.TransformPoint(this.meshVertices[120]);
    }
}
