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
        private static readonly Uri ServerUri = new Uri("https://mt1s.www.vivox.com/api2");
        private const string TokenDomain = "mt1s.vivox.com";
        private const string Issuer = "hugozh5545-vi18-dev";
        private const string SecretKey = "just055";
        
        private static readonly TimeSpan LogInTimeOut = TimeSpan.FromSeconds(90);

        private Client client = new Client();
        private AccountId accountId;

        public ILoginSession loginSession;
        public LoginState LoginState { get; private set; }
        private const string ChannelName = "sampleChannelName";

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

        public void LogIn()
        {
            string uniqueId = Guid.NewGuid().ToString();
            // for proto purposes only, need to get a real token from server eventually
            accountId = new AccountId(Issuer, uniqueId, TokenDomain);
            loginSession = client.GetLoginSession(accountId);
            loginSession.PropertyChanged += OnLoginSessionPropertyChanged;
            loginSession.BeginLogin(ServerUri,
                loginSession.GetLoginToken(SecretKey, LogInTimeOut),
                asyncResult =>
                {
                    try
                    {
                        loginSession.EndLogin(asyncResult);
                    }
                    catch (Exception e)
                    {
                        loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                        Debug.Log(e.Message);
                    }
                });
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
                    loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void LogOut()
        {
            if (loginSession != null && LoginState != LoginState.LoggedOut &&
                LoginState != LoginState.LoggingOut)
            {
                // OnUserLoggedOutEvent?.Invoke();
                loginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                loginSession.Logout();
            }
        }

        public void JoinChannel()
        {
            // vivoxVoiceManager.JoinChannel(CHANNEL_NAME,ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.AudioOnly);
        }
    }
}