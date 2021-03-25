using UnityEngine;
using UnityEngine.UI;
using VivoxUnity;
using Vivox;
using Button = UnityEngine.UI.Button;

// TODO: file from Vivox sample; not implemented yet
/// <summary>
/// Text Chat UI in game scene
/// </summary>
public class TextChatUI : MonoBehaviour
{
    private VivoxManager vivoxManager;

    [SerializeField] private Button sendButton;
    [SerializeField] private InputField messageInputField;
    [SerializeField] private Text newMessageText;

    private void Awake()
    {
        this.vivoxManager = VivoxManager.Instance;
        this.vivoxManager.OnTextMessageLogReceivedEvent += OnTextMessageLogReceivedEvent;
    }

    private void OnDestroy() => this.vivoxManager.OnTextMessageLogReceivedEvent -=
        OnTextMessageLogReceivedEvent;


    private void ClearOutTextField()
    {
        this.messageInputField.text = string.Empty;
        this.messageInputField.Select();
        this.messageInputField.ActivateInputField();
    }

    private void OnTextMessageLogReceivedEvent(string sender,
        IChannelTextMessage channelTextMessage)
    {
        var text = channelTextMessage.Message;
        this.newMessageText.text = text;
    }

    public void BtnSendText()
    {
        if (string.IsNullOrEmpty(this.messageInputField.text)) return;

        this.vivoxManager.SendTextMessage(this.messageInputField.text);
        ClearOutTextField();
    }
}
