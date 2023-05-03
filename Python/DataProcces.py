"""
This script is created to calculate the csi and modified csi parametres from the ecg data
As a start it will only calculate the CSI and modCSI parametres, the script will return this parametres to the broker.
The process time on this iteration is 0.1 sec for 30 sec of data.


The vision is to use machine learning to detect a siezure atack based on the ech data and not only the head coded CSI-threshole.


To run this create a data-provider that sends data to the broker on the topic "ECG/Series/Raw" with the following json format: 
{
    "PatientID": "1",
    "Timestamp": "1234567890",
    "SampleRate": 250,
    "Data": [[0,0],[0,0],[0,0],[0,0],[0,0],[0,0]] 
}

where Data is a list of lists with the following format, where the timestamp can be replaced with a zero array:
[
    [timestamp, voltage],
    ...
    [timestamp, voltage]
]



"""
#%% Import libraries
import paho.mqtt.client as mqtt #pip install paho-mqtt
from QRSDetector import QRSDetector #pip install QRSDetector
from hrvanalysis import  * #pip install hrvanalysis

import numpy as np #pip install numpy
import json #pip install json
import datetime  #pip install datetime



# will subscribe to the following topics on the broker
# and publish the last will message on disconnect
def on_connect(client, userdata, flags, rc):
    client.subscribe(Topic_Status)
    client.subscribe(Topic_Series_Raw)
    client.publish(Topic_Status_Python, "Online", qos=0, retain=True)
    print("Connected with result code "+str(rc))

# When a message is received, this function is called
def on_message(client, userdata, msg):

    # Decode the message payload and check the topic
    message = msg.payload
    ms = message.decode("utf-8")

    # Switch case for the different topics
    if msg.topic == Topic_Status_CSSURE: # ASP.Net status
        print("ASP.Net status: " + str(message.decode("utf-8")))
        """
        Add some logic here to handle if the ASP.Net status gets offline
        """
    
    elif msg.topic == Topic_Series_Raw: #Handle the raw data
        
        try: 
            #Deserialize the json string
            # Ready for the real data type, but it requires some changes to match the new dataformat
            ecgObject = json.loads(ms)
            
            # Extract the ecg data and the patient id
            ecgdata = np.array(ecgObject['Data'])
            param = dict()
            param['PatientId'] = ecgObject['PatientID']
            param['Timestamp'] = ecgObject['Timestamp']
            param['TimeProcess_s'] = 0
            
            # Calculate the parametres from the ecg data
            # ecgdata should be a numpy array with shape (n,2)
            # ecgdata = [timestamp, voltage]
            # timestamp can be replaced with a zero array
            # Sample frequency should be ~250 Hz
            param.update(CalcParametres(ecgdata))
            
            # Calculate the time it took to get and process the data
            t1 = float(ecgObject['Timestamp'])
            t2 = datetime.datetime.now().timestamp()
            tdif = t2-t1
            param['TimeProcess_s'] = str(tdif)

            # Publish the parametres to the broker
            json_object = json.dumps(param, indent = 5) 
            client.publish(Topic_Series_Filtred, json_object)

        except: # If the data is not in the correct format
            client.publish(Topic_Series_Filtred, "Error", qos=1, retain=True)

    else:
        print("Unknown topic: " + msg.topic+ "\n\t"+str(message.decode("utf-8")))
        
    # print("=========  END on_message ============================")


# When the client disconnects from the broker    
def on_disconnect(client, userdata, rc):
    # client.publish(Topic_Status_Python+"/disconnect", "Disconnected", qos=1, retain=True)
    print("Disconnected with result code "+str(rc))


# When the client subscribes to a topic
def on_subscribe(client, userdata, mid, granted_qos):
    print("Subscribed: "+str(mid)+" QOS:"+str(granted_qos))

# Calculate the parametres from the ecg data
def CalcParametres(data):

    # Will return a dictionary with the parametres calculated from the ecg data
    # This is QRS detection    
    qrs_detector = QRSDetector(
        data,
        plot_data=False, 
        show_plot=True)

    # Extract the qrs peaks and the peak values
    peakIdx = qrs_detector.qrs_peaks_indices
    peakval = qrs_detector.qrs_peak_value
    
    # Calculate the RR intervals in ms
    rr_interval = []
    for idx in np.arange(len(peakIdx)-1):
        rr_interval.append(((peakIdx[idx+1] - peakIdx[idx])/250)*1000)
    
    # Create a dictionary with the parametres to return
    param = dict()
    param['len_rr'] = len(rr_interval)
    
    # There should be at least 5 RR intervals to calculate the parametres
    if len(rr_interval)>4:
        
        # Calculate the time domain parametres
        # HR Mean, HR Std, etc.
        time_domain_features_all = get_time_domain_features(rr_interval)
        param['mean_hr'] = time_domain_features_all['mean_hr']
        
        # According to Jespers Jeppesens studie we need to calculate the csi 30, 50 and 100, and the modified csi 100 which needes 30, 50 and 100 RR intervals to be calculated
        # https://www.researchgate.net/publication/270658448_Using_Lorenz_plot_and_Cardiac_Sympathetic_Index_of_heart_rate_variability_for_detecting_seizures_for_patients_with_epilepsy
        
        # find the csi 30, 50 and 100
        if len(rr_interval)>30:
            res = rr_interval[-30:]
            inter30 =  get_csi_cvi_features(res)
            param['csi_30'] = inter30['csi']
        else: param['csi_30'] =0
        
        if len(rr_interval)>50:
            res = rr_interval[-50:]
            inter50 =  get_csi_cvi_features(res)
            param['csi_50'] = inter50['csi']
        else: param['csi_50'] = 0
        
        if len(rr_interval)>100:
            res = rr_interval[-100:]
            inter100 =  get_csi_cvi_features(res)
            param['csi_100'] = inter100['csi']
            param['Modified_csi_100'] = inter100['Modified_csi']
        else: 
            param['csi_100'] = 0
            param['Modified_csi_100'] = 0
        
        # Calculate the non-linear domain parametres
        # CSI, Modified CSI, etc. for the full RR interval signal
        NonLineardomainfeatures = get_csi_cvi_features(rr_interval)
        param['csi'] = NonLineardomainfeatures['csi']
        param['Modified_csi'] = NonLineardomainfeatures['Modified_csi']
        param.update(NonLineardomainfeatures)
        
    # Add the RR intervals and the filtered ecg to the dictionary
    param['rr_intervals_ms'] = rr_interval
    param['filtered_ecg'] = qrs_detector.filtered_ecg_measurements.tolist()
    
    
    return param



# When the client connects to the broker
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message
client.on_disconnect = on_disconnect
client.on_subscribe = on_subscribe


#region Topics
Topic_Status = "ECG/Status/#";
Topic_Status_CSSURE = "ECG/Status/CSSURE";
Topic_Status_Python = "ECG/Status/Python";
Topic_Status_Python_Disconnect = "ECG/Status/Python/Disconnect";

Topic_Series_Raw = "ECG/Series/CSSURE2PYTHON";
Topic_Series_Filtred = "ECG/Series/PYTHON2CSSURE";

Topic_Result = "ECG/Result/#";
Topic_Result_CSI = "ECG/Result/CSI";
Topic_Result_ModCSI = "ECG/Result/ModCSI";
Topic_Reuslt_RR = "ECG/Result/RR-Peak";
#endregion


# Create the last will message
client.will_set(Topic_Status_Python, payload="Offline", qos=0, retain=True)
client.connect(host="localhost", port=1883, keepalive=120)


# Will run the client forever, unless you interrupt it
client.loop_forever()


