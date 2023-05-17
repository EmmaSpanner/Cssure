using Cssure.AlarmSenders;
using Cssure.Constants;
using Cssure.DTO;
using Cssure.Models;
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
        private IEmailSender alarmService;

        public bool shouldAlarm = true;

        public MqttClient Client => client;

        const byte QOS = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; //Defalut = QoS1

        public UserList UserList { get; }
        //Todo: Db interaktion
        public MQTTServiceLocalPython(ProcessedECGDataService processedDataService,UserList userList, IEmailSender alarmService)

        //public MQTTServiceLocalPython(UserList userList)
        {
            this.alarmService = alarmService;
            //Todo: Db interaktion
            this.processedDataService = processedDataService;
            UserList = userList;
            string host = "assure.au-dev.dk";
            //host = "localhost";
            client = new MqttClient(host);
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
                    willTopic: Topics.Topic_Status_CSSUREPY,
                    willMessage: "Offline",
                    willRetain: true,
                    willQosLevel: QOS
                    );

                Client.Publish(Topics.Topic_Status_CSSUREPY, System.Text.Encoding.UTF8.GetBytes("Online"), QOS, retain: true);
                
                
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

                var message = System.Text.Encoding.UTF8.GetString(e.Message);
                if (topic == Topics.Topic_Status_Python)
                {
                    Debug.WriteLine(message.ToString());
                }
                else if (topic == Topics.Topic_Series_Filtred)
                {

                    //var message = System.Text.Encoding.UTF8.GetString(e.Message);
                    CSI_DTO csi = JsonSerializer.Deserialize<CSI_DTO>(message)!;

                    //todo: Force email
                    //csi.Alarm.CSI30_Alarm = true;
                    if (csi.Alarm.CSI30_Alarm || csi.Alarm.CSI50_Alarm || csi.Alarm.CSI100_Alarm || csi.Alarm.ModCSI100_Alarm)
                    {
                        //If user has added patientId and it has been 5 minutes since the last alarm an email should be sent.
                        if (UserList.Users.ContainsKey(csi.PatientID))
                        {
                            IUserMetadata user = UserList.Users[csi.PatientID];
                            string[] userEmails = user.GetCaregiversEmail();
                            if(user.GetAlarmExpirey() == null)
                            {
                                user.SetAlarmExpirey(DateTime.Now);
                            }

                            if(DateTime.Now > user.GetAlarmExpirey())
                            {
                                user.SetAlarmExpirey(DateTime.Now.AddMinutes(1));
                                try
                                {
                                    await alarmService.SendEmailAsync(userEmails, "ALARM", "Alarm! A seizure occured at " + DateTimeOffset.FromUnixTimeMilliseconds(csi.TimeStamp).LocalDateTime);
                                } catch (Exception ex)
                                {
                                    Debug.WriteLine("Exception occured when trying to send alarm email: " + ex.ToString());
                                }
                               
                            }


                        }
                    }
                    else
                    {
                        //Everythings is normal
                    }


                    //Todo: Db interaktion POST CSI
                    //await processedDataService.postCSI(csi);


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
                        $"\n \t\t MOD_CSI:{csi.Alarm.ModCSI100_Alarm}" +
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
