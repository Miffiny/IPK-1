# Chat Client
For VUT FIT students:
DO NOT COPY THIS CODE OR ANY PART OF IT, OTHERWISE YOU WILL BE DISCIPLINED BY THE DISCIPLINARY COMMITTEE.
## Overview
The goal of the project is to create a chat client that communicates with a remote server using the IPK24CHAT protocol. The client uses the UDP and TCP transport protocols at the user's choice.

## Table of Contents
- [Project Name](#project-name)
  - [Overview](#overview)
  - [Table of Contents](#table-of-contents)
  - [Installation](#installation)
  - [Usage](#usage)
  - [Structure](#structure)
  - [Tests](#tests)

## Installation
To install the client, just download the zip archive with the project to the folder you need and unzip the contents. In the root folder, use the **make** command to build the **ipk24chat-client executable**, which will be moved to the current folder. Run the program with the **-h** argument to see a list of available arguments and how to use them. The first **make** might take a few minutes.

```
$ make
dotnet publish App/App.csproj -r linux-x64 -c Release -o bin/Release/linux-x64 --self-contained true
MSBuild version 17.9.6+a4ecab324 for .NET
  Determining projects to restore...
  Restored /home/User/rootFolder/App/App.csproj (in 1.75 min).
  App -> /home/User/rootFolder/App/bin/Release/net8.0/linux-x64/App.dll
  Optimizing assemblies for size. This process might take a while.
  App -> /home/User/rootFolder/bin/Release/linux-x64/
mv bin/Release/linux-x64/App bin/Release/ipk24chat-client
mv bin/Release/ipk24chat-client ./ # Move to the root directory
```


## Usage
After running the program with the arguments you need, you can enter the **/help** command, which will list the available commands for communicating with the server. To end communication with the server, use the **interrupt signal** and write any message. This message will not be sent to the server.

## Structure
A client consists of several layers of classes. There was used principle of "from the specific to the general".

### Low level
At the lowest level are classes responsible for a specific type of message. The methods of these classes can read and compose new messages of their type. However, some classes (for example, for the **Auth or Join** types) cannot read messages because they are used exclusively by the user. Classes for the **Err** and **Reply** types, on the contrary, do not have methods for generating messages.

### Medium level
At a higher level are the **ServerParser** and **UserParser** classes. These classes are an **intermediate link** between the client class and the message type classes. At this level, the message is parsed and methods are applied to it according to the message type. In the case of **UserParser**, a message in a ready-to-send format is returned to its caller. In the case of a **ServerParser**, the received message in the appropriate format is sent to stdout or stderr, depending on its type.

### High level
At the next level, there are client classes named **TCP** and **UDP**. These classes are responsible for establishing communication with the server, sending and receiving messages. Clients asynchronously process user and server input using the appropriate **asynchronous Tasks** and call parsers for received messages. To prevent receiving duplicate messages from the server, the ID of each message is stored and when each new input is received from the server, the corresponding ID is compared with the stored ones.

### Top level
At the highest level is the **Program** class, which has the **Main** method. This method parses the arguments of the entire program and, based on them, calls the appropriate client, passing it all the necessary parameters.

### Special classes
Special classes include **ChatFSM** and **MessageTracker**. **ChatFSM** represents a finite state machine that is used at the middle and high levels for process control and for communication between asynchronous Tasks. **The transition between the Error state and the End state occurs automatically**. During the End state, the client terminates Tasks and sends a BYE message to the server. **MessageTracker** is a class that creates temporary storage for sent messages. The Task responsible for processing user input stores the message in this storage before sending it to the server. The task responsible for processing the input from the server removes the corresponding message from the storage upon receiving CONFIRM. **The storage itself is self-regulating;** each recorded message has a time stamp and the number of times it has been sent. After every certain period of time, the message is sent to the server again.

### TCP version
The program has only three classes for TCP, the client itself and two parsers. Each class is prefixed with "TCP". The TCP protocol is much less fussy, so there is no need to share responsibility unlike UDP.

## Tests
Testing was conducted on Windows (native) and Linux (WSL Ubuntu) operating systems. Each class was tested separately starting from the lowest level. For each message type, I simulated user input as a string and server input as an array of bytes. This way, when writing code for higher levels, I was sure that the lower levels would work. To test special classes, I debugged the program, wrote down the current state of the finite state machine, and checked the contents of temporary storages to make sure they were working correctly. When I had the client code ready, I tested the program from the user's point of view on the server, writing messages, commands, and checking the FSM.

A few examples:

```
$ ./ipk24chat-client -h
Usage: program.exe -t <transport_protocol> -s <server_address> [-p <server_port>] [-d <udp_confirmation_timeout>] [-r <max_udp_retransmissions>] [-h]
Arguments:
  -t    User provided   tcp or udp      Transport protocol used for connection
  -s    User provided   IP address or hostname  Server IP or hostname (mandatory)
  -p    4567    uint16  Server port
  -d    250     uint16  UDP confirmation timeout
  -r    3       uint8   Maximum number of UDP retransmissions
  -h                    Prints program help output and exits
```
This shows the beginning of communication with the server (I have changed the secret, it does not match the actual one)

```
$ ./ipk24chat-client -t udp -s 147.229.8.244 -p 4567
trying to send something
You need to authenticate before sending messages.
/auth xbaran21 1dcff831-3e85-445c-a6f3-a21ab7147c3e SomeTest
Success: Authentication successful.
Server: SomeTest joined discord.general.
msg
SomeTest: msg
/join discord.verified-2
Server: SomeTest joined discord.verified-2.
Success: Channel discord.verified-2 successfully joined.
and here
SomeTest: and here
/rename MyNewName
okay?
MyNewName: okay?
```

During testing, I found a problem: tasks run until the state of the finite state machine is End. However, if the state has changed, in order to check the condition again, the task needs to execute its body, that is, either receive a new message from the server or process user input. When any task receives any inout, it will immediately stop and the program will continue its work by sending a Bye message to the server. A potential solution to the problem may be to create another task that will constantly monitor the current state of the FSM.
