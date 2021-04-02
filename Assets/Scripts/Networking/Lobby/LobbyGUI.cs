using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyGUI : MonoBehaviour
{
    [Header("GUI References")]
    [SerializeField] private TMP_InputField joinMatchInput;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private List<Selectable> lobbySelectables = new List<Selectable>();
    [SerializeField] private Canvas searchCanvas;
    public const string PlayerPrefsNameKey = "PlayerName";

    private bool searching = false;

    void Start()
    {
        joinMatchInput.onValidateInput += delegate(string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
            return;

        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        nameInputField.text = defaultName;
    }

    public void SetPlayerName(string name)
    {
        lobbySelectables.ForEach(x => x.interactable = !string.IsNullOrEmpty(name));
    }

    public void SavePlayerName()
    {
        CanvasController.Instance.SetDisplayName(nameInputField.text);
        if (!string.IsNullOrEmpty(nameInputField.text))
            PlayerPrefs.SetString(PlayerPrefsNameKey, nameInputField.text);
    }

    public void SearchGame()
    {
        StartCoroutine(SearchingForGame());
    }

    public void SearchCancel()
    {
        searching = false;
    }

    IEnumerator SearchingForGame()
    {
        EnableSearchCanvas(true);

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
                CanvasController.Instance.RequestSearchMatch();
            }
            yield return null;
        }

        EnableSearchCanvas(false);
    }

    public void EnableSearchCanvas(bool isEnabled)
    {
        this.searchCanvas.enabled = isEnabled;
    }
}
