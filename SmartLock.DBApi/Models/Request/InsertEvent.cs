using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLock.DBApi.Models.Request
{
    public class InsertEvent
    {
        [Required]
        public int EventTypeId { get; set; } // E.g., 1 = ButtonLock, 2 = RemoteLock, etc.

        public Guid? DeviceId { get; set; } // Device GUID

        public String? TagUid { get; set; } // RFID tag UID, if applicable

        public DateTime? CreatedAt { get; set; } 
    }
}
