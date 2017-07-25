/*
 * Author:          Kameron Sheppard
 * Organization:    UMD UAS Test Site
 * Date:            Summer 2017
 * 
 */


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

    Texture2D image_tex; //holds the texture that will go on this screen
    bool initialized = false; //indicates whether or not this screen initialization has finished
    int frame = 0; // the frame count, used to limit access to .shared.txt, giving MATLAB time to write
    public int feed; // the feed used by this screen
    public float threshold = .5f; //the threshold needed to indicate that a screen is 'Hot'
    string w_image_path = "";  //the windows path to the Image Directory
    string w_main_path = ""; //the windows path to the main directory
    string current_file = ""; //the image that is currently loaded onto the screen
    bool done = true; //indicates an image has finished loading, 
    public bool matlab_active = false; //for testing, indicates Image Processing is active
    public bool target_detector_active = false; //for testing, indicates MCR is active
    public bool vlc_active = false; //for testing, Indicates VLC is active
    float confidence; //the confidence ratio for this particular feed
    public bool moved = false; //detects whether the screen is finished spawning in
    public bool enhancing = false; //detected whether the screen is growing
    public bool dehancing = false; //detects whether the screen is shrinking
    public float userRadius = 15; //the radius of circle around the user; needs to be set
    float timeconstant = 3f; //controls dampening rate
    float ar = .3f / .5333f; //aspect ratio of screen
    System.Diagnostics.Stopwatch timer; //used to time how long the marker is no longer detected
    public Collider UISphere; //cylinder to which screens are mapped
    System.Diagnostics.Stopwatch spawnTimer; //timer for when the screen spawns
    System.Diagnostics.Stopwatch growthTimer; //timer for how long the screen is growing


    /*
     * Precondition:
     * Postcondition:
     * Notes:           Not much happens here anymore
     */
    void Start() {
        timer = new System.Diagnostics.Stopwatch();
        growthTimer = new System.Diagnostics.Stopwatch();
    }


    /*
     * Precondition:    This screen's confidence value must have exceeded the
     *                  threshold or bool enhancing must be true
     * Postcondition:   Makes the Screen Bigger
     * Notes:           
     * 
     */
    public void enhanceScreen() {
        Debug.Log("assigning new transform");
        //check to see if the timer needs to be started
        if (!growthTimer.IsRunning)
            growthTimer.Reset();
            growthTimer.Start();
        float size = Mathf.Exp(-timeconstant * growthTimer.ElapsedMilliseconds / 1000f) * .4667f *
            Mathf.Cos(growthTimer.ElapsedMilliseconds / 100f - Mathf.PI) + .4667f;
        this.transform.localScale = new Vector3(.5333f + size, 1 + size, .3f + size * ar);
        //timer controls how long the screen grows
        if (growthTimer.ElapsedMilliseconds > 3000) {
            Debug.Log("enhance is over");
            enhancing = false;
            transform.localScale = new Vector3(1f, 1.5f, .5625f);
            growthTimer.Stop();
        }

        Vector3 point = CoV();
        Debug.Log("work is done");
        Vector3 position = transform.position;
        float x = Mathf.Lerp(position.x, point.x, growthTimer.ElapsedMilliseconds / 4000f);
        float y = Mathf.Lerp(position.y, point.y, growthTimer.ElapsedMilliseconds / 4000f);
        float z = Mathf.Lerp(position.z, point.z, growthTimer.ElapsedMilliseconds / 4000f);
        transform.position = new Vector3(x, y, z);

    }

    /*
     * Precondition:    The screen's confidence value must be below the threshold or
     *                  dehancing must be true
     * Postcondition:   Shrinks the screen back to its original size
     * Notes:           
     */
    public void dehanceScreen() {
        //check to see if the timer must be started
        if (!growthTimer.IsRunning) {
            growthTimer.Reset();
            growthTimer.Start();
            Debug.Log("something worked" + growthTimer.ElapsedMilliseconds);
            dehancing = true;
        }
        float size = Mathf.Exp(-timeconstant * growthTimer.ElapsedMilliseconds / 1000f) * .4667f *
            Mathf.Cos(growthTimer.ElapsedMilliseconds / 100f + Mathf.PI / 2) - .4667f;
        this.transform.localScale = new Vector3(1 + size, 1 + size, .5625f + size * ar);
        //timer controls how long the screen shrinks
        if (growthTimer.ElapsedMilliseconds > 3000) {
            dehancing = false;
            growthTimer.Stop();
            transform.localScale = new Vector3(.5333f, 1, .3f);
        }


    }

    /*
     * Precondition:    None
     * Postcondition:   Initializes all the values read from .config
     * Notes:           This method is always called by the ScreenSpawner
     *                  class when creating the screen
     *                  May need to add initialization of confidence value
     *                  From a portability perspective, its better to simply pass
     *                  All the data and let the user decide what they need
     *                  that way, it someone else were to add an intialization
     *                  parameter, it would not break everyone elses code
     * 
     */
    public void Initialize(Dictionary<string, string> data) {
        feed = Convert.ToInt32(data["feed"]);
        w_main_path = data["location"];
        w_image_path = w_main_path + "/Images";
        matlab_active = Convert.ToBoolean(data["matlab_active"]);
        vlc_active = Convert.ToBoolean(data["vlc_active"]);
        target_detector_active = Convert.ToBoolean(data["target_detector_active"]);
        userRadius = Convert.ToSingle(data["user_radius"]);
        setRotation();
        initialized = true;
        spawnTimer = new System.Diagnostics.Stopwatch();
        spawnTimer.Start();

    }

    /*
     * Precondition:    Screen must already be on the radius of the thing
     * Postcondition:   
     * Notes:           x represents around the cyclinder while
     *                  y represents vertical height
     */
    public void push(float x, float y) {

    }

    /*
     * Precondition:    Feed Must be set in the Unity Editor
     * Postcondition:   Returns the path to the Feed Folder for this screen
     * Notes:           Should use this to make w_image_path obsolete
     */
    string getFeedPath() {
        return w_image_path + "/Feed" + feed;
    }

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

    float radiansToDegrees(float rad) {
        return rad * (180.0f / Mathf.PI);
    }

    float degreesToRadians(float deg) {
        return deg * (Mathf.PI / 180.0f);
    }

    


    /*
     * Precondition:    None
     * Postcondition:   Rotates the Screen so that the visible part is facing 0, 0, 0
     * Notes:           Y Position is ignored
     * 
     */
    public void setRotation() {
        Vector3 nR = this.transform.position; //short for newRotation
        //Debug.Log(radiansToDegrees(Mathf.Atan2(nR.z, nR.x)));
        this.transform.eulerAngles = new Vector3(90, -radiansToDegrees(Mathf.Atan2(nR.z, nR.x)) - 90, 0);
    }

    /*
     * Precondition:    moved must be false
     * Postcondition:   Moves the screen to its appropiate location
     * Notes:           Need to get rid of the timer, it causes faults if there's lag
     */
    public void moveIn() {
        //for detecting when the screen reaches its destination

        float angle = Mathf.Atan2(transform.position.z, transform.position.x);
        //Debug.Log("angle is " + radiansToDegrees(angle));
        if (spawnTimer.ElapsedMilliseconds > 3104) {
            moved = true;
        }
        transform.position = new Vector3(Mathf.Cos(angle + Time.deltaTime) * userRadius,
            transform.position.y,
            Mathf.Sin(angle + Time.deltaTime) * userRadius);
        setRotation();
        
    }


    //this function gets the result of the image processing from matlab
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
        if (!matlab_active)
            return;
        //attempt to open a stream and read from the file
        try {
            StreamReader sr = new StreamReader(w_main_path + "/.shared.txt");
            string data;
            //skip all the lines except the relevant ones
            for (int i = 1; i <= feed; i++) {
                data = sr.ReadLine();
                //add a check in this location to see if it is a number
                //Char.IsDigit()
                if (i == feed) {
                    /* Largely uncessary as main.sh no longer writes to .shared.txt
                    for (int j = 0; j < data.Length) {
                        if (!Char.IsDigit(data[j]) || data[j] == ".") {
                            Debug.Log("There's no number here");
                        }
                    }*/
                    float tmp_confidence = Convert.ToSingle(data);
                    //check to see if a false zero has occured
                    if (tmp_confidence == 0 && !timer.IsRunning) {
                        Debug.Log("staring up the timer");
                        timer.Start();
                    }
                    else if (timer.IsRunning && timer.ElapsedMilliseconds > 10000) {
                        Debug.Log("1000 miliseconds have passed");
                        confidence = tmp_confidence;
                        timer.Reset();
                    }
                    else if (tmp_confidence > 0) {
                        timer.Reset();
                        confidence = tmp_confidence;
                    }
                }
            }
            sr.Close();
        }
        catch (FormatException) {
            //Debug.Log("Matlab has yet to write to .shared.txt");
        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }
    }


    /*
     * Precondtion:     Touch is connected
     * Postcondition:   Processes Input from the Oculus Touch
     * Notes:           This one's going to be difficult
     */
    void handleInput() {
        string[] sticks = UnityEngine.Input.GetJoystickNames();
        //the controllers are out of battery or are not connected
        if (sticks.Length == 0) {
            Debug.Log("Nothing detected");
        }
        //Process each Joystick step by step
        for (int i = 0; i < sticks.Length; i++) {

        }
    }

    /*
     * Precondition:    DroneControl must have moved on several images, Feed cannot be mainFeed
     * Postcondition:   Deletes the older images in the Feed Folder controlled by this Screen
     * Notes:           For some reason, this function deletes images in large chunks as opposed 
     *                  to a few images deleted more frequently 
     *                  This is likely due to how System calls for deletion work in C#          
     * 
     */
    void removeImages() {
        string name = getFeedPath();
        string[] files = Directory.GetFiles(name);
        //look through all the files in the Feed Folder
        for (int j = 0; j < files.Length; j++) {
            //if the file is not the one currently being loaded, delete it
            if (files[j] != current_file
                && !files[j].Contains("Test1.jpg")
                && !files[j].Contains(".swp")) {
                //Debug.Log("found some files");
                System.IO.File.Delete(files[j]);
            }
        }
            
        
    }

    /*
     * Precondition:    None
     * Postcondition:   
     * Notes:           Does nothing now
     */
    private void OnCollisionStay(Collision collision) {
        //Debug.Log("collision detected");
    }

    /*
     * Precondition:    None
     * Postcondition:   Removes the Y component from a Vector3
     * Notes:
     * 
     */
    private Vector3 zeroY(Vector3 input) {
        return new Vector3(input.x, 0, input.z);
    }

    /*
     * Precondition:    None
     * Postcondition:   Maps the location of a screen back onto the cylinder
     * Notes:
     */
    private void setLocation() {
        Vector3 zeroedLocation = zeroY(transform.position);
        float distance = Vector3.Distance(zeroedLocation, new Vector3());
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(0, transform.position.y, 0) + (zeroedLocation.normalized * (userRadius + 1)), 
            -zeroedLocation);
        if (UISphere.Raycast(ray, out hit, userRadius + 5)) {
            transform.position = hit.point;
        }
        else {

        }
    }

    /*
     * Precondition:    VLC must have be active for this function to run
     * Postcondition:   Returns the name of the most recent image file 
     *                  in the directory
     * Notes:           
     */
    string getNextImage() {
        float timeStart = Time.time;
        //if vlc is inactive, load the test image
        if (!vlc_active)
            return w_image_path + "/Test1.jpg";
        string output = "";
        DateTime best_time;
        string best_name = "";
        string[] files = Directory.GetFiles(getFeedPath());
        //no files were found in the Directory
        if (files.Length == 0)
            return "File_not_found";
        best_name = files[0];
        best_time = File.GetLastWriteTime(files[0]);
        //iterate through all files in folder, looking for the most recent one
        for (int i = 1; i < files.Length; i++) {
            //more recent =:= greater DateTime
            //must have at least lenth and be a png file
            if (File.GetLastWriteTime(files[i]) > best_time &&
                files[i].Length > 3 &&
                files[i].Substring(files[i].Length - 4, 4) == ".png") {
                best_name = files[i];
                best_time = File.GetLastWriteTime(files[i]);
            }
        }
        output = best_name;
        return output;
    }


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

    }

    /*
     * Precondition:    None
     * Postcondition:   Returns a point on the cylinder that the camera is looking at
     * Notes:
     */
    Vector3 CoV() {
        Vector3 direction = new Vector3(Mathf.Sin(-Camera.main.transform.eulerAngles.y),
                            Mathf.Sin(-Camera.main.transform.eulerAngles.x),
                            Mathf.Cos(-Camera.main.transform.eulerAngles.y));
        Ray ray = new Ray(Camera.main.transform.position + direction * (userRadius + 5), -direction);
        RaycastHit hit;
        UISphere.Raycast(ray, out hit, 20);
        return hit.point;

    }

    // Update is called once per frame
    /*
     * Precondition:    None
     * Postcondition:   checks for video; Quits the Game if necessary
     * Notes:           
     * 
     */
    void Update () {
        //continue enlarging screen when necessary
        setRotation();
        setLocation();
        if (enhancing) {
            Debug.Log("attempting enhancement");
            enhanceScreen();
        }
        if (transform.position.y > 8) {
            transform.position = new Vector3(transform.position.x, 8, transform.position.z);
        }
        else if (transform.position.y < -8) {
            transform.position = new Vector3(transform.position.x, -8, transform.position.z);
        }
        //shrink screen when no longer valid
        if (dehancing || (!enhancing && confidence < threshold &&
            transform.localScale.z > .4f)) {
            dehanceScreen();
        }
        if (!initialized)
            return;
        if (done) {
            if (!moved) {
                moveIn();
            }
            else if (!GetComponent<Renderer>().isVisible){
                //Debug.Log("can't see");
                //Vector3 cov = CoV();
                //transform.position = (transform.position - cov).normalized * Time.deltaTime;
                
            }
            Debug.Log("frame incrementing");
            frame++;
            receiveVideo();
            //only get the imageProcessing data if its the mainScreen
            //should put this in another function when possible
            if (matlab_active && frame % 5 == 0) {
                getImageProcess();
                if (confidence > threshold && transform.localScale.z < .6f) {
                    enhancing = true;
                }
            }
        }
        //Quit the Game if Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
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
 * 
 * 
 * At some point I should make a program to verify that the configuration file is valid
 * Also should error proof my code, so that certain errors don't occur while other
 * errors cause the program to crash in a predefined way and don't keep the program
 * running pointlessly
 * Also need to figure out how to kill matlab
 * Can potentially be done by the shell
 * 
 * 
 * TODO List
 * 
 * VR Polishing
 * -------------
 * Screen Spwaning
 * - When Screens Spawn they should fly in one by one at a much cleaner speed and kinda bounce
 * - Add a configuration file option that specifies the spawn locations of the screens
 * - A timer for the ScreenSpawner is a bad idea because lag
 * 
 * Screen Interaction
 * - User should be able to move screens using the Oculus Touch
 * - Movement should be fluid
 * - Add clamp to movement so movement below a certain speed is removed
 * - Screens pulled to center of user's gaze
 * 
 * Marker Detection
 * 
 * Background
 * - Running Matlab locally can save the application path via command line
 * 
 * 
 * Matlab Polishing
 * ----------------
 * 
 * Initiation
 * Add file location of configuration file via command line
 * Add real config file reading that doesn't assume the location
 * 
 * Bash Polishing
 * --------------
 * Add Location for Test Image
 * Standardize Test Image
 * Fix Command Line reading of arrays
 * Add optional brackets for singular testing
 * Add  
 * Add command line option guide
 */
    