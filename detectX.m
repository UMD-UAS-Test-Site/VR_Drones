%This program must perform the following functions
% 1. Receive video/images from either Unity or QGC
% 2. Using NN, rank images by their confidence values
% 3. Return some simple information to Unity with this information


%1 is easy on the MATLAB side, open('pathtoVideo.mp4')
%2 might actually be relatively easy, considering the example problems
%3 is incredibly easy, just change the standard output

%this mypath stuff is actually uncessary because Matlab needs to look in
%all the feeds, not just 1 of them
load('rcnnTargets.mat', 'rcnn');


while true 
    for i=1:4
        filename = "D:\Files\DroneSwarm\.shared.txt";
        mypath = [char("D:\Files|DroneSwarm\Images\Feed")  char(i)];
        choice = ["1", "2", "3", "4"];
        s ="";
        %%% Part 1
        %This part of the program will involve reading from a video file
        d = dir(mypath);
        [dx,dx] = sort([d.datenum]);
        imgfile = d(dx == 1).name;
        image = imread([mypath '\' imgfile]);
    
        %%% Part 2
        %I have no idea what MiniBatchSize or 128 do
        [bboxes, score, label] = detect(rcnn, image, 'MiniBatchSize', 128);
        %need to iterate through scores to determine if any exceed a
        %threshold
        
    end
    %%% Part 3
    %This part of the program will involve opening a file, writing to it, and
    %closing it, to ensure that Unity can then read the file.
    pause(1);
    
    file = fopen(filename, 'w');
    for i = 1:4
        s = s + choice(i);
    end
    fprintf(file, "s\n");
    fclose(file);
end

%may need clearvars (variables) in order to free up some memory