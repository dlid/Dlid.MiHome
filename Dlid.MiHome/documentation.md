# Dlid.MiHome

A simple C# library for communication with Xiaomi devices using the Xiaomo Mi Home Binary Protocol.

- Main goal is to provide a generic Binary Protocol Wrapper to send requests and receive responses to  the devices
- Secondary goal is to provide typed interfaces for specific devices (currently Vacuum Robot)

## MiDevice

The `MiDevice` is the core class that will communicate with the Xiaomi device. Using this class you can send raw JSON payloads to the device and receive the JSON response back.

This class is meant to be used by more specific device classes in the future.

### Connecting to a device

This library will require you to already know the **IP Adresss** and the **token** of your device.

See Links section if you need a guide for how to retreive your token. For me, installing the v5.4.54 version of the mobile app worked nicely.

    var device = new MiDevice("192.168.68.119", "TOKENSTRING");

This will create and prepare an instance for your device, but will not actually connect to it yet. 

The actual connection will take place the first time you send a command.

### Summary of the protocol

Before sending commands it can be useful to know how the protocol works. This is well documented in other places so I will only summarize it here:

- First a handshake is sent to the device. This will return a device ID and a timestamp
- All command requests will contain an id, a timestamp (based on response timestamp) and the device id
- For command requests your token is used to encrypt the data
- Any command response is also decrypted using your token

### A simple command

You will never have to think about the Request ID or the timestamps. This will be handled internally in the library.

However, if you send an ID into your payload then the library will not override it.

If you want to send a simple command you can do it by simply sending the method name into the Send method:


    var device = new MiDevice("192.168.87.119", "TOKENSTRING");
    device.Send("app_start");

This will generate a payload that looks something like this:

    {
     "id": 100,
     "method": "app_start",
     "params": []
    }

### Handling the result

The response will return a response object containing the JSON response string that you can do what you want with.

    var device = new MiDevice("192.168.87.119", "TOKENSTRING");
    var response = device.Send("get_clean_summary");
    if (response.Success) {
      Console.WriteLine("Response: " + response.ResponseText);
    }

# MiVacuumDevice

The `MiVacuumDevice` is a wrapper for the Xiaomi Vacuum Robots. This class is using the `MiDevice` class to  wrap functionality and give typed methods and responses for these devices.


## Start

Start vacuuming.

    {
     "method": "app_Start",
     "params": []
    }

- Will get status
- Will start cleaning if status is not cleaning

## Stop

## Charge

## Pause

## FindMe

## GetConsumables

## ResetConsumables

## GetCleanSummary

## GetCleanRecord

## GetCleanRecordMap

## GetMap

## GetStatus

## GetSerialNumber

## GetDnDTimer

## SetDnDTimer

## SetTimer

## UpdateTimer

## GetTimers

## DeleteTimer

## GetTimezone

## SetTimezone

## SetCustomMode

## StartZonedClean

## GotoTarget

# GetInfo (generic)

# GetRouter (generic)


# Links



- Xiaomi Mi Home Binary Protocol. - https://github.com/OpenMiHome/mihome-binary-protocol/blob/master/doc/PROTOCOL.md
- Xiaomi Mi Robot Vacuum Protocol. https://github.com/marcelrv/XiaomiRobotVacuumProtocol
- Obtain Mi Home device token. - https://github.com/jghaanstra/com.xiaomi-miio/blob/master/docs/obtain_token.md
- 