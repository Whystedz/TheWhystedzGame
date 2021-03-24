using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the score of the player (or whatever this is attached to).
/// 
/// Last updated 19-03-2021 by Shifat
/// </summary>
public class Score : MonoBehaviour
{
    public int CurrentScore { get; private set; }
    [SerializeField] private int scoreIncrement = 1;

    [SerializeField] private string collectibleTag;
    [SerializeField] private GameEvent scoreChangedEvent;

    void Start() => CurrentScore = 0;

    public void AddToScore(int amountToAdd)
    {
        CurrentScore += amountToAdd;
        this.scoreChangedEvent.Raise();
    }

    public void SubstractFromScore(int amountToSubstract)
    {
        CurrentScore = CurrentScore - amountToSubstract < 0 ? 0 : CurrentScore - amountToSubstract;
        this.scoreChangedEvent.Raise();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(this.collectibleTag))
        {
            AddToScore(scoreIncrement);

            // TODO: Destroying gameobjects can bloat garbage collection. Maybe make a pool?
            Destroy(other.gameObject);
        }
    }
}
