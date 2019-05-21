# OmnikApi
Omnik Solar API in C#

I have done some research into the protocol used by Omnik and have created an API in C# on the Microsoft .NET 4.5.2 & 4.7.2 Frameworks which can be used to query and parse the statistics from the Omnik device. Thanks to several great resources online and some investigation myself I managed to create the most rich and full open source API yet available on the internet for the Omnik Solar Inverters. Find the source code in this Github repository. The code is overly commented inline and contains a working sample console application, so for someone with some Microsoft .NET development skills it should be very easy to understand and implement in your own software. It supports both pulling data out an Omnik (actively connecting to it and requesting its statistics) as well as having the Omnik push data to us at an approximate 5 minute interval.

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

Version 1.0.3.1 - May 21, 2019

- Also added support for .NET Standard 2.0

Version 1.0.3.0 - May 6, 2019

- Converted to a multi targeted framework making it available to .NET Framework 4.5.2 and .NET Framework 4.7.2

1.0.2.0 - August 18, 2017

- Compiled back against .NET Framework 4.5.2 as that is still in support and I received messages from people still on Windows 7 RTM who could otherwise not use it anymore

1.0.1.0 - August 16, 2017

- Modified the TestConsole code to output to the console the full RAW data received. Also if a pull fails, it will show why it failed. If the code crashes for you, send me this raw output so I can analyze if i.e. the protocol has changed in your version of the Omnik solar inverter.

1.0.0.1 - August 16, 2017

- The library and TestConsole are now compliled against the Microsoft .NET 4.6.2 framework as the 4.5 framework was no longer supported

## Troubleshooting

In case you can't get the code to work, feel free to reach out. I'll be happy to help if I can. If the output does show you the "Incoming data from pull action", send along a copy of the exact output provided there. Some scenarios I hear about more often:

### Code crashes during execution

- Ensure you've entered the proper values in the App.config. Make sure you use the serial number of the WiFi module in your Omnik. This is NOT the same as the serial number of the Omnik Solar Inverter itself. Keep pressing the white button on your Omnik Solar Inverter until it shows S/N: on the display. This is the number that you'll need to use as the serial number.

## Feedback

Feel free to use it to suit your needs. If you believe something could be improved or need help, feel free to let me know.

Koen Zomers
mail@koenzomers.nl
