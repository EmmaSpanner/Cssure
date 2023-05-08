namespace Cssure.DTO
{
    public class ECGBatchSeriesData
    {
        //public int[][] ECGChannel1 { get; set; }
        public string PatientID { get; set; }
        public List<long> TimeStamp { get; set; }
        public List<int[]> ECGChannel1 { get; set; } //int[][]
        public List<int[]> ECGChannel2 { get; set; }
        public List<int[]> ECGChannel3 { get; set; }
        public int Samples { get; set; }
    }

    public class ECGBatchSeriesDataDTO
    {
        //public int[][] ECGChannel1 { get; set; }
        public string PatientID { get; set; }
        public long[] TimeStamp { get; set; }
        public int[][] ECGChannel1 { get; set; } //int[][]
        public int[][] ECGChannel2 { get; set; }
        public int[][] ECGChannel3 { get; set; }
        public int Samples { get; set; }
        public float[] CSINormMax { get; set;}
        public float[] ModCSINormMax { get; set;}
    }
}
