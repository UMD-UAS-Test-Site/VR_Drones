#!/bin/bash
#/mnt/d/Program\ Files/VideoLAN/VLC/vlc.exe rtsp://admin:@192.168.1.10/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp --video-filter=scene --scene-prefix='Test_img' --scene-path='D:\\Files\\DroneSwarm\\Images' &
#/mnt/d/Files/DroneSwarm/$1

#/d/Program\ Files/VideoLAN/VLC/vlc.exe
set -e
clean="off"

build="TestBuild.exe"

#recognize the command line arguments
#v tells main to run VLC
#m tells main to run Matlab
#c tells main to remove image files from the Image Feeds directories
#d indicates that the program is in debug mode, for now no need to trigger
#t indicates that a Standalone Matlab executable is being used, must be used with m
#f indicates the number of streams that are being used, must be used with v
#b indicates that a nondefault build has been specified

#change of plans, Will always use a configuration file, and specified parameters will override the config file
#config=$(sed -n -e 's/--config-mode=\(\(on\)\|\(off\)\)/\1/p' <<< $@)

parse_arguments () {

   # this function is first ran on each line of config
   # its then run on all the command line arguments
   # That's why $@ is necessary
   # first check to ensure the user tried to set a value before ensure they
   # set it correctly
   if [[ $@ =~ "--sleep-mode=" ]]
   then
      sleep_mode=$(sed -n -e 's/.*--sleep-mode=\(\(on\)\|\(off\)\).*/\1/p' <<< $@)
      if [ "$sleep_mode" = "" ]
      then
         echo "sleep mode not properly set"
      fi
   fi
   if [[ $@ =~ "--unity-mode=" ]]
   then
      unity_mode=$(sed -n -e 's/.*--unity-mode=\(\(on\)\|\(off\)\).*/\1/p' <<< $@)
      if [ "$unity_mode" = "" ]
      then 
         echo "unity mode not properly set"
	 unity_mode=off
      fi
   fi
   if [[ $@ =~ "--vlc-mode=" ]]
   then
      #get the actual value
      vlc_mode=$(sed -n -e 's/.*--vlc-mode=\(\(on\)\|\(off\)\).*/\1/p' <<< $@)
      #make sure its a valid option
      if [ "$vlc_mode" = "" ]
      then
         echo "vlc mode not set"
         vlc_mode=off
      fi
   fi
   if [[ $@ =~ "--vlc-location=" ]]
   then
      vlc_location=$(sed -n -e 's/.*--vlc-location=\(.*\).*/\1/p' <<< $@)
      if [ "$vlc_location" = "" ]
      then
         echo "Invalid VLC Executable location specified"
         exit
      fi
   fi
   if [[ $@ =~ "--matlab-mode=" ]]
   then
      matlab_mode=$(sed -n -e 's/.*--matlab-mode=\(\(on\)\|\(off\)\|\(compiled\)\).*/\1/p' <<< $@)
      if [ "$matlab_mode" = "" ]
      then
         echo "matlab mode not set"
         matlab_mode=off
      fi
   fi
   if [[ $@ =~ "--matlab-location=" ]]
   then
      matlab_location=$(sed -n -e 's/.*--matlab-location=\(.*\).*/\1/p' <<< $@)
   fi
   if [[ $@ =~ "--camera-ip=" ]]
   then
      camera_ip=($(sed -n -e 's/.*--camera-ip=\[\(.*\)\].*/\1/p' <<< $@))
      if [ "${camera_ip[1]}" = "" ]
      then
         echo "Camera IP address not properly set"
	 exit
      fi
   fi
   if [[ $@ =~ "--camera-user=" ]] 
   then
      camera_user=($(sed -n -e 's/.*--camera-user=\[\(.*\)\].*/\1/p' <<< $@))
      if [ "${camera_user[1]}" = "" ]
      then
         echo "Camera Usernames not properly set"
	 exit
      fi
   fi
   if [[ $@ =~ "--camera-password=" ]]
   then
      camera_password=($(sed -n -e 's/.*--camera-password=\[\(.*\)\].*/\1/p' <<< $@))
      if [ "${camera_password[1]}" = "" ]
      then
         echo "Camera passwords not properly set"
	 exit
      fi
   fi
   if [[ $@ =~ "--camera-feeds=" ]]
   then
      camera_feeds=$(sed -n -e 's/.*--camera-feeds=\([0-9]*\).*/\1/p' <<< $@)
      if [ "$camera_feeds" = "" ]
      then
         echo "Camera feeds not properly set"
	 camera_feeds=0
      fi
   fi
   if [[ $@ =~ "--camera-ratio" ]]
   then
      camera_ratio=$(sed -n -e 's/.*--camera-ratio=\([0-9]*\).*/\1/p' <<< $@)
      if [ "$camera_ratio" = "" ]
      then
         echo "Camera ratio not properly set"
	 camera_ratio=20
      fi
   fi
   if [[ $@ =~ "--windows-location" ]]
   then
      windows_location=$(sed -n -e 's/.*--windows-location=\(.*\).*/\1/p' <<< $@)
      if [ "$windows_location" = "" ]
      then
         echo "Windows location not properly set"
	 exit
      fi
   fi
   if [[ $@ =~ "--main-location" ]]
   then
      main_location=$(sed -n -e 's/.*--main-location=\(.*\).*/\1/p' <<< $@)
      if [ "$main_location" = "" ]
      then
         echo "Main location not properly set"
	 exit
      fi
   fi
   if [[ $@ =~ "--clean" ]]
   then
      clean="on"
   fi
}

while IFS= read -r line
do
   parse_arguments "--$line"
done < "config.txt"

parse_arguments "$@"

#Put the part of your code that writes to .config here
> .config
content="windows-location="$windows_location"\n"
content=$content"main-location="$main_location"\n"
content=$content"unity-mode="$unity_mode"\n"
content=$content"vlc-location="$vlc_location"\nvlc-mode="$vlc_mode"\n"
content=$content"matlab-location="$matlab_location"\nmatlab-mode="$matlab_mode"\n"
content=$content"camera-ip=["
for ((i=0; i<$camera_feeds; i++))
do
   content=$content${camera_ip[$i]}" "
done
content=$content"\b]\n"
content=$content"camera-user=["
for ((i=0; i<$camera_feeds; i++))
do
   content=$content${camera_user[$i]}" "
done
content=$content"\b]\n"
content=$content"camera-password=["
for ((i=0; i<$camera_feeds; i++))
do
   content=$content${camera_ip[$i]}" "
done
content=$content"\b]\n"
content=$content"camera-feeds="$camera_feeds"\n"
content=$content"camera-ratio="$camera_ratio
echo -e $content >> .config
cp .config /c/Users/Public/.config

#exit

#begin configuration for new files if necessary
if [ ! -d "Images" ]
then
   mkdir "Images"
fi

if [ ! -e ".kill.txt" ]
then
   touch "kill.txt"
fi
> ".kill.txt"
for ((i=0; i<$camera_feeds; i++))
do
   if [ ! -d "Images/Feed$(($i + 1))" ]
   then
      mkdir "Images/Feed$(($i + 1))"
      cp "Test1.jpg" "Images/Feed$(($i + 1))/Test1.jpg"
   fi
done

#empty out .shared.txt and write to it
> .shared.txt
#unity will use this file to set its own arguments
#Matlab will most likely no longer be using it
if [ "$unity_mode" = "on" ]
then
   ./$build &
fi
if [ "$Sleep_mode" = "on" ]
then
   sleep 2
fi
if [ "$clean" = "on" ] 
then
   rm $main_location/Images/Feed*/*.png
   rm $main_location/Images/Feed*/*.png.swp
fi
if [ "$vlc_mode" = "on" ] 
then
   for ((i=0; i<$camera_feeds; i++))
   do
   #/d/Program\ Files/VideoLAN/VLC/vlc.exe &
   "$vlc_location" rtsp://admin:@${camera_ip[$i]}/user=${camera_user[$i]}_password=${camera_password[$i]}_channel=2_stream=0.sdp --video-filter=scene --scene-ratio=$camera_ratio --scene-prefix='Test_img' --scene-width=640 --scene-height=360 --scene-path=$windows_location"/Images/Feed"$(($i + 1)) &
   done
fi
if [ "$matlab_mode" = "on" ] 
then
    # Using Matlab Runtime,
    $matlab_location -r "run('D:/Files/DroneSwarm/detectX.m')"
fi
if [ "$matlab_mode" = "compiled" ]
then
   $main_location/Target_detector/for_testing/Target_detector.exe &
fi
