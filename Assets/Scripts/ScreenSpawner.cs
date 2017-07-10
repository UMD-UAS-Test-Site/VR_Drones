using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class ScreenSpawner : MonoBehaviour {


    int total_feeds;
    bool matlab_active;
    bool target_detector_active;
    bool vlc_active;
    DateTime startTime;
    string w_main_path;
    string w_image_path;
    Mesh theMesh;
    DroneControl[] screens;
    public int curveRadius;
    public int userRadius;
    public List<Vector3> spawnPositions;
    public Material main;



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
        Destroy(screen.GetComponent<MeshCollider>());
        BoxCollider collider = screen.AddComponent<BoxCollider>();
        /*Rigidbody body = screen.AddComponent<Rigidbody>();
        //body.isKinematic = true;
        body.useGravity = false;*/
        collider.size = new Vector3(10, 1, 10); // All these numbers need to be dynamically set
        //at some point
        //the center has to do with y positions of the points on the plane
        //the size has to do with idk???? it's definitely not a box
        //10 is probably good always, plane dimensions are weird, 1:1 is good for the 
        //y dimensions because that mean's its set to real distance on the collider center
        //can confirm, actually 1 unit wide
        //setting to .5 will technically include all points
        //keep track of min and max y values when transforming plane to get y sizecollider.center = new Vector3(0, .4f, 0);
        Vector3[] vertices = theMesh.vertices;
        for (int i = 0; i < theMesh.vertexCount; i++) {
            Vector3 old = vertices[i];
            //Debug.Log("old " + old.y + " new " + (Math.Sqrt(225 + old.x * old.x) - 15));
            vertices[i] = new Vector3(old.x, Mathf.Sqrt(225 + old.x * old.x) - 15, old.z);
        }
        theMesh.vertices = vertices;
        theMesh.RecalculateBounds();
        theMesh.RecalculateNormals();
        //change y to 90 to make the orientation correct
        //screen.transform.Rotate(0, -90, -90);
        byte[] img = System.IO.File.ReadAllBytes("D:/Files/DroneSwarm/Test1.jpg");
        Texture2D image_tex = new Texture2D(360, 640,
            TextureFormat.DXT1, false);
        //if the image does not load; This is problematic
        if (!image_tex.LoadImage(img)) {
            Debug.Log("mega failure");
        }
        screen.GetComponent<Renderer>().material.mainTexture = image_tex;
        /*DroneControl controller =*/ screen.AddComponent<DroneControl>();
        return screen;
    }

    // Use this for initialization
    void Start () {
        spawnPositions = new List<Vector3>();
        spawnPositions.Add(new Vector3(0, 0, 0));
        //create Plane
       
        //rotation is always X 90 and the Y Rotation changes the image moves

        //TODO: add a line here changing the mesh collider to be a box collider
        /*
          
          
        toDraw = new List<Vector3>();
        bad = false;
        
        GameObject ob = Instantiate(new_screen, new Vector3(0, 0, -10), Quaternion.identity);
        Debug.Log("something");
        Mesh myMesh = ob.GetComponentInChildren<MeshFilter>().mesh;
        foreach (Vector3 v in myMesh.vertices) {
            Debug.Log(v);
        }
        for (int i = 0; i < myMesh.triangles.Length - 3; i++) {
            Debug.Log(myMesh.triangles[i]
                + " " + myMesh.triangles[i + 1]
                + " " + myMesh.triangles[i + 2]);
        }
        return;
        
        
         
        //also screens should be spawned behind the camera
        GameObject obj = Instantiate(original_screen, new Vector3(0, 0, 0), Quaternion.identity);
        //obj.transform.Rotate(90, 0, 0);
        theMesh = obj.GetComponentInChildren<MeshFilter>().mesh;
        //theMesh.subMeshCount = 2;
        //theMesh.SetTriangles
        //return;
        int[] subMesh = new int[72];


        for (int i = 0; i < theMesh.vertices.Length; i++) {
            ver.Add(theMesh.vertices[i]);
        }
        //Debug.Log()
        //theMesh.Clear();
        //theMesh.subMeshCount = 2;
        
        int k = 0; //vertex counter
                   //find all the triangles that matter
        for (int i = 0; i < 24; k += 3) {
            if (k > 297) {
                Debug.Log("Only found " + i);
                break;
            }
            bool con = false;
            //go through all three vertices in the triangle
            for (int j = k; j < k + 3; j++) {
                Vector3 temp = theMesh.vertices[theMesh.triangles[j]];
                Vector3 test = new Vector3(0, temp.y, temp.z);
                Debug.Log(Vector3.Distance(test, new Vector3(0, 0, -4.7f)));
                //Check to see if the distance is bad
                if (Vector3.Distance(test, new Vector3(0, 0, -4.7f)) > 5.0f) {
                    Debug.Log("Cut offs by radius");
                    con = true;
                    break;
                }
            }
            //this triangle is no good
            if (con) {
                continue;
            }
            Vector3 v1 = theMesh.vertices[theMesh.triangles[k]]
                - theMesh.vertices[theMesh.triangles[k + 1]];
            Vector3 v2 = theMesh.vertices[theMesh.triangles[k]]
                - theMesh.vertices[theMesh.triangles[k + 2]];
            Vector3 v3 = Vector3.Cross(v1, v2).normalized;
            if (Mathf.Abs(v3.x) < .3f && Mathf.Abs(v3.y) < .3f && Mathf.Abs(v3.z) > .5f) {
                Debug.Log("found some");
                for (int l = 0; l < 3; l++) {
                    subMesh[i + l] = theMesh.triangles[k + l]; 
                }
                i++;
            }
            else {
                //Debug.Log("v3 is" + v3);
                //Debug.Log("v2 is" + v2);
                //Debug.Log("v1 is " + v1);
                Debug.Log("massive failure");
            }
            if (i == 24) {
                Debug.Log("found them all");
                theMesh.subMeshCount = 2;
                theMesh.SetTriangles(subMesh, 1);
            }
        }
        MeshRenderer mr = obj.GetComponentInChildren<MeshRenderer>();
        byte[] img = System.IO.File.ReadAllBytes("D:/Files/DroneSwarm/Test1.jpg");
        Texture2D image_tex = new Texture2D(1, 1, TextureFormat.DXT1, false);
        if (!image_tex.LoadImage(img)) {
            Debug.Log("mega failure");
        }
        //obj.GetComponentInChildren<Renderer>().materials[1].mainTexture = image_tex;
        return;
        */



        startTime = DateTime.Now;
        string data = "";
        // the shell will generate a config file for every single run of main
        // unity will always read from this config file
        // need to implement some parsing so that # are ignored
        try {
            StreamReader sr = new StreamReader("C:/Users/Public/.config");
            while (!sr.EndOfStream) {
                data = sr.ReadLine();
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
            }
        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }
        //total_feeds = 2;    
        screens = new DroneControl[total_feeds];
        //create all the screens
        Debug.Log("Making " + total_feeds + " screen" + (total_feeds == 1 ? "" : "s"));
        for (int i = 0; i < total_feeds; i++) {
            screens[i] = createScreen().GetComponent<DroneControl>();
            screens[i].Initialize(i+1, w_main_path,
            matlab_active, vlc_active, target_detector_active);
            float angle = -Mathf.PI * i / 8.0f;
            screens[i].transform.position = new Vector3(Mathf.Cos(angle) * userRadius, i % 2 == 0 ? 0:5, Mathf.Sin(angle) * userRadius);
            screens[i].setRotation();

        }
    }

    // Update is called once per frame
    void Update () {
		
	}
    

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
            //Look For VLC instances and Close them
            /*foreach (var process in System.Diagnostics.Process.GetProcesses()) {
                try {
                    Debug.Log(process.ProcessName);
                }
                catch {

                }
            } */

            //foreach (var process in System.Diagnostics.Process.GetProcessesByName("vlc")) {
            //    process.CloseMainWindow();
            //    process.Close();
            }
            //remove extra files
            for (int i = 1; i <= 4; i++) {
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
