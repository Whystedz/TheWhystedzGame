using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class NetworkComboPlayer : NetworkBehaviour
{
    [SerializeField] private Animator animator;

    [Range(0, 100f)]
    [SerializeField] private float cooldownMax;
    
    [SerializeField] private bool displayCombos;
    [SerializeField] private bool displayComboHintInfos;
    [SerializeField] private bool canTriggerCombos;

    [SerializeField] private float afterComboPause = 0.5f;

    private float cooldownProgress;

    public bool IsOnCooldown { get; private set; }

    private NetworkComboPlayer[] teammates;
    private NetworkComboPlayer[] teammatesAndSelf;

    public HashSet<ComboInfo> Combos = new HashSet<ComboInfo>();
    public HashSet<ComboHintInfo> ComboHintInfos = new HashSet<ComboHintInfo>();

    private NetworkPlayerMovement playerMovement;

    [SerializeField] private Transform comboParticleGenerator;

    private NetworkDigging networkDigging;
    TileManager tileManager = TileManager.GetInstance();

    private static int nInstances;
    private static int nInstancesThatHaveUpdated;
    private static List<NetworkComboPlayer> queuedTriggerCombosPlayer;
    private static List<NetworkTile> currentlyHighlightedTiles;
    private static Team clientPlayerTeam;

    public bool IsClientPlayer = false;
    public bool isOnClientPlayerTeam;

    [SerializeField] private NetworkComboParticleGenerator networkComboParticleGenerator;
    private List<NetworkComboParticleIndicator> comboParticleIndicators;
    
    private void Start()
    {
        networkDigging = this.GetComponent<NetworkDigging>();
        // NetworkComboManager.Instance = FindObjectOfType<NetworkComboManager>();
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();

        nInstances += 1;

        queuedTriggerCombosPlayer = new List<NetworkComboPlayer>();
        currentlyHighlightedTiles = new List<NetworkTile>();
        comboParticleIndicators = new List<NetworkComboParticleIndicator>();

        this.cooldownProgress = this.cooldownMax;

        if (base.hasAuthority)
        {
            IsClientPlayer = true;
            NetworkComboManager.Instance.localPlayer = this;
            CmdRegisterToComboManager();
        }

        if (this.IsClientPlayer)
        {
            clientPlayerTeam = this.GetComponent<Teammate>().Team;
            isOnClientPlayerTeam = true;
        }
        else
        {
            isOnClientPlayerTeam = this.GetComponent<Teammate>().Team == clientPlayerTeam;
        }
    }

    public void InitializeComboParticleIndicators()
    {
        foreach (Transform comboParticleGeneratorTransform in this.comboParticleGenerator)
        {
            var comboParticleIndicator = comboParticleGeneratorTransform.GetComponent<NetworkComboParticleIndicator>();
            if (comboParticleIndicators.Contains(comboParticleIndicator))
                continue;
            comboParticleIndicators.Add(comboParticleIndicator);
            comboParticleIndicator.UpdateParticles(Combos, ComboHintInfos);
        }
    }

    [Command]
    public void CmdRegisterToComboManager()
    {
        NetworkComboManager.Instance.RegisterPlayer();
    }

    void Update()
    {
        if (!this.isOnClientPlayerTeam)
            return;

        if (nInstancesThatHaveUpdated == 0)
            BeforeAllComboPlayersHaveUpdated();

        if (this.teammates is null || this.teammates.Count() == 0)
            GetTeammates();

        this.Combos.Clear();
        this.ComboHintInfos.Clear();

        CheckCombosForPlayer(this);

        if (base.hasAuthority)
        {
            CooldownUpdate();

            if (this.displayCombos) 
                HighlightCombosForPlayer(this);

            if (this.displayComboHintInfos)
                foreach (var comboParticleIndicator in comboParticleIndicators)
                    comboParticleIndicator.UpdateParticles(Combos, ComboHintInfos);
            
            if (this.canTriggerCombos
                && NetworkInputManager.Instance.GetInitiateCombo()
                && Combos.Count() > 0
                && !this.IsOnCooldown)
                    QueuePlayerForTriggerCombos(this);
        }

        nInstancesThatHaveUpdated += 1;

            if (nInstancesThatHaveUpdated == nInstances)
                AfterAllComboPlayersHaveUpdated();
    }

    private static void BeforeAllComboPlayersHaveUpdated()
    {
        ClearAllComboHighlighting();
    }

    private void AfterAllComboPlayersHaveUpdated()
    {
        TriggerAllQueuedCombos();

        nInstancesThatHaveUpdated = 0;
        queuedTriggerCombosPlayer.Clear();
    }

    private static void ClearAllComboHighlighting()
    {
        foreach (var tile in currentlyHighlightedTiles)
            tile.ResetComboHighlighting();
        currentlyHighlightedTiles.Clear();
    }

    private void QueuePlayerForTriggerCombos(NetworkComboPlayer player)
    {
        queuedTriggerCombosPlayer.Add(player);
    }

    private void TriggerAllQueuedCombos()
    {
        foreach (var player in queuedTriggerCombosPlayer)
        {
            var playerPlayerMovement = player.GetComponent<NetworkPlayerMovement>();

            playerPlayerMovement.DisableMovement();

            TriggerCombosForPlayer(player);

            playerPlayerMovement.DisableMovementFor(this.afterComboPause, true);
        }
    }

    public void GetTeammates()
    {
        var team = this.GetComponent<Teammate>().Team;

        this.teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team && teammate.gameObject != this.gameObject)
            .Select(teammate => teammate.GetComponent<NetworkComboPlayer>())
            .ToArray();
        
        this.teammatesAndSelf = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team)
            .Select(teammate => teammate.GetComponent<NetworkComboPlayer>())
            .ToArray();

        networkComboParticleGenerator.GenerateComboParticleindicators();
    }

    public List<NetworkComboPlayer> Teammates(bool includeSelf)
    {
        var teammatesResult = new List<NetworkComboPlayer>(this.teammates); // ??? TODO

        if (includeSelf)
            teammatesResult.Add(this);

        return teammatesResult;
    }

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

    private void CheckCombosForPlayer(NetworkComboPlayer player)
    {
        CheckLineCombosForPlayer(player);
        CheckTriangleCombosForPlayer(player);
    }

    private void HighlightCombosForPlayer(NetworkComboPlayer player)
    {
        var alreadyHighlightedPlayers = new HashSet<NetworkComboPlayer>();

        HighlightCombosForPlayer(player, alreadyHighlightedPlayers);
    }

    private void HighlightCombosForPlayer(NetworkComboPlayer player, HashSet<NetworkComboPlayer> alreadyHighlightedPlayers)
    {
        if (NetworkComboManager.Instance.ShowExtendedTeamCombos)
        {
            if (alreadyHighlightedPlayers.Contains(player))
                return;

            alreadyHighlightedPlayers.Add(player);
        }

        foreach (var combo in player.Combos)
            HighlightCombo(combo, alreadyHighlightedPlayers);
    }

    private void HighlightCombo(ComboInfo combo, HashSet<NetworkComboPlayer> alreadyHighlightedPlayers)
    {
        if (combo.IsTriggered)
            return;

        foreach (var player in combo.Players)
            if (player != this && NetworkComboManager.Instance.ShowExtendedTeamCombos)
                HighlightCombosForPlayer(player, alreadyHighlightedPlayers);

        foreach (var tile in combo.Tiles)
        {
            NetworkTile targetTile = this.tileManager.GetTileScript(tile);

            if (tile.TileState == TileState.Normal
                && !currentlyHighlightedTiles.Contains(targetTile))
            {
                targetTile.HighlightTileComboDigPreview();
                currentlyHighlightedTiles.Add(targetTile);
            }
        }
    }

    private void CheckLineCombosForPlayer(NetworkComboPlayer player)
    {
        var teammatesE = this.teammates
            .Where(teammate => !teammate.IsOnCooldown && teammate.gameObject.activeSelf);
        if (teammatesE.Count() == 0)
            return;

        teammatesE = teammatesE
            .Where(teammate => IsWithinHintingDistance(player, teammate, NetworkComboManager.Instance.TriangleDistance, NetworkComboManager.Instance.HighlightTolerance));
        if (teammatesE.Count() < 1)
            return;

        foreach (var teammate in teammatesE)
            CheckLineCombo(player, teammate);
    }

    private void CheckLineCombo(NetworkComboPlayer a, NetworkComboPlayer b)
    {
        if (IsWithinTriggeringDistance(a, b, NetworkComboManager.Instance.LineDistance))
        {
            HandleTriggerableLineCombo(a, b);
            return;
        }

        AddLineComboHint(a, b);
    }

    internal void TriggerCombosForPlayer(NetworkComboPlayer comboPlayer)
    {
        var alreadyTriggeredPlayers = new HashSet<NetworkComboPlayer>();
        
        TriggerCombosForPlayer(comboPlayer, alreadyTriggeredPlayers);
    }

    internal void TriggerCombosForPlayer(NetworkComboPlayer comboPlayer, HashSet<NetworkComboPlayer> alreadyTriggeredPlayers)
    {
        if (NetworkComboManager.Instance.TriggerExtendedTeamCombos)
        {
            if (alreadyTriggeredPlayers.Contains(comboPlayer))
                return;

            alreadyTriggeredPlayers.Add(comboPlayer);
        }

        foreach (var combo in comboPlayer.Combos)
            TriggerCombo(combo, alreadyTriggeredPlayers);

        this.animator.SetTrigger("Shoot");
    }

    private void TriggerCombo(ComboInfo combo, HashSet<NetworkComboPlayer> alreadyTriggeredPlayers)
    {
        if (combo.IsTriggered)
            return;

        combo.IsTriggered = true;

        foreach (var player in combo.Players)
        {
            player.StartCooldown();

            if (player != this && NetworkComboManager.Instance.TriggerExtendedTeamCombos)
                TriggerCombosForPlayer(player, alreadyTriggeredPlayers);
        }

        foreach (var tile in combo.Tiles)
            DigTile(tile);
    }

    private static void AddLineComboHint(NetworkComboPlayer a, NetworkComboPlayer b)
    {
        var comboHint = new ComboHintInfo
        {
            OriginPlayer = a,
            TargetPlayer = b,
            ComboType = ComboType.Line,
            MoveTowards = true
        };

        if (!HasComboHint(a, comboHint))
            a.ComboHintInfos.Add(comboHint);
    }

    private void HandleTriggerableLineCombo(NetworkComboPlayer a, NetworkComboPlayer b)
    {
        var combo = new ComboInfo
        {
            ComboType = ComboType.Line,
            Players = new List<NetworkComboPlayer> { a, b },
            Center = (a.transform.position + b.transform.position) / 2,
        };

        var distanceAToCenter = Vector3.Distance(a.transform.position, combo.Center);
        var distanceBToCenter = Vector3.Distance(b.transform.position, combo.Center);
        var shortestDistanceToCenter = distanceAToCenter < distanceBToCenter ? distanceAToCenter : distanceBToCenter;

        var tilesOccupiedByTeam = this.teammatesAndSelf
            .Select(teammate => NetworkTile.FindTileAtPosition(teammate.transform.position));

        var colliders = Physics.OverlapSphere(combo.Center, shortestDistanceToCenter);
        List<NetworkTile> tilesGameObjects = colliders
            .Where(collider => collider.GetComponentInParent<NetworkTile>() != null)
            .Select(collider => collider.GetComponentInParent<NetworkTile>())
            .Distinct()
            .Where(tile => !tilesOccupiedByTeam.Contains(tile)
                && IsWithinLineBounds(tile.transform.position,
                    a.transform.position,
                    b.transform.position)
                && tile.IsDiggable())
            .OrderBy(tile => Vector3.Distance(combo.Center, tile.transform.position))
            .Take(NetworkComboManager.Instance.MaxTilesInLineCombo)
            .ToList();

        combo.Tiles = tilesGameObjects.Select(tile => tile.TileInfo).ToList();

        if (combo.Tiles.Count == 0)
        {
            AddLineComboHint(a, b);
            return;
        }

        Combos.Add(combo);
    }

    private void CheckTriangleCombosForPlayer(NetworkComboPlayer player)
    {
        var teammates = this.teammates
            .Where(teammate => teammate != this);

        var teammatesE = teammates
            .Where(teammate => !teammate.IsOnCooldown && teammate.gameObject.activeSelf);
        if (teammatesE.Count() == 0)
            return;

        teammatesE = teammatesE
            .Where(teammate => IsWithinHintingDistance(
                player,
                teammate,
                NetworkComboManager.Instance.TriangleDistance,
                NetworkComboManager.Instance.HighlightTolerance));
        if (teammatesE.Count() < 2)
            return;

        foreach (var teammateB in teammatesE)
            foreach (var teammateC in teammatesE)
                if (AreDistinct(player, teammateB, teammateC))
                    CheckTriangleCombo(player, teammateB, teammateC);
    }

    private static bool AreDistinct(NetworkComboPlayer player, NetworkComboPlayer teammateB, NetworkComboPlayer teammateC)
    {
        return player != teammateB 
            && player != teammateC 
            && teammateB != teammateC;
    }

    private void CheckTriangleCombo(NetworkComboPlayer a, NetworkComboPlayer b, NetworkComboPlayer c)
    {
        if (IsWithinTriggeringDistance(a, b, NetworkComboManager.Instance.TriangleDistance)
            && IsWithinTriggeringDistance(a, c, NetworkComboManager.Instance.TriangleDistance)
            && IsWithinTriggeringDistance(b, c, NetworkComboManager.Instance.TriangleDistance))
        {
            HandleTriggerableTriangleCombo(a, b, c);
            return;
        }

        AddTriangleComboHint(a, b, c);
    }

    private static void AddTriangleComboHint(NetworkComboPlayer a, NetworkComboPlayer b, NetworkComboPlayer c)
    {
        var comboHintB = new ComboHintInfo
        {
            OriginPlayer = a,
            TargetPlayer = b,
            ComboType = ComboType.Triangle,
            MoveTowards = true
        };
        var comboHintC = new ComboHintInfo
        {
            OriginPlayer = a,
            TargetPlayer = c,
            ComboType = ComboType.Triangle,
            MoveTowards = true
        };

        if (!HasComboHint(a, comboHintB))
            a.ComboHintInfos.Add(comboHintB);

        if (!HasComboHint(a, comboHintB))
            a.ComboHintInfos.Add(comboHintC);
    }

    private static bool HasComboHint(NetworkComboPlayer player, ComboHintInfo newComboHint)
    {
        return player.ComboHintInfos
                    .Where(comboHint => comboHint.OriginPlayer == newComboHint.OriginPlayer
                        && comboHint.TargetPlayer == newComboHint.TargetPlayer)
                    .FirstOrDefault() != null;
    }

    private void HandleTriggerableTriangleCombo(NetworkComboPlayer a, NetworkComboPlayer b, NetworkComboPlayer c)
    {
        var combo = new ComboInfo
        {
            ComboType = ComboType.Triangle,
            Players = new List<NetworkComboPlayer> { a, b, c, },
            Center = (a.transform.position + b.transform.position + c.transform.position) / 3,
        };           

        var distanceAToCenter = Vector3.Distance(a.transform.position, combo.Center);
        var distanceBToCenter = Vector3.Distance(b.transform.position, combo.Center);
        var distanceCToCenter = Vector3.Distance(c.transform.position, combo.Center);
        var shortestDistanceToCenter = distanceAToCenter < distanceBToCenter ? distanceAToCenter : distanceBToCenter;
        shortestDistanceToCenter = shortestDistanceToCenter < distanceCToCenter ? shortestDistanceToCenter : distanceCToCenter;

        var teammates = this.teammates;

         var tilesOccupiedByTeam = this.teammatesAndSelf
            .Select(teammate => NetworkTile.FindTileAtPosition(teammate.transform.position));

        var colliders = Physics.OverlapSphere(combo.Center, shortestDistanceToCenter);
        var tilesGameObjects = colliders
            .Where(collider => collider.GetComponentInParent<NetworkTile>() != null)
            .Select(collider => collider.GetComponentInParent<NetworkTile>())
            .Distinct()
            .Where(tile => !tilesOccupiedByTeam.Contains(tile)
                && IsWithinTriangle(tile.transform.position,
                    a.transform.position,
                    b.transform.position,
                    c.transform.position)
                && tile.IsDiggable())
            .OrderBy(tile => Vector3.Distance(combo.Center, tile.transform.position))
            .Take(NetworkComboManager.Instance.MaxTilesInTriangleCombo);

        combo.Tiles = tilesGameObjects.Select(tile => tile.TileInfo).ToList();

        var overlappingLineCombos = Combos
            .Where(availableCombo => availableCombo.ComboType == ComboType.Line
                && availableCombo.Players.Intersect(combo.Players).Count() == 2);

        foreach (var overlappingLineCombo in overlappingLineCombos)
            combo.Tiles.AddRange(overlappingLineCombo.Tiles);

        if (combo.Tiles.Count == 0)
        {
            AddTriangleComboHint(a, b, c);
            return;
        }

        Combos
            .RemoveWhere(availableCombo => overlappingLineCombos.Contains(availableCombo));

        Combos.Add(combo);
    }

    private bool IsWithinTriggeringDistance(NetworkComboPlayer a, NetworkComboPlayer b, float maxTriggerDistance, float minDistance = 0f)
    {
        var distance = Vector3.Distance(a.transform.position, b.transform.position);

        return distance >= minDistance && distance <= maxTriggerDistance;
    }

    private bool IsWithinHintingDistance(NetworkComboPlayer a, NetworkComboPlayer b,
        float maxTriggerDistance, float hintingTolerance, float minDistance = 0f)
    {
        var distance = Vector3.Distance(a.transform.position, b.transform.position);

        var lowerBound = minDistance;
        var upperBound = maxTriggerDistance + hintingTolerance;

        return distance >= lowerBound && distance <= upperBound;
    }

    public void DigTile(TileInfo targetTile) => this.networkDigging.DigCombo(targetTile);
    public float MaxHighlightingDistance() => NetworkComboManager.Instance.TriangleDistance + NetworkComboManager.Instance.HighlightTolerance;
    public float GetCooldownMax() => this.cooldownMax;
    public float GetCooldownProgress() => this.cooldownProgress;
    private NetworkTile TileCurrentlyOn() => this.playerMovement.TileCurrentlyOn();

    // Credits to https://www.youtube.com/watch?v=WaYS1gEXEFE
    // Check that video for a great explanation of how we can manage this via math!
    private bool IsWithinTriangle(Vector3 point, Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
    {
        Vector2 P = new Vector2(point.x, point.z);
        Vector2 A = new Vector2(vertexA.x, vertexA.z);
        Vector2 B = new Vector2(vertexB.x, vertexB.z);
        Vector2 C = new Vector2(vertexC.x, vertexC.z);

        var AB = B - A;
        var AC = C - A;

        var w1 = (AC.x * (A.y - P.y) + AC.y * (P.x - A.x)) / (AB.x * AC.y - AB.y * AC.x);
        var w2 = (P.y - A.y - w1 * AB.y) / AC.y;

        return (w1 >= 0)
            && (w2 >= 0)
            && ((w1 + w2) <= 1f);
    }

    private bool IsWithinLineBounds(Vector3 point, Vector3 endA, Vector3 endB)
    {
        /*
         * Let H1 H2 be corners of the line combo box on a's side
         * Let H3 H4 be corners of the line combo box on b's side
        */
        Vector3 A = endA;
        A.y = 0;
        Vector3 B = endB;
        B.y = 0;

        var AB = B - A;
        var directionAH1 = (Quaternion.AngleAxis(-90, Vector3.up) * AB).normalized;

        var H1 = A + NetworkComboManager.Instance.LineThickness / 2 * directionAH1;
        var H2 = A - NetworkComboManager.Instance.LineThickness / 2 * directionAH1;
        var H3 = B + NetworkComboManager.Instance.LineThickness / 2 * directionAH1;
        var H4 = B - NetworkComboManager.Instance.LineThickness / 2 * directionAH1;

        Debug.DrawLine(A, H1);
        Debug.DrawLine(A, H2);
        Debug.DrawLine(B, H3);
        Debug.DrawLine(B, H4);

        /*
         * There's "bound" to be a more optimal way of computing this (pun intended),
         * but for now, since a rectangle is just 2 triangles, let's reuse our old equation:
        */
        return IsWithinTriangle(point, H1, H3, H2)
            || IsWithinTriangle(point, H2, H3, H4);
    }
}
