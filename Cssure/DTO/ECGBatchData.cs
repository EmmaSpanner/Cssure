namespace Cssure.DTO
{
    public class ECGBatchData
    {
        //public int[][] ECGChannel1 { get; set; }
        public string PatientID { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public int[] ECGChannel1 { get; set; } //int[][]
        public int[] ECGChannel2 { get; set; }
        public int[] ECGChannel3 { get; set; } //List<List<int>>
        public int Samples { get; set; }
    }
}
