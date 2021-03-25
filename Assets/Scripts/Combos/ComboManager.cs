using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [Range(5f, 50f)]
    [SerializeField] private float distanceCombo2;
    public float DistanceCombo2 { get => this.distanceCombo2; }
    
    [Range(0, 10f)]
    [SerializeField] private float triggerToleranceCombo2;
    public float TriggerToleranceCombo2 { get => this.triggerToleranceCombo2; }

    [Range(5f, 50f)]
    [SerializeField] private float distanceCombo3;
    public float DistanceCombo3 { get => this.distanceCombo3; }
    
    [Range(0, 10f)]
    [SerializeField] private float triggerToleranceCombo3;
    public float TriggerToleranceCombo3 { get => this.triggerToleranceCombo3; }

    [Range(5f, 50f)]
    [SerializeField] private float distanceCombo4;
    public float DistanceCombo4 { get => this.distanceCombo4; }
    
    [Range(0, 10f)]
    [SerializeField] private float triggerToleranceCombo4;
    public float TriggerToleranceCombo4 { get => this.triggerToleranceCombo4; }

    [Range(0, 50f)]
    [SerializeField] private float highlightingTolerance;
    public float HighlightingTolerance { get => this.highlightingTolerance; }

    [SerializeField] private Color hintColor;
    public Color HintColor { get => this.hintColor; }
    [SerializeField] private Color combo2Color;
    public Color Combo2Color { get => this.combo2Color; }
    [SerializeField] private Color combo3Color;
    public Color Combo3Color { get => this.combo3Color; }
    [SerializeField] private Color combo4Color;
    public Color Combo4Color { get => this.combo4Color; }
    
    // https://www.calculator.net/triangle-calculator.html?vc=120&vx=&vy=&va=30&vz=1&vb=30&angleunits=d&x=55&y=8
    public const float ComboRadiusModifier = 0.57735f;
}
