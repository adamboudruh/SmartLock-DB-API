using SmartLock.DBApi.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartLock.DBApi.DataAccess
{
    public class Setting
    {
        public int SettingId { get; set; }
        public string Name { get; set; } = null!;
        public string DefaultValue { get; set; } = null!;
        public ICollection<DeviceSetting> DeviceSettings { get; set; } = new List<DeviceSetting>();

    }
}