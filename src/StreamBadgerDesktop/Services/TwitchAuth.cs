using System;

namespace StreamBadgerDesktop.Services
{
    public class TwitchAuth
    {
        public void SetSessionData(SessionData sessionData)
        {
            SessionData = sessionData;
        }

        public SessionData SessionData { get; private set; }

        public event Action Authenticated;
    }
}