using Newtonsoft.Json.Linq;
using uPLibrary.Networking.M2Mqtt;

namespace Cssure.Services
{
    public interface IMQTTService
    {
        void OpenConnection();
        void CloseConnection();
        bool Publish_RawData(string topic, byte[] data);

    }
     
    public interface IPythonMQTTService : IMQTTService
    {

    }

    public interface IBssureMQTTService : IMQTTService
    {

    }
}
