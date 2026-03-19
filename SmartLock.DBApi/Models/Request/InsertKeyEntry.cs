namespace SmartLock.DBApi.Models.Request
{
    public class InsertKeyEntry
    {
        public string Name { get; set; }
        public string TagUid { get; set; }
        public string? Color { get; set; }
    }
}
