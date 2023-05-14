using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;

namespace Cssure.Models
{
    public class UserList
    {
        public IDictionary<string, IUserMetadata> Users;
        
        public UserList()
        {
            Users = new Dictionary<string, IUserMetadata>();
        }
    }

    public class UsersMetadata : IUserMetadata
    {
        private  string name;
        private  int userId;
        private  string[] email;
        private  float[] maxcsi;
        private  float[] maxmodcsi;
        private DateTime alarmExpirey;

        private ECGBatchSeriesData bufferedECG = new ECGBatchSeriesData()
        {
            ECGChannel1 = new List<int[]>(),
            ECGChannel2 = new List<int[]>(),
            ECGChannel3 = new List<int[]>(),
            TimeStamp = new List<long>(),
            Samples = 0,

        };


        public ECGBatchSeriesData BufferedECG
        {
            get { return bufferedECG; }
            set { bufferedECG = value; }
        }

        public UsersMetadata(string Name, int UserId, string[] email, float[] maxcsi, float[] maxmodcsi)
        {
            name = Name;
            userId = UserId;
            this.email = email;
            this.maxcsi = maxcsi;
            this.maxmodcsi = maxmodcsi;
        }

        public string[] GetCaregiversEmail()
        {
            return email;
        }

        public DateTime GetAlarmExpirey()
        {
            return alarmExpirey;
        }

        public void SetAlarmExpirey(DateTime _alarmExpirey)
        {
            alarmExpirey = _alarmExpirey;
        }

        public float[] GetMaxNormalCsi()
        {
            return maxcsi;
        }

        public float[] GetMaxNormalModCsi()
        {
            return maxmodcsi;
        }

        public string GetName()
        {
            return name;
        }

        public int GetUserID()
        {
            return userId;
        }

        public void SetCaregiversEmail(string[] newEmailList)
        {
            email = newEmailList;
        }

        public void SetMaxNormalCsi(float[] newMaxCSI)
        {
            maxcsi= newMaxCSI;
        }

        public void SetMaxNormalModCsi(float[] newMaxMODCSI)
        {
            maxmodcsi= newMaxMODCSI;
        }
    }
}
