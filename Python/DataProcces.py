"""
This script is created to calculate the csi and modified csi parametres from the ecg data

"""

"""Psuedo code:

Step 1: Get the data
    Get raw ecg data from the broker
    Data will be provided form the ASP.Net server on the topic "ECG/Series/Raw"
    It will be a json string with the following format, as described further down

Step 2: Rearage the data
    It needs to be a continous signal

Step 3: Calculate the parametres 
    Calculate the essential parametres
    3.1 First step is to detect the QRS peaks
    3.2 Then calculate the RR intervals
    3.3 Then calculate essensial parameters
            CSI:
                There is calculated 4 types of CSI parametres; CSI (Hole signal lenght), CSI30, CSI50 and CSI100
                The nummer reffrens to how many RR intervals that is used to calculate the parametres
            Modified CSI parametres:
                There is calculated 4 types of modified CSI parametres; ModCSI (Hole signal lenght), ModCSI30, ModCSI50 and ModCSI100
            MeanHR: Mean heart rate

Step 4: Decession support
    The decission support is based on the CSI parametres
    The decission support is based on the modified CSI parametres

Step 5: Rearange the data
    Rearange the data to ????

Step 6: Publish the data
    Publish the data to the broker on the topic "ECG/Series/Filtred"
    It will be a json string with the following format, as described further down


Data streams:
The data will have a sample rate of 250 Hz
There is needed 100 RR peak to calculate the parametres
Assummed that the pulse is be between 40 and 200 bpm (40/60 = 0.67 Hz and 200/60 = 3.33 Hz)
    For pulse 40 bpm it will require 250/0.67 = 373 samples to measure the beat
    For CSI100: 100*373 = 37300 samples long to measure the beat (37300/250 = 149.2 s = 2.5 min)
    For CSI30: 30*373 = 11190 samples long to measure the beat (11190/250 = 44.76 s)
    
    For pulse 200 bpm it will require 250/3.33 = 75 samples to measure the beat
    For CSI100: 100*75 = 7500 samples long to measure the beat (7500/250 = 30 s)
    For CSI30: 30*75 = 2250 samples long to measure the beat (2250/250 = 9 s)
    
One channel is a array of arrays, where the inner array is 12 samples long (12/250 = 0.048 s = 48 ms)
The outer array is 3 min long (250*60*3 = 45000 samples long) and contains 45000/12 = 3750 inner arrays

The data will be sent every minute (60 s)


"""
"""Incomming data format:
    {
        "PatientID": "1",
        "Ch1": [
            [0,1,2,3,4,5,6,7,8,9,10,11],
            [0,1,2,3,4,5,6,7,8,9,10,11],
            [0,1,2,3,4,5,6,7,8,9,10,11],
            ...
            ]
        "Ch2": [
            [0,1,2,3,4,5,6,7,8,9,10,11],
            [0,1,2,3,4,5,6,7,8,9,10,11],
            ...
            ] 
        "Ch3": [
            [0,1,2,3,4,5,6,7,8,9,10,11],
            [0,1,2,3,4,5,6,7,8,9,10,11],
            ...
            ]
        "Timestamps": [
            Timestamp1,
            Timestamp2,
            Timestamp3,
            ...
            ]  
    }
"""
"""Returning data format (ISH...):
    {
        "PatientID": "1",
        "Timestamp": Timestamp, #Timestamp for the last batch of data
        "TimeProcess_s": TimeProcess_s,
        "EssentialParametres": {
            "RRTotal": RRTotal
            "RRIntervals": RRIntervals
            "MeanHR": MeanHR,
            "CSI": CSI,
            "CSI30": CSI30,
            "CSI50": CSI50,
            "CSI100": CSI100,
            "ModCSI": ModCSI,
            "ModCSI30": ModCSI30,
            "ModCSI50": ModCSI50,
            "ModCSI100": ModCSI100,
            }
        
    }


"""

#%% Import libraries

#region Import libraries
import paho.mqtt.client as mqtt #pip install paho-mqtt
from QRSDetector import QRSDetector #pip install QRSDetector
from hrvanalysis import  * #pip install hrvanalysis

import numpy as np #pip install numpy
import json #pip install json
import datetime  #pip install datetime

#endregion

#region Step 0: Setup the MQTT client

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
    messageEncoded = msg.payload
    message = messageEncoded.decode("utf-8")

    # Switch case for the different topics
    if msg.topic == Topic_Status_CSSURE: # ASP.Net status
        print("ASP.Net status: " + str(message))
        """
        Add some logic here to handle if the ASP.Net status gets offline
        """
    
    elif msg.topic == Topic_Series_Raw: #Handle the raw data
        ProcessingAlgorihtm(message)
    else:
        print("Unknown topic: " + msg.topic+ "\n\t"+str(message))
        
    # print("=========  END on_message ============================")

# When the client disconnects from the broker    
def on_disconnect(client, userdata, rc):
    # client.publish(Topic_Status_Python+"/disconnect", "Disconnected", qos=1, retain=True)
    print("Disconnected with result code "+str(rc))

# When the client subscribes to a topic
def on_subscribe(client, userdata, mid, granted_qos):
    print("Subscribed: "+str(mid)+" QOS:"+str(granted_qos))

#endregion

#region Step 1: Get the data
""" To get the data this project will use MQTT protocol
    The data will be provided from the ASP.Net server on the topic "ECG/Series/Raw"
    It will be a json string thats need to be decoded
"""

#Deserialize the json string
def DeserializeJson(ms):
    # Ready for the real data type, but it requires some changes to match the new dataformat
    ecgObject = json.loads(ms)

    return ecgObject


#endregion

#region Step 2: Rearage the data

def RearangeData(ecgdata):
    # incomming data format:
    # ecgdata = {
    #     "PatientID": "1",
    #     "Ch1": [
    #         [0,1,2,3,4,5,6,7,8,9,10,11],
    #         [0,1,2,3,4,5,6,7,8,9,10,11],
    #         [0,1,2,3,4,5,6,7,8,9,10,11],
    #         ...
    #         ]
    #     "Ch2": [
    #         [0,1,2,3,4,5,6,7,8,9,10,11],
    #         [0,1,2,3,4,5,6,7,8,9,10,11],
    #        ...]
    #     "Ch3": [[]],
    #     "Timestamps": [Timestamp1, Timestamp2, Timestamp3, ...]
    #       }

    #So every channel needs to be fatteend out to a 1D array
    # and the timestamps needs to be extracted to create a same length array for the timestamps
    
    ch1 = np.array(ecgdata['Ch1']).flatten()
    ch2 = np.array(ecgdata['Ch2']).flatten()
    ch3 = np.array(ecgdata['Ch3']).flatten()
    timestamp_mock = np.zeros(len(ch1))
    
    # add the timestampmock to each channel as a new row
    ch1 = np.c_[timestamp_mock,ch1] 
    ch2 = np.c_[timestamp_mock,ch2]
    ch3 = np.c_[timestamp_mock,ch3]
    
    return ch1, ch2, ch3

#endregion

#region Step 3: Calculate the parametres

def TimeDiffer(ecgObject):
    # Calculate the time it took to get and process the data
    t1 = ecgObject['Timestamp'][-1]
    t2 = datetime.datetime.now().timestamp()
    tdif = t2-t1
    return tdif



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
            param['CSI30'] = inter30['csi']
            param['ModCSI30'] = inter30['Modified_csi']
        else: 
            param['CSI30'] =0
            param['ModCSI30'] = 0
        
        if len(rr_interval)>50:
            res = rr_interval[-50:]
            inter50 =  get_csi_cvi_features(res)
            param['CSI50'] = inter50['csi']
            param['ModCSI50'] = inter50['Modified_csi']
        else: 
            param['CSI50'] = 0
            param['ModCSI50'] = 0
        
        if len(rr_interval)>100:
            res = rr_interval[-100:]
            inter100 =  get_csi_cvi_features(res)
            param['CSI100'] = inter100['csi']
            param['ModCSI100'] = inter100['Modified_csi']
        else: 
            param['CSI100'] = 0
            param['ModCSI100'] = 0
        
        # Calculate the non-linear domain parametres
        # CSI, Modified CSI, etc. for the full RR interval signal
        NonLineardomainfeatures = get_csi_cvi_features(rr_interval)
        param['csi'] = NonLineardomainfeatures['csi']
        param['Modified_csi'] = NonLineardomainfeatures['Modified_csi']
        param.update(NonLineardomainfeatures)
        
    # Add the RR intervals and the filtered ecg to the dictionary
    # param['rr_intervals_ms'] = rr_interval
    # param['filtered_ecg'] = qrs_detector.filtered_ecg_measurements.tolist()
    
    
    return param


#endregion

#region Step 4: Decission support
def DecissionSupport(CSINormMax,ModCSINormMax,ch):
    """ Due to the reshearsh of Jesper Jeppesen
    
    CSI30 will be 1.65 times over the normal value
    CSI50 will be 2.15 times over the normal value
    CSI100 will be 1.57 times over the normal value
    ModCSI30 is not used
    ModCSI50 is not used
    ModCSI100 will be 1.80 times over the normal value 
    """
    if ch["CSI30"] / CSINormMax[0] > 1.65:
        ch["CSI30_Alarm"] = "Seizure"
    else:
        ch["CSI30_Alarm"] = "No seizure"
    
    if ch["CSI50"] / CSINormMax[1] > 2.15:
        ch["CSI50_Alarm"] = "Seizure"
    else:
        ch["CSI50_Alarm"] = "No seizure"
        
    if ch["CSI100"] / CSINormMax[2] > 1.57:
        ch["CSI100_Alarm"] = "Seizure"
    else:
        ch["CSI100_Alarm"] = "No seizure"
    
    if ch["ModCSI100"] / ModCSINormMax[2] > 1.80:
        ch["ModCSI100_Alarm"] = "Seizure"
    else:
        ch["ModCSI100_Alarm"] = "No seizure"
        
    return ch
    
#endregion

#region Step 5: Rearange the data

def RearangeDataBack(ecgObject, Findings_ch1, Findings_ch2, Findings_ch3, timeDifferent):
    allParametres = dict()
        
    allParametres["PatientID"] = ecgObject["PatientID"]
    allParametres["Timestamp"] = ecgObject["Timestamp"][-1]
    allParametres["TimeProcess_s"] = timeDifferent
    allParametres["SeriesLength_s"] = len(ecgObject["Timestamp"])*12/250
    allParametres["Ch1"] = Findings_ch1
    allParametres["Ch2"] = Findings_ch2
    allParametres["Ch3"] = Findings_ch3
    return allParametres

#endregion

#region Step 6: Publish the data
def EncodeJson(dict):
    
    json_object = json.dumps(dict, indent = 5) 
    return json_object

def PublishData(json_object):
    try:
        client.publish(Topic_Series_Filtred, json_object)
    except: # If the data is not in the correct format
        client.publish(Topic_Series_Filtred, "Error", qos=1, retain=True)
#endregion

#region Call the functions
def ProcessingAlgorihtm(message):
    try:
        ecgObject = DeserializeJson(message)
        ch1, ch2, ch3 = RearangeData(ecgObject)
        timeDifferent = TimeDiffer(ecgObject)
        if timeDifferent>10:
            client.publish(Topic_Series_Filtred, "TimeError", qos=1, retain=True)
        else:
            Findings_ch1 = CalcParametres(ch1)
            Findings_ch2 = CalcParametres(ch2)
            Findings_ch3 = CalcParametres(ch3)
            
            Findings_ch1 = DecissionSupport(ecgObject['CSINormMax'],ecgObject['ModCSINormMax'],Findings_ch1)
            
            
            allParametres = RearangeDataBack(ecgObject,Findings_ch1, Findings_ch2, Findings_ch3, timeDifferent)
            json_object = EncodeJson(allParametres)
            PublishData(json_object)
    except:
        client.publish(Topic_Series_Filtred, "Error", qos=1, retain=True)

#endregion




"""Running code"""
#region Set up and run the MQTT client
# When the client connects to the broker
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message
client.on_disconnect = on_disconnect
client.on_subscribe = on_subscribe




# Create the last will message
client.will_set(Topic_Status_Python, payload="Offline", qos=0, retain=True)
client.connect(host="localhost", port=1883, keepalive=120)


# Will run the client forever, unless you interrupt it
client.loop_forever()

#endregion
