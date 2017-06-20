#!/bin/bash
#/mnt/d/Program\ Files/VideoLAN/VLC/vlc.exe rtsp://admin:@192.168.1.10/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp --video-filter=scene --scene-prefix='Test_img' --scene-path='D:\\Files\\DroneSwarm\\Images' &
#/mnt/d/Files/DroneSwarm/$1

#recognize the command line arguments
#v tells main to run VLC
#m tells main to run Matlab
#c tells main to remove image files from the Image Feeds directories
#d indicates that the program is in debug mode, for now no need to trigger
#t indicates that a Standalone Matlab executable is being used, must be used with m
#f indicates the number of streams that are being used, must be used with v
#b indicates that a nondefault build has been specified
arg=""
v=0
m=0
c=0
d=0
t=0
f=0
build=TestBuild.exe
stream_cnt=1
while getopts ":vmctf:" opt; do
    case $opt in
        v)
            v=1
            arg="$arg"v
            ;;
        m)
            m=1
            arg="$arg"m
            ;;
        c)
            c=1
            arg="$arg"c
            ;;
	d)
	    d=1
            ;;
	t)
            t=1
            arg="$arg"t
            ;;
        f)
            f=1
            arg="$arg"f
            stream_cnt=$OPTARG
            ;;
        b)
            build=$OPTARG
            ;;
        :)
            echo "need options"
            exit
            ;;
    esac
done
#empty out .shared.txt and write to it
> .shared.txt
echo $arg >> .shared.txt
#unity will use this file to set its own arguments
#Matlab will most likely no longer be using it
./$build &
#consider adding some information to the .shared.txt file to tell
#bash when unity has finished launching
sleep 2
if [ $c -eq 1 ] 
then
    echo "Deleting previous image files"
    rm /mnt/d/Files/DroneSwarm/Images/Feed*/*.png
fi
if [ $v -eq 1 ] 
then
    /mnt/d/Program\ Files/VideoLAN/VLC/vlc.exe rtsp://admin:@192.168.1.11/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp --video-filter=scene --scene-ratio=25 --scene-prefix='Test_img' --scene-height=640 --scene-width=360 --scene-path='D:\\Files\\DroneSwarm\\Images\\Feed1' &
fi
if [ $m -eq 1 ] && [ $t -eq 0 ] 
then
    # Using Matlab Runtime, the following line is uncessary
    /mnt/d/Program\ Files/MATLAB/R2017a/bin/matlab.exe -automation -r "run('D:\\Files\\DroneSwarm\\detectX.m')" &
    #/mnt/d/Files/DroneSwarm/Target_detector/for_testing/Target_detector.exe
fi
if [ $t -eq 1 ]
then
   /mnt/d/Files/DroneSwarm/Target_detector/for_testing/Target_detector.exe &
fi
if [ $stream_cnt -eq 2 ] || [ $stream_cnt -eq 3 ]
then
   /mnt/d/Program\ Files/VideoLAN/VLC/vlc.exe rtsp://admin:@192.168.1.12/user=admin_password=tlJwpbo6_channel=2_stream=0.sdp --video-filter=scene --scene-ratio=25 --scene-prefix='Test_img' --scene-height=640 --scene-width=360 --scene-path='D:\\Files\\DroneSwarm\\Images\\Feed2' &
fi
if [ $stream_cnt -eq 3 ]
then
   /mnt/d/Program\ Files/VideoLAN/VLC/vlc.exe rtsp://admin:@192.168.1.10/user=admin_password=tlJwpbo6_channel=3_stream=0.sdp --video-filter=scene --scene-ratio=25 --scene-prefix='Test_img' --scene-height=640 --scene-width=360 --scene-path='D:\\Files\\DroneSwarm\\Images\\Feed3' &
fi

