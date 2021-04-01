using System.Linq;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int CurrentScore { get; private set; }
    
    private Team team;
    private TeamScore teamScore;

    void Start()
    {
        CurrentScore = 0;
        
        this.team = this.GetComponent<Teammate>().Team;

        teamScore = FindObjectsOfType<TeamScore>()
            .SingleOrDefault(teamscore => teamscore.Team == this.team);
    }

    public void Add(int amountToAdd)
    {
        CurrentScore += amountToAdd;

        teamScore.Add(amountToAdd);
    }

    public void Substract(int amountToSubstract)
    {
        CurrentScore -= amountToSubstract;

        if (CurrentScore < 0)
            CurrentScore = 0;

        teamScore.Substract(amountToSubstract);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
            AddCollectableToScore(other.GetComponent<Collectable>());
    }

    private void AddCollectableToScore(Collectable collectable)
    {
        Add(collectable.PointsWorth);

        Destroy(collectable.gameObject);
    }
}
