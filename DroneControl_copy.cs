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
    public bool vlc_active = false; //for testing, turns on vlc components
    int f_rate = 20;
    uint[] order; //stores the importance of the video feeds from matlab



    /*
     * Precondition:    None
     * Postcondition:   Sets up all the needed File Systems to Obtain images, 
     *                  Starts Matlab and VLC if they are active
     * Notes:           Should probably put paths and whatnot into global strings
     * 
     */
    void Start() {
        order = new uint[4];
        getImageProcess();        
    }
    // matlab -automation -r 'run('D:\Files\DroneSwarm\detectX.m')'
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

        if (mainScreen) {
            //close matlab
            if (matlab != null && matlab_active) {
                //probably possible to predict which instance of Matlab to kill
                //by investigating its start-time
                foreach (var process in 
                         System.Diagnostics.Process.GetProcessesByName("matlab")) {
                    process.CloseMainWindow();
                    process.Close();
                }
                matlab = null;
            }
        }
        //delete image files
        if (vlc_active) {
            foreach (var process in System.Diagnostics.Process.GetProcessesByName("vlc")) {
                process.CloseMainWindow();
                process.Close();
            }
            //remove extra files
            string[] files = Directory.GetFiles(getFeedPath());
            for (int i = 0; i < files.Length; i++) {
                if (files[i].Substring(files[i].Length - 4, 4) == ".png"
                    || files[i].Substring(files[i].Length - 4, 4) == ".swp") {
                    System.IO.File.Delete(files[i]);
                }
            }
        }
    }

    string getFeedPath() {
        return w_image_path + "/Feed" + feed;
    }

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
        if (next_file == current_file || next_file == "File_not_found") {
            done = true;
            return;
        }
        byte[] data = System.IO.File.ReadAllBytes(next_file);
        image_tex = new Texture2D((int)GetComponent<Transform>().localScale.x,
            (int)GetComponent<Transform>().localScale.z,
            TextureFormat.DXT1, false);
        if (!image_tex.LoadImage(data)) {
            Debug.Log("mega failure");
        }
        GetComponent<Renderer>().material.mainTexture = image_tex;
        current_file = next_file;
        done = true;
        removeImages();
    }


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
    uint getImageProcess() {
        //check to see if Matlab is active
        if (!matlab_active)
            return 1;
        //attempt to open a stream and read from the file
        try {
            StreamReader sr = new StreamReader(w_main_path + "/.shared.txt");
            string data = sr.ReadLine();
            for (int i = 0; i < 4; i++) 
                order[i] = Convert.ToUInt32(data[i]);
            sr.Close();   
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
        return order[0];
    }

    /*
     * Precondition:    DroneControl must have moved on several images, Feed cannot be mainFeed
     * Postcondition:   Deletes the older images in the Feed Folder controlled by this Screen
     * Notes:           This function needs to be written           
     * 
     */
    void removeImages() {
        //only the mainScreen deletes images
        if (mainScreen) {
            string[] files = Directory.GetFiles(getFeedPath());
            //look through all the files in the Feed Folder
            for (int i = 0; i < files.Length; i++) {
                //if the file is not the one currently being loaded, delete it
                if (files[i] != current_file 
                    && ! files[i].Contains("Test1.jpg")
                    && ! files[i].Contains(".swp")) {
                    System.IO.File.Delete(files[i]);
                }
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
        if (!vlc_active)
            return w_image_path +  "/Test1.jpg";
        string output = "";
        DateTime best_time;
        string best_name = "";
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
        return output;
    }


    //display the information received from MATLAB and QGC in a clear way
    /*
     * Precondition:    
     * Postcondition:   
     * Notes:           This function has been superceded by displayData, might be used in
     *                  the future
     * 
     */
    void displayData() {
        //This function needs to keep track of the video feeds obtained by What's his face in a way. Yeah
        //Needs to keep track of the number of times that certain things
    }

    // Update is called once per frame
    /*
     * Precondition:    None
     * Postcondition:   checks for video; Quits the Game if necessary
     * Notes:           Need to change it so this is not done every single update
     * 
     */
    void Update () {
        if (done) {
            receiveVideo();
        }
        //Quit the Game if Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        Debug.Log(current_file);
	}
}
