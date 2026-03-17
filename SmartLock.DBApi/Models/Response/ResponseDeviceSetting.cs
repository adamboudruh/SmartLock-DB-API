namespace SmartLock.DBApi.Models.Response
{
    public class ResponseDeviceSetting
    {
        public int SettingId { get; set; }
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string DefaultValue { get; set; } = null!;
    }
}