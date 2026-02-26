using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SmartLock.DBApi.Models
{
    public class Status<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public List<string>? StatusDetails {  get; set; }
        public T? Data { get; set; }
    }

    public class Status
    {
        public HttpStatusCode StatusCode { get; set; }
        public List<string>? StatusDetails { get; set; }
    }
}
