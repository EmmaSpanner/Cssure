#%%
import paho.mqtt.client as mqtt
import numpy as np
from scipy import signal
import json
import matplotlib.pyplot as plt
import datetime

"""
Dette dokument skal imiterer CSSURE og derved sende data til DataProcces.py over MQTT.
Data skal sendes over Topic_Series_Raw. 
Hvor dataet skal komme tilbage på Topic_Series_Filtred mf.

"""


def on_connect(client, userdata, flags, rc):
    client.subscribe(Topic_Status_Python)
    client.subscribe(Topic_Series_Filtred)
    client.publish(Topic_Status_ASPNet, "Online", qos=1, retain=True)
    print("Connected with result code "+str(rc))
    
ms = ""
def on_message(client, userdata, msg):
    print("=========  Start on_message ============================")
    
    if msg.topic == Topic_Series_Filtred:
        aList = json.loads(msg.payload.decode("utf-8"))
        """print """
        print(aList['len_rr'])
        
    # Switch case for the different topics
    
def on_disconnect(client, userdata, rc):
   
    client.publish(Topic_Status_ASPNet, "Offline", qos=1, retain=True)
    print("Disconnected with result code "+str(rc))
    
def on_subscribe(client, userdata, mid, granted_qos):
    print("Subscribed: "+str(mid)+" "+str(granted_qos))
    
    

def getdataAndSend():
    cwd = "C:/Users/madsn/Documents/BME/TELE/Repos/Project/Cssure/Python"
    
        # data = np.loadtxt(cwd+'/ecg_data_1.csv', skiprows=1,
        # delimiter=',')[:,:]
    data = np.loadtxt(cwd+'/ecg.csv', skiprows=1, delimiter=';')[:,:]
    data[:,0] = data[:,0]*0
    x_resampled = signal.resample(data[:,1], int(len(data[:,1])/2))
    data = np.c_[np.zeros(len(x_resampled)),x_resampled]
    data = data[1:10000,:]
    """
    Should send data in batches of varieng length to simulate
    real time data
    Its should increment in length of 12 samples each time and
    add them to a fifo queue, with max length of 3000 samples"""
    meta = dict()
    meta["PatientID"] = "1234567890"
    meta["Timestamp"] = str(datetime.datetime.now().timestamp())
    meta["SampleRate"] = 250
    meta["Data"] = []
    
    queue = list()
    for i in range(0, len(data), 12):
        queue.append(data[i:i+12])
        # Sleep 0.04 milliseconds
        # time.sleep(40/1000)
        meta["Timestamp"] = str(datetime.datetime.now().timestamp())
        
        # Remove first element if queue is longer than 252*3 samples (3 sec)
        secPrSection = 30
        if len(queue) > (250/12)*secPrSection:
            queue.pop(0)
        
        #Sent every 252 samples (1 sec)
        if (i>0 and i%int(252*1) == 0):
            queue2 = np.array(queue).reshape((-1,2))
            print("batch: "+ str(i)+ "     Samples " + str(len(queue*12)) + " in queue")
            meta["Data"] = queue2.tolist()
            client.publish(Topic_Series_Raw, json.dumps(meta))


    queue = list()
    meta["Timestamp"] = str(datetime.datetime.now().timestamp())
    data = np.c_[np.zeros(len(x_resampled)),x_resampled]
    queue= data
    queue2 = np.array(queue).reshape((-1,2))
    meta["Data"] = queue2.tolist()
    print("Sent all data")
    client.publish(Topic_Series_Raw, json.dumps(meta))


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

        
client.will_set(Topic_Status_ASPNet, payload="Offline", qos=2, retain=True)
client.connect("localhost", 1883, 60)

getdataAndSend()

# json_object = json.dumps(param, indent = 4) 
# client.publish("ECG/test", json_object)
# client.loop_forever()






