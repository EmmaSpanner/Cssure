using Cssure.Constants;
using Cssure.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Cssure.Services
{

    public class MQTTServiceLocalPython : IPythonMQTTService
    {
        private readonly string clientId;
        private readonly MqttClient client;
        public MqttClient Client => client;

        const byte QOS = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; //Defalut = QoS1

        public MQTTServiceLocalPython(IIpAdresses ipAdresses)
        {
            var tempUrl = ipAdresses.getIP();
            var url = tempUrl.Split("//")[1].Split(":")[0];

            client = new MqttClient(url);
            clientId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Start MQTT service og conncet med LastWill 
        /// </summary>
        public void OpenConnection()
        {
            if (!Client.IsConnected)
            {
                Client.MqttMsgPublishReceived += MQTTMessageReceivedCSIdata; //Alle modtaget beskeder havner her
                Client.MqttMsgSubscribed += MQTTSubscriptionEstablished; //Repons på alle subscribtion ender her

                Client.Connect(
                    clientId: clientId,
                    username: "",
                    password: "",
                    cleanSession: false,
                    keepAlivePeriod: 60,
                    willFlag: true,
                    willTopic: Topics.Topic_Status_CSSURE,
                    willMessage: "Offline",
                    willRetain: true,
                    willQosLevel: QOS
                    );

                Client.Publish(Topics.Topic_Status_CSSURE, System.Text.Encoding.UTF8.GetBytes("Online"), QOS, retain: true);
                Client.Subscribe(new string[] { Topics.Topic_Status_Python }, new byte[] { QOS });
                Client.Subscribe(new string[] { Topics.Topic_Series_Filtred }, new byte[] { QOS });
            }
        }

        public void CloseConnection()
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
                Client.Publish(topic, message, QOS, false);
                return succes;
            }
            return false;
        }

        /// <summary>
        /// Her for vi alle beskeder tilbage
        /// Data kommer ind som et json object og derved kan det parse ned i et CSI_DTO object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MQTTMessageReceivedCSIdata(object sender, MqttMsgPublishEventArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetString(e.Message);
            var topic = e.Topic;
            try
            {
                CSI_DTO csi = JsonSerializer.Deserialize<CSI_DTO>(message)!;
                Debug.WriteLine($"Message received from <<{topic}>>:");
                Debug.WriteLine($"Vital parametres: " +
                    $"\n \t Timestamp: {csi.Timestamp}" +
                    $"\n \t len_rr: {csi.len_rr}" +
                    $"\n \t HeartRate: {csi.mean_hr}" +
                    $"\n \t CSI:{csi.csi} " +
                    $"\n \t\t CSI30:{csi.csi_30} " +
                    $"\n \t\t CSI50:{csi.csi_50} " +
                    $"\n \t\t CSI100:{csi.csi_100}" +
                    $"\n \t MOD_CSI: {csi.Modified_csi}" +
                    $"\n \t\t MOD_CSI100: {csi.Modified_csi_100}" +
                    $"\n\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debug.WriteLine($"Message received from <<{topic}>>: " + message);
            }
        }

        private void MQTTSubscriptionEstablished(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed to topic " + e.MessageId);
        }
    }
}
