using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Cssure.Services
{



    public class MQTTServiceLocalPython : IMQTTService
    {

        //private static readonly MqttClient client = new MqttClient("broker.hivemq.com");
        //private static readonly MqttClient client = new MqttClient("192.168.0.128");
        //private static readonly MqttClient client = new MqttClient("192.168.77.212");

        private readonly MqttClient client;
        public MqttClient Client => client;

        const byte QOS = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; //Defalut = QoS1
        private readonly string clientId;




        public MQTTServiceLocalPython()
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
                    keepAlivePeriod: 60,
                    willFlag: true,
                    willTopic: Topics.Topic_Status_CSSURE,
                    willMessage: "Offine",
                    willRetain: true,
                    willQosLevel: QOS
                    );

                Client.Publish(Topics.Topic_Status_CSSURE, System.Text.Encoding.UTF8.GetBytes("Online"), QOS, retain: true);

                Client.Subscribe(new string[] { Topics.Topic_Status_Python }, new byte[] { QOS });
                Client.Subscribe(new string[] { Topics.Topic_Result }, new byte[] { QOS });

            }
        }


        public void CloseConncetion()
        {
            if (Client.IsConnected)
            {
                Client.Publish(Topics.Topic_Status_CSSURE, System.Text.Encoding.UTF8.GetBytes("Offline"), QOS, retain: true);
                Client.Disconnect();
            }
        }


        public bool Publish_RawData(string topic, byte[] message)
        {
            var succes = Client.IsConnected;
            if (succes)
            {
                 Client.Publish("Mads", message, QOS, false);
                return succes;
            }
            return false;

        }

        /// <summary>
        /// Her for vi alle beskeder tilbage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
