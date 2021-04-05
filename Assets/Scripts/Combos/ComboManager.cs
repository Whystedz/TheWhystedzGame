using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] [Range(0, 64)] private float highlightTolerance;
    [SerializeField] private Color hintColor = Color.white;
    [SerializeField] private bool showExtendedTeamCombos = true;
    [SerializeField] private bool triggerExtendedTeamCombos = true;

    [Header("Line Combo")]
    [SerializeField] [Range(0, 22)] private float lineDistance = 5;
    [SerializeField] [Range(0, 20)] private float lineThickness = 1;
    [SerializeField] [Range(0, 20)] private int maxTilesInLineCombo = 20;
    [SerializeField] private Color lineColor = Color.blue;

    [Header("Triangle Combo")]
    [SerializeField] [Range(0, 42)] private float triangleDistance = 10;
    [Tooltip("Ensure the maxTilesInTriangleCombo is set to at least twice the amount of the line combo's!")]
    [SerializeField] [Range(0, 64)] int maxTilesInTriangleCombo = 64;
    [SerializeField] private Color triangleColor = Color.red;

    private List<ComboPlayer> players;
    public List<Combo> CombosAvailable;

    private void Start()
    {
        FindPlayers();

        CombosAvailable = new List<Combo>();
    }

    private void FindPlayers() => this.players = FindObjectsOfType<ComboPlayer>().ToList();

    private void Update()
    {
        if (this.players.Count() == 0)
            FindPlayers();

        CombosAvailable.Clear();
        
        CheckCombos();

        AssignCombosToPlayers();
    }

    private void AssignCombosToPlayers()
    {
        foreach (var combo in CombosAvailable)
            combo.InitiatingPlayer.Combos.Add(combo);
    }

    public void HighlightPlayersCombos(ComboPlayer comboPlayer) =>
        HighlightCombos(comboPlayer.Combos);

    internal void HighlightComboHints(ComboPlayer comboPlayer)
    {
        foreach (var comboHint in comboPlayer.ComboHints)
            HighlightComboHint(comboHint);
    }

    internal void TriggerCombos(ComboPlayer comboPlayer)
    {
        foreach (var combo in comboPlayer.Combos)
            TriggerCombo(combo);
    }

    private void TriggerCombo(Combo combo)
    {
        if (combo.IsTriggered)
            return;
        
        combo.IsTriggered = true;
        combo.InitiatingPlayer.StartCooldown();

        foreach (var player in combo.Players)
        {
            player.StartCooldown();

            if (this.triggerExtendedTeamCombos)
                foreach (var extendedCombo in player.Combos)
                    TriggerCombo(extendedCombo);
        }

        foreach (var tile in combo.Tiles)
            tile.DigTile();
    }

    private void HighlightComboHint(ComboHint comboHint)
    {
        Highlight(comboHint.OriginPlayer,
            comboHint.TargetPlayer,
            this.hintColor);
    }

    private void HighlightCombos(List<Combo> combos)
    {
        foreach (var combo in combos)
            HighlightCombo(combo);
    }

    private void HighlightCombo(Combo combo, bool isExtendedCombo = false, List<Combo> visitedExtendedCombos = null)
    {
        if (isExtendedCombo && visitedExtendedCombos is null)
            return; // recursion exit case for extended combos

        foreach (var tile in combo.Tiles)
            StartCoroutine(tile.HighlightTileComboDigPreview());

        foreach (var playerA in combo.Players)
            foreach (var playerB in combo.Players)
            {
                if (playerA == playerB)
                    continue;

                var comboColor = combo.ComboType == ComboType.Line ? this.lineColor : this.triangleColor;
                
                Highlight(playerA, playerB, comboColor);
            }

        if (this.showExtendedTeamCombos)
            HighlightExtendedCombo(combo, visitedExtendedCombos);
    }

    private void HighlightExtendedCombo(Combo combo, List<Combo> visitedExtendedCombos)
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

                var updatedVisitedExtendedCombos = new List<Combo>
                {
                    combo
                };

                if (visitedExtendedCombos != null)
                    updatedVisitedExtendedCombos.AddRange(visitedExtendedCombos);

                HighlightCombo(extendedCombo, true, updatedVisitedExtendedCombos);
            }
        }
    }

    private void CheckCombos()
    {
        foreach (var player in this.players)
        {
            if (!player.gameObject.activeSelf)
                return;

            CheckLineCombosForPlayer(player);
            CheckTriangleCombosForPlayer(player);
        }
    }

    private void CheckLineCombosForPlayer(ComboPlayer player)
    {
        var teammates = player.Teammates(false).ToArray();

        teammates = teammates
            .Where(teammate => !teammate.IsOnCooldown && teammate.gameObject.activeSelf)
            .ToArray();
        if (teammates.Count() == 0)
            return;

        teammates = teammates
            .Where(teammate => IsWithinHintingDistance(player, teammate, this.triangleDistance, this.highlightTolerance))
            .ToArray();
        if (teammates.Count() < 1)
            return;

        foreach (var teammate in teammates)
            CheckLineCombo(player, teammate);
    }

    private void CheckLineCombo(ComboPlayer a, ComboPlayer b)
    {
        if (IsWithinTriggeringDistance(a, b, this.lineDistance))
        {
            HandleTriggerableLineCombo(a, b);
            return;
        }

        AddLineComboHint(a, b);
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
        a.ComboHints.Add(comboHint);
    }

    private void HandleTriggerableLineCombo(ComboPlayer a, ComboPlayer b)
    {
        var combo = new Combo
        {
            ComboType = ComboType.Line,
            InitiatingPlayer = a,
            Players = new List<ComboPlayer> { a, b },
            Center = (a.transform.position + b.transform.position) / 2,
        };

        var distanceAToCenter = Vector3.Distance(a.transform.position, combo.Center);
        var distanceBToCenter = Vector3.Distance(b.transform.position, combo.Center);
        var shortestDistanceToCenter = distanceAToCenter < distanceBToCenter ? distanceAToCenter : distanceBToCenter;

        var teammates = a.Teammates(true);

        var tilesOccupiedByTeam = teammates
            .Select(teammate => teammate.TileCurrentlyAbove());

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
            .Take(this.maxTilesInLineCombo)
            .ToList();

        if (combo.Tiles.Count() == 0) return;

        CombosAvailable.Add(combo);
    }

    private void CheckTriangleCombosForPlayer(ComboPlayer player)
    {
        var teammates = player.Teammates(false).ToArray();

        teammates = teammates
            .Where(teammate => !teammate.IsOnCooldown && teammate.gameObject.activeSelf)
            .ToArray();
        if (teammates.Count() == 0)
            return;

        teammates = teammates
            .Where(teammate => IsWithinHintingDistance(player, teammate, this.triangleDistance, this.highlightTolerance))
            .ToArray();
        if (teammates.Count() < 2)
            return;

        CheckTriangleCombo(player, teammates[0], teammates[1]);

        // With 4 players in range, 
        // a second or even third set of 3 may be possible involving this player:
        if (teammates.Count() >= 3)
        {
            CheckTriangleCombo(player, teammates[0], teammates[2]);
            CheckTriangleCombo(player, teammates[1], teammates[2]);
        }
    }

    private void CheckTriangleCombo(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        if (IsWithinTriggeringDistance(a, b, this.triangleDistance)
            && IsWithinTriggeringDistance(a, c, this.triangleDistance)
            && IsWithinTriggeringDistance(b, c, this.triangleDistance))
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
        a.ComboHints.Add(comboHintB);
        a.ComboHints.Add(comboHintC);
    }

    private void HandleTriggerableTriangleCombo(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        var combo = new Combo
        {
            ComboType = ComboType.Triangle,
            InitiatingPlayer = a,
            Players = new List<ComboPlayer> { a, b, c, },
            Center = (a.transform.position + b.transform.position + c.transform.position) / 3,
        };

        var distanceAToCenter = Vector3.Distance(a.transform.position, combo.Center);
        var distanceBToCenter = Vector3.Distance(b.transform.position, combo.Center);
        var distanceCToCenter = Vector3.Distance(c.transform.position, combo.Center);
        var shortestDistanceToCenter = distanceAToCenter < distanceBToCenter ? distanceAToCenter : distanceBToCenter;
        shortestDistanceToCenter = shortestDistanceToCenter < distanceCToCenter ? shortestDistanceToCenter : distanceCToCenter;

        var teammates = a.Teammates(true);

        var tilesOccupiedByTeam = teammates
            .Where(player => player.TileCurrentlyAbove() != null)
            .Select(player => player.TileCurrentlyAbove());

        var colliders = Physics.OverlapSphere(combo.Center, shortestDistanceToCenter);
        combo.Tiles = colliders
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
            .Take(this.maxTilesInTriangleCombo)
            .ToList();

        var overlappingLineCombos = CombosAvailable
            .Where(availableCombo => availableCombo.ComboType == ComboType.Line
                && availableCombo.Players.Intersect(combo.Players).Count() == 2);
        
        foreach (var overlappingLineCombo in overlappingLineCombos)
            combo.Tiles.AddRange(overlappingLineCombo.Tiles);

        if (combo.Tiles.Count == 0) return;

        CombosAvailable
            .RemoveAll(availableCombo => overlappingLineCombos.Contains(availableCombo));

        CombosAvailable.Add(combo);
    }

    private void Highlight(ComboPlayer a, ComboPlayer b, Color color) =>
        Debug.DrawLine(a.transform.position, b.transform.position, color);

    public float MaxHighlightingDistance() => this.triangleDistance + this.highlightTolerance;

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

        var H1 = A + this.lineThickness / 2 * directionAH1;
        var H2 = A - this.lineThickness / 2 * directionAH1;
        var H3 = B + this.lineThickness / 2 * directionAH1;
        var H4 = B - this.lineThickness / 2 * directionAH1;

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
 