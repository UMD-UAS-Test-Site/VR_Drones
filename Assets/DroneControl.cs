using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * The VR portion of the program needs to do several things
 * 1. Receive commands from the user
 * 2. Send commands to the Ground Station
 * 3. Receive data from MATLAB
 * 4. Receive and Display data from the Ground State
 * 5. Display visual data from the Ground Station
 * 
 */

/* 
* Receiving Commands will be last, because I'm not sure about exactly what type of controller will be used
* However I should still make skeleton functions for the other control methods
* 
* Receiving data from MATLAB will be pretty simple, in the Start script it will be necessary to run an instance of the MATLAB sccript.
* MATLAB will then spit out numbers every now and then, and my program will respond
* 
* Sending commands to the Ground Station is should also come later its unknown whether or not this is still lin my project scope
* 
* Receiving and Displaying data from Ground Station should come later.  This is the part Josh said will be most difficult
* 
* Didsplaying the data is also kinda difficult.  I have yet to redesign the GUI so its not entirely clear what this will look like
* 
* Order 3, 5(pseudocode), 2, 1, 4
* 
*/
public class DroneControl : MonoBehaviour {

    Texture2D image_tex;
    WWW imageLocation; // the URL/File Location of the next image to be loaded
    public uint feed;
    public bool mainScreen = false; //Is this DroneControl running on the main screen
    string w_image_path = "D:/Files/DroneSwarm/Images";
    string w_main_path = "D:/Files/DroneSwarm";
    string current_file = "";
    bool done = true; //indicates an image has finished loading, 
    public bool matlab_active = false; //for testing, turns on Matlab componenet
    public bool target_detector_active = false;
    public bool vlc_active = false; //for testing, turns on vlc components
    //public bool var_f_rate = false; //indicates whether the frame rate is different
    // from the default frame rate
    //int f_rate = 20;
    float[] confidence; //stores the importance of the video feeds from matlab
    System.Diagnostics.Process ls; //ls process; outdated System.IO is now used instead
    System.Diagnostics.Process matlab; //matlab process; outdated, app launched from main.sh 
    System.Diagnostics.Process vlc; //vlc process; outdated, app launched from main.sh
    /*string nextImage = ""; //contains the name of the next image to be loaded
    //string bash_path = "/mnt/d/Files/DroneSwarm"; //obsolete; bash doesn't have permissions on standalone applications
    //string old_bash_path = "/mnt/c/Users/Kameron/AppData/LocalLow/DefaultCompany/Drone\\ Swarm";
    //bool resume = false; //ndicates an image has started loading, but did not complete
    //string location = "D:\\Files\\DroneSwarm\\Images\\Feed";
    //float refresh = .2f; //obsolete; timer indicates a new image should be loaded
    //swap to simply look for the most recent file
    //float last = 0; //obsolete; UNIX time the last image was loaded onto the screen*/

    /*
     * Precondition:    None
     * Postcondition:   Sets up all the needed File Systems to Obtain images, 
     *                  Starts Matlab and VLC if they are active
     * Notes:           Should probably put paths and whatnot into global strings
     * 
     */
    void Start() {
        confidence = new float[4];

        // read .shared.txt to find out what mode it was launched in from the 
        // shell
        if (!Application.isEditor) {
            try {
                StreamReader sr = new StreamReader(w_main_path + "/.shared.txt");
                string data = sr.ReadLine();
                vlc_active = data.Contains("v") ? true : false;
                matlab_active = data.Contains("m") && mainScreen ? true : false;
                target_detector_active = data.Contains("t") && mainScreen ? true : false;
            }
            catch (Exception e) {
                Debug.Log(e.Message);
                Debug.Log("coulnd not read settings from .shared.txt");
            }

        }

    }
    /*check to see if the frame rate is different from normal
    try {
        StreamReader sr = new StreamReader(w_main_path + "/.shared.txt");
        f_rate =  Convert.ToInt32(sr.ReadLine());
        sr.Close();
    }
    catch (Exception e) {
        Debug.Log(e.Message);
    }

     Launch VLC if that application is active
            if (vlc_active) {
                vlc = new System.Diagnostics.Process();
                vlc.StartInfo.UseShellExecute = false;
                vlc.StartInfo.FileName = "bash";
    vlc.StartInfo.WorkingDirectory = "D:\\Files\\DroneSwarm";
                vlc.StartInfo.Arguments = "-c '/mnt/d/Files/DroneSwarm/launchVLC.sh '";    
     "rtsp://admin:@192.168.1.10/" 
    + "user=admin_password=tlJwpbo6_channel=1_stream=0.sdp " 
    + "--video-filter=scene --scene-ratio=10 --scene-prefix='Test_img' " 
    + "--scene-path='D:\\Files\\DroneSwarm\\Images\\Feed" + feed + "'"; 
                Debug.Log(vlc.StartInfo.Arguments);
                try {
                    vlc.Start();
                }
                catch (Exception e) {
                    Debug.Log(e.Message);
                    Debug.Log(e.InnerException);
                }
            }

     * Launch matlab if that application is necessary
     * Also gives matlab the feed number in arg1

           if (matlab_active) {
               matlab = new System.Diagnostics.Process();
               matlab.StartInfo.UseShellExecute = false;
               matlab.StartInfo.FileName = "matlab";
               matlab.StartInfo.Arguments = "-automation -r arg1=" + feed +
                                           "; run('D:\\Files\\DroneSwarm\\detectX.m')";
               try {
                   matlab.Start();
               } catch (Exception e) {
                   Debug.Log(e.Message);
               }
          }
}
// matlab -automation -r 'run('D:\Files\DroneSwarm\detectX.m')'
*/

    /*
     * 
     * Precondition:    Application Must Quit
     * Postcondition:   Sends Kill Signals to Matlab and VLC if they are running
     *                  Also removes all png files from the Feed Directories
     * Notes:           Please note: this process will kill all instances of MATLAB
     *                  This function must be updated at some point so that only
     *                  the mainScreen does any deleting and deletes for all images
     * 
     */
    private void OnApplicationQuit() {
        //only the mainScreen performs cleanup operations
        if (mainScreen) {
            //close matlab
            if (matlab != null && matlab_active && !target_detector_active) {
                //probably possible to predict which instance of Matlab to kill
                //by investigating its start-time
                foreach (var process in
                         System.Diagnostics.Process.GetProcessesByName("matlab")) {
                    process.CloseMainWindow();
                    process.Close();
                }
                matlab = null;
            }
            if (target_detector_active) {
                Debug.Log("searching for Target_detector");
                foreach (var process in
                         System.Diagnostics.Process.GetProcessesByName("Target_detector")) {
                    process.CloseMainWindow();
                    process.Close();
                    Debug.Log("we found it");
                }
            }

            //only need to close VLC and delete images if VLC is active
            if (vlc_active) {
                //Look For VLC instances and Close them
                foreach (var process in System.Diagnostics.Process.GetProcessesByName("vlc")) {
                    process.CloseMainWindow();
                    process.Close();
                }
                //remove extra files
                string path_name = getFeedPath().Substring(0, getFeedPath().Length - 1);
                for (int i = 1; i <= 4; i++) {
                    string[] files = Directory.GetFiles(path_name + i);
                    //look through files and if they are png or swp files, delete them
                    //probably need to add some sort of delay to prevent Sharing Violations
                    for (int j = 0; j < files.Length; j++) {
                        if (files[j].Contains(".png") || files[j].Contains(".swp")) {
                            System.IO.File.Delete(files[j]);
                        }
                    }
                }
            }
        }
    }


    //The only messages that would come directly from QGC are error messages
    /*
     * Precondition:    
     * Postcondition:   
     * Notes:           
     * 
     /
    void receiveCommand() {

    }
    */
    
    /*
     * Precondition:    Feed Must be set in the Unity Editor
     * Postcondition:   Returns the path to the Feed Folder for this screen
     * Notes:           
     */
    string getFeedPath() {
        return w_image_path + "/Feed" + feed;
    }

    //this function sends fly commands to QGC
    /*
     * Precondition:    
     * Postcondition:   
     * Notes:  This function is no longer included in the project scope         
     * 
     
    void relayCommand() {
    
    }
    */

    // this function gets the video feeds from QGC
    // This function will make use of Resource Load / Unload, (lol)
    // QGC will save video files into folders with specific names
    // Unity will load them, display them, then unload them, then possibly delete them
    /*
     * Precondition:    The Path to the location of the image files must be set
     *                  There must be at least one image in the specified path
     *                  That image must be the most recent file in that directory
     * Postcondition:   Loads the image onto the texture
     * Notes:           At the current moment, this is running every single update
     *                  This is unnecessary
     *                  
     */
    void receiveVideo() {
        done = false;
        string next_file = getNextImage();
        //Check to see if VLC has stopped sending commands
        if (next_file == current_file || next_file == "File_not_found") {
            done = true;
            return;
        }
        byte[] data = System.IO.File.ReadAllBytes(next_file);
        image_tex = new Texture2D((int)GetComponent<Transform>().localScale.x,
            (int)GetComponent<Transform>().localScale.z,
            TextureFormat.DXT1, false);
        //if the image does not load; This is problematic
        if (!image_tex.LoadImage(data)) {
            Debug.Log("mega failure");
        }
        GetComponent<Renderer>().material.mainTexture = image_tex;
        current_file = next_file;
        done = true;
        removeImages();
    }
        /*
        if (resume) {
            Debug.Log("had to wait on image " + nextImage);
            if (imageLocation.isDone) {
                imageLocation.LoadImageIntoTexture(image_tex);
                GetComponent<Renderer>().material.mainTexture = image_tex;
                resume = false;
                last = Time.time;
            }
        }
        check to see if it is now time to display a new frame
        else if (last + refresh < Time.time) {
            if (mainScreen) {
                uint newFeed = getImageProcess();
                if (newFeed != feed) {
                    Debug.Log("Feed has changed");

                }
                feed = newFeed;
            }
            image_tex = new Texture2D((int)GetComponent<Transform>().localScale.x,
                (int)GetComponent<Transform>().localScale.z,
                TextureFormat.DXT1, false);
            imageLocation = new WWW("file:///" + Application.persistentDataPath + "/Feed" + feed 
                            + "/" + getNextImage());
            Debug.Log(imageLocation.url);
            Debug.Log("next is : " + getNextImage());
            if (imageLocation.isDone) {
                imageLocation.LoadImageIntoTexture(image_tex);
                GetComponent<Renderer>().material.mainTexture = image_tex;
                last = Time.time;
            }
            else {
                resume = true;
            }
        }
        */


    //this function gets the result of the iamge processing from matlab
    /*
     * Precondition:    This function will not perform any work unless Matlab is active
     *                  The Path must be correctly set
     * Postcondition:   Reads the information stored inside the .shared.txt file and stores it
     * Notes:           This function must open and close the .shared file each time 
     *                  it is run to ensure all changes are discrete
     *                  This function only uses the shared.txt file for Read only
     *                  This function returns feed 1 by default
     * 
     */
    void getImageProcess() {
        //check to see if Matlab is active
        //Debug.Log("getting the results");
        if (!matlab_active)
            return;
        //attempt to open a stream and read from the file
        try {
            StreamReader sr = new StreamReader(w_main_path + "/.shared.txt");
            string data;
            for (int i = 0; i < 3; i++) {
                data = sr.ReadLine();
                confidence[i] = Convert.ToSingle(data);
            }
            sr.Close();
        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    /*
     * Precondition:    DroneControl must have moved on several images, Feed cannot be mainFeed
     * Postcondition:   Deletes the older images in the Feed Folder controlled by this Screen
     * Notes:           For some reason, this function deletes images in large chunks as opposed 
     *                  to a few images deleted more frequently           
     * 
     */
    void removeImages() {
        //need to fix this so that images are properly dealth with
        //right now it deletes images from other feeds almost as soon as they are created
        //also reluctant to make each individual screen delete its own because
        //that would make mainScreen testing impossible

        //fix ?? no checks on screen, instead, only checks
        string name = getFeedPath();
        string[] files = Directory.GetFiles(name);
        //look through all the files in the Feed Folder
        //Debug.Log("looking for files");
        //Debug.Log(files.Length);
        for (int j = 0; j < files.Length; j++) {
            //if the file is not the one currently being loaded, delete it
            if (files[j] != current_file
                && !files[j].Contains("Test1.jpg")
                && !files[j].Contains(".swp")) {
                Debug.Log("found some files");
                System.IO.File.Delete(files[j]);
            }
        }
            
        
    }

    /*
     * Precondition:    VLC must have be active for this function to run
     * Postcondition:   Returns the name of the most recent image file 
     *                  in the directory
     * Notes:           
     */
    string getNextImage() {
        //check to see if vlc is active
        float timeStart = Time.time;
        //if vlc is inactive, load the test image
        if (!vlc_active)
            return w_image_path + "/Test1.jpg";
        string output = "";
        DateTime best_time;
        string best_name = "";
        /*DirectoryInfo location = new DirectoryInfo(Application.persistentDataPath);
        //var File = location.GetFiles().O */
        string[] files = Directory.GetFiles(getFeedPath());
        if (files.Length == 0)
            return "File_not_found";
        best_name = files[0];
        best_time = File.GetLastWriteTime(files[0]);
        //iterate through all files in folder, looking for the most recent one
        for (int i = 1; i < files.Length; i++) {
            //more recent =:= greater DateTime
            if (File.GetLastWriteTime(files[i]) > best_time &&
                files[i].Substring(files[i].Length - 3, 3) == "png") {
                best_name = files[i];
                best_time = File.GetLastWriteTime(files[i]);
            }
        }
        output = best_name;
        //Debug.Log("nextImage Time" + (Time.time - timeStart));

        return output;
    }
        /*        ls = new System.Diagnostics.Process();
        //        ls.StartInfo.UseShellExecute = false;
        //        ls.StartInfo.CreateNoWindow = true;
        //        ls.StartInfo.RedirectStandardOutput = true;
        //        ls.StartInfo.FileName = "bash";
        //        ls.StartInfo.Arguments = "-c 'ls -t " + bash_path 
        //            + "/Feed" + feed 
        //            + "/'";
        //        ls.StartInfo.RedirectStandardError = true;
        //attempt to launch the ls command
        //        try {
        //            ls.Start();
        //get the first line of ls
        //            output = ls.StandardOutput.ReadLine();
        //        }
        //        catch (Exception e) {
        //            Debug.Log(e.Message);
        //        }
        //        finally {
        //            ls.Dispose();
        //            ls = null;
        //        }
        */


    //display the information received from MATLAB and QGC in a clear way
    /*
     * Precondition:    This screen must be the main one
     * Postcondition:   Performs all the necessary and miscellaneous computations associated
     *                  with displaying the data; i.e. displaying sensor data
     * Notes:           This function has been superceded by displayData, might be used in
     *                  the future
     * 
     */
    void displayData() {
        if (mainScreen) {

        }
    }

    // Update is called once per frame
    /*
     * Precondition:    None
     * Postcondition:   checks for video; Quits the Game if necessary
     * Notes:           
     * 
     */
    void Update () {
        //if Unity has finished loading the previous images
        if (done) {
            receiveVideo();
            //only get the imageProcessing data if its the mainScreen
            //should put this in another function when possible
            if (mainScreen && matlab_active) {
                getImageProcess();
                uint index = 0;
                float max = -1f;
                //find the biggest confidence and switch the mainScreen
                for (uint i = 0; i < 3; i++) {
                    if (confidence[i] > max) {
                        max = confidence[i];
                        index = i;
                    }
                }
                feed = index + 1;
            }
        }
        //Quit the Game if Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        //Debug.Log(current_file);
	}
}

/*
 * Need to create some standardized test
 * Things that needed to be tested are
 * Does Unity properly read the options from a file
 * Does Unity properly kill unneeded processes
 * Does Unity get rid of unneeded files (Need to put in some sleep command so .swp
 * files can be deleted)
 * Does the program respond correctly when the nextImage has not changed or doesn't exist
 * Can Unity load local images
 * Can Unity read data presented by Matlab
 * Are files correctly deleted throughout
 * are only the newer files correctly deleted
 * Does Unity recognize keypresses
 * is the feed changed during runtime?
 * 
 */