# JumperGuy
Sample project showcasing custom rigidbody based character controller along with all of the supporting systems.

## Features
| Feature           | Progress        | Description                                                                                                                                                                                 |
|:------------------|:----------------|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Dynamic Gravity` | **Done**        | Supports multiple overlapping gravity volumes with masking IDs,<br/>Rotate character according to current gravity.<br/>Support for priority system to override gravity in certain regions.  |
| `Step Offset`     | **Done**        | Supports Step offset using multiple raycasts for smooth step transition.                                                                                                                    |
| `State System`    | **Done**        | Custom scriptable object based state system implemented as a standalone feature and can be easily extended to be used for custom classes.                                                   |
| `Update System`   | **Done**        | Custom Update manager to reduce reflection based Update Calls and add support for sliced Update<br/>Implemented as an **Standalone** System.                                                |
| `Spring System`   | **In Progress** | A generic Spring System for versatile use.<br/>Used for :<br/> - Player Grounding Forces<br/> - Player rotation Torque<br/> - Procedural Camera Movement <br/> - Procedural weapon movement |


## ScreenShots
### Scene 1
<p align="center">
  <img src="Recordings/Image Sequence_001_0000.png" width="400" title="hover text" alt="Playground Scene">
  <img src="Recordings/Image Sequence_004_0000.png" width="400" alt="Water Shader">
</p>
Scene shows:

- Moving/Rotating platforms
- Physics interactions between character and rigidbody objects
- Sample Water Shader

### Scene 2 (Small Planet)
<p align="center">
  <img src="Recordings/Image Sequence_002_0000.png" width="400" title="hover text" alt="Planet Scene">
</p>
Scene shows:

- Dynamic Gravity
- Moving/Rotating platforms

## License
None

**Private Repository**  
**By, Ruchir Raj**
****
