using Cssure.Constants;
using Cssure.DTO;
using Cssure.MongoDB.Services;
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
        private ProcessedECGDataService processedDataService;
        public MqttClient Client => client;

        const byte QOS = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; //Defalut = QoS1

        public MQTTServiceLocalPython(ProcessedECGDataService processedDataService)
        {
           
            this.processedDataService = processedDataService;

            client = new MqttClient("assure.au-dev.dk");
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
                     username: "s1",
                    password: "passwordfors1",
                    cleanSession: false,
                    keepAlivePeriod: 60,
                    willFlag: true,
                    willTopic: Topics.Topic_Status_CSSURE + "/py",
                    willMessage: "Offline",
                    willRetain: true,
                    willQosLevel: QOS
                    );

                Client.Publish(Topics.Topic_Status_CSSURE + "/py", System.Text.Encoding.UTF8.GetBytes("Online"), QOS, retain: true);
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
        private async void MQTTMessageReceivedCSIdata(object sender, MqttMsgPublishEventArgs e)
        {
            string topic = e.Topic;
            try
            {
                

               
                if (topic == Topics.Topic_Status_Python)
                {
                    Debug.WriteLine(e.Message.ToString());
                }
                else if (topic == Topics.Topic_Series_Filtred)
                {

               
                    var message = System.Text.Encoding.UTF8.GetString(e.Message);
                    CSI_DTO csi = JsonSerializer.Deserialize<CSI_DTO>(message)!;
                    await processedDataService.postCSI(csi);
                    Debug.WriteLine($"Message received from <<{topic}>>:");
                    Debug.WriteLine($"Vital parametres: " +
                        $"\n \t PatientID: {csi.PatientID}" +
                        $"\n \t TimeStamp: {csi.TimeStamp}" +
                        $"\n \t SeriesLength_s: {csi.SeriesLength_s}" +
                        $"\n \t TimeProcess_s:{csi.TimeProcess_s} " +
                        $"\n \t len_rr:{csi.ECGChannel1.len_rr} " +

                        $"\n \t mean_hr:{csi.ECGChannel1.mean_hr} " +
                        $"\n \t\t mean_hr30:{csi.ECGChannel1.mean_hr30} " +
                        $"\n \t\t mean_hr50:{csi.ECGChannel1.mean_hr50} " +
                        $"\n \t\t mean_hr100:{csi.ECGChannel1.mean_hr100} " +


                        $"\n \t CSI Values: " +
                        $"\n \t\t CSI:{csi.ECGChannel1.csi} " +
                        $"\n \t\t CSI30:{csi.ECGChannel1.CSI30} " +
                        $"\n \t\t CSI50:{csi.ECGChannel1.CSI50} " +
                        $"\n \t\t CSI100:{csi.ECGChannel1.CSI100} " +
                        $"\n \t\t ModCSI:{csi.ECGChannel1.Modified_csi} " +
                        $"\n \t\t ModCSI30:{csi.ECGChannel1.ModCSI30} " +
                        $"\n \t\t ModCSI50:{csi.ECGChannel1.ModCSI50} " +
                        $"\n \t\t ModCSI100:{csi.ECGChannel1.ModCSI100} " +


                        $"\n \t Alarms: " +
                        $"\n \t\t CSI30:{csi.Alarm.CSI30_Alarm} " +
                        $"\n \t\t CSI50:{csi.Alarm.CSI50_Alarm} " +
                        $"\n \t\t CSI100:{csi.Alarm.CSI100_Alarm}" +
                        $"\n \t\t MOD_CSI: {csi.Alarm.ModCSI100_Alarm}" +
                        $"\n\n");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("HER");
                Debug.WriteLine(ex.ToString());
                Debug.WriteLine($"Message received from <<{topic}>>: " + e.Message);
            }
        }

        private void MQTTSubscriptionEstablished(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed to topic " + e.MessageId);
        }
    }
}
