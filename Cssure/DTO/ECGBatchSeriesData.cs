namespace Cssure.DTO
{
    public class ECGBatchSeriesDataDTO
    {
        //public int[][] ECGChannel1 { get; set; }
        public string PatientID { get; set; }
        public float[] CSINormMax { get; set; }
        public float[] ModCSINormMax { get; set; }
        public long[] TimeStamp { get; set; }
        public int[][] ECGChannel1 { get; set; } //int[][]
        public int[][] ECGChannel2 { get; set; }
        public int[][] ECGChannel3 { get; set; }
        public int Samples { get; set; }
    }
}
