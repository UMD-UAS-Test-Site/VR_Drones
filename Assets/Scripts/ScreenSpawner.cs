using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class ScreenSpawner : MonoBehaviour {


    int total_feeds; //total number of screens to be spawned
    bool matlab_active;//matlab enabled
    bool target_detector_active; //matlab compiled mode
    bool vlc_active;//vlc enabled
    DateTime startTime; //the time this application was started
    string w_main_path;//windows path to main directory
    string w_image_path; //windows path to images
    DroneControl[] screens; //holds all the screens that have spawned
    public int curveRadius; //radius of curvature of screens
    public float userRadius;//raidus of cylinder around user
    public Material main; //default material for screens
    public PhysicMaterial physmaterial; //physics material for screens
    public Collider UISphere; //collider of cylinder around user
    public Vector3[] oneFeed; //spawn locations for one screen
    public Vector3[] twoFeeds;
    public Vector3[] threeFeeds;
    public Vector3[] fourFeeds; //spawn locations for four screens
    int current; //current screen being spawnd



    /*
     * Precondition:    Requires the total_feeds, the active variables and mainpath to be set
     *                  Also requires curveRadius and userRadius to be set
     * Postcondition:   Creates and intializes a Screen with an attached DroneControl script
     * Notes:
     * 
     */
    GameObject createScreen() {
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Plane);
        screen.transform.localScale = new Vector3(.5333f, 1, .3f);
        screen.transform.position = new Vector3(-10, 0, 0);
        screen.GetComponent<MeshRenderer>().material = main;
        Mesh theMesh = screen.GetComponent<MeshFilter>().mesh;

        //Destroy(screen.GetComponent<MeshCollider>());
        screen.GetComponent<MeshCollider>().convex = true;
        //screen.GetComponent<MeshCollider>().isTrigger = true;
        BoxCollider collider = screen.AddComponent<BoxCollider>();
        collider.material = physmaterial;
        Rigidbody body = screen.AddComponent<Rigidbody>();
        //body.isKinematic = true;
        body.useGravity = false;
        body.drag = .5f;
        collider.size = new Vector3(13, 2f, 13); // All these numbers need to be dynamically set
        //at some point
        //the center has to do with y positions of the points on the plane
        //the size has to do with idk???? it's definitely not a box
        //10 is probably good always, plane dimensions are weird, 1:1 is good for the 
        //y dimensions because that mean's its set to real distance on the collider center
        //can confirm, actually 1 unit wide
        //setting to .5 will technically include all points
        //keep track of min and max y values when transforming plane to get y sizecollider.center = new Vector3(0, .4f, 0);
        Vector3[] vertices = theMesh.vertices;
        //Map all the vertices to a new, curved location
        for (int i = 0; i < theMesh.vertexCount; i++) {
            Vector3 old = vertices[i];
            //Debug.Log("old " + old.y + " new " + (Math.Sqrt(225 + old.x * old.x) - 15));
            vertices[i] = new Vector3(old.x, Mathf.Sqrt(225 + old.x * old.x) - 15, old.z);
        }
        theMesh.vertices = vertices;
        theMesh.RecalculateBounds();
        theMesh.RecalculateNormals();
        screen.AddComponent<DroneControl>();
        return screen;
    }







    // Use this for initialization
    void Start () {
        current = 0;
        oneFeed = new Vector3[1];
        twoFeeds = new Vector3[2];
        threeFeeds = new Vector3[3];
        fourFeeds = new Vector3[4];
        oneFeed[0] = new Vector3(0, 1, -15);
        twoFeeds[1] = new Vector3(-3, 1, -14.6969f);
        twoFeeds[0] = new Vector3(3, 1, -14.6969f);
        threeFeeds[2] = new Vector3(-3, -1.5f, -14.6969f);
        threeFeeds[1] = new Vector3(0, 2.5f, -15);
        threeFeeds[0] = new Vector3(3, -1.5f, -14.6969f);
        fourFeeds[2] = new Vector3(-3, -1.5f, -14.6969f);
        fourFeeds[3] = new Vector3(-3, 2.5f, -14.6969f);
        fourFeeds[0] = new Vector3(3, -1.5f, -14.6969f);
        fourFeeds[1] = new Vector3(3, 2.5f, -14.6969f);
        userRadius = GetComponent<CapsuleCollider>().radius;
        startTime = DateTime.Now;
        string data = "";
        UISphere = GetComponent<CapsuleCollider>();
        // the shell will generate a config file for every single run of main
        // unity will always read from this config file
        // need to implement some parsing so that # are ignored
        try {
            StreamReader sr = new StreamReader("C:/Users/Public/.config");
            //read the data
            while (!sr.EndOfStream) {
                data = sr.ReadLine();
                //get vlc mode
                if (data.Contains("vlc")) {
                    if (data.Contains("mode")) {
                        if (data.Contains("on"))
                            vlc_active = true;
                        else
                            vlc_active = false;
                    }

                }
                //find out whether or not matlab is on
                else if (data.Contains("matlab")) {
                    if (data.Contains("mode")) {
                        //matlab-mode=
                        if (data.Contains("on")) {
                            matlab_active = true;
                            target_detector_active = false;
                        }
                        else if (data.Contains("compiled")) {
                            matlab_active = target_detector_active = true;
                        }
                        //if the command cannot be read, assume false
                        else {
                            matlab_active = target_detector_active = false;
                        }
                    }
                }
                //Get the number of feeds
                //Make it easier to scale as opposed to assuming 1-3 feeds
                else if (data.Contains("camera-feeds=")) {
                    total_feeds = Convert.ToInt32(data.Substring(13, data.Length - 13));
                }
                //Get the main location of the directories
                //probably not necessary
                //this would also needs to be converted to windows
                else if (data.Contains("windows-location=")) {
                    w_main_path = data.Substring(17, data.Length - 17);
                    w_image_path = w_main_path + "/Images";
                    //Debug.Log(w_main_path);
                    //Debug.Log(w_image_path);
                    //Debug.Log(data);
                }
                else if (data.Contains("unity-spawns=")) {
                   //Parser not written yet, please input spawns manually

                }
            }
        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }
        //total_feeds = 2;    
        screens = new DroneControl[total_feeds];
        //create all the screens
        //Debug.Log("Making " + total_feeds + " screen" + (total_feeds == 1 ? "" : "s"));
        spawnScreen(0);
    }

    /*
     * Precondition:    Start function must get data from config file
     * Postcondition:   Creates all the screens
     * Notes
     */
    void spawnScreen(int i) {
        screens[i] = createScreen().GetComponent<DroneControl>();
        Dictionary<string, string> info = new Dictionary<string, string>();
        info["feed"] = Convert.ToString(i + 1);
        info["location"] = w_main_path;
        info["matlab_active"] = Convert.ToString(matlab_active);
        info["vlc_active"] = Convert.ToString(vlc_active);
        info["target_detector_active"] = Convert.ToString(target_detector_active);
        info["user_radius"] = Convert.ToString(userRadius);
        screens[i].Initialize(info);
        screens[i].UISphere = UISphere;
        float angle = -Mathf.PI * i / 8.0f;
        //assign locations to all screens
        switch (total_feeds) {
            case 1:
                screens[i].transform.position = oneFeed[i];
                break;
            case 2:
                screens[i].transform.position = twoFeeds[i];
                break;
            case 3:
                screens[i].transform.position = threeFeeds[i];
                break;
            case 4:
                screens[i].transform.position = fourFeeds[i];
                break;
        }
        screens[i].setRotation();
    }

    // Update is called once per frame
    void Update () {
        //check to see if the previous screen has finished spawning
        //and there are still screens left to spawn
        if (screens[current].moved && current < total_feeds - 1) {
            current++;
            spawnScreen(current);
        }
	}
    

    /*
     * Precondition:    None
     * Postcondition:   Sends a Kill signal to VLC, deletes image files
     * Notes:
     */
    private void OnApplicationQuit() {
        if (matlab_active && !target_detector_active) {
            //probably possible to predict which instance of Matlab to kill
            //by investigating its start-time
            foreach (var process in
                     System.Diagnostics.Process.GetProcessesByName("matlab")) {
                //indicates that this instance of Matlab started after the 
                //program began executing, likely indicates that it was started
                //by main.sh
                Debug.Log("found some matlab");
                if (process.StartTime.Add(new TimeSpan(0, 0, 30)) > startTime) {
                    Debug.Log("the Matlab is young " + process.StartTime);
                    process.CloseMainWindow();
                    process.Close();
                }
            }
        }
        if (target_detector_active) {
            Debug.Log("searching for Target_detector");
            bool done = false;
            DateTime quit_time = DateTime.Now;
            while (!done && DateTime.Now < quit_time.Add(new TimeSpan(0, 0, 10))) {
                try {
                    System.IO.StreamWriter file =
                        new System.IO.StreamWriter(w_main_path + "/.kill.txt");
                    file.Write("Execution has finished");
                    file.Close();
                    done = true;
                }
                catch (Exception e) {
                    Debug.Log(e.Message);
                    Debug.Log("could not open kill file");
                }
            }
        }

        //only need to close VLC and delete images if VLC is active
        if (vlc_active) {
            }
            //remove extra files
            for (int i = 1; i <= total_feeds; i++) {
                string[] files = Directory.GetFiles(w_image_path + "/Feed" + i);
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
