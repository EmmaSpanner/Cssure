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
        public MqttClient Client => client;


        private const string Topic_Data     = "ECG/Data/raw";
        private const string Topic_Reply    = "ECG/Data/RR";
        private const string Topic_Status_C = "ECG/Status/C";
        private const string Topic_Status_Py= "ECG/Status/Py";


        const byte QOS = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
        private readonly string clientId;

        public MQTTManager()
        {
            client = new MqttClient("localhost");
            clientId = Guid.NewGuid().ToString();
        }

        public void StartMQTTService()
        {
            if (!Client.IsConnected)
            {
                Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                Client.MqttMsgSubscribed += Client_MqttMsgSubscribed;

                Client.Connect(
                    clientId: clientId,
                    username: "",
                    password: "",
                    cleanSession: false,
                    keepAlivePeriod:60,
                    willFlag: true,
                    willTopic: Topic_Status_C,
                    willMessage: "Offine",
                    willRetain: true,
                    willQosLevel: 1
                    );
                Client.Publish(Topic_Status_C, System.Text.Encoding.UTF8.GetBytes("Online"), QOS, retain: true);

                Client.Subscribe(new string[] { Topic_Reply }, new byte[] { QOS });

                byte[] ConnectionMessage = System.Text.Encoding.UTF8.GetBytes("Conncetion started From C#");
                Client.Publish(Topic_Data, ConnectionMessage, QOS, false);
            }
        }

        public bool SendMQTTData(string message)
        {
            var succes = Client.IsConnected;
            if (succes)
            {
                var time = DateTime.Now;
                string times = time.ToString();
                byte[] sendMessages = 
                    System.Text.Encoding.UTF8.GetBytes(
                        "Send From C#: \n" + 
                        message 
                        + "\n at" + times);

                Client.Publish(Topic_Data, sendMessages, QOS, false);
                return succes;
            }
            return false;
                
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var t = System.Text.Encoding.UTF8.GetString(e.Message);
            Debug.WriteLine("Modtaget");
            Debug.WriteLine("Message received:" + t);
        }


        private void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed to topic " + e.MessageId);
        }

    }
}
