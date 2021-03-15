using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
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

        private Client client = new Client();
        private AccountId accountId;

        private ILoginSession loginSession;
        public LoginState LoginState { get; private set; }
        private const string channelName = "sampleChannelName";

        private void Awake()
        {
            // vivoxVoiceManager = VivoxVoiceManager.Instance;
            client.Uninitialize();
            client.Initialize();
            DontDestroyOnLoad(this);
        }

        private void OnApplicationQuit()
        {
            client.Uninitialize();
        }

        #region Login Methods

        /// <summary>
        /// Login a player
        /// </summary>
        public void LogIn()
        {
            string uniqueId = Guid.NewGuid().ToString();
            // TODO: for proto purposes only, need to get a real token from server eventually
            accountId = new AccountId(issuer, uniqueId, tokenDomain);
            loginSession = client.GetLoginSession(accountId);
            loginSession.PropertyChanged += OnLoginSessionPropertyChanged;
            loginSession.BeginLogin(serverUri,
                loginSession.GetLoginToken(secretKey, tokenExpiration),
                asyncResult =>
                {
                    try
                    {
                        loginSession.EndLogin(asyncResult);
                    }
                    catch (Exception e)
                    {
                        loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                        Debug.LogError(e.Message);
                    }
                });
        }

        public void LogOut()
        {
            if (loginSession == null || LoginState == LoginState.LoggedOut ||
                LoginState == LoginState.LoggingOut) return;
            // OnUserLoggedOutEvent?.Invoke();
            loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
            loginSession.Logout();
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
                    loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
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
        /// <param name="channelName">channel name</param>
        /// <param name="isConnectAudio">whether to connect audio</param>
        /// <param name="isConnectText">whether to connect text</param>
        /// <param name="channelType">the type of channel</param>
        public void JoinChannel(string channelName, bool isConnectAudio, bool isConnectText,
            ChannelType channelType)
        {
            if (LoginState == LoginState.LoggedIn)
            {
                var channelId = new ChannelId(issuer, channelName, tokenDomain, channelType);
                var channelSession = loginSession.GetChannelSession(channelId);
                channelSession.PropertyChanged += OnChannelSessionPropertyChanged;
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

        public void LeaveChannel(IChannelSession channelSession)
        {
            channelSession.Disconnect();
        }

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

        #endregion


        // bind with UI Login Button
        public void BtnLogin()
        {
            LogIn();
        }

        public void BtnJoinChannel()
        {
            JoinChannel(channelName, true, false, ChannelType.NonPositional);
        }
    }
}