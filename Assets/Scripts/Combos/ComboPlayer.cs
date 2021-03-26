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

    private List<ComboPlayer> teammates;
    public List<ComboPlayer> Teammates { get => this.teammates; }

    private void Awake()
    {
        var team = this.GetComponent<Teammate>().Team;

        this.teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team && teammate != this)
            .Select(teammate => teammate.GetComponent<ComboPlayer>())
            .ToList();
    }

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

        CooldownUpdate();

        //Check4Combos();  
        //Check3Combos();  
        //Check2Combos();
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
