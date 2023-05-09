namespace Cssure.DTO
{
    public class CSI_DTO
    {
        public string PatientID { get; set; }
        public long TimeStamp { get; set; }
        public float TimeProcess_s { get; set; }
        public float SeriesLength_s { get; set; }
        public Alarm Alarm { get; set; }
        public Ecgchannel ECGChannel1 { get; set; }
        public Ecgchannel ECGChannel2 { get; set; }
        public Ecgchannel ECGChannel3 { get; set; }
    }

    public class Alarm
    {
        public int CSI30_Alarm { get; set; }
        public int CSI50_Alarm { get; set; }
        public int CSI100_Alarm { get; set; }
        public int ModCSI100_Alarm { get; set; }
    }


    public class Ecgchannel
    {
        public int len_rr { get; set; }
        public float CSI30 { get; set; }
        public float ModCSI30 { get; set; }
        public float mean_hr30 { get; set; }
        public float CSI50 { get; set; }
        public float ModCSI50 { get; set; }
        public float mean_hr50 { get; set; }
        public float CSI100 { get; set; }
        public float ModCSI100 { get; set; }
        public float mean_hr100 { get; set; }
        public float mean_hr { get; set; }
        public float csi { get; set; }
        public float Modified_csi { get; set; }
        public float cvi { get; set; }
    }



}
