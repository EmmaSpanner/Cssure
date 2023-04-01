using Cssure.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Cssure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MqttController : Controller
    {

        ////private static readonly MqttClient client = new MqttClient("broker.hivemq.com");
        //private static readonly MqttClient client = new MqttClient("192.168.0.128");
        //private static readonly MqttClient client = new MqttClient("192.168.77.212");
        private static readonly MqttClient client = new MqttClient("localhost");

        private readonly string topic_Data;
        public string Topic_Data => topic_Data;


        private readonly string topic_Reply;
        public string Topic_Reply => topic_Reply;

        public MqttClient Client => client;

        public MqttController()
        {
            topic_Data = "ECG_Data";
            topic_Reply = "ECG_Reply";
        }

        [HttpPost("/Start")]
        public IActionResult Post()
        {
            if (!Client.IsConnected)
            {

                Client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                Client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
                Client.Connect(Guid.NewGuid().ToString());
                Client.Subscribe(new string[] { Topic_Reply }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
               
                Client.Publish(Topic_Data, System.Text.Encoding.UTF8.GetBytes("Conncetion started From C#"), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);

                return Ok(new { message = "Det sku godt" });
            }
            else return BadRequest(new { message = "Der er allerede forbindelse til mqtt" });
        }


        [HttpGet("/Senddata")]
        public IActionResult Get()
        {
            if (Client.IsConnected)
            {

                doPython_FromMQTT();
                return Ok(new { message = "Det sku godt"}); //
            }
            else return BadRequest(new {message= "MQTT er ikke connected"});

        }


        private void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine("Subscribed to topic " + e.MessageId);
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var t = System.Text.Encoding.UTF8.GetString(e.Message);
            Debug.WriteLine("Modtaget");
            Debug.WriteLine("Message received:" + t);


        }
        private List<string> doPython_FromMQTT()
        {
            var time = DateTime.Now;
            string times = time.ToString();
            Client.Publish(Topic_Data, System.Text.Encoding.UTF8.GetBytes("Test From C# at: "+ times), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);

                
            return new List<string>();

        }
    }
}
