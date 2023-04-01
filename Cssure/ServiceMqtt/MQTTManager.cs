using Cssure.Hub;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Cssure.ServiceMqtt
{

    public interface IMQTTManager
    {
        void StartMQTTService();
        bool SendMQTTData(string message);
    }

    public class MQTTManager: IMQTTManager
    {

        //private static readonly MqttClient client = new MqttClient("broker.hivemq.com");
        //private static readonly MqttClient client = new MqttClient("192.168.0.128");
        //private static readonly MqttClient client = new MqttClient("192.168.77.212");
       
        private readonly MqttClient client;
        private readonly string topic_Data;
        public string Topic_Data => topic_Data;


        private readonly string topic_Reply;
        public string Topic_Reply => topic_Reply;

        public MqttClient Client => client;

        const byte QOS = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;

        public MQTTManager()
        {
            topic_Data = "ECG_Data";
            topic_Reply = "ECG_Reply";
            client = new MqttClient("localhost");
        }

        public void StartMQTTService()
        {
            if (!Client.IsConnected)
            {
                Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                Client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
                Client.Connect(Guid.NewGuid().ToString());
                Client.Subscribe(new string[] { Topic_Reply }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

                Client.Publish(Topic_Data, System.Text.Encoding.UTF8.GetBytes("Conncetion started From C#"), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            }
        }

        public bool SendMQTTData(string message)
        {
            var succes = Client.IsConnected;
            if (succes)
            {
                var time = DateTime.Now;
                string times = time.ToString();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Send From C#: \n" + message + "\n at" + times);
                

                Client.Publish(Topic_Data, bytes, QOS, false);
                return succes;
            }
            return false;
                
        }

        private void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed to topic " + e.MessageId);
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var t = System.Text.Encoding.UTF8.GetString(e.Message);
            Debug.WriteLine("Modtaget");
            Debug.WriteLine("Message received:" + t);
        }
    }
}
