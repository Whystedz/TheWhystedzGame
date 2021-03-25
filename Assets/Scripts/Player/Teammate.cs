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
    public Team Team { get => this.team; }

    private List<Teammate> teammates;
    public List<Teammate> Teammates { get => this.teammates; }

    private void Awake()
    {
        this.teammates = GameObject.FindObjectsOfType<Teammate>()
            .Where(teammate => teammate.Team == this.team && teammate != this)
            .ToList();
    }
}
