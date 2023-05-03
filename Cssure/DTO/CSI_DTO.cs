namespace Cssure.DTO
{
    public class CSI_DTO
    {
        public string PatientId { get; set; }
        public string Timestamp { get; set; }
        public string TimeProcess_s { get; set; }
        public int len_rr { get; set; }
        public float mean_hr { get; set; }
        public float csi_30 { get; set; }
        public float csi_50 { get; set; }
        public float csi_100 { get; set; }
        public float Modified_csi_100 { get; set; }
        public float csi { get; set; }
        public float Modified_csi { get; set; }
        public float cvi { get; set; }
        public float[] rr_intervals_ms { get; set; }
        public float[] filtered_ecg { get; set; }
    }
}
