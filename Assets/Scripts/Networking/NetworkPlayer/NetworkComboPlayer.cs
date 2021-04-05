using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkComboPlayer : NetworkBehaviour
{
    [Range(0, 100f)]
    [SerializeField] private float cooldownMax;
    
    [SerializeField] private bool displayCombos;
    [SerializeField] private bool displayComboHintInfos;
    [SerializeField] private bool canTriggerCombos;
    [SerializeField] private bool showExtendedTeamCombos = true;

    [SerializeField] private Color hintColor = Color.white;
    [SerializeField] private Color lineColor = Color.blue;
    [SerializeField] private Color triangleColor = Color.green;

    private InputManager inputManager = InputManager.GetInstance();

    private float cooldownProgress;

    public bool IsOnCooldown { get; private set; }

    private List<NetworkComboPlayer> teammates = new List<NetworkComboPlayer>();

    public List<ComboInfo> Combos = new List<ComboInfo>();
    public List<ComboHintInfo> ComboHintInfos = new List<ComboHintInfo>();

    private NetworkComboManager comboManager;

    private NetworkDigging networkDigging;
    TileManager tileManager = TileManager.GetInstance();

    public override void OnStopClient()
    {
        CmdRemoveFromComboManager();
    }

    private void Start()
    {
        networkDigging = this.GetComponent<NetworkDigging>();

        RefreshTeammates();

        this.comboManager = FindObjectOfType<NetworkComboManager>();

        this.cooldownProgress = this.cooldownMax;
        CmdRegisterToComboManager();
        comboManager.localComboPlayer = this;
    }

    public List<NetworkComboPlayer> Teammates (bool includeSelf)
    {
        var teammatesResult = new List<NetworkComboPlayer>(this.teammates);

        if (includeSelf)
            teammatesResult.Add(this);

        return teammatesResult;
    }

    public void RefreshTeammates()
    {
        var team = this.GetComponent<Teammate>().Team;
        this.teammates.Clear();

        this.teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team && teammate.gameObject != this.gameObject)
            .Select(teammate => teammate.GetComponent<NetworkComboPlayer>())
            .ToList();
    }

    [Command]
    public void CmdRegisterToComboManager()
    {
        this.comboManager.RegisterPlayer(this.GetComponent<NetworkIdentity>().netId);
    }

    [Command]
    public void CmdRemoveFromComboManager()
    {
        this.comboManager.RemoveNullPlayer();
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            CooldownUpdate();
            
            if (displayComboHintInfos)
                HighlightComboHintInfos(this);
            if (displayCombos)
                HighlightPlayersCombos(this);
            
            if (this.canTriggerCombos
                && inputManager.GetDigging()
                && Combos.Count() > 0
                && !this.IsOnCooldown)
                TriggerCombos();
        }

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

    public TileInfo TileCurrentlyAbove()
    {
        var colliders = Physics.OverlapSphere(transform.position, 0.1f);

        var tile = colliders
            .Where(collider => collider.GetComponentInParent<NetworkTile>() != null)
            .Select(collider => collider.GetComponentInParent<NetworkTile>())
            .OrderBy(tile => Vector3.Distance(this.transform.position, tile.transform.position))
            .First();

        if (tile is null)
            Debug.LogError($"Tile is null", this.gameObject);

        return tile.TileInfo;
    }

    public void DigTile(TileInfo targetTile)
    {
        networkDigging.CmdDigTile(targetTile);
    }

    public void HighlightPlayersCombos(NetworkComboPlayer comboPlayer) =>
        HighlightCombos(comboPlayer.Combos);

    internal void HighlightComboHintInfos(NetworkComboPlayer comboPlayer)
    {
        foreach (var ComboHintInfo in comboPlayer.ComboHintInfos)
            HighlightComboHintInfo(ComboHintInfo);
    }

    private void HighlightComboHintInfo(ComboHintInfo ComboHintInfo)
    {
        Highlight(ComboHintInfo.OriginPlayer,
            ComboHintInfo.TargetPlayer,
            this.hintColor);
    }

    private void HighlightCombos(List<ComboInfo> combos)
    {
        foreach (var combo in combos)
            HighlightCombo(combo);
    }

    private void HighlightCombo(ComboInfo combo, bool isExtendedCombo = false, List<ComboInfo> visitedExtendedCombos = null)
    {
        if (isExtendedCombo && visitedExtendedCombos is null)
            return; // recursion exit case for extended combos

        foreach (var tile in combo.Tiles)
        {
            NetworkTile targetTile = this.tileManager.GetTileScript(tile);
            if(targetTile)
                targetTile.HighlightTileComboDigPreview();
        }

        if (this.showExtendedTeamCombos)
            HighlightExtendedCombo(combo, visitedExtendedCombos);
    }

    private void HighlightExtendedCombo(ComboInfo combo, List<ComboInfo> visitedExtendedCombos)
    {
        foreach (var player in combo.Players)
        {
            if (player == combo.InitiatingPlayer)
                continue;

            foreach (var extendedCombo in player.Combos)
            {
                if (visitedExtendedCombos != null 
                    && visitedExtendedCombos.Contains(extendedCombo))
                    return;

                var updatedVisitedExtendedCombos = new List<ComboInfo>
                {
                    combo
                };

                if (visitedExtendedCombos != null)
                    updatedVisitedExtendedCombos.AddRange(visitedExtendedCombos);

                HighlightCombo(extendedCombo, true, updatedVisitedExtendedCombos);
            }
        }
    }

    private void Highlight(NetworkComboPlayer a, NetworkComboPlayer b, Color color) =>
        Debug.DrawLine(a.transform.position, b.transform.position, color);
}
