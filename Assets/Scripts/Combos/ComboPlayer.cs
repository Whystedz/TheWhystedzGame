using Mirror.Cloud.Examples.Pong;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

    private List<ComboPlayer> teammates;

    private void Awake()
    {
        var team = this.GetComponent<Teammate>().Team;

        this.teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team && teammate.gameObject != this.gameObject)
            .Select(teammate => teammate.GetComponent<ComboPlayer>())
            .ToList();
    }

    public List<ComboPlayer> Teammates (bool includeSelf)
    {
        var teammatesResult = new List<ComboPlayer>(this.teammates);

        if (includeSelf)
            teammatesResult.Add(this);

        return teammatesResult;
    }

    void Start()
    {
        this.comboManager = FindObjectOfType<ComboManager>();

        this.team = FindObjectsOfType<ComboPlayer>().Where(member => member != this).ToArray();
    }

    void Update()
    {
        // TODO include more digging logic in the following PR

        CooldownUpdate();
    }

    // TODO will be used for the digging hole phase
    private void StartCooldown()
    {
        this.IsOnCooldown = true;
        this.cooldownProgress = this.cooldownMax;
    }

    private void CooldownUpdate()
    {
        this.cooldownProgress -= Time.deltaTime;
        
        if (this.cooldownProgress <= 0)
        {
            this.IsOnCooldown = false;
            this.cooldownProgress = this.cooldownMax;
        }
    }

    public Tile TileCurrentlyAbove()
    {
        var colliders = Physics.OverlapSphere(transform.position, 0.1f);

        var tile = colliders
            .Where(collider => collider.GetComponentInParent<Tile>() != null)
            .Select(collider => collider.GetComponentInParent<Tile>())
            .OrderBy(tile => Vector3.Distance(this.transform.position, tile.transform.position))
            .First();

        if (tile is null)
            Debug.LogError($"Tile is null", this.gameObject);

        return tile;
    }
}
