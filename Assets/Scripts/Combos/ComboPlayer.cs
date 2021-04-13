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
    
    [SerializeField] private float afterComboPause = 0.5f;

    private InputManager inputManager;


    private float cooldownProgress;

    public bool IsOnCooldown { get; private set; }

    private ComboPlayer[] teammates;
    private ComboPlayer[] teammatesAndSelf;

    public HashSet<Combo> Combos;
    public HashSet<ComboHint> ComboHints;

    private ComboManager comboManager;
    private PlayerMovement playerMovement;
    private AnimationManager animationManager;

    [SerializeField] private Transform comboParticleGenerator;

    public bool IsClientPlayer;

    private static int nInstances;
    private static int nInstancesThatHaveUpdated;
    private static List<ComboPlayer> queuedTriggerCombosPlayer;
    private static List<Tile> currentlyHighlightedTiles;
    private static Team clientPlayerTeam;

    public bool isOnClientPlayerTeam;

    private List<ComboParticleIndicator> comboParticleIndicators;

    private void Awake()
    {
        GetTeammates();

        this.comboManager = FindObjectOfType<ComboManager>();
        this.playerMovement = FindObjectOfType<PlayerMovement>();
        this.Combos = new HashSet<Combo>();
        this.ComboHints = new HashSet<ComboHint>();

        this.animationManager = GetComponentInChildren<AnimationManager>();

        this.cooldownProgress = this.cooldownMax;

        nInstances += 1;

        queuedTriggerCombosPlayer = new List<ComboPlayer>();
        currentlyHighlightedTiles = new List<Tile>();


        clientPlayerTeam = this.GetComponent<Teammate>().Team;
        isOnClientPlayerTeam = true;

        InitializeComboParticleIndicators();
    }

    void Start()
    {
        inputManager = InputManager.GetInstance();

        isOnClientPlayerTeam = this.GetComponent<Teammate>().Team == clientPlayerTeam;
    }


    public void InitializeComboParticleIndicators()
    {
        comboParticleIndicators = new List<ComboParticleIndicator>();
        foreach (Transform comboParticleGeneratorTransform in this.comboParticleGenerator)
        {
            var comboParticleIndicator = comboParticleGeneratorTransform.GetComponent<ComboParticleIndicator>();
            comboParticleIndicators.Add(comboParticleIndicator);
            comboParticleIndicator.UpdateParticles(Combos, ComboHints);
        }
    }

    void Update()
    {
        if (!this.isOnClientPlayerTeam)
            return;

        if (nInstancesThatHaveUpdated == 0)
            BeforeAllComboPlayersHaveUpdated();

        CooldownUpdate();

        if (this.teammates.Count() == 0)
            GetTeammates();

        this.Combos.Clear();
        this.ComboHints.Clear();

        CheckCombosForPlayer(this);

        if (this.displayCombos) 
            HighlightCombosForPlayer(this);

        if (this.displayComboHints)
            foreach (var comboParticleIndicator in comboParticleIndicators)
                comboParticleIndicator.UpdateParticles(this.Combos, this.ComboHints);

        if (this.canTriggerCombos
            && inputManager.GetInitiateCombo()
            && Combos.Count() > 0
            && !this.IsOnCooldown)
                QueuePlayerForTriggerCombos(this);

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

    private void QueuePlayerForTriggerCombos(ComboPlayer player)
    {
        queuedTriggerCombosPlayer.Add(player);
    }

    private void TriggerAllQueuedCombos()
    {
        foreach (var player in queuedTriggerCombosPlayer)
        {
            var playerPlayerMovement = player.GetComponent<PlayerMovement>();

            playerPlayerMovement.DisableMovement();

            TriggerCombosForPlayer(player);

            playerPlayerMovement.DisableMovementFor(this.afterComboPause, true);
        }
    }

    private void GetTeammates()
    {
        var team = this.GetComponent<Teammate>().Team;

        this.teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team && teammate.gameObject != this.gameObject)
            .Select(teammate => teammate.GetComponent<ComboPlayer>())
            .ToArray();

        this.teammatesAndSelf = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == team)
            .Select(teammate => teammate.GetComponent<ComboPlayer>())
            .ToArray();
    }

    public List<ComboPlayer> Teammates(bool includeSelf)
    {
        var teammatesResult = new List<ComboPlayer>(this.teammates); // ??? TODO

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

    private void CheckCombosForPlayer(ComboPlayer player)
    {
        CheckLineCombosForPlayer(player);
        CheckTriangleCombosForPlayer(player);
    }

    private void HighlightCombosForPlayer(ComboPlayer player)
    {
        var alreadyHighlightedPlayers = new HashSet<ComboPlayer>();

        HighlightCombosForPlayer(player, alreadyHighlightedPlayers);
    }

    private void HighlightCombosForPlayer(ComboPlayer player, HashSet<ComboPlayer> alreadyHighlightedPlayers)
    {
        if (this.comboManager.ShowExtendedTeamCombos)
        {
            if (alreadyHighlightedPlayers.Contains(player))
                return;

            alreadyHighlightedPlayers.Add(player);
        }

        foreach (var combo in player.Combos)
            HighlightCombo(combo, alreadyHighlightedPlayers);
    }

    private void HighlightCombo(Combo combo, HashSet<ComboPlayer> alreadyHighlightedPlayers)
    {
        if (combo.IsTriggered)
            return;

        foreach (var player in combo.Players)
            if (player != this && this.comboManager.ShowExtendedTeamCombos)
                HighlightCombosForPlayer(player, alreadyHighlightedPlayers);

        foreach (var tile in combo.Tiles)
            if (tile.tileState == TileState.Normal
                && !currentlyHighlightedTiles.Contains(tile))
            {
                tile.HighlightTileComboDigPreview();
                currentlyHighlightedTiles.Add(tile);
            }
    }

    private void CheckLineCombosForPlayer(ComboPlayer player)
    {
        var teammatesE = this.teammates
            .Where(teammate => !teammate.IsOnCooldown && teammate.gameObject.activeSelf);
        if (teammatesE.Count() == 0)
            return;

        teammatesE = teammatesE
            .Where(teammate => IsWithinHintingDistance(player, teammate, this.comboManager.TriangleDistance, this.comboManager.HighlightTolerance));
        if (teammatesE.Count() < 1)
            return;

        foreach (var teammate in teammatesE)
            CheckLineCombo(player, teammate);
    }

    private void CheckLineCombo(ComboPlayer a, ComboPlayer b)
    {
        if (IsWithinTriggeringDistance(a, b, this.comboManager.LineDistance))
        {
            HandleTriggerableLineCombo(a, b);
            return;
        }

        AddLineComboHint(a, b);
    }

    internal void TriggerCombosForPlayer(ComboPlayer comboPlayer)
    {
        var alreadyTriggeredPlayers = new HashSet<ComboPlayer>();
        
        TriggerCombosForPlayer(comboPlayer, alreadyTriggeredPlayers);
    }

    internal void TriggerCombosForPlayer(ComboPlayer comboPlayer, HashSet<ComboPlayer> alreadyTriggeredPlayers)
    {
        if (this.comboManager.TriggerExtendedTeamCombos)
        {
            if (alreadyTriggeredPlayers.Contains(comboPlayer))
                return;

            alreadyTriggeredPlayers.Add(comboPlayer);
        }

        foreach (var combo in comboPlayer.Combos)
            TriggerCombo(combo, alreadyTriggeredPlayers);

        comboPlayer.GetComponentInChildren<AnimationManager>().PlayShootingAnimation();
    }

    private void TriggerCombo(Combo combo, HashSet<ComboPlayer> alreadyTriggeredPlayers)
    {
        if (combo.IsTriggered)
            return;

        combo.IsTriggered = true;

        foreach (var player in combo.Players)
        {
            player.StartCooldown();

            if (player != this && this.comboManager.TriggerExtendedTeamCombos)
                TriggerCombosForPlayer(player, alreadyTriggeredPlayers);
        }

        foreach (var tile in combo.Tiles)
            tile.DigTile();
    }

    private static void AddLineComboHint(ComboPlayer a, ComboPlayer b)
    {
        var comboHint = new ComboHint
        {
            OriginPlayer = a,
            TargetPlayer = b,
            ComboType = ComboType.Line,
            MoveTowards = true
        };

        if (!HasComboHint(a, comboHint))
            a.ComboHints.Add(comboHint);
    }

    private void HandleTriggerableLineCombo(ComboPlayer a, ComboPlayer b)
    {
        var combo = new Combo
        {
            ComboType = ComboType.Line,
            Players = new List<ComboPlayer> { a, b },
            Center = (a.transform.position + b.transform.position) / 2,
        };

        var distanceAToCenter = Vector3.Distance(a.transform.position, combo.Center);
        var distanceBToCenter = Vector3.Distance(b.transform.position, combo.Center);
        var shortestDistanceToCenter = distanceAToCenter < distanceBToCenter ? distanceAToCenter : distanceBToCenter;

        var tilesOccupiedByTeam = this.teammatesAndSelf
            .Select(teammate => Tile.FindTileAtPosition(teammate.transform.position));

        var colliders = Physics.OverlapSphere(combo.Center, shortestDistanceToCenter);
        combo.Tiles = colliders
            .Where(collider => collider.GetComponentInParent<Tile>() != null)
            .Select(collider => collider.GetComponentInParent<Tile>())
            .Distinct()
            .Where(tile => !tilesOccupiedByTeam.Contains(tile)
                && IsWithinLineBounds(tile.transform.position,
                    a.transform.position,
                    b.transform.position)
                && tile.IsDiggable())
            .OrderBy(tile => Vector3.Distance(combo.Center, tile.transform.position))
            .Take(this.comboManager.MaxTilesInLineCombo)
            .ToList();

        if (combo.Tiles.Count == 0)
        {
            AddLineComboHint(a, b);
            return;
        }

        Combos.Add(combo);
    }

    public float GetCooldownMax() => this.cooldownMax;
    public float GetCooldownProgress() => this.cooldownProgress;

    private void CheckTriangleCombosForPlayer(ComboPlayer player)
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
                this.comboManager.TriangleDistance,
                this.comboManager.HighlightTolerance));
        if (teammatesE.Count() < 2)
            return;

        foreach (var teammateB in teammatesE)
            foreach (var teammateC in teammatesE)
                if (AreDistinct(player, teammateB, teammateC))
                    CheckTriangleCombo(player, teammateB, teammateC);
    }

    private static bool AreDistinct(ComboPlayer player, ComboPlayer teammateB, ComboPlayer teammateC)
    {
        return player != teammateB 
            && player != teammateC 
            && teammateB != teammateC;
    }

    private void CheckTriangleCombo(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        if (IsWithinTriggeringDistance(a, b, this.comboManager.TriangleDistance)
            && IsWithinTriggeringDistance(a, c, this.comboManager.TriangleDistance)
            && IsWithinTriggeringDistance(b, c, this.comboManager.TriangleDistance))
        {
            HandleTriggerableTriangleCombo(a, b, c);
            return;
        }

        AddTriangleComboHint(a, b, c);
    }

    private static void AddTriangleComboHint(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        var comboHintB = new ComboHint
        {
            OriginPlayer = a,
            TargetPlayer = b,
            ComboType = ComboType.Triangle,
            MoveTowards = true
        };
        var comboHintC = new ComboHint
        {
            OriginPlayer = a,
            TargetPlayer = c,
            ComboType = ComboType.Triangle,
            MoveTowards = true
        };

        if (!HasComboHint(a, comboHintB))
            a.ComboHints.Add(comboHintB);

        if (!HasComboHint(a, comboHintB))
            a.ComboHints.Add(comboHintC);
    }

    private static bool HasComboHint(ComboPlayer player, ComboHint newComboHint)
    {
        return player.ComboHints
                    .Where(comboHint => comboHint.OriginPlayer == newComboHint.OriginPlayer
                        && comboHint.TargetPlayer == newComboHint.TargetPlayer)
                    .FirstOrDefault() != null;
    }

    private void HandleTriggerableTriangleCombo(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        var combo = new Combo
        {
            ComboType = ComboType.Triangle,
            Players = new List<ComboPlayer> { a, b, c, },
            Center = (a.transform.position + b.transform.position + c.transform.position) / 3,
        };           

        var distanceAToCenter = Vector3.Distance(a.transform.position, combo.Center);
        var distanceBToCenter = Vector3.Distance(b.transform.position, combo.Center);
        var distanceCToCenter = Vector3.Distance(c.transform.position, combo.Center);
        var shortestDistanceToCenter = distanceAToCenter < distanceBToCenter ? distanceAToCenter : distanceBToCenter;
        shortestDistanceToCenter = shortestDistanceToCenter < distanceCToCenter ? shortestDistanceToCenter : distanceCToCenter;

        var teammates = this.teammates;

        var tilesOccupiedByTeam = this.teammatesAndSelf
            .Select(teammate => Tile.FindTileAtPosition(teammate.transform.position));

        var colliders = Physics.OverlapSphere(combo.Center, shortestDistanceToCenter);
        var enumerableTiles = colliders
            .Where(collider => collider.GetComponentInParent<Tile>() != null)
            .Select(collider => collider.GetComponentInParent<Tile>())
            .Distinct()
            .Where(tile => !tilesOccupiedByTeam.Contains(tile)
                && IsWithinTriangle(tile.transform.position,
                    a.transform.position,
                    b.transform.position,
                    c.transform.position)
                && tile.IsDiggable())
            .OrderBy(tile => Vector3.Distance(combo.Center, tile.transform.position))
            .Take(this.comboManager.MaxTilesInTriangleCombo);
        combo.Tiles = enumerableTiles.ToList();

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

    public float MaxHighlightingDistance() => this.comboManager.TriangleDistance + this.comboManager.HighlightTolerance;

    private bool IsWithinTriggeringDistance(ComboPlayer a, ComboPlayer b, float maxTriggerDistance, float minDistance = 0f)
    {
        var distance = Vector3.Distance(a.transform.position, b.transform.position);

        return distance >= minDistance && distance <= maxTriggerDistance;
    }

    private bool IsWithinHintingDistance(ComboPlayer a, ComboPlayer b,
        float maxTriggerDistance, float hintingTolerance, float minDistance = 0f)
    {
        var distance = Vector3.Distance(a.transform.position, b.transform.position);

        var lowerBound = minDistance;
        var upperBound = maxTriggerDistance + hintingTolerance;

        return distance >= lowerBound && distance <= upperBound;
    }

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

        var H1 = A + this.comboManager.LineThickness / 2 * directionAH1;
        var H2 = A - this.comboManager.LineThickness / 2 * directionAH1;
        var H3 = B + this.comboManager.LineThickness / 2 * directionAH1;
        var H4 = B - this.comboManager.LineThickness / 2 * directionAH1;

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

    private Tile TileCurrentlyOn() => this.playerMovement.TileCurrentlyOn();
}
