using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILobby : MonoBehaviour
{
    public static UILobby instance;

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
    [SerializeField] private int minPlayers = 2;

    public const string PlayerPrefsNameKey = "PlayerName";

    GameObject playerLobbyUI;

    bool searching = false;

    void Start()
    {
        instance = this;
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
        LobbyPlayer.LocalPlayer.DisplayName = nameInputField.text;

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

    public void HostSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            if (playerLobbyUI != null)
                Destroy(playerLobbyUI);
            playerLobbyUI = SpawnUIPlayerPrefab(LobbyPlayer.LocalPlayer);
            matchIDText.text = matchID;
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

    public void JoinSuccess(bool success, string matchID)
    {
        if (success)
        {
            lobbyCanvas.enabled = true;
            if (playerLobbyUI != null)
                Destroy(playerLobbyUI);
            playerLobbyUI = SpawnUIPlayerPrefab(LobbyPlayer.LocalPlayer);
            matchIDText.text = matchID;
        }
        else
        {
            joinMatchInput.interactable = true;
            nameInputField.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public GameObject SpawnUIPlayerPrefab(LobbyPlayer player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        newUIPlayer.transform.SetSiblingIndex(player.PlayerIndex - 1);
        return newUIPlayer;
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

    public void SearchSuccess(bool success, string matchID)
    {
        if (success)
        {
            searchCanvas.enabled = false;
            searching = false;
            JoinSuccess(success, matchID);
        }
    }

    public void SearchCancel()
    {
        searching = false;
    }

    public void DisconnectLobby()
    {
        if (playerLobbyUI != null)
            Destroy(playerLobbyUI);
        LobbyPlayer.LocalPlayer.DisconnectGame();

        lobbyCanvas.enabled = false;
        joinMatchInput.interactable = true;
        nameInputField.interactable = true;
        lobbySelectables.ForEach(x => x.interactable = true);
        startGameButton.SetActive(false);
    }
}
