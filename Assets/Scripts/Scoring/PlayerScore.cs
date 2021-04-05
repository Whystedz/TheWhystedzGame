using System.Linq;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int CurrentScore { get; private set; }
    
    private Team team;
    private TeamScore teamScore;
    [SerializeField] private PlayerScoreUI playerScoreUI;

    private PlayerAudio playerAudio;

    void Start()
    {
        CurrentScore = 0;
        
        this.team = this.GetComponent<Teammate>().Team;

        teamScore = FindObjectsOfType<TeamScore>()
            .SingleOrDefault(teamscore => teamscore.Team == this.team);

        this.playerAudio = GetComponentInParent<PlayerAudio>();
    }

    public void Add(int amountToAdd)
    {
        CurrentScore += amountToAdd;

        this.teamScore.Add(amountToAdd);
    }

    public void Substract(int amountToSubstract)
    {
        CurrentScore -= amountToSubstract;

        if (CurrentScore < 0)
            CurrentScore = 0;

        this.teamScore.Substract(amountToSubstract);
        this.playerScoreUI.UpdateScore(CurrentScore);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
            AddCollectableToScore(other.GetComponent<Collectable>());
    }

    private void AddCollectableToScore(Collectable collectable)
    {
        playerAudio.PlayCollectAudio();
        Add(collectable.PointsWorth);

        this.playerScoreUI.UpdateScore(CurrentScore);

        collectable.Collect();
    }
}
