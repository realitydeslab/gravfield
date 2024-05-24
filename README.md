# Unity Project

## System Requirement:
Unity Version: 2022.3.20f1
Start Scene: Assets/Scenes/GravField_Infrastructure

## Network Configuration:
Folder: Assets/Scenes/GravField_Infrastructure/Assets
- UDP from Coda: 
  File: OscConnectionReceiverFromCoda
  Default IP: 127.0.0.1, Port: 13600

- UDP to Coda: 
  File: OscConnectionSenderToCoda
  Default IP: 127.0.0.1,  Port: 13500
  
- UDP to Live: 
  File: OscConnectionSenderToLive
  Default IP: 192.168.0.136,  Port: 7000


## For Testing
### Solo Mode:
  If you don't have people around you to test, you can enable Solo Mode. In Solo mode, it will play a video in which 3 performers are dancing and send you osc signals at the same time.

To eanble Solo mode, find GameManager in Unity Editor and set IsSoloMode to true in inspector

### ShortCut:
- F5: Toggle Panel of parameters sent to Live 
- F6: Toggle Panel of parameters received from Coda
- F8: Toggle Info Panel
- Alpha 1: Chane to "Chain" Mode
- Alpha 2: Chane to "Spring" Mode
- Alpha 3: Chane to "Magmetic" Mode

---
# Coda Project

## System Requirement:
Nodejs Version: v16.20

## System Machanism:
As coda runs in web environment, it can't receive UDP messages directly. You have to start a UDP server as a message transfer station, then the UDP server will communicate with coda via websocket.

### Run Order
- Start UDP server by running command in CodaProject folder<br>
```npm run socket```
- Start Coda by running command in CodaProject folder<br>
```npm run dev```

### IP Configuration
To change the ip address that UDP server connects with, open ```/playground/server.js``` and modify ip and port in it.
#### Listening 
```
const osc = require('osc');
// Create an osc.js UDP Port listening on port 8888.
const udpPort = new osc.UDPPort({
  localAddress: '0.0.0.0',
  localPort: 13500,
  metadata: true,
});
```
#### Sending
```
const unityOscIp = "127.0.0.1";
const unityOscPort = 13600;
```

## Avaliable Functions
- ```performer(id)``` to get performer position. 
- ```oscto(id, address)``` to send command to specific performer.
- ```osctoall(address)``` to send command to all performers.
- ```pfm_maxy(ids)``` to calculate max of Y position among specific performers.
- ```pfm_miny(ids)``` to calculate min of Y position among specific performers.
- ```pfm_dist(ids)``` to calculate distance between specific performers.

## Avaliable Address
Sending data to osc adress is setting a parameter in Unity application. Current available address includes
- ```mode``` to change mode of effect

> Coda will add ```/``` automatically to complete osc address for you.

## Usage Example 

```
const sm = periodic(1000)
// change to mode 0
.constant(0)
.osctoall("mode");
```

```
const sm = periodic(10)
// show position of performer A
.performer("A")
.plot()
// calculate distance between A and C
.pfm_dist("AC")
.plot()
// set thickness react with distance
.scale({ outmin: 0, outmax: 10 })
.oscto("AC", "thickness");
```


