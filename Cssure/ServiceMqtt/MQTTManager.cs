using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using static IronPython.Modules._ast;

namespace Cssure.ServiceMqtt
{

    public interface IMQTTManager
    {
        void OpenConncetion();
        void CloseConncetion();
        bool Publish_RawData(int message);
    }

    public class MQTTManager: IMQTTManager
    {

        //private static readonly MqttClient client = new MqttClient("broker.hivemq.com");
        //private static readonly MqttClient client = new MqttClient("192.168.0.128");
        //private static readonly MqttClient client = new MqttClient("192.168.77.212");
       
        private readonly MqttClient client;
        public MqttClient Client => client;

        const byte QOS = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; //Defalut = QoS1
        private readonly string clientId;



        #region Topics
        private const string Topic_Status_ASPNet = "ECG/Status/ASP.Net";
        private const string Topic_Status_Python = "ECG/Status/Python";

        private const string Topic_Series_Raw = "ECG/Series/Raw";
        private const string Topic_Series_Filtred = "ECG/Series/Filtred";

        private const string Topic_Result = "ECG/Result/#";
        private const string Topic_Result_CSI = "ECG/Result/CSI";
        private const string Topic_Result_ModCSI = "ECG/Result/ModCSI";
        private const string Topic_Reuslt_RR = "ECG/Result/RR-Peak";
        #endregion


        public MQTTManager()
        {
            client = new MqttClient("localhost");
            clientId = Guid.NewGuid().ToString(); //Nyt unikt ClientID
        }

        /// <summary>
        /// Start MQTT service og conncet med LastWill 
        /// </summary>
        public void OpenConncetion()
        {
            if (!Client.IsConnected)
            {
                Client.MqttMsgPublishReceived += NewMQTTMessageReceived; //Alle modtaget beskeder havner her
                Client.MqttMsgSubscribed += NewMQTTSubscriptionEstablished; //Repons på alle subscribtion ender her

                Client.Connect(
                    clientId: clientId,
                    username: "",
                    password: "",
                    cleanSession: false,
                    keepAlivePeriod:60,
                    willFlag: true,
                    willTopic: Topic_Status_ASPNet,
                    willMessage: "Offine",
                    willRetain: true,
                    willQosLevel: QOS
                    );

                Client.Publish(Topic_Status_ASPNet, System.Text.Encoding.UTF8.GetBytes("Online"), QOS, retain: true);

                Client.Subscribe(new string[] { Topic_Status_Python }, new byte[] { QOS });
                Client.Subscribe(new string[] { Topic_Result }, new byte[] { QOS });

            }
        }


        public void CloseConncetion()
        {
            if (Client.IsConnected)
            {
                Client.Publish(Topic_Status_ASPNet, System.Text.Encoding.UTF8.GetBytes("Offline"), QOS, retain: true);
                Client.Disconnect();
            }
        }


        public bool Publish_RawData(int message)
        {
            var succes = Client.IsConnected;
            if (succes)
            {
                var time = DateTime.Now;
                string times = time.ToString();
                byte[] sendMessages = 
                    System.Text.Encoding.UTF8.GetBytes(message.ToString());

                Client.Publish(Topic_Series_Raw, sendMessages, QOS, false);
                return succes;
            }
            return false;
                
        }

        private void NewMQTTMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetString(e.Message);
            var topic = e.Topic; 
            Debug.WriteLine($"Message received from <<{topic}>>: " + message);
        }


        private void NewMQTTSubscriptionEstablished(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed to topic " + e.MessageId);
        }

    }
}
