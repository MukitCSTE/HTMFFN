## What is your experiment about
### Experiment : ML20/21-5.2. Implement HTM FeedForward network
* Project Paper: [Link](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/tree/MiniColumns/Source/MyProject/Documentation)
* Project Link: [Link](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2020-2021/tree/MiniColumns/Source/MyProject/ML2021-5.2%20HTM%20FeedForward%20network)
The HTM FeedForward Network has a multilayer architecture. Essentially, we attempted to construct a synapse network and investigate its learning behavior between lower layer L4 and upper layer L2, much like the human brain.

## Summary

To implement the entire project in the Azure cloud environment, we actually take the following steps:

### STEP 1: Use an Azure Function as a Scalable Queue Message Receiver Handler.
As we have to deal with a lot of variables and messages in this project in order to conduct this experiment. So we created a Queue Message Receiver handler, which is an Azure function based on HTTP TRIGGER. We built this Azure function in such a way that it can handle any number of messages in a queue. Any TestUser or Professor can send any message to our Queue Message Receiver Handler at any time using any REST client App such as POST MAN, Talent Api, and so on. Here is the architecture diagram we use to handle large volumes of messages.

![image](https://user-images.githubusercontent.com/2386584/135710280-b5f1425e-8e89-4d29-8c93-ca0655dd7bd3.png)



* #### Messages POST Request Url
```
https://messagereceiver20210914015141.azurewebsites.net/api/Msg-receiver-function
```
![image](https://user-images.githubusercontent.com/2386584/135713526-625b24a2-e259-407b-9718-0f4aa57f91f5.png)



* #### An Example of a Message That We Follow To Initiate Our Code:
During each POST request, all messages must be sent under the messages json array of objects. On each  message under messages array, we only used the name of the configuration blob files. hich are pre  stored in a input container at Azure Blob storage. Currently, there are some configuration files there, but the user can upload any configuration json file with any name to blob storage .After that they only  have to pass the file name at the message parameters.

```
{
  "messages": [
    {
      "Common_config_blob": "Common_confiquration_01.json",
      "L4_config_blob": "L4_config_blob.json",
      "L2_config_blob": "L2_config_blob.json",
      "Sequence_blob": "Sequence_blob_01.json"
      
    },
    
     {
      "Common_config_blob": "Common_confiquration_01.json",
      "L4_config_blob": "L4_config_blob.json",
      "L2_config_blob": "L2_config_blob.json",
      "Sequence_blob": "Sequence_blob_01.json"
      
    },
    .......
    ....    //All subsequent messages in each POST request in a message queue must adhere to this model structure.
    ..
          
  ]
}

```

### 2. Blob Conatiner for Input Files:

Because we need to build layer L4, layer L2, training sequence files, and other common files to implement the HTM Feed Forward Network. So, in this section, we actually create all of the configuration files for L4,L2, Tarining Sequences, and Common configuration files in. json format and upload them to Azure Blob Storage as input containers, which will allow us to build the entire network dynamically at run time. The file's parameters can be changed by the user. However, the name of the parameters cannot be changed. Files for Confiquation The names of the parameters should be similar to the sample down there.


![image](https://user-images.githubusercontent.com/2386584/135716018-be9be3a9-a86c-4615-95ca-03f22a580e94.png)




* #### L4 Configuration .json File Sample

```
{
  "GlobalInhibition": true,
  "LocalAreaDensity": -1,
  "NumActiveColumnsPerInhArea": 0.02,
  "InhibitionRadius": 15,
  "DutyCyclePeriod": 25,
  "MaxSynapsesPerSegment": 0.02,
  "ActivationThreshold": 15,
  "ConnectedPermanence": 0.5,
  "PermanenceDecrement": 0.25,
  "PermanenceIncrement": 0.15,
  "PredictedSegmentDecrement": 0.1
}
```

* #### L2 Configuration .json File Sample

```
{
  "GlobalInhibition": true,
  "LocalAreaDensity": -1,
  "NumActiveColumnsPerInhArea": 0.1,
  "InhibitionRadius": 15,
  "DutyCyclePeriod": 25,
  "MaxSynapsesPerSegment": 0.05,
  "ActivationThreshold": 15,
  "ConnectedPermanence": 0.5,
  "PermanenceDecrement": 0.25,
  "PermanenceIncrement": 0.15,
  "PredictedSegmentDecrement": 0.1
}
```



* #### Training Sequence .json File Sample
```
{
  
    "input_sequence": [13,14,13,15,16,13,17,18,19,13]
}
```

* ####  Common confiquration .json File Sample
```
  {
  "expId": "EXP02",
  "cellsPerColumnL4": 20,
  "numColumnsL4": 500,
  "cellsPerColumnL2": 20,
  "numColumnsL2": 500,
  "inputBits": 100,
  "minOctOverlapCycles": 1,
  "maxBoost": 10,
  "max": 20, // value of max variable must have to be larger than any data in Training sequence
  "maxCycles": 2000,
  "msgQueeTraceId": "MSG02",
  "expDoneby": "Mukit"

}
```



### 2. Provide the URL of the Docker repository of your executable *MyProject* in the Azure Container Registry.
 
* #### Option1 - From Docker File : You can build image and register the remository at Docker container hub from our Docker File
1. Navicate to our project root directory from Terminal
  
  - C:\se-cloud-2020-2021\Source>cd MyCloudProjectSample

2. Building image from Docker file

  - C:\se-cloud-2020-2021\Source\MyCloudProjectSample>docker build -t feedforwardnet:v1.0 .
  - docker images ( to find the image id)

3. Execute Process
  - docker run <image_id>

![image](https://user-images.githubusercontent.com/2386584/135739921-68a88bec-eefd-40ca-bbb9-1f450a5f9c00.png)


* #### Option 2 - Publishing at Azure Container Registry:  

Whole project is also published to the Azure caónatiner registry as well. But to run this project from azure platform  Azure Containr Instnace(ACI) is needed. ACI can be created both in from GUI or CMD


-Docker Repository Url :

```
docker pull feedforwardnetcontainer.azurecr.io/mycloudproject:latest

```
-Deployment to azure container registry:

![image](https://user-images.githubusercontent.com/2386584/135742118-87e85e79-6e7e-4eda-a20c-5cbb33e2496f.png)



![image](https://user-images.githubusercontent.com/2386584/135742097-f83eebae-5be2-4390-aeb0-23763134f4da.png)






### 3. Provide the sample JSON message that we (me) need to sent to the queue to run your code.

- **Message model**
```
{
  "messages": [
    {
      "Common_config_blob": "confiquration json file name will have to place here", // please look at Guide Line 2
      "L4_config_blob": "configuartion json file name will have to place here from azure input container",
      "L2_config_blob": "configuration json file name will have to place here from azure input container",
      "Sequence_blob": "confiquation json file name will have to place here from azure input container"
      
    }
  ]
}

```
- 	**Guide Line to Professor to Start**
1. Peofessor will  send either one or multiple message under messages , which is a json array object in a  message queue on each request via http POST at the following url
   https://messagereceiver20210914015141.azurewebsites.net/api/Msg-receiver-function
2. For configuration json files name in the message ; we have uoload some jsom blob files at the azure input caontainer. Professor can test with the default file name there. Or
   it is also poosible to make any custom configuration files and uploading them to the input container



### 4. Describe the Experiment Result Output Table 

```

| Result        	| Description 	                                           
|---------------	|-----------------------------------------------------------|
| Partition Key 	| Id to Partion a group of Messages result          	    |  
| Row Key        	| Id to identify each messages result          	            | 
| Timestamp     	| Timestamp when experiment starts in UTC.                  |  
| Experiment_ID 	| Id number of Experiment           	                    |        
| GroupName     	| Group name of the project group            	            |        
| Exp_Done_By     	| Name of the person who did the experiment                 |        
| Message_id    	| Indicate a chunk of queue messages that POST at a time    |        
| Sequence_id   	| Queue Message sequence id            	                    |     
| Start_TimeUtc    	| End time of experiment in UTC             	            | 
| Status        	| Boolean True If experiment executed otherwise False       | 
| Accuracy      	| Show the leaning accuracy percentage            	    | 
| Total_Match_repeat    | Total match repeat with 100% accuracy             	    | 
| EndTimeUtc    	| End time of experiment in UTC             	            |              
| Duration_Sec   	| Execution time in second              	            |        
| OutputFileName  	| Experiment Out blob file name at output container         |        
| Exp_result_log	| Hold Experiment Final log message          	                    

```

![image](https://user-images.githubusercontent.com/2386584/135718642-e33e2a95-c95d-42be-8d36-b0ee9a3bbe11.png)


### 5. Experiment Result OutPut Container

Whenever a sequence learning experiment based on a message has completed, the entire experiment result log file is saved than to an output container as a .txt blob file object using azure blob storage service. The Result Output table will also hold the file name of experimented result.

![image](https://user-images.githubusercontent.com/2386584/135723125-efab5120-5e44-4911-a6f1-c63e7c0a1478.png)

