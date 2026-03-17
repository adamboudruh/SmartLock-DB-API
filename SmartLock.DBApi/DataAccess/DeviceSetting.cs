namespace SmartLock.DBApi.DataAccess
{
    public class DeviceSetting
    {
        public Guid DeviceSettingId { get; set; }
        public Guid DeviceId { get; set; }
        public int SettingId { get; set; }
        public string Value { get; set; } = null!;

        // Navigation
        public Device Device { get; set; } = null!;
        public Setting Setting { get; set; } = null!;
    }
}