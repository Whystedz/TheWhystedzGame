using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] [Range(0, 64)] private float highlightTolerance;
    [SerializeField] private Color hintColor = Color.white;

    [Header("Line Combo")]
    [SerializeField] [Range(0, 22)] private float lineDistance = 5;
    [SerializeField] [Range(0, 20)] private float lineThickness = 1;
    [SerializeField] [Range(0, 20)] private int maxTilesInLineCombo = 5;
    [SerializeField] private Color lineColor = Color.blue;

    [Header("Triangle Combo")]
    [SerializeField] [Range(0, 42)] private float triangleDistance = 10;
    [Tooltip("Ensure the maxTilesInTriangleCombo is set to at least twice the amount of the line combo's!")]
    [SerializeField] [Range(0, 64)] int maxTilesInTriangleCombo = 10;
    [SerializeField] private Color triangleColor = Color.red;

    private List<ComboPlayer> players;
    public List<Combo> CombosAvailable;

    private void Start()
    {
        this.players = FindObjectsOfType<ComboPlayer>().ToList();

        CombosAvailable = new List<Combo>();
    }

    private void Update()
    {
        CombosAvailable.Clear();
        
        CheckCombos();

        HighlightAvailableCombos();
    }

    private void HighlightAvailableCombos()
    {
        foreach (var combo in CombosAvailable)
            foreach (var playerA in combo.Players)
                foreach (var playerB in combo.Players)
                    if (playerA != playerB)
                    {
                        var comboColor = combo.ComboType == ComboType.Line ? this.lineColor : this.triangleColor;
                        Highlight(playerA, playerB, comboColor);

                        foreach (var tile in combo.Tiles)
                            StartCoroutine(tile.HighlightTileForOneFrame());
                    }
    }

    private void CheckCombos()
    {
        foreach (var player in this.players)
        {
            CheckLineCombosForPlayer(player);
            CheckTriangleCombosForPlayer(player);
        }
    }

    private void CheckLineCombosForPlayer(ComboPlayer player)
    {
        var teammates = player.Teammates(false).ToArray();

        teammates = teammates
            .Where(teammate => !teammate.IsOnCooldown)
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

        HandleLineComboHint(a, b);
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
                    b.transform.position))
            .ToList();

        CombosAvailable.Add(combo);
    }

    private void HandleLineComboHint(ComboPlayer a, ComboPlayer b) =>
        Highlight(a, b, this.hintColor);

    private void CheckTriangleCombosForPlayer(ComboPlayer player)
    {
        var teammates = player.Teammates(false).ToArray();

        teammates = teammates
            .Where(player => !player.IsOnCooldown)
            .ToArray();
        if (teammates.Count() == 0)
            return;

        teammates = teammates
            .Where(teammate => IsWithinHintingDistance(player, teammate, this.triangleDistance, this.highlightTolerance))
            .ToArray();
        if (teammates.Count() < 2)
            return;

        CheckTriangleCombo(player, teammates[0], teammates[1]);

        // With 4 players, a second set of 3 may be possible involving this player:
        if (teammates.Count() >= 3)
            CheckTriangleCombo(player, teammates[1], teammates[2]);
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

        HandleTriangleComboHint(a, b, c);
    }

    private void HandleTriangleComboHint(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        Highlight(a, b, this.hintColor);
        Highlight(a, c, this.hintColor);
        Highlight(b, c, this.hintColor);
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
                    c.transform.position))
            .OrderBy(tile => Vector3.Distance(combo.Center, tile.transform.position))
            .Take(this.maxTilesInTriangleCombo > 0 ? this.maxTilesInTriangleCombo : 100)
            .ToList();

        var overlappingLineCombos = CombosAvailable
            .Where(availableCombo => availableCombo.ComboType == ComboType.Line
                && availableCombo.Players.Intersect(combo.Players).Count() == 2);
        
        foreach (var overlappingLineCombo in overlappingLineCombos)
            combo.Tiles.AddRange(overlappingLineCombo.Tiles);

        CombosAvailable
            .RemoveAll(availableCombo => overlappingLineCombos.Contains(availableCombo));

        CombosAvailable.Add(combo);
    }

    private void Highlight(ComboPlayer a, ComboPlayer b, Color color) =>
        Debug.DrawLine(a.transform.position, b.transform.position, color);

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
    private bool IsWithinTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        var ab = b - a;
        var ac = c - a;

        var w1 = (ac.x * (a.z - point.z) + ac.z * (point.x - a.x)) / (ab.x * ac.z - ab.z * ac.x);
        var w2 = (point.z - a.z - w1 * ab.z) / ac.z;

        return (w1 >= 0)
            && (w2 >= 0)
            && ((w1 + w2) <= 1f);
    }

    private bool IsWithinLineBounds(Vector3 point, Vector3 a, Vector3 b)
    {
        /*
         * Let h1 h2 be corners of the line combo box on a's side
         * Let h3 h4 be corners of the line combo box on b's side
        */
        var ab = b - a;
        var directionAH1 = (Quaternion.AngleAxis(-45, Vector3.up) * ab).normalized;

        var h1 = a + this.lineThickness / 2 * directionAH1;
        var h2 = a - this.lineThickness / 2 * directionAH1;
        var h3 = b + this.lineThickness / 2 * directionAH1;
        var h4 = b - this.lineThickness / 2 * directionAH1;

        /*
         * There's "bound" to be a more optimal way of computing this (pun intended),
         * but for now, since a rectangle is just 2 triangles, let's reuse our old equation:
        */
        return IsWithinTriangle(point, h1, h2, h3)
            || IsWithinTriangle(point, h2, h3, h4);
    }
}
