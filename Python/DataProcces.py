#%% Import libraries
import paho.mqtt.client as mqtt

def on_connect(client, userdata, flags, rc):
    client.subscribe(Topic_Status_ASPNet)
    client.subscribe(Topic_Series_Raw)
    client.subscribe(Topic_Status_Python_Disconnect)
    client.publish(Topic_Status_Python, "Online", qos=1, retain=True)
    print("Connected with result code "+str(rc))
    
ms = ""
def on_message(client, userdata, msg):
    print("=========  Start on_message ============================")
    # Switch case for the different topics
    global ms
    message = msg.payload
    ms = message.decode("utf-8")
    if msg.topic == Topic_Status_ASPNet:
        print("ASP.Net status: " + str(message.decode("utf-8")))
    elif msg.topic == Topic_Series_Raw:
        print("Raw data: " + str(message.decode("utf-8")))
        signal = int(message.decode("utf-8"))

        
        #===================  Filter  ===================
        signalfiltred = signal+1
        #encode signalfiltred to byte
        signalfiltred = str(signalfiltred)
        
        
        #===================  CSI  ===================
        CSI = 2
        
        #===================  ModCSI  ===================
        ModCSI = 1
        
        #===================  RR-Peak  ===================
        RR = 0.75
        
        client.publish(Topic_Series_Filtred, ""+(signalfiltred))
        client.publish(Topic_Result_CSI, CSI)
        client.publish(Topic_Result_ModCSI, ModCSI)
        client.publish(Topic_Reuslt_RR, RR)
    else:
        print("Unknown topic: " + msg.topic)
    print("=========  END on_message ============================")
    
    
def on_disconnect(client, userdata, rc):
   
    client.publish(Topic_Status_Python, "Offline", qos=1, retain=True)
    print("Disconnected with result code "+str(rc))
    
def on_subscribe(client, userdata, mid, granted_qos):
    print("Subscribed: "+str(mid)+" "+str(granted_qos))
    
    
    

client_userdata = 0

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message;
client.on_disconnect = on_disconnect
client.on_subscribe = on_subscribe


# client.connect("broker.hivemq.com", 1883, 60)
# client.connect("192.168.0.128", 1883, 60)
# client.connect("192.168.77.212", 1883, 60)

#region Topics
Topic_Status = "ECG/Status/#";
Topic_Status_ASPNet = "ECG/Status/ASP.Net";
Topic_Status_Python = "ECG/Status/Python";
Topic_Status_Python_Disconnect = "ECG/Status/Python/Disconnect";

Topic_Series_Raw = "ECG/Series/Raw";
Topic_Series_Filtred = "ECG/Series/Filtred";

Topic_Result = "ECG/Result/#";
Topic_Result_CSI = "ECG/Result/CSI";
Topic_Result_ModCSI = "ECG/Result/ModCSI";
Topic_Reuslt_RR = "ECG/Result/RR-Peak";
#endregion

        
client.will_set(Topic_Status_Python, payload="Offline", qos=2, retain=True)
client.connect("localhost", 1883, 60)

client.loop_forever()


