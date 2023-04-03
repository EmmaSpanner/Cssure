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
#%% 
# Split data into test and training with 20% in test set.

# Import the train_test_split 
from sklearn.model_selection import train_test_split


# Split the data into training and test sets
split = 0.2
X_train, X_test, Y_train, Y_test = train_test_split(x, y, test_size=split, random_state=42)

print("X_train shape: ", X_train.shape)
print("Y_train shape: ", Y_train.shape)
print("X_test shape: ", X_test.shape)
print("Y_test shape: ", Y_test.shape)

#ndarray to dataframe
X_train = pd.DataFrame(X_train)
X_test = pd.DataFrame(X_test)
Y_train = pd.DataFrame(Y_train)
Y_test = pd.DataFrame(Y_test)

#%% KNN
# Import the KNeighborsClassifier

from sklearn.neighbors import KNeighborsClassifier
from sklearn.metrics import confusion_matrix
from sklearn.metrics import accuracy_score



# Cross validation to find the best k value for KNN classifier 
from sklearn.model_selection import cross_val_score
k_range = range(1, 30)
cv_scores = pd.DataFrame()
Accuracy_Train = []
Accuracy_Test = []


for k in k_range:
    knn = KNeighborsClassifier(n_neighbors=k)
    cv_score = cross_val_score(knn, X_train, Y_train, cv=10, scoring='accuracy')

    cv_scores[k] = np.append(cv_score , [cv_score.mean()])
    knn.fit(X_train, Y_train)

    #% predict and evaluate
    Accuracy_Train.append(accuracy_score(Y_train, knn.predict(X_train)))
    Accuracy_Test.append(accuracy_score(Y_test, knn.predict(X_test)))
    
    

#%%
# # Plot the accuracy for different k values
# fig = plt.figure(figsize=(10,6))
# plt.title('KNN: Varying Number of Neighbors')

# for k in k_range:
#     plt.plot(np.ones(len(cv_scores[k]))*k,cv_scores[k], 'o',color='blue', alpha=0.4, markersize=3)
    
# row = cv_scores.iloc[-1]
# plt.plot(k_range,row, '-', color='red' , label='Mean Accuracy for CV')
# plt.plot(k_range,Accuracy_Train, '-o', color='black', label='Train Accuracy')
# plt.plot(k_range,Accuracy_Test, '-o', color='green', label='Test Accuracy')


# plt.xlabel('Value of K for KNN')
# plt.ylabel('Cross-Validated Accuracy')
# plt.xticks(k_range)
# plt.grid()
# plt.legend()
# plt.show()

#%%
# Task 2: Træn en lineær klassifier 
"""
Træn en lineær klassifier på samme datasæt. Brug sklearn.linear_model.LogisticRegression.

Hvordan performer den?
"""

from sklearn.linear_model import LogisticRegression


Accuracy_Train = []
Accuracy_Test = []
iterRange = range(1, 100)
for i in iterRange:
    # link to documentation: https://scikit-learn.org/stable/modules/generated/sklearn.linear_model.LogisticRegression.html
    clf = LogisticRegression(random_state=0,max_iter=i).fit(X_train, Y_train)
    # Accuracy_Train.append((clf.score(X_train, Y_train)))
    # Accuracy_Test.append((clf.score(X_test, Y_test)))
    print("Iteration: ", i, " Train Accuracy: ", clf.score(X_train, Y_train), " Test Accuracy: ", clf.score(X_test, Y_test))
    
    
# fig = plt.figure(figsize=(10,6))
# plt.title('Logistic Regression: Varying Number of Iterations')

# plt.plot(iterRange,Accuracy_Train, '-o', color='black', label='Train Accuracy')
# plt.plot(iterRange,Accuracy_Test, '-o', color='green', label='Test Accuracy')

# # graph settings
# plt.legend(loc='lower right')
# plt.ylim(0.75, 1)
# plt.grid()
# plt.show()
