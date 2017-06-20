mypath = char('D:\Files\DroneSwarm\Images\Training');

d = dir(mypath);
my_file = fopen('D:\Files\DroneSwarm\result.txt', 'w');

for file = d'
    if (~isequal(file.name, '..') && ~isequal(file.name, '.'))
        image = imread([mypath '\' file.name]);
        [bboxes, score, label] = detect(rcnn, image, 'MiniBatchSize', 128);
        if (isempty(score))
           fprintf(my_file, file.name + " BAD\n");
        else
           fprintf(my_file, file.name + " GOOD\n"); 
        end
    end
end
fclose(my_file);

