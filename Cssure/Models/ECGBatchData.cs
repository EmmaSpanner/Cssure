using MongoDB.Bson;

namespace Cssure.Models
{
    public class ECGBatchData
    {
        public ObjectId _id { get; set; }
        public string PatientID { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public int[] ECGChannel1 { get; set; }
        public int[] ECGChannel2 { get; set; }
        public int[] ECGChannel3 { get; set; }
        public int Samples { get; set; }
    }
}
