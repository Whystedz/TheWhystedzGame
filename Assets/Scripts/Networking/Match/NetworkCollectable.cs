using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCollectable : NetworkBehaviour
{
    [SerializeField] private int pointsWorth;
    public int PointsWorth { get => this.pointsWorth; }

    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float verticalAnimationAmplitude = 0.5f;

    private Vector3 restPosition;
    private float verticalAnimationOffset;

    protected void Awake()
    {
        this.restPosition = transform.position;
        this.verticalAnimationOffset = UnityEngine.Random.Range(-this.verticalAnimationAmplitude, this.verticalAnimationAmplitude)
            * 2*Mathf.PI;
    }

    public void UpdateRestPosition(Vector3 restPosition) => this.restPosition = restPosition;

    protected void Update()
    {
        this.transform.Rotate(Vector3.up * this.rotationSpeed, Space.Self);

        this.transform.position = restPosition
            + Vector3.up * Mathf.Sin(Time.time + this.verticalAnimationOffset) * this.verticalAnimationAmplitude;
    }

    public virtual void Collect() 
    {
        Destroy(this.gameObject);
    }
}
