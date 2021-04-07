using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vivox;
using VivoxUnity;

public class SpeakIndicator : MonoBehaviour
{
    [SerializeField] private Sprite mutedSprite;

    [SerializeField] private Sprite speakingSprite;

    [SerializeField] private Sprite notSpeakingSprite;

    [SerializeField] private Image chatStateImage;

    private VivoxManager vivoxManager;
    public IParticipant Participant { get; private set; }

    private bool isMuted;


    private bool isSpeaking;

    public bool IsMuted
    {
        get => this.isMuted;
        private set
        {
            if (Participant.IsSelf)
            {
                // mute/unmute local input device
                this.vivoxManager.AudioInputDevices.Muted = value;
            }

            this.isMuted = value;
            UpdateSprite();
        }
    }

    public bool IsSpeaking
    {
        get => this.isSpeaking;
        private set
        {
            if (!IsMuted)
            {
                this.isSpeaking = value;
                UpdateSprite();
            }
        }
    }


    public void Init(IParticipant participantPlayer)
    {
        this.vivoxManager = VivoxManager.Instance;
        Participant = participantPlayer;

        IsMuted = Participant.IsSelf ? this.vivoxManager.AudioInputDevices.Muted : Participant.LocalMute;
        IsSpeaking = Participant.SpeechDetected;

        // bind isSpeaking to vivox participant property
        Participant.PropertyChanged += (sender, args) =>
        {
            IsSpeaking = args.PropertyName switch
            {
                "SpeechDetected" => Participant.SpeechDetected,
                _ => IsSpeaking
            };
        };
        Debug.Log("SpeakIndicator initialized!");
    }

    private void UpdateSprite()
    {
        if (this.isMuted) this.chatStateImage.sprite = this.mutedSprite;
        else if (this.isSpeaking) this.chatStateImage.sprite = this.speakingSprite;
        else this.chatStateImage.sprite = this.notSpeakingSprite;
    }

    public void ToggleMute()
    {
        Debug.Log("Toggle mute!");
        IsMuted = !IsMuted;
    }
}