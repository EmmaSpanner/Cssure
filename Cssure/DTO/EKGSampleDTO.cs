using MongoDB.Bson;

namespace Cssure.DTO
{
    public class EKGSampleDTO
    {
        public ObjectId _id { get; set; }
        public sbyte[] RawBytes { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string PatientId { get; set; }
    }
}
