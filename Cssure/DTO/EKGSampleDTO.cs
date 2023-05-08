namespace Cssure.DTO
{
    public class EKGSampleDTO
    {
        public sbyte[] RawBytes { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string PatientId { get; set; }
    }
}
