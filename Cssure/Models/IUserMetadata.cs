namespace Cssure.Models
{
    public interface IUserMetadata
    {
        public string GetName();
        public int GetUserID();
        public string[] GetCaregiversEmail();
        public void SetCaregiversEmail(string[] newEmailList);
        
        public float[] GetMaxNormalCsi();
        public void SetMaxNormalCsi(float[] newMaxCSI);
        public float[] GetMaxNormalModCsi();
        public void SetMaxNormalModCsi(float[] newMaxMODCSI);

        public DateTime GetAlarmExpirey();

        public void SetAlarmExpirey(DateTime _alarmExpirey);
    }
}