using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FloatingPlayerInfo : NetworkBehaviour
{
    [SerializeField] private TextMesh playerNameText;
    [SerializeField] private GameObject floatingInfo;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string PlayerName;

    void OnNameChanged(string _Old, string _New)
    {
        playerNameText.text = PlayerName;
    }

    public override void OnStartLocalPlayer()
    {
        string name = "Player" + Random.Range(100, 999);
        CmdSetupPlayer(name);
    }

    [Command]
    public void CmdSetupPlayer(string name) => PlayerName = name;


    void Update()
    {
        floatingInfo.transform.LookAt(floatingInfo.transform.position - Camera.main.transform.forward);
    }
}
