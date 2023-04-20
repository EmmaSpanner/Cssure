CSSURE is web api, server and backend, what we call a kinderæg.


To launch this project
	When using this project you should change the IP-address in launchSetting.json in the properties folder to your IP-address.
	
	Its the applicationUrl in line 17 that contains the IP-addresses that should be changed. 
	
	First make sure the Client (ASSURE) and server (CSSURE) is connected to the same hotspot.
	
	To find the IP-address of the hotspot:
	- Open "Kommandoprompt"
	- Type "ipconfig"
	- Find the IPv4 Address under "Wireless LAN adapter Wi-Fi"
	

To launch the MQTT-Broker
	This project requires the use of an MQTT broker running on the same device. 
	One option for this is to install the Mosquitto broker, which can be done as follows:
	
	1. Configure your firewall settings:
	1.1 Go to "Windows Defender Firewall with Advanced Security"
	1.2 Add a new "Inbound rule"
	1.3 Select the predefined "File and Printer Sharing" option and check the "File and Printer Sharing (Echo Request - ICMPv4-In) - with "Domain" as profile.
	
	2. Install Mosquitto (Broker):
		2.1 Download the latest version of Mosquitto from https://mosquitto.org/download/ (current version is 2.0.15).
		2.2 Navigate to the Mosquitto folder and open "mosquitto.conf" in VS Code as an administrator.
		2.3 Change line 234 from "#listener" to "listener 1883".
		2.4 Change line 532 from "#allow_anonymous false" to "allow_anonymous true".
		2.5 Save the changes and restart your computer.
		2.6 Open "Windows PowerShell" as administrator and run the command "net stop mosquitto", followed by "net start mosquitto".
	
	3. Verify that the Mosquitto broker is running:
	3.1 Download the latest version of MQTT Explorer from http://mqtt-explorer.com/ (current version is 0.4.0-beta).
	3.2 Open MQTT Explorer.
	3.3 Enter the following parameters:
		Name: "My local MQTT broker"
		Protocol: "mqtt://"
		Host: "localhost"
		Port: "1883"
	
	Once you have completed these steps, you should be able to use the MQTT broker with this project.
	

To launch the Python Data Processing
	The following text provides instructions for launching the Python environment for a project that requires Anaconda environment with paho.mqtt to calculate essential parameters to predict seizure attacks. 
	
	1. If you don't have Anaconda-navigator and or VS-code, follow the steps below:
		1.1 Download the latest version of Anaconda-navigator from https://www.anaconda.com/download/.
		1.2 Download the latest version of VS-Code from https://code.visualstudio.com/download.
		Go to step 2
	
	2. If you have the Anaconda-navigator and want the exact same environment as the developer (the safe solution), follow the steps below:
		2.1 Open the "Anaconda prompt" and navigate to the Python folder for the project, 
			for example, "C:\Users\madsn\Documents\BME\TELE\repos\Project\Cssure\Python".
		2.2 Run the command "conda env create --name CssureEnv -f environment.yml".
		2.3 Run the command "conda activate CssureEnv".
		Go to step 4.

		If an update in the environment should occur, complete the next few steps:
			2.5 Open the "Anaconda prompt" and navigate to the Python folder for the project, 
				for example, "C:\Users\madsn\Documents\BME\TELE\repos\Project\Cssure\Python".
			2.6 Run the command "conda env update --name CssureEnv --file environment.yml --prune".
		
	3. If you have the Anaconda-navigator and just want to add the must-required packaged (the unsafe solution), follow the steps below:
		3.1 Open the wanted environment.
		3.2 Install the following packaged: 
			"conda install -c conda-forge paho-mqtt"
		Go to step 4.
	
	4. Run the DataProcces.py
		4.1 Open the "Anaconda prompt" and navigate to the Python folder for the project, 
			for example, "C:\Users\madsn\Documents\BME\TELE\repos\Project\Cssure\Python".
		4.2 Run the command "conda activate CssureEnv".
		4.3 Rund the command "python DataProcces.py".
	
	Once you have completed these steps, you should be able to detect seizures with this project.
		
		
Enjoy receiving and processing raw data!