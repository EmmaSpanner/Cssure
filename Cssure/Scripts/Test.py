#%%

import numpy as np
#%%

import numpy as np
import matplotlib.pyplot as plt
import mne
import os #Til udtræk af path
import pandas as pd

directory = os.getcwd()
datafolder = "Data"
datafilename = "C:\\Users\\madsn\\Documents\\BME\\MAL\\exercises\\Data\\CreditCard.csv"

#Join the path
filePath = os.path.join(directory, datafolder, datafilename)

allData = np.genfromtxt(datafilename,names=True,delimiter=",",dtype=None,encoding=None)


#% 
# Seperate data into x and y
# Extract column 'card' and use it as 'y'
# (rows with 'yes' are changed to 1, rows with 'no' are changed to 0).
y = allData['card']
y = np.where(y == '"yes"', 1, 0)



# Replace yes and no with 1 and 0 in x
x = allData[['reports','age','income','share','expenditure','owner','selfemp','dependents','months','majorcards','active']]

# x['owner'] = allData['owner'] 

# replase yes and no with 1 and 0 in x
x['owner'] = np.where(x['owner']  == '"yes"', 1, 0)
x['selfemp'] = np.where(x['selfemp']  == '"yes"', 1, 0)


from tabulate import tabulate
data = x
headers = x.dtype.names
print (tabulate(data[0:50], headers=headers))