namespace Cssure.Services
{
    using System;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;

    public class MqttService : IMqttService
    {
        private readonly MqttClient client;

        public MqttClient Client => client;
        public MqttService() 
        {
            client = new MqttClient("172.20.10.4");
            string clientId = Guid.NewGuid().ToString();
            Connect(client, clientId);
        }

        public void Connect(MqttClient client, string clientId)
        {
            if(!Client.IsConnected)
            {
                client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                client.MqttMsgSubscribed += client_MqttMsqSubsribed;

                client.Connect(clientId);

                //client.Subscribe(new string[] { "SeizureDetectionSystem" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                //Publish("SeizureDetectionSystem", System.Text.Encoding.UTF8.GetBytes("Hej fra Cssure"));

                client.Subscribe(new string[] { "SeizureDetectionSystemBssure" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

            }
        }

        public void CloseConnection()
        {
            if(Client.IsConnected)
            {
                Client.Disconnect();
            }
        }

        public void Publish(string topic, byte[] data)
        {
            if(Client.IsConnected)
            {
                client.Publish(topic, data, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            }
        }

        //This code runs when a message is received
        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            int[] bytesAsInts = Array.ConvertAll(e.Message, c => (int)c);
            Console.WriteLine("Message received: " + string.Join(",", bytesAsInts));
        }

        //This code runs when the client has subscribed to a topic
        static void client_MqttMsqSubsribed(object senser, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Subscribed to topic: " + e.MessageId);
        }
    }
}
