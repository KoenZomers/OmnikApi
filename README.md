# OmnikApi
Omnik Solar API in C#

I have done some research into the protocol used by Omnik and have created an API in C# on the Microsoft .NET 4.62 framework which can be used to query and parse the statistics from the Omnik device. Thanks to several great resources online and some investigation myself I managed to create the most rich and full open source API yet available on the internet for the Omnik Solar Inverters. Find the source code in this Github repository. The code is overly commented inline and contains a working sample console application, so for someone with some Microsoft .NET development skills it should be very easy to understand and implement in your own software. It supports both pulling data out an Omnik (actively connecting to it and requesting its statistics) as well as having the Omnik push data to us at an approximate 5 minute interval.

More information and to see it live being used in action against my solar panels, go to https://www.zomers.eu/domotics/solarpanels

## Download

For a directly executable version of this tool, [download it here](https://github.com/KoenZomers/OmnikApi/raw/master/Downloads/KoenZomers.Omnik.TestConsole.zip)

You can edit the values in the appSettings section in the KoenZomers.Omnik.TestConsole.exe.config file to have it connect to your Omnik Solar inverter.

## Available via NuGet

You can also pull this API in as a NuGet package by adding "KoenZomers.Omnik.Api" or running:

Install-Package KoenZomers.Omnik.Api

Package statistics:
https://www.nuget.org/packages/KoenZomers.Omnik.Api

## Usage

To use this functionality directly from your own code, you can use it as follows.

Create a new instance of the Omnik Controller using:

```C#
KoenZomers.Omnik.Api.Controller controller = new KoenZomers.Omnik.Api.Controller();

```

Register for the OmnikStatisticsAvailable event which will be triggered regardless whether you do a push or a pull towards the Omnik:

```C#
controller.OmnikStatisticsAvailable += OmnikStatisticsAvailable;
```

The signature of the OmnikStatisticsAvailable method is:

```C#
void OmnikStatisticsAvailable(Statistics statistics)
```

### Receiving data from Omnik (Push)

If you want to wait for the Omnik Solar Converter to push its statistics to your code in approximately 5 minute intervals (non changeable), signal the controller instance to start listening for data from the Omnik Solar Inverter:

```C#
controller.StartListeners();
```

### Pulling data from Omnik (Pull)

Instead you can also actively connect to the Omnik Solar Inverter and pull its current statistics out. This can be done at any interval you'd like. To do this, use:

```C#
controller.PullData("ip address or dns name of your Omnik Solar Inverter", "Omnik WiFi Serial Number");
```

The WiFi Serial Number you can retrieve by keep pushing the white button on the front of the Omnik Solar inverter until the display reads S/N: \<number>. This is the number you'll use for "Omnik WiFi Serial Number".

## Version History

1.0.0.1 - August 16, 2017

- The library and TestConsole are now compliled against the Microsoft .NET 4.6.2 framework as the 4.5 framework was no longer supported

## Feedback

Feel free to use it to suit your needs. If you believe something could be improved or need help, feel free to let me know.

Koen Zomers
mail@koenzomers.nl
