%   Author: Kameron Sheppard
%   Organization: UMD UAS Test Site
%   Date: Summer 2017


%This program must perform the following functions
% 1. Receive video/images from either Unity or QGC
% 2. Using NN, rank images by their confidence values
% 3. Return some simple information to Unity with this information


%1 is easy on the MATLAB side, open('pathtoVideo.mp4')
%2 might actually be relatively easy, considering the example problems
%3 is incredibly easy, just change the standard output

%this mypath stuff is actually uncessary because Matlab needs to look in
%all the feeds, not just 1 of them
clear;
%cd 'D:\Files\DroneSwarm'
%main_path = 'D:\Files\DroneSwarm';
%if isdeployed
C = textread('C:\Users\Public\.config', '%s', 'delimiter', '\n');
len = length(C{1});
main_path = C{1}(18:len);
%end
load([main_path '\Docs\rcnn2.mat'], 'rcnn2');
threshold = .90;
ratings = [0, 0, 0]; %holds the confidence values for each feed
sharelocation = [main_path '\.shared.txt'];
used_files = ["", "", ""];
while true 
    %look through all three feeds
    for i=1:3
        D = textread([main_path '\.kill.txt'], '%s', 'delimiter', '\n');
        if ~isempty(D) && isdeployed
           exit
        end
        if ~isempty(D) && ~ isdeployed
           return
        end
        
        ratings(i) = 0;
        mypath = [main_path char('\Images\Feed')  char(string(i))];
        %%% Part 1
        %This part of the program will involve reading from a video file
        d = dir(mypath);
        num_files = length(d);
        max_frame = 0;
        max_name = '';
        for j=1:num_files
            filename = d(j).name;
            if isequal(filename, '.') || isequal(filename, '..') || isequal(filename, 'Test1.jpg')
               continue; 
            else
               name_length = length(filename);
               frame_num = str2double(filename(name_length - 8: name_length - 4)); 
               if frame_num > max_frame
                  max_frame = frame_num;
                  max_name = filename;
               end
            end
        end
        if isequal(max_name, '')
           used_files(i) = "Test1.jpg";
           continue; 
        end
        used_files(i) = max_name;
        image = imread([mypath '\' max_name]);
        
        %%% Part 2
        %I have no idea what MiniBatchSize or 128 do
        
        [bboxes, score, label] = detect(rcnn2, image, 'MiniBatchSize', 128);
        %nothing was found

        if (isempty(score))
           ratings(i) = 0;
        %otherwise report the confidence
        else
           ratings(i) = max(score); 
        end

        
            
    end
        

    %%% Part 3
    %This part of the program will involve opening a file, writing to it, and
    %closing it, to ensure that Unity can then read the file.

    
    file = -1;
    while file == -1
       file = fopen(sharelocation, 'w');
    end
    %write the confidences to file
    for i = 1:3
        fprintf(file, ratings(i) + "\n");
    end
    fclose(file);
    exceed = 0;
  
end



%may need clearvars (variables) in order to free up some memory