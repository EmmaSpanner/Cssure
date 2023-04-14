using Newtonsoft.Json.Linq;
using uPLibrary.Networking.M2Mqtt;

namespace Cssure.Services
{
    public interface IMQTTService
    {
        void OpenConncetion();
        void CloseConncetion();
        bool Publish_RawData(string topic, byte[] data);

    }
}
