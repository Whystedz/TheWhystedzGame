using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboPlayer : MonoBehaviour
{
    [Range(0, 100f)]
    [SerializeField] private float cooldownMax;
    
    [SerializeField] private bool displayCombos;
    [SerializeField] private bool displayComboHints;
    [SerializeField] private bool canTriggerCombos;

    private InputManager inputManager;

    void Start() => inputManager = InputManager.GetInstance();

    private float cooldownProgress;

    public bool IsOnCooldown { get; private set; }

    private List<ComboPlayer> teammates;

    public List<Combo> Combos;
    public List<ComboHint> ComboHints;

    private ComboManager comboManager;

    [SerializeField] private Transform comboParticleGenerator;

    private void Awake()
    {
        var team = this.GetComponent<Teammate>().Team;

        this.teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team && teammate.gameObject != this.gameObject)
            .Select(teammate => teammate.GetComponent<ComboPlayer>())
            .ToList();

        this.comboManager = FindObjectOfType<ComboManager>();
        this.Combos = new List<Combo>();
        this.ComboHints = new List<ComboHint>();

        this.cooldownProgress = this.cooldownMax;
    }

    public List<ComboPlayer> Teammates (bool includeSelf)
    {
        var teammatesResult = new List<ComboPlayer>(this.teammates);

        if (includeSelf)
            teammatesResult.Add(this);

        return teammatesResult;
    }

    void Update()
    {
        // TODO include more digging logic in the following PR

        CooldownUpdate();

        if (displayComboHints)
            this.comboManager.HighlightComboHints(this);
        if (displayCombos)
            this.comboManager.HighlightPlayersCombos(this);

        if (this.canTriggerCombos
            && inputManager.GetDigging()
            && Combos.Count() > 0
            && !this.IsOnCooldown)
            TriggerCombos();

        foreach (Transform child in comboParticleGenerator)
            child.GetComponent<ComboParticleIndicator>().UpdateParticles(this.Combos, this.ComboHints);

        this.Combos.Clear();
        this.ComboHints.Clear();
    }

    private void TriggerCombos() => this.comboManager.TriggerCombos(this);

    internal void StartCooldown()
    {
        this.IsOnCooldown = true;
        this.cooldownProgress = this.cooldownMax;
    }

    private void CooldownUpdate()
    {
        if (!this.IsOnCooldown)
            return;
        
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
