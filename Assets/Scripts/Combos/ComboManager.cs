using Mirror.Cloud.Examples.Pong;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private float lineDistance;
    [SerializeField] private float lineTolerance;
    
    [SerializeField] private float triangleDistance;
    [SerializeField] private float triangleTolerance;
    
    [SerializeField] private float squareDistance;
    [SerializeField] private float squareTolerance;
    
    [SerializeField] private float highlightTolerance;

    [SerializeField] private Color hintColor;
    [SerializeField] private Color lineColor;
    [SerializeField] private Color triangleColor;
    [SerializeField] private Color squareColor;

    private List<ComboPlayer> players;

    private void Start()
    {
        this.players = FindObjectsOfType<ComboPlayer>().ToList();
    }

    private void Update()
    {
        foreach (var player in this.players)
            CheckTriangleCombosForPlayer(player);
    }

    private void CheckTriangleCombosForPlayer(ComboPlayer player)
    {
        var teammates = player.Teammates.ToArray();

        // Narrow down by cooldown
        teammates = teammates
            .Where(player => !player.IsOnCooldown)
            .ToArray();
        if (teammates.Count() == 0)
            return;

        // Narrow down by max distance -- if we cannot highlight, consider it out of max distance
        teammates = teammates
            .Where(teammate => IsWithinRange(player, teammate, this.triangleDistance, this.highlightTolerance))
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
        // If we made it here, then it is at least in bounds for highlighting the combo

        // Let's see if we can trigger it
        if (IsWithinRange(a, b, this.triangleDistance, this.triangleTolerance)
            && IsWithinRange(a, c, this.triangleDistance, this.triangleTolerance)
            && IsWithinRange(b, c, this.triangleDistance, this.triangleTolerance))
        {
            HighlightTriggerableTriangleCombo(a, b, c);
            return;
        }

        HighlightTriangleComboWithHint(a, b, c);
    }

    private void HighlightTriangleComboWithHint(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        //Debug.Log("Hint");
        Highlight(a, b, this.hintColor);
        Highlight(a, c, this.hintColor);
        Highlight(b, c, this.hintColor);
    }

    private void HighlightTriggerableTriangleCombo(ComboPlayer a, ComboPlayer b, ComboPlayer c)
    {
        //Debug.Log($"Triangle Combo between {this.name}, {b.name}, and {c.name}!");
        Highlight(a, b, this.triangleColor);
        Highlight(a, c, this.triangleColor);
        Highlight(b, c, this.triangleColor);
    }

    private void Highlight(ComboPlayer a, ComboPlayer b, Color color)
    {
        Debug.DrawLine(a.transform.position, b.transform.position, color);
    }

    private bool IsWithinRange(ComboPlayer a, ComboPlayer b, float requiredDistance, float tolerance)
    {
        var distance = Vector3.Distance(a.transform.position, b.transform.position);

        var lowerBound = requiredDistance - tolerance;
        var upperBound = requiredDistance + tolerance;

        return distance >= lowerBound && distance <= upperBound;
    }
}
