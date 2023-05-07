#%%
import paho.mqtt.client as mqtt
import numpy as np
from scipy import signal
import json
import matplotlib.pyplot as plt
import datetime
import time

"""
Dette dokument skal imiterer CSSURE og derved sende data til DataProcces.py over MQTT.
Data skal sendes over Topic_Series_Raw. 
Hvor dataet skal komme tilbage på Topic_Series_Filtred mf.

"""


def on_connect(client, userdata, flags, rc):
    client.subscribe(Topic_Status_Python)
    client.subscribe(Topic_Series_Filtred)
    client.publish(Topic_Status_CSSURE, "Online", qos=0, retain=True)
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
   
    client.publish(Topic_Status_CSSURE, "Offline", qos=1, retain=True)
    print("Disconnected with result code "+str(rc))
    
def on_subscribe(client, userdata, mid, granted_qos):
    print("Subscribed: "+str(mid)+" "+str(granted_qos))
    
    

def getdataAndSend():
    cwd = "C:/Users/madsn/Documents/BME/TELE/Repos/Project/Cssure/Python/DataProvider"
    
        # data = np.loadtxt(cwd+'/ecg_data_1.csv', skiprows=1,
        # delimiter=',')[:,:]
    data = np.loadtxt(cwd+'/ecg.csv', skiprows=1, delimiter=';')[:,:]
    data[:,0] = data[:,0]*0
    x_resampled = signal.resample(data[:,1], int(len(data[:,1])/2))
    data = np.c_[np.zeros(len(x_resampled)),x_resampled]
    # data = data[1:10000,:]
    """
    Should send data in batches of varieng length to simulate
    real time data
    Its should increment in length of 12 samples each time and
    add them to a fifo queue, with max length of 3000 samples"""
    meta = dict()
    meta["PatientID"] = "1234567890"
    meta["Timestamp"] = []
    meta["CSINormMax"] = [15.35,15.49,17.31]
    meta["ModCSINormMax"] = [9074,8485,8719]
    meta["Ch1"] = []
    meta["Ch2"] = []
    meta["Ch3"] = []
    
    queue = list()
    queueTime = list()
    for i in range(0, len(data), 12):
        # Sleep (1/250)*12 = 0.048 seconds
        time.sleep(0.048)
        
        queue.append(data[i:i+12,1])
        queueTime.append(datetime.datetime.now().timestamp())
        # Remove first element if queue is longer than 3 min ~= (250sample/s*180s)/12 = 3750 batches
        secPrBatch = 3750
        if len(queue) > secPrBatch:
            queue.pop(0)
            queueTime.pop(0)
        
        #Sent every 252 samples (1 sec)
        if (i>30*60 and i%int(252*1) == 0):
            queue2 = np.array(queue).reshape((-1,12))
            meta["Timestamp"] = queueTime
            print("batch: "+ str(len(queue))+ "     Samples " + str(len(queue*12)) + " in queue")
            meta["Ch1"] = queue2.tolist()
            meta["Ch2"] = queue2.tolist()
            meta["Ch3"] = queue2.tolist()
            client.publish(Topic_Series_Raw, json.dumps(meta))


    # queue = list()
    # meta["Timestamp"] = str(datetime.datetime.now().timestamp())
    # data = np.c_[np.zeros(len(x_resampled)),x_resampled]
    # queue= data
    # queue2 = np.array(queue).reshape((-1,2))
    # meta["Data"] = queue2.tolist()
    # print("Sent all data")
    # client.publish(Topic_Series_Raw, json.dumps(meta))


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
Topic_Status_CSSURE = "ECG/Status/CSSURE";
Topic_Status_Python = "ECG/Status/Python";

Topic_Series_Raw = "ECG/Series/CSSURE2PYTHON";
Topic_Series_Filtred = "ECG/Series/PYTHON2CSSURE";


Topic_Result = "ECG/Result/#";
Topic_Result_CSI = "ECG/Result/CSI";
Topic_Result_ModCSI = "ECG/Result/ModCSI";
Topic_Reuslt_RR = "ECG/Result/RR-Peak";
#endregion

        
client.will_set(Topic_Status_CSSURE, payload="Offline", qos=0, retain=True)
client.connect(host="localhost", port=1883, keepalive=60)

getdataAndSend()

client.disconnect()

# json_object = json.dumps(param, indent = 4) 
# client.publish("ECG/test", json_object)
# client.loop_forever()






