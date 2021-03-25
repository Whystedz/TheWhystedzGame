using System;
using System.ComponentModel;
using UnityEngine;
using VivoxUnity;

namespace Vivox
{
    public class VivoxManager : MonoBehaviour
    {
        // Vivox server credentials
        private static readonly Uri serverUri = new Uri("https://mt1s.www.vivox.com/api2");
        private const string tokenDomain = "mt1s.vivox.com";
        private const string issuer = "hugozh5545-vi18-dev";
        private const string secretKey = "just055";

        private static readonly TimeSpan tokenExpiration = TimeSpan.FromSeconds(90);

        // events/delegates
        public delegate void ChannelTextMessageChangedHandler(string sender,
            IChannelTextMessage channelTextMessage);

        public event ChannelTextMessageChangedHandler OnTextMessageLogReceivedEvent;

        // vivox client data
        private Client client = new Client();
        private AccountId accountId;

        private ILoginSession loginSession;
        public LoginState LoginState { get; private set; }

        // TODO: channelName only for prototyping; create unique channel name once teamManager is setup
        private const string channelName = "sampleChannelName";
        private ChannelId channelId;

        // maintain single instance
        private static readonly object myLock = new object();
        private static VivoxManager instance;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static VivoxManager Instance
        {
            get
            {
                lock (myLock)
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
            var uniqueId = Guid.NewGuid().ToString();
            // TODO: for proto purposes only, need to get a real token from server eventually
            this.accountId = new AccountId(issuer, uniqueId, tokenDomain);
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
                this.channelId = new ChannelId(issuer, channelName, tokenDomain, channelType);
                var channelSession = this.loginSession.GetChannelSession(this.channelId);
                channelSession.PropertyChanged += OnChannelSessionPropertyChanged;
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

        public void LeaveChannel(IChannelSession channelSession) => channelSession.Disconnect();

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

        // bind with UI Login Button
        public void BtnLogin() => LogIn();

        public void BtnJoinChannel() => JoinChannel(true, true, ChannelType.NonPositional);
    }
}
