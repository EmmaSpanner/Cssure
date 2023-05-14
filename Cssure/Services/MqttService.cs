﻿namespace Cssure.Services
{
    using Cssure.AlarmSenders;
    using Cssure.Constants;
    using Cssure.DTO;
    using Cssure.Models;
    using Cssure.MongoDB;
    using Cssure.MongoDB.Services;
    using System;
    using System.Text.Json;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;

    public class MqttService : IBssureMQTTService
    {
        private readonly MqttClient client;
        private readonly string clientId;
        private IRawDataService rawDataService;
        private IEmailSender alarmService;
        public MqttClient Client => client;
        public MqttService(IRawDataService rawDataService)
        {
            this.alarmService = alarmService;
            this.rawDataService = rawDataService;
        }

        public UserList UserList { get; }

        public MqttService(IRawDataService rawDataService, UserList userList)
        {
            this.rawDataService = rawDataService;
            UserList = userList;
            client = new MqttClient("assure.au-dev.dk");
            clientId = Guid.NewGuid().ToString();
            this.alarmService = alarmService;
        }

        public void OpenConnection()
        {
            if (!Client.IsConnected)
            {
                Client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                Client.MqttMsgSubscribed += client_MqttMsqSubsribed;

                //Client.Connect(clientId);
                Client.Connect(
                        clientId: clientId,
                        username: "s1",
                        password: "passwordfors1",
                        cleanSession: false,
                        keepAlivePeriod: 60,
                        willFlag: true,
                        willTopic: Topics.Topic_Status_CSSURE,
                        willMessage: "Offline",
                        willRetain: true,
                        willQosLevel: MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
                        );

                //client.Subscribe(new string[] { "SeizureDetectionSystem" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                //Publish("SeizureDetectionSystem", System.Text.Encoding.UTF8.GetBytes("Hej fra Cssure"));

                Client.Publish(Topics.Topic_Status_CSSURE, System.Text.Encoding.UTF8.GetBytes("Online"), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, retain: true);
                Client.Subscribe(new string[] { Topics.Topic_Series_FromBSSURE }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Client.Subscribe(new string[] { Topics.Topic_User }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            }
        }

        public void CloseConnection()
        {
            if (Client.IsConnected)
            {
                Client.Disconnect();
            }
        }


        public bool Publish_RawData(string topic, byte[] data)
        {
            if (Client.IsConnected)
            {
                client.Publish(topic, data, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        //This code runs when a message is received
        async void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {

            if (Client.IsConnected)
            {
                var message = System.Text.Encoding.UTF8.GetString(e.Message);
                var topic = e.Topic;
                if (topic == Topics.Topic_Series_FromBSSURE)
                {
                    EKGSampleDTO temp = JsonSerializer.Deserialize<EKGSampleDTO>(message)!;
                    await Task.Run(() => rawDataService.ProcessData(temp));
                }
                else if (topic.Contains("ECG/Userdata"))
                {
                    UserDataDTO temp = JsonSerializer.Deserialize<UserDataDTO>(message)!;
                    UsersMetadata usersMetadata = new UsersMetadata(temp.UserId, 0, temp.Emails, temp.CSINormMax, temp.ModCSINormMax);
                    
                    if (!UserList.Users.ContainsKey(temp.UserId))
                    {
                        UserList.Users.Add(temp.UserId, usersMetadata);
                    }
                    else
                    {
                        UserList.Users[temp.UserId] = usersMetadata;
                    }


                }
            }
        }

        //This code runs when the client has subscribed to a topic
        async void client_MqttMsqSubsribed(object senser, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Subscribed to topic: " + e.MessageId);
        }
    }
}
