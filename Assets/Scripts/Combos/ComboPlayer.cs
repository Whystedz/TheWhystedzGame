using Mirror.Cloud.Examples.Pong;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboPlayer : MonoBehaviour
{
    [Range(0, 10f)]
    [SerializeField] private float cooldownMax;

    private ComboManager comboManager;
    private ComboPlayer[] team;
    private float cooldownProgress;

    public bool IsOnCooldown { get; private set; }

    public bool IsInvolvedInACombo4;
    public bool IsInvolvedInACombo3;

    [SerializeField] private GameObject debugSphereOuterPrefab;
    [SerializeField] private GameObject debugSphereInnerPrefab;

    private GameObject[,] debugSpheresCombo4;
    private GameObject[,] debugSpheresCombo3;

    private void InstantiateAllDebugSpheres()
    {
        this.debugSpheresCombo4 = new GameObject[3, 4];
        for (var setIndex = 0; setIndex < 3; ++setIndex)
        {
            this.debugSpheresCombo4[setIndex, 0] = Instantiate(this.debugSphereOuterPrefab, this.transform); // Outer
            for (var sphereIndex = 1; sphereIndex < 4; ++sphereIndex)
                this.debugSpheresCombo4[setIndex, sphereIndex] = Instantiate(this.debugSphereInnerPrefab, this.transform); // Inner
        }

        this.debugSpheresCombo3 = new GameObject[2, 3];
        for (var setIndex = 0; setIndex < 2; ++setIndex)
        {
            this.debugSpheresCombo3[setIndex, 0] = Instantiate(this.debugSphereOuterPrefab, this.transform); // Outer
            for (var sphereIndex = 1; sphereIndex < 3; ++sphereIndex)
                this.debugSpheresCombo3[setIndex, sphereIndex] = Instantiate(this.debugSphereInnerPrefab, this.transform); // Inner
        }

        DeactivateAllDebugSpheres();
    }

    private void DeactivateAllDebugSpheres()
    {
        foreach (var sphere in this.debugSpheresCombo4)
                sphere.SetActive(false);

        foreach (var sphere in this.debugSpheresCombo3)
                sphere.SetActive(false);
    }

    void Start()
    {
        this.comboManager = FindObjectOfType<ComboManager>();

        this.team = FindObjectsOfType<ComboPlayer>().Where(member => member != this).ToArray();

        InstantiateAllDebugSpheres();
    }

    // Update is called once per frame
    void Update()
    {
        //DeactivateAllDebugSpheres();

        //CooldownUpdate();

        //Check4Combos();  
        //Check3Combos();  
        //Check2Combos();
    }

    private void LateUpdate()
    {
        IsInvolvedInACombo4 = false;
        IsInvolvedInACombo3 = false;
    }

    private void Check4Combos()
    {
        // Narrow down by cooldown
        var applicableTeamMembers = this.team.Where(player => !player.IsOnCooldown).ToList();
        if (applicableTeamMembers.Count() == 0)
            return;

        // Narrow down by max distance -- highlighting will be the max
        applicableTeamMembers = applicableTeamMembers.Where(player =>
            IsInRange(this, player, comboManager.DistanceCombo3, comboManager.HighlightingTolerance)
        ).ToList();
        if (applicableTeamMembers.Count() < 3)
            return;

        var a = this;
        var b = applicableTeamMembers[0];
        var c = applicableTeamMembers[1];
        var d = applicableTeamMembers[2];

        if (!IsInRange(a, b, comboManager.DistanceCombo4, comboManager.TriggerToleranceCombo4))
            return;

        if (!IsInRange(a, c, comboManager.DistanceCombo4, comboManager.TriggerToleranceCombo4))
            return;

        if (!IsInRange(a, d, comboManager.DistanceCombo4, comboManager.TriggerToleranceCombo4))
            return;

        if (!IsInRange(b, c, comboManager.DistanceCombo4, comboManager.TriggerToleranceCombo4))
            return;

        if (!IsInRange(b, d, comboManager.DistanceCombo4, comboManager.TriggerToleranceCombo4))
            return;

        if (!IsInRange(c, d, comboManager.DistanceCombo4, comboManager.TriggerToleranceCombo4))
            return;

        Debug.Log($"Epic 4 Combo between {this.name}, {b.name}, {c.name}, and {d.name}!");
        
        Highlight(a, b, this.comboManager.Combo4Color);
        Highlight(a, c, this.comboManager.Combo4Color);
        Highlight(a, d, this.comboManager.Combo4Color);
        Highlight(b, c, this.comboManager.Combo4Color);
        Highlight(b, d, this.comboManager.Combo4Color);
        Highlight(c, d, this.comboManager.Combo4Color);
        
        this.IsInvolvedInACombo4 = true;
    }

    private void Check3Combos()
    {
        // Narrow down by cooldown
        var applicableTeamMembers = this.team.Where(player => !player.IsOnCooldown).ToList();
        if (applicableTeamMembers.Count() == 0)
            return;

        // TODO update this
        // Narrow down by max distance -- highlighting will be the max
        applicableTeamMembers = applicableTeamMembers.Where(player =>
            IsInRange(this, player, comboManager.DistanceCombo3, comboManager.HighlightingTolerance)
        ).ToList();
        if (applicableTeamMembers.Count() < 2)
            return;

        Check3Combo(applicableTeamMembers[0], applicableTeamMembers[1], 0);

        // With 4 players, a second set of 3 may be possible involving this player:
        if (applicableTeamMembers.Count() >= 3)
            Check3Combo(applicableTeamMembers[1], applicableTeamMembers[2], 1);
    }

    private void Check3Combo(ComboPlayer b, ComboPlayer c, int setIndex)
    {
        var a = this; // for the sake of consistency with b and c

        var centerOfMass = (a.transform.position + b.transform.position + c.transform.position) / 3;
        var directionToCenter = (centerOfMass - this.transform.position).normalized;
        var centerOfComboDircle = a.transform.position + directionToCenter * ComboManager.ComboRadiusModifier * this.comboManager.DistanceCombo3;

        Debug.DrawLine(
            a.transform.position,
            centerOfComboDircle, 
            Color.grey);

        var comboSphere = this.debugSpheresCombo3[setIndex, 0];
        comboSphere.SetActive(true);
        comboSphere.transform.position = centerOfComboDircle;
        var comboSphereRadius = ComboManager.ComboRadiusModifier * this.comboManager.DistanceCombo3;
        comboSphere.transform.localScale = 2 * Vector3.one * comboSphereRadius;

        var toleranceSphereB = this.debugSpheresCombo3[setIndex, 1];
        toleranceSphereB.SetActive(true);
        toleranceSphereB.transform.position = centerOfComboDircle 
            + Quaternion.AngleAxis(-60f, Vector3.up) * directionToCenter * comboSphereRadius;
        toleranceSphereB.transform.localScale = 2 * Vector3.one * this.comboManager.TriggerToleranceCombo3;

        var toleranceSphereC = this.debugSpheresCombo3[setIndex, 2];
        toleranceSphereC.SetActive(true);
        toleranceSphereC.transform.position = centerOfComboDircle
            + Quaternion.AngleAxis(60f, Vector3.up) * directionToCenter * comboSphereRadius;
        toleranceSphereC.transform.localScale = 2 * Vector3.one * this.comboManager.TriggerToleranceCombo3;

        if (!IsInRange(a, b, comboManager.DistanceCombo3, comboManager.TriggerToleranceCombo3))
            return;

        if (!IsInRange(a, c, comboManager.DistanceCombo3, comboManager.TriggerToleranceCombo3))
            return;

        if (!IsInRange(b, c, comboManager.DistanceCombo3, comboManager.TriggerToleranceCombo3))
            return;

        Debug.Log($"3 Combo between {this.name}, {b.name}, and {c.name}!");

        this.IsInvolvedInACombo3 = true;

        if (!this.IsInvolvedInACombo4)
        {
            Highlight(a, b, this.comboManager.Combo3Color);
            Highlight(a, c, this.comboManager.Combo3Color);
            Highlight(b, c, this.comboManager.Combo3Color);
        }
    }

    private void Check2Combos()
    {
        // Narrow down by cooldown
        var applicableTeamMembers = this.team.Where(player => !player.IsOnCooldown);
        if (applicableTeamMembers.Count() == 0)
            return;

        // Narrow down by max distance -- highlighting will be the max
        applicableTeamMembers = applicableTeamMembers.Where(player => 
            IsInRange(this, player, comboManager.DistanceCombo2, comboManager.HighlightingTolerance)
        );
        if (applicableTeamMembers.Count() == 0)
            return;

        foreach (var player in applicableTeamMembers)
        {
            var canTrigger = IsInRange(this, player, this.comboManager.DistanceCombo2, this.comboManager.TriggerToleranceCombo2);
            var color = canTrigger ? this.comboManager.Combo2Color : this.comboManager.HintColor;

            if (!this.IsInvolvedInACombo3 && !this.IsInvolvedInACombo4)
                Highlight(this, player, color);
        }

    }

    private void Highlight(ComboPlayer a, ComboPlayer b, Color color)
    {
        //Debug.DrawLine(a.transform.position, b.transform.position, color);
    }

    private bool IsInRange(ComboPlayer a, ComboPlayer b, float requiredDistance, float tolerance)
    {
        var distance = Vector3.Distance(a.transform.position, b.transform.position);

        var lowerBound = requiredDistance - tolerance;
        var upperBound = requiredDistance + tolerance;

        return distance >= lowerBound && distance <= upperBound;
    }

    private void StartCooldown()
    {
        this.IsOnCooldown = true;
        this.cooldownProgress = this.cooldownMax;
    }

    private void CooldownUpdate()
    {
        this.cooldownProgress -= Time.deltaTime;
        
        if (this.cooldownProgress <= 0)
            this.IsOnCooldown = false;
    }
}
