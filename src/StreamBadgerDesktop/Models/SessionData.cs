using MessagePack;

namespace StreamBadgerDesktop
{
    public class SessionData
    {
        public string AccessToken { get; set; }
        
        public string Id { get; set; }

        public string Name { get; set; }
        
        public string RefreshToken { get; set; }
    }
}