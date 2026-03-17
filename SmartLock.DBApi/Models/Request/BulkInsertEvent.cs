using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLock.DBApi.Models.Request
{
    public class BulkInsertEvent
    {
        public List<InsertEvent> Events { get; set; } = new();
    }
}
