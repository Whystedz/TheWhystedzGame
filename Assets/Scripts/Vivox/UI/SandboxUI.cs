using System;
using UnityEngine;
using UnityEngine.UI;
using Vivox;
using VivoxUnity;

public class SandboxUI : MonoBehaviour
{
    [SerializeField] private Dropdown teamDropdown;
    [SerializeField] private Toggle undergroundToggle;
    [SerializeField] private InputField usernameInputField;
    [SerializeField] private SpeakIndicator speakIndicator;
    private VivoxManager vivoxManager;

    private void Awake()
    {
        this.vivoxManager = VivoxManager.Instance;

        this.vivoxManager.OnParticipantAddedEvent += OnParticipantAdded;
        
        // initial setup for these states
        UpdateTeamColour();
        
    }

    private void OnDestroy()
    {
        this.vivoxManager.OnParticipantAddedEvent -= OnParticipantAdded;
    }

    private void OnParticipantAdded(string username, ChannelId channelId, IParticipant participant)
    {
       this.speakIndicator.Init(participant);
    }
    public void Login()
    {
        this.usernameInputField.interactable = false;
        var username = this.usernameInputField.text;
        this.vivoxManager.Username = username;
        this.vivoxManager.LogIn();
    }

    public void Join()
    {
        this.teamDropdown.interactable = false;
        this.vivoxManager.JoinChannel(true, true, ChannelType.NonPositional);
    }

    public void UpdateUndergroundState()
    {
        var isUnderground = this.undergroundToggle.isOn;
        Debug.Log($"Underground set to: {isUnderground}");
        if (isUnderground)
        {
            this.vivoxManager.AddPlayNoiseOnSpeechCallback();
            // this.vivoxManager.AddPlayNoiseOnAudioEnergyAboveThresholdCallback();
        }
        else
        {
            this.vivoxManager.RemovePlayNoiseOnSpeechCallback();
            // this.vivoxManager.RemovePlayNoiseOnAudioEnergyAboveThresholdCallback();
        }
    }

    public void UpdateTeamColour()
    {
        var team = (VivoxManager.TeamColour) this.teamDropdown.value;
        Debug.Log($"Team set to: {team}");
        this.vivoxManager.ChannelName = team switch
        {
            VivoxManager.TeamColour.RED => "red",
            VivoxManager.TeamColour.BLUE => "blue",
            _ => this.vivoxManager.ChannelName
        };
    }
}