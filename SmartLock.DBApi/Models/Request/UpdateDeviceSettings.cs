namespace SmartLock.DBApi.Models.Request
{
    public class UpdateDeviceSettings
    {
        public List<UpdateSettingEntry> Settings { get; set; } = new();
    }

    public class UpdateSettingEntry
    {
        public int SettingId { get; set; }
        public string Value { get; set; } = null!;
    }
}
