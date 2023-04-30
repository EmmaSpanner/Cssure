namespace Cssure.DTO
{
    public class PyDTO
    {
        public string PatientID { get; set; }
        public string Timestamp { get; set; }
        public int SampleRate { get; set; }
        public float[][] Data { get; set; }
    }
}
