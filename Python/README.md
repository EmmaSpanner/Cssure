""" DataProcces.py
    This script is created to calculate the csi and modified csi parametres from the ecg data
    
    This script is created by the group of master students at from Electrical and Computer Engineering, at Aarhus University 2023
    
    This script can be runned directly from the terminal with the following command: python DataProcces.py
    or can be deployed in a docker container

"""

""" Run the script locally:
    To run the script an eviroment with the following libraries is needed:
        - paho-mqtt
        - numpy
        - numpy-base
        - numpydoc
        - jsonschema
        - python-fastjsonschema
        - python-lsp-jsonrpc
        - ujson
        - python-dateutil
        - matplotlib
        - matplotlib-base
        - matplotlib-inline
        - scikit-learn
        - pip:
            - hrv-analysis
            - scipy
    
    The environment can automaticly be created with anaconda
    1. Step is to install anaconda from https://www.anaconda.com/products/individual
    2. Open anaconda prompt (terminal) and navigate to the folder where the env.yml file is located
    3. Create the environment with the following command: conda env create -f env.yml
    4. Activate the environment with the following command: conda activate Temo
    5. Run the script with the following command: python DataProcces.py


"""

""" Deploy in a docker container:
    To deploy the script in a docker container the following steps is needed:
    1. Install docker from https://www.docker.com/products/docker-desktop
    2. Open a terminal and navigate to the folder where the Dockerfile is located
    3. Build the docker image with the following command: docker build -t csi_calculator .
    4. Run the docker image with the following command: docker run -it csi_calculator
"""

"""Explain the code as Pseudocode:
    The code is divided into 7 steps:
    
    Step 1: Get the data
        Get raw ecg data from the broker
        Data will be provided form the ASP.Net server on the topic "ECG/Series/Raw"
        It will be a json string with the following format, as described further down
    
    Step 2: Rearrange the data
        It needs to be a continous signal
    
    Step 3: QRSDetector
        Detect the QRS peaks   
    
    Step 4: RR-interval + CSI and ModCSI
        4.1 Then calculate the RR intervals
        4.2 Then calculate essensial parameters
                CSI:
                    There is calculated 4 types of CSI parametres; CSI (Hole signal lenght), CSI30, CSI50 and CSI100
                    The nummer reffrens to how many RR intervals that is used to calculate the parametres
                Modified CSI parametres:
                    There is calculated 4 types of modified CSI parametres; ModCSI (Hole signal lenght), ModCSI30, ModCSI50 and ModCSI100
                MeanHR: Mean heart rate
    
    
    Step 5: Decession support
        The decission support is based on the CSI parametres
        The decission support is based on the modified CSI parametres
    
    Step 6: Rearrange the data
        Rearange the data from step 4 and 5
    
    Step 7: Publish the data
        Publish the data to the broker
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