using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILobby : MonoBehaviour
{
    public static UILobby Instance { get; set; }

    [Header("Host Join")]
    [SerializeField] private TMP_InputField joinMatchInput;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private List<Selectable> lobbySelectables = new List<Selectable>();
    [SerializeField] private Canvas lobbyCanvas;
    [SerializeField] private Canvas searchCanvas;

    [Header("Lobby")]
    [SerializeField] private Transform UIPlayerParent;
    [SerializeField] private GameObject UIPlayerPrefab;
    [SerializeField] private TMP_Text matchIDText;
    [SerializeField] private GameObject startGameButton;
    public TMP_Text[] playerNameTexts;
    public TMP_Text[] playerReadyTexts;

    public const string PlayerPrefsNameKey = "PlayerName";

    bool searching = false;

    void Start()
    {
        Instance = this;
        joinMatchInput.onValidateInput += delegate(string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return; }

        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name)
    {
        lobbySelectables.ForEach(x => x.interactable = !string.IsNullOrEmpty(name));
    }

    public void SavePlayerName()
    {
        LobbyPlayer.LocalPlayer.CmdSetDisplayName(nameInputField.text);

        PlayerPrefs.SetString(PlayerPrefsNameKey, LobbyPlayer.LocalPlayer.DisplayName);
    }

    // Creates a match ID
    public void HostPrivate()
    {
        joinMatchInput.interactable = false;
        nameInputField.interactable = false;

        lobbySelectables.ForEach(x => x.interactable = false);

        LobbyPlayer.LocalPlayer.HostGame(false);
    }

    public void HostPublic()
    {
        joinMatchInput.interactable = false;
        nameInputField.interactable = false;

        lobbySelectables.ForEach(x => x.interactable = false);

        LobbyPlayer.LocalPlayer.HostGame(true);
    }

    public void HostSuccess(bool success, Match match)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            matchIDText.text = match.MatchID;
            //UpdateLobbyUI(match);
            startGameButton.SetActive(true);
        }
        else
        {
            joinMatchInput.interactable = true;
            nameInputField.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public void Join()
    {
        joinMatchInput.interactable = false;
        nameInputField.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        LobbyPlayer.LocalPlayer.JoinGame(joinMatchInput.text);
    }

    public void JoinSuccess(bool success, Match match)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            matchIDText.text = match.MatchID;
            //UpdateLobbyUI(match);
        }
        else
        {
            joinMatchInput.interactable = true;
            nameInputField.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public void StartGame()
    {
        LobbyPlayer.LocalPlayer.StartGame();
    }

    public void SearchGame()
    {
        Debug.Log($"Searching for game...");
        StartCoroutine(SearchingForGame());
    }

    IEnumerator SearchingForGame()
    {
        searchCanvas.enabled = true;

        searching = true;
        float searchInterval = 1f;
        float currentTime = 1f;
        
        while(searching)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = searchInterval;
                LobbyPlayer.LocalPlayer.SearchGame();
            }
            yield return null;
        }

        searchCanvas.enabled = false;
    }

    public void SearchSuccess(bool success, Match match)
    {
        if (success)
        {
            searchCanvas.enabled = false;
            searching = false;
            JoinSuccess(success, match);
        }
    }

    public void SearchCancel()
    {
        searching = false;
    }

    public void DisconnectLobby()
    {
        LobbyPlayer.LocalPlayer.DisconnectGame();

        lobbyCanvas.enabled = false;
        joinMatchInput.interactable = true;
        nameInputField.interactable = true;
        lobbySelectables.ForEach(x => x.interactable = true);
        startGameButton.SetActive(false);
    }

    public void SetReadyState()
    {
        LobbyPlayer.LocalPlayer.ReadyUp();
    }

    public void UpdateLobbyUI(Match currentRoom)
    {
        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = string.Empty;
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < currentRoom.Players.Count; i++)
        {
            Debug.Log($"Player {i} name {currentRoom.Players[i].DisplayName}");
            playerNameTexts[i].text = currentRoom.Players[i].DisplayName;
            playerReadyTexts[i].text = currentRoom.Players[i].IsReady ?
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
        }
    }
}
