# Implement HTM FeedForward network

In this experiment we have developed L4-L2 HTM Feed Forward network with neocortex v1.0.15 and train the network with various sequence and finally observes the behaviour of output layer L2 using HTM Classifier

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See the notes down on how to deploy the project and experiment on a live system.

### Prerequisites


```
1) .NET Core 5.0
2) Neocortex Api V 1.0.15
```

### Installing

A step by step to install HTM Feed Forward Network in local Machine


 1) From git bash terminal clone branch using this command:
    ```
    git clone https://github.com/MukitCSTE/HTMFFN.git  
    
    ```
 
 2) Then visit to ..\ML2021-5.2 HTM FeedForward network from direcory where the branch is cloned 
 3) Click on ML2021-5.2 HTM FeedForward network.sln file to open up the project solution file in git
 5) Then got to FeedForwardNetExperiment.cs file
 6) Pass your desire input squence on inputValuesn to test Squence Learning with HTM FFN.
  ```
    Example:
    
    List<double> inputValues = new List<double>(new double[] { 2.0, 4.0, 2.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 7.0, 6.0, 10.0, 4.0, 12.0, 15.0, 14.0, 15.0, 16.0, 17.0, 14.0, 15.0, 17.0, 8.0, 10.0, 9.0, 12.0, 6.0, 7.0, 6.0, 7.0, 6.0, 7.0})
  ``` 
     
 7) Go to rogram cs file. Make an object forFeedForwardNetExperiment.cs 
 
  
          FeedForwardNetExperiment experiment = new FeedForwardNetExperiment();
          experiment.FeedForwardNetTest();
          
      
          
8) Run the experiment in debug mode form vs studio 
  



## Built With

* [DotnetCore](https://dotnet.microsoft.com/download/dotnet/5.0) - The framework used
* [NeoCortexApi](https://www.nuget.org/packages/NeoCortexApi/) - Dependency Management



## Authors

* **Md. Mukit Khan** - *Programmer* 
* **Chimerie Arun** - *Developer* 
* **Md. Rabiul Islam** - *SQA* 
* **Md. Nabuat AI Jahid** - *Developer* 
* **SM Mehedi** - *Azure cloud & IT expert* 

## License

This project is licensed under the GNU Plublic license

## Acknowledgments

* NUMENTA
* NeoCortexApi
* FUAS-SE-Cloud-2021 colleagues

