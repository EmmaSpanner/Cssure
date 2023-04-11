namespace Cssure.Services
{
    public interface IMqttService
    {
        void Publish(string topic, byte[] data);
    }
}
