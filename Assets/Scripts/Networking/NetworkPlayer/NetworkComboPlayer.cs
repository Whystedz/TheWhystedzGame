using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkComboPlayer : MonoBehaviour
{
    [Range(0, 100f)]
    [SerializeField] private float cooldownMax;
    
    [SerializeField] private bool displayCombos;
    [SerializeField] private bool displayComboHintInfos;
    [SerializeField] private bool canTriggerCombos;

    private InputManager inputManager;

    void Start() => inputManager = InputManager.GetInstance();

    private float cooldownProgress;

    public bool IsOnCooldown { get; private set; }

    private List<NetworkComboPlayer> teammates;

    public List<ComboInfo> Combos;
    public List<ComboHintInfo> ComboHintInfos;

    private NetworkComboManager comboManager;

    private NetworkDigging networkDigging;

    private void Awake()
    {
        networkDigging = this.GetComponent<NetworkDigging>();

        var team = this.GetComponent<Teammate>().Team;

        this.teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team && teammate.gameObject != this.gameObject)
            .Select(teammate => teammate.GetComponent<NetworkComboPlayer>())
            .ToList();

        this.comboManager = FindObjectOfType<NetworkComboManager>();
        this.Combos = new List<ComboInfo>();
        this.ComboHintInfos = new List<ComboHintInfo>();

        this.cooldownProgress = this.cooldownMax;
    }

    public List<NetworkComboPlayer> Teammates (bool includeSelf)
    {
        var teammatesResult = new List<NetworkComboPlayer>(this.teammates);

        if (includeSelf)
            teammatesResult.Add(this);

        return teammatesResult;
    }

    void Update()
    {
        CooldownUpdate();

        if (displayComboHintInfos)
            this.comboManager.HighlightComboHintInfos(this);
        if (displayCombos)
            this.comboManager.HighlightPlayersCombos(this);

        if (this.canTriggerCombos
            && inputManager.GetDigging()
            && Combos.Count() > 0
            && !this.IsOnCooldown)
            TriggerCombos();

        this.Combos.Clear();
        this.ComboHintInfos.Clear();
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

    public NetworkTile TileCurrentlyAbove()
    {
        var colliders = Physics.OverlapSphere(transform.position, 0.1f);

        var tile = colliders
            .Where(collider => collider.GetComponentInParent<NetworkTile>() != null)
            .Select(collider => collider.GetComponentInParent<NetworkTile>())
            .OrderBy(tile => Vector3.Distance(this.transform.position, tile.transform.position))
            .First();

        if (tile is null)
            Debug.LogError($"Tile is null", this.gameObject);

        return tile;
    }

    public void DigTile(TileInfo targetTile)
    {
        networkDigging.CmdDigTile(targetTile);
    }
}
