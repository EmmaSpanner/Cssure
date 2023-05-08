namespace Cssure.DTO
{
    public class EKGSampleDTO
    {
        public sbyte[] rawBytes { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public string patientId { get; set; }


    }
}
