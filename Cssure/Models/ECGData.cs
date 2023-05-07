namespace Cssure.Models
{
    public class ECGData
    {
        //public int[][] ECGChannel1 { get; set; }
        public int PatientID { get; set; }
        public string TimeStamp { get; set; }
        public List<List<int>> ECGChannel1 { get; set; } //int[][]
        public List<List<int>> ECGChannel2 { get; set; }
        public List<List<int>> ECGChannel3 { get; set; }
        public int Samples { get; set; }
    }
}
