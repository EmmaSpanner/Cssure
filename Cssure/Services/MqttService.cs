namespace Cssure.Services
{
    using Cssure.Constants;
    using System;
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

        public void OpenConncetion()
        {
            //DecodingBytes.DecodingByteArray.DecodeBytes(new sbyte[] { 57, 12, -47, 12, 3, 32, 44, 63, 34, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, -13, -3, 12, 11, 2, 1, 10, -4, -10, -4, 0, -11, -7, 1, -4, -17, 13, -2, 16, -8, -19, 11, -1, 3, -6, 6, 12, -18, 0, 10, -2, -15, 13, 13, 6, -7, 10});
            //DecodingBytes.DecodingByteArray.DecodeBytes(new sbyte[] { 57, 12, -47, 12, 3, 32, 44, 63, 34, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, -15, -2, 14, 13, 1, 1, 8, -4, -10, -4, 0, -11, -7, 1, -4, -17, 13, -2, 16, -8, -19, 11, -1, 3, -6, 6, 12, -18, 0, 10, -2, -15, 13, 13, 6, -7, 10});
            //DecodingBytes.DecodingByteArray.DecodeBytes(new sbyte[] { 57, 12, -47, 12, 3, 32, 44, 63, 34, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, -17, -5, 17, 15, 4, 1, 13, -4, -10, -4, 0, -11, -7, 1, -4, -17, 13, -2, 16, -8, -19, 11, -1, 3, -6, 6, 12, -18, 0, 10, -2, -15, 13, 13, 6, -7, 10});
            //DecodingBytes.DecodingByteArray.DecodeBytes(new sbyte[] { 57, 12, -47, 12, 3, 32, 44, 63, 34, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, -19, -1, 11, 14, 5, 1, 16, -4, -10, -4, 0, -11, -7, 1, -4, -17, 13, -2, 16, -8, -19, 11, -1, 3, -6, 6, 12, -18, 0, 10, -2, -15, 13, 13, 6, -7, 10});
            if (!Client.IsConnected)
            {
                Client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                Client.MqttMsgSubscribed += client_MqttMsqSubsribed;

                Client.Connect(clientId);

                //client.Subscribe(new string[] { "SeizureDetectionSystem" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                //Client.Publish("SeizureDetectionSystem", System.Text.Encoding.UTF8.GetBytes("Hej fra Cssure"));

                Client.Subscribe(new string[] { Topics.Topic_Series_FromBSSURE }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            }
        }

        public void CloseConncetion()
        {
            if(Client.IsConnected)
            {
                Client.Disconnect();
            }
        }


        public bool Publish_RawData(string topic, byte[] data)
        {
            if(Client.IsConnected)
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
            int[] bytesAsInts = Array.ConvertAll(e.Message, c => (int)c);
            Console.WriteLine("Message received: " + string.Join(",", bytesAsInts));

            await Task.Run(() => rawDataService.ProcessData(e.Message));
        }

        //This code runs when the client has subscribed to a topic
        void client_MqttMsqSubsribed(object senser, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Subscribed to topic: " + e.MessageId);
        }


    }
}
