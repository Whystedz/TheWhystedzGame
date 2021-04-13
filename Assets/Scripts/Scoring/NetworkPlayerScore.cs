using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayerScore : NetworkBehaviour
{
    private List<NetworkPlayerScoreUI> playerUIs = new List<NetworkPlayerScoreUI>();

    [SyncVar(hook = nameof(OnScoreChanged))]
    public int currentScore;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string displayName; // is set by match controller

    private Team team;

    public TeamScoreManager teamScore;
    [SerializeField] private GameObject playerScoreUIPrefab;
    private NetworkPlayerScoreUI localUICopy;

    private GameObject teamUIGameObject = null;

    [SerializeField] PlayerAudio playerAudio;

    public override void OnStartAuthority() => this.team = GetComponent<Teammate>().Team;

    private void Start()
    {
        if (isServer) return;
        
        // get Team UI as a parent to instantiate the playerScoreUI
        GameObject teamUIGameObject = null;
        if (GetComponent<Teammate>().Team == Team.RedTeam)
            teamUIGameObject = GameObject.Find("Left Team UI");

        else if (GetComponent<Teammate>().Team == Team.BlueTeam)
            teamUIGameObject = GameObject.Find("Right Team UI");

        var playerScoreUIGameObject = Instantiate(this.playerScoreUIPrefab, teamUIGameObject.transform);
        var playerScoreUI = playerScoreUIGameObject.GetComponent<NetworkPlayerScoreUI>();

        if(base.hasAuthority)
        {
            localUICopy = playerScoreUI;
            CmdSetDisplayName(LobbyCanvasController.Instance.displayName);
            CmdSetScore(0);
            localUICopy.UpdateName(LobbyCanvasController.Instance.displayName);
            localUICopy.UpdateScore(0);
        }

        this.playerUIs.Add(playerScoreUI);
    }

    // hook function on set DisplayName
    private void OnNameChanged(string oldName, string newName)
    {
        foreach (var scoreUI in playerUIs)
            scoreUI.UpdateName(newName);
    }

    // hook function on set CurrentScore
    private void OnScoreChanged(int oldScore, int newScore)
    {
        foreach (var scoreUI in playerUIs)
            scoreUI.UpdateScore(this.currentScore);
    }

    [Command]
    public void CmdSetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Command]
    public void CmdSetScore(int score)
    {
        this.currentScore = score;
    }

    public void Add(int amountToAdd)
    {
        int total = this.currentScore + amountToAdd;
        CmdSetScore(total);

        localUICopy.UpdateScore(total);

        TeamScoreManager.Instance.AddScore(this.team, amountToAdd);
    }

    public void Subtract(int amountToSubtract)
    {

        int total = this.currentScore - amountToSubtract;

        if (total < 0)  
            total = 0;
            
        CmdSetScore(total);

        localUICopy.UpdateScore(total);

        TeamScoreManager.Instance.SubstractScore(this.team, amountToSubtract);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(base.hasAuthority)
            if (other.CompareTag("Collectable"))
                AddCollectableToScore(other.GetComponent<NetworkCollectable>());
    }

    private void AddCollectableToScore(NetworkCollectable collectable)
    {
        this.playerAudio.PlayCollectAudio();
        Add(collectable.PointsWorth);

        collectable.CmdCollect();
    }
}
