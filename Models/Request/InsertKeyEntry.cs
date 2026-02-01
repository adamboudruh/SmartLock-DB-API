using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLock_DB_API.Models.Request
{
    public class InsertKeyEntry
    {
        public string Name { get; set; }
        public string TagUid { get; set; }
    }
}
