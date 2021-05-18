using MessagePack;

namespace StreamBadgerLogin.Models
{
    [MessagePackObject]
    public class SessionData
    {
        [Key(0)]
        public string AccessToken { get; set; }
        
        [Key(1)]
        public string Id { get; set; }

        [Key(2)]
        public string Name { get; set; }
        
        [Key(3)]
        public string RefreshToken { get; set; }
    }
}