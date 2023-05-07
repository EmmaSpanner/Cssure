namespace Cssure.DTO
{
    public class ECGBatchSeriesData
    {
        //public int[][] ECGChannel1 { get; set; }
        public string PatientID { get; set; }
        public List<DateTimeOffset> TimeStamp { get; set; }
        public List<int[]> ECGChannel1 { get; set; } //int[][]
        public List<int[]> ECGChannel2 { get; set; }
        public List<int[]> ECGChannel3 { get; set; }
        public int Samples { get; set; }
    }
}
