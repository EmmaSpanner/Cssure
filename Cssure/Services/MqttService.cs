﻿namespace Cssure.Services
{
    using Cssure.Constants;
    using Cssure.DTO;
    using System;
    using System.Text.Json;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;

    public class MqttService : IBssureMQTTService
    {
        private readonly MqttClient client;
        private readonly string clientId;
        private IRawDataService rawDataService;

        public MqttClient Client => client;
        public MqttService(IIpAdresses ipAdresses, IRawDataService rawDataService)
        {
            this.rawDataService = rawDataService;

            var tempUrl = ipAdresses.getIP();
            var url = tempUrl.Split("//")[1].Split(":")[0];

            client = new MqttClient(url);
            clientId = Guid.NewGuid().ToString();
        }

        public void OpenConnection()
        {
            if (!Client.IsConnected)
            {
                Client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                Client.MqttMsgSubscribed += client_MqttMsqSubsribed;

                Client.Connect(clientId);

                //client.Subscribe(new string[] { "SeizureDetectionSystem" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                //Publish("SeizureDetectionSystem", System.Text.Encoding.UTF8.GetBytes("Hej fra Cssure"));

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
                if (e.Topic == Topics.Topic_Series_FromBSSURE)
                {
                    var message = System.Text.Encoding.UTF8.GetString(e.Message);
                    var topic = e.Topic;
                    EKGSampleDTO temp = JsonSerializer.Deserialize<EKGSampleDTO>(message)!;
                    await Task.Run(() => rawDataService.ProcessData(temp));
                }
            }
        }

        //This code runs when the client has subscribed to a topic
        void client_MqttMsqSubsribed(object senser, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Subscribed to topic: " + e.MessageId);
        }
    }
}
