# Dlid.MiHome
C# library for communicating with Xiaomi devices using the MiHome UDP Binary Protocol [<sup>1</sup>](#1).

[![Develop Status](https://github.com/dlid/Dlid.MiHome/workflows/develop-build/badge.svg)](https://github.com/dlid/Dlid.MiHome/actions) [![Master Status](https://github.com/dlid/Dlid.MiHome/workflows/master-build/badge.svg)](https://github.com/dlid/Dlid.MiHome/actions)

The main goal of this library is to provide a rudimentary Request/Response interface to communicate with your Xiaomi devices. 

The library requires you to know the IP Address and the  Token [<sup>2</sup>](#2) of your device.

# Installation

This library is available as an unlisted Nuget package until a more stable release is available.

It can still be installed in your project by using the Package Manager Console and using following command where *?.?.?* is the  version you want.

    Install-Package Dlid.MiHome -Version ?.?.?
    
See https://www.nuget.org/packages/Dlid.MiHome/ for the latest version published.

# MiDevice

The `MiDevice` is the core class that will allow you to communicate with your Xiaomi device.

The goal with this class is to be a general way to send and receive data to and from the devices. For this reason this class will not contain any pre-defined methods for specific devices.

The class handles the following:

- UDP Socket connection to the device
- Handshake and management of Device ID and server timestamps
- Package encryption and decryption using the device token that you provide
- Handling retries of commands

## Request format

The MiHome Binary Protocol accepts JSON payloads  in the following format:

    {
        method: string,
        id: number;
        params: any[];
    }

The commands available per device is not something that I have investigated a lot yet. I've been trying things out toward my Vacuum cleaner using marcelrv's Xiaomi Robot Vacuum Protocol [<sup>3</sup>](#3).

- `method` is the name of the method you want to send to the device
- `id` should be an increasing number unique per request. This will be handled internally by the `MiDevice` class.
- `params` are parameters that specific methods may need.

## Your first command

    // Initiate using IP and token
    var device = new MiDevice("192.168.83.119", "TOKEN");

    // Send first command
    device.Send(new {
        method = "app_goto_target",
        params = [23100,22600]
    });

The object you send will be serialized to JSON, so make sure the character casing is correct. 

Sending an object like this will give you total control of the data you send in case you want to try things out. But since most commands are in the same format you can send the same command like this:

    device.Send("app_goto_target", 23100, 22600);

## Response

The Send method will return a `MiHomeResponse` object. This contains a boolean `Success` property and a string `ResponseText`.

- `ResponseText` will contain the decrypted JSON string returned from the device
- `Success` will be true if a response was successfully retreived from the device. It will be false if a response could not be received for any reason.

# MiVacuumDevice

(Draft)

Personally I have a Vacuum Robot, and that was the main reason for why I wanted to create this library.

This class will be a typed interface toward the Xiaomi Vacuum Robots. It will utilize the `MiDevice` core class in order to give a typed interface toward the Vacuum devices. I develop this class in parallel with the MiDevice so I get resolve any quirks or things that I've forgotten in the MiDevice class.

Note that both this and the MiDevice is in progress and may very well contain issues.

# Notes for future features 

- Support for Async invocations
- Look closer at error handling
- Typed device specific interfaces
- More Unit Tests and reports

# References and Links

1. Xiaomi Mi Home Binary Protocol. - <a class="anchor" id="1"></a> https://github.com/OpenMiHome/mihome-binary-protocol/blob/master/doc/PROTOCOL.md [2019-11-24]
2. Obtain Mi Home device token. - <a class="anchor" id="2"></a> https://github.com/jghaanstra/com.xiaomi-miio/blob/master/docs/obtain_token.md [2019-11-24]
3. Xiaomi Robot Vacuum Protocol. - <a class="anchor" id="3"></a> https://github.com/marcelrv/XiaomiRobotVacuumProtocol [2019-11-24]


# Version History

## 0.0.7 GetConsumables

- Implemented GetConsumables to get hours of consumables including calculated lifespan percentage
- Fixed bug with the Response `As<T>` method that did not check Success flag properly

## 0.0.6 First version

- Core MiDevice class is working and can communicate well with my Vacuum device

