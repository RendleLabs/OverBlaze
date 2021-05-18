namespace StreamBadger.Models
{
    public class UploadImageModel
    {
        public string Name { get; set; }
        public string Css { get; set; }
        public string Sound { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Top { get; set; }
        public int? Left { get; set; }
    }
}