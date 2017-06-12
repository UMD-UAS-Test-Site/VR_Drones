#!/bin/bash
#/mnt/d/Program\ Files/VideoLAN/VLC/vlc.exe rtsp://admin:@192.168.1.10/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp --video-filter=scene --scene-prefix='Test_img' --scene-path='D:\\Files\\DroneSwarm\\Images' &
#/mnt/d/Files/DroneSwarm/$1

#recognize the command line arguments
#v tells main to run VLC
#m tells main to run Matlab
#c tells main to remove image files from the Image Feeds directories
#d indicates that the program is in debug mode, for now no need to trigger
arg=""
v=0
m=0
c=0
d=0
while getopts "vmc" opt; do
    case $opt in
        v)
            v=1
            arg=($arg)v
            ;;
        m)
            m=1
            arg=($arg)m
            ;;
        c)
            c=1
            arg=($arg)c
            ;;
	d)
	    d=1
            ;;
    esac
done
#empty out .shared.txt and write to it
> .shared.txt
echo $arg >> .shared.txt
#unity will use this file to set its own arguments
#Matlab will most likely no longer be using it
./$2 &
#consider adding some information to the .shared.txt file to tell
#bash when unity has finished launching
sleep 4
if [ $c -eq 1 ] 
then
    echo "Deleting previous image files"
fi
if [ $v -eq 1 ] 
then
    echo "running vlc"
    /mnt/d/Program\ Files/VideoLAN/VLC/vlc.exe rtsp://admin:@192.168.1.10/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp --video-filter=scene --scene-ratio=20 --scene-prefix='Test_img' --scene-path='D:\\Files\\DroneSwarm\\Images\\Feed1' &
fi
if [ $m -eq 1 ] 
then
    echo "running matlab"
    #/mnt/d/Program\ Files/Matlab/R2017a/bin/matlab.exe -automation -r "run('D:\\Files\\DroneSwarm\\detectX.m')"
fi
