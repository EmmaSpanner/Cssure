namespace Cssure.Constants
{
    public interface IIpAdresses
    {
        public string getIP();

    }

    public class IpAdresses : IIpAdresses
    {
        private readonly string ip;
        public IpAdresses(string ip)
        {
            this.ip = ip;
        }
        public string getIP() 
        {
            if (this.ip != null) 
            {
                return this.ip;
            }
            else
            {
                return "http://localhost";
            }
        }

    }
}
