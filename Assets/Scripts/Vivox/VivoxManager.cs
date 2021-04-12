using System;
using System.ComponentModel;
using UnityEngine;
using VivoxUnity;

namespace Vivox
{
    public class VivoxManager : MonoBehaviour
    {
        // TODO: only for sandbox; team should be set in team manager
        public enum TeamColour
        {
            RED = 0,
            BLUE = 1
        }

        // Vivox server credentials
        private static readonly Uri serverUri = new Uri("https://mt1s.www.vivox.com/api2");
        private const string tokenDomain = "mt1s.vivox.com";
        private const string issuer = "hugozh5545-vi18-dev";
        private const string secretKey = "just055";

        // max waiting time when making network connections
        private static readonly TimeSpan tokenExpiration = TimeSpan.FromSeconds(90);

        # region delegates/events

        public delegate void ChannelTextMessageChangedHandler(string sender, IChannelTextMessage channelTextMessage);

        public event ChannelTextMessageChangedHandler OnTextMessageLogReceivedEvent;

        public delegate void ParticipantStatusChangedHandler(string username, ChannelId channel, IParticipant participant);

        public event ParticipantStatusChangedHandler OnParticipantAddedEvent;
        public event ParticipantStatusChangedHandler OnParticipantRemovedEvent;

        public delegate void OnSpeechDetectedHandler(string username, ChannelId channel, bool value);

        public event OnSpeechDetectedHandler OnSpeechDetectedEvent;

        public delegate void OnAudioEnergyUpdatedHandler(string username, ChannelId channel, double value);

        public event OnAudioEnergyUpdatedHandler OnAudioEnergyUpdatedEvent;

        #endregion


        [Range(0, 1)] [SerializeField] private double audioThreshold;

        // vivox client data
        private Client client = new Client();
        private AccountId accountId;
        public string Username { get; set; }

        private ILoginSession loginSession;
        public LoginState LoginState { get; private set; }

        // TODO: channelName only for prototyping; create unique channel name once teamManager is setup
        public string ChannelName { get; set; }
        private ChannelId channelId;
        
        public IAudioDevices AudioInputDevices => this.client.AudioInputDevices;
        public IAudioDevices AudioOutputDevices => this.client.AudioOutputDevices;

        // maintain single instance
        private static readonly object MyLock = new object();
        private static VivoxManager instance;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static VivoxManager Instance
        {
            get
            {
                lock (MyLock)
                {
                    if (instance == null)
                    {
                        // Search for existing instance.
                        instance = (VivoxManager) FindObjectOfType(typeof(VivoxManager));

                        // Create new instance if one doesn't already exist.
                        if (instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<VivoxManager>();
                            singletonObject.name = typeof(VivoxManager) + " (Singleton)";
                        }
                    }

                    // Make instance persistent even if its already in the scene
                    DontDestroyOnLoad(instance.gameObject);
                    return instance;
                }
            }
        }

        private void Awake()
        {
            this.client.Uninitialize();
            this.client.Initialize();
            DontDestroyOnLoad(this);
        }

        private void OnApplicationQuit() => this.client.Uninitialize();

        #region Login Methods

        /// <summary>
        /// Login a player
        /// </summary>
        public void LogIn()
        {
            this.accountId = new AccountId(issuer, Username, tokenDomain);
            this.loginSession = this.client.GetLoginSession(this.accountId);
            this.loginSession.PropertyChanged += OnLoginSessionPropertyChanged;
            this.loginSession.BeginLogin(serverUri,
                this.loginSession.GetLoginToken(secretKey, tokenExpiration),
                asyncResult =>
                {
                    try
                    {
                        this.loginSession.EndLogin(asyncResult);
                    }
                    catch (Exception e)
                    {
                        this.loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                        Debug.LogError(e.Message);
                    }
                });
        }

        public void LogOut()
        {
            if (this.loginSession == null || LoginState == LoginState.LoggedOut ||
                LoginState == LoginState.LoggingOut) return;
            // OnUserLoggedOutEvent?.Invoke();
            this.loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
            this.loginSession.Logout();
        }

        // callback on login state changed

        #endregion

        #region Channel Methods

        /// <summary>
        /// Join a channel
        /// </summary>
        /// <param name="isConnectAudio">whether to connect audio</param>
        /// <param name="isConnectText">whether to connect text</param>
        /// <param name="channelType">the type of channel</param>
        public void JoinChannel(bool isConnectAudio, bool isConnectText, ChannelType channelType)
        {
            if (LoginState == LoginState.LoggedIn)
            {
                this.channelId = new ChannelId(issuer, ChannelName, tokenDomain, channelType);
                var channelSession = this.loginSession.GetChannelSession(this.channelId);
                channelSession.PropertyChanged += OnChannelSessionPropertyChanged;
                channelSession.Participants.AfterKeyAdded += OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved += OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated += OnParticipantValueUpdated;
                channelSession.MessageLog.AfterItemAdded += OnMessageLogReceived;
                channelSession.BeginConnect(isConnectAudio, isConnectText, true,
                    channelSession.GetConnectToken(secretKey, tokenExpiration),
                    asyncResult =>
                    {
                        try
                        {
                            channelSession.EndConnect(asyncResult);
                        }
                        catch (Exception e)
                        {
                            channelSession.PropertyChanged -= OnChannelSessionPropertyChanged;
                            Debug.LogError(e.Message);
                        }
                    });
            }
            else
            {
                Debug.LogError("[Vivox] Cannot join a channel when not logged in.");
            }
        }

        public void LeaveChannel() => LeaveChannel(this.loginSession.GetChannelSession(this.channelId));
        public void LeaveChannel(IChannelSession channelSession) => channelSession.Disconnect();

        #endregion

        #region Callbacks

        private void OnChannelSessionPropertyChanged(object sender,
            PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var session = (IChannelSession) sender;
            switch (session.ChannelState)
            {
                case ConnectionState.Connecting:
                    Debug.Log("Channel connecting...");
                    break;
                case ConnectionState.Connected:
                    Debug.Log("Channel connected. ");
                    break;
                case ConnectionState.Disconnecting:
                    Debug.Log("Channel disconnecting... ");
                    break;
                case ConnectionState.Disconnected:
                    Debug.Log("Channel disconnected. ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnLoginSessionPropertyChanged(object sender,
            PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if ("State" != propertyChangedEventArgs.PropertyName) return;
            LoginState = ((ILoginSession) sender).State;
            switch (LoginState)
            {
                case LoginState.LoggingIn:
                    Debug.Log("[Vivox] Logging in...");
                    break;
                case LoginState.LoggedIn:
                    Debug.Log("[Vivox] Login Success! ");
                    break;
                case LoginState.LoggingOut:
                    Debug.Log("[Vivox] Logging out...");
                    break;
                case LoginState.LoggedOut:
                    Debug.Log("[Vivox] Logged out. ");
                    this.loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnParticipantAdded(object sender, KeyEventArg<string> keyEventArg)
        {
            // cast sender to a dictionary of participants
            var source = (IReadOnlyDictionary<string, IParticipant>) sender;
            var participant = source[keyEventArg.Key];
            var username = participant.Account.Name;
            var channelId = participant.ParentChannelSession.Key;

            Debug.Log($"[Vivox] OnParticipantAddedEvent: {username} added in {channelId}");
            OnParticipantAddedEvent?.Invoke(username, channelId, participant);
        }

        private void OnParticipantRemoved(object sender, KeyEventArg<string> keyEventArg)
        {
            // cast sender to a dictionary of participants
            var source = (IReadOnlyDictionary<string, IParticipant>) sender;
            // Look up the participant via the key.
            var participant = source[keyEventArg.Key];
            var username = participant.Account.Name;
            var channel = participant.ParentChannelSession.Key;
            var channelSession = participant.ParentChannelSession;

            if (participant.IsSelf)
            {
                Debug.Log($"Unsubscribing from: {channelSession.Key.Name}");
                // Now that we are disconnected, unsubscribe.
                channelSession.PropertyChanged -= OnChannelSessionPropertyChanged;
                channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
                channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
                channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;

                // Remove session.
                var user = this.client.GetLoginSession(this.accountId);
                user.DeleteChannelSession(channelSession.Channel);
            }
        }

        private void OnParticipantValueUpdated(object sender, ValueEventArg<string, IParticipant> valueEventArg)
        {
            var source = (IReadOnlyDictionary<string, IParticipant>) sender;
            // Look up the participant via the key.
            var participant = source[valueEventArg.Key];

            var username = valueEventArg.Value.Account.Name;
            var channelId = valueEventArg.Value.ParentChannelSession.Key;
            var property = valueEventArg.PropertyName;

            switch (property)
            {
                case "SpeechDetected":
                {
                    Debug.Log($"[Vivox] OnSpeechDetectedEvent: {username}; Speak: {valueEventArg.Value.SpeechDetected}.");
                    OnSpeechDetectedEvent?.Invoke(username, channelId, valueEventArg.Value.SpeechDetected);
                    break;
                }
                case "AudioEnergy":
                {
                    OnAudioEnergyUpdatedEvent?.Invoke(username, channelId, valueEventArg.Value.AudioEnergy);
                    break;
                }
            }
        }

        private void PlayNoiseOnSpeechDetected(string username, ChannelId channelId, bool isSpeaking)
        {
            if (username == Username)
            {
                if (isSpeaking)
                {
                    this.loginSession.StartAudioInjection("Assets/Audio/White-Noise (1).wav");
                }
                else
                {
                    this.loginSession.StopAudioInjection();
                }
            }
        }

        private void PlayNoiseOnAudioEnergyAboveThreshold(string username, ChannelId channelId, double energy)
        {
            if (username == Username)
            {
                if (energy > this.audioThreshold)
                {
                    this.loginSession.StartAudioInjection("Assets/Audio/White-Noise (1).wav");
                }
                else
                {
                    this.loginSession.StopAudioInjection();
                }
            }
        }

        private void OnMessageLogReceived(object sender,
            QueueItemAddedEventArgs<IChannelTextMessage> textMessage)
        {
            OnTextMessageLogReceivedEvent?.Invoke(textMessage.Value.Sender.DisplayName,
                textMessage.Value);
        }

        #endregion

        public void SendTextMessage(string messageToSend)
        {
            if (ChannelId.IsNullOrEmpty(this.channelId))
            {
                throw new ArgumentException("Must provide a valid ChannelId");
            }

            if (string.IsNullOrEmpty(messageToSend))
            {
                throw new ArgumentException("Must provide a message to send");
            }

            var channelSession = this.loginSession.GetChannelSession(this.channelId);
            channelSession.BeginSendText(messageToSend,
                asyncResult =>
                {
                    try
                    {
                        channelSession.EndSendText(asyncResult);
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"SendTextMessage failed with exception {e.Message}");
                    }
                });
        }


        public void AddPlayNoiseOnSpeechCallback() => OnSpeechDetectedEvent += PlayNoiseOnSpeechDetected;
        public void AddPlayNoiseOnAudioEnergyAboveThresholdCallback() => OnAudioEnergyUpdatedEvent += PlayNoiseOnAudioEnergyAboveThreshold;

        public void RemovePlayNoiseOnSpeechCallback()
        {
            OnSpeechDetectedEvent -= PlayNoiseOnSpeechDetected;

            // immediately stop audio injection, not waiting until OnSpeechDetectedEvent is invoked 
            this.loginSession.StopAudioInjection();
        }

        public void RemovePlayNoiseOnAudioEnergyAboveThresholdCallback()
        {
            OnAudioEnergyUpdatedEvent -= PlayNoiseOnAudioEnergyAboveThreshold;

            // immediately stop audio injection, not waiting until OnAudioEnergyUpdatedEvent is invoked 
            this.loginSession.StopAudioInjection();
        }
    }
}