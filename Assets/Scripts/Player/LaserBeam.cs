using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] private Transform laserStartPoint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float delay = 0.3f;
    [SerializeField] private Vector3 laserOffset;
    private float timer = 0f;
    private bool canStart;

    public void SetTarget(Vector3 targetPosition) => this.targetPosition = targetPosition + this.laserOffset;
    public void SetStartPoint(Transform point) => this.laserStartPoint = point;
    public void Enable() => this.lineRenderer.enabled = true;
    public void Disable() => this.lineRenderer.enabled = false;
    public void StartLaster() => canStart = true;
    private void Awake() => this.lineRenderer.enabled = false;
    
    private void Update()
    {
        if (!this.canStart || this.laserStartPoint == null)
            return;

        if (this.timer > this.duration + this.delay)
        {
            this.canStart = false;
            this.timer = 0f;
            Disable();
            return;
        }

        timer += Time.deltaTime;

        if (this.timer < this.delay)
            return;
        else if(this.timer < this.duration + this.delay)
        {
            Enable();
            this.lineRenderer.SetPosition(0, this.laserStartPoint.position);
            this.lineRenderer.SetPosition(1, this.targetPosition);
        }
    }
}
