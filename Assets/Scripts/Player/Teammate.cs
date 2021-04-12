using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Team
{
    RedTeam,
    BlueTeam
}

public class Teammate : MonoBehaviour
{
    [SerializeField] private Team team;

    public Team Team
    {
        get => this.team;
        set => this.team = value;
    }

    public List<Teammate> Teammates { get; private set; }

    private void Awake()
    {
        Teammates = FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == this.team && teammate != this)
            .ToList();
    }
}
