using System.Collections.Generic;
using UnityEngine;
using Vivox;
using VivoxUnity;

public class VoiceUI : MonoBehaviour
{
    private VivoxManager vivoxManager;

    private Dictionary<ChannelId, List<SpeakIndicator>> speakIndicators =
        new Dictionary<ChannelId, List<SpeakIndicator>>();

    private void Awake()
    {
        this.vivoxManager = VivoxManager.Instance;

        // TODO: Only for prototype; this should be done in the lobby
        this.vivoxManager.Username = "Player1";
        this.vivoxManager.LogIn();
        // this.vivoxManager.JoinChannel();
    }

    private void UpdateParticipantIndicator(IParticipant participant, ChannelId channelId, bool isAddParticipant)
    {
        // bind the indicator with a correct participant
        if (isAddParticipant)
        {
            // find the correct speakIndicator by participant
            var speakIndicator = this.speakIndicators[channelId].Find(
                (x) => participant.Equals(x.Participant));
            
        }
    }

    private void OnParticipantAdded(string username, ChannelId channelId, IParticipant participant)
    {
        UpdateParticipantIndicator(participant, channelId, true);
    }

}