# VR_Drones
VR Application made with Unity to aggregate video data from a swarm of drones

For Users:
This application requires a installation of VLC Media Player and MCR 9.2
It is intended for use on a Windows computer although with a version of Matlab
Compiler, it should be runable on UNIX systems
It can be run from the git bash shell
In order to run this application please edit the config.txt to include all the requried
fields
windows-location is the Windows path to the main directory of the application
Please specify paths as C:/path/to/location
main-location is the bash path to the same directory
Please specify paths as /c/path/to/location
vlc-location is the path to the vlc executable 
Please specify paths as /c/Program Files/VideoLAN/VLC/vlc.exe
camera-ip are the IP addresses of the cameras used by the application
Please specify ips as [192.169.1.1] for a single camera
Please specify ips as [192.168.1.1 192.168.1.2] for multiple cameras
camera-user are the usernames of the cameras,
Please speicfy as [user1] for a single camera
Please speicfy as [user1 user2] for multiple cameras
camera-password are the passwords for the cameras,
Please specify as [password1] for a single camera
Please speicfy as [password1 password2] for multiple cameras
Please note, the IP addresses, users, and passwords are all paired
If a camera's IP address occurs first its password and username should
also occur first
camera-feeds is the number of feeds
This application currently supports up to 4 feeds
unity-mode should be set to on
matlab-mode should be set to compiled
vlc-mode should be set to on
sleep-mode should be set to on
rcnn should be set to rcnn2
build should be set to TestBuild.exe

In order to use VLC to read from an IP camera you'll need to modify your ethernet
settings



For Developers:		
