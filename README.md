# Unity Project for GravField

## System Requirement:
Unity Version: 2022.3.20f1
Start Scene: Assets/Scenes/GravField_Infrastructure

## Network Configuration:
Folder: Assets/Scenes/GravField_Infrastructure/Assets
- UDP from Coda: <br>
  File: OscConnectionReceiverFromCoda<br>
  Default IP: 127.0.0.1, Port: 13600

- UDP to Coda: <br>
  File: OscConnectionSenderToCoda<br>
  Default IP: 127.0.0.1,  Port: 13500
  
- UDP to Live: <br>
  File: OscConnectionSenderToLive<br>
  Default IP: 192.168.0.136,  Port: 7000


## For Testing
### Solo Mode:
  If you don't have people around you to test, you can enable Solo Mode. In Solo mode, it will play a video in which 3 performers are dancing and send you osc signals at the same time.

> **To enable Solo mode, find GameManager in Unity Editor and set IsSoloMode to true in inspector**

### ShortCut:
- F5: Toggle Panel of parameters sent to Live 
- F6: Toggle Panel of parameters received from Coda
- F8: Toggle Info Panel
- Alpha 1: Chane to "Chain" Mode
- Alpha 2: Chane to "Spring" Mode
- Alpha 3: Chane to "Magmetic" Mode