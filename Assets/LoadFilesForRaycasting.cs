using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class LoadFilesForRaycasting : MonoBehaviour {


    GameObject camHolder;

    List<Vector3> camPositions;

    List<Vector3> camNormals;
    List<Vector3> camPerps;

    public GameObject scannedObj;

    public float sensorHeight = 35;
    public float focalLength = 110;

    public Vector2 imageDim = new Vector2(8688, 5792);

    public float imageScale = 0.3f;

    public float maxRange = 10;

    public Material notSelected;
    public Material selected;

    public float coCNumber = 0.029f;
    public float aperture = 13;

    public float realScale;



    public bool automaticCapture = false;



    List<Vector2> currTiePoints;

    int[] triangles;
    MeshFilter mf;
    Vector3[] vertices;

    Color[] initColors;

    Color[] resetColor;


    int[] seenPixels;


    int counter = 0;
    int dofCounter = 0;

    float dofClosestDist_old = 0f;

    int seenCounter = 0;

    int normDirCounter = 0;

    int[] multiviewSeenPixels;

    int[,] focusPixels;

    float[,] visualAngles;

    Vector3[,] visualDirections;

	// Use this for initialization
	void Start () {
        camHolder = GameObject.Find("CamHolder");

        camPositions = new List<Vector3>();
        camNormals = new List<Vector3>();
        camPerps = new List<Vector3>();





        string cameraPath = "Assets/CameraSeenPixels_things/Bunny/cameraPos.txt";

        LoadStuff(cameraPath, out camPositions);

        
        for (int i = 0; i < camPositions.Count; i++)
        {

            //camPositions[i] *= 2; // Remove this if not for really small objects that have not been made larger

            //camPositions[i] *= 2;// Remove this if not for really small objects that have not been made larger

            camPositions[i] *= realScale;

            
        }
        scannedObj.transform.localScale*=realScale;

        string normalPath = "Assets/CameraSeenPixels_things/Bunny/cameraNorm.txt";

        LoadStuff(normalPath, out camNormals);

        string perpPath = "Assets/CameraSeenPixels_things/Bunny/cameraPerp.txt";

        LoadStuff(perpPath, out camPerps);


        mf = scannedObj.GetComponent<MeshFilter>();


        

        
       

        triangles = mf.mesh.triangles;
        vertices = mf.mesh.vertices;


        Debug.Log(mf.mesh.vertexCount);

        Debug.Log("Verts " + vertices.Length + " tringle " + triangles.Length);
        initColors = new Color[vertices.Length];

        resetColor = new Color[vertices.Length];

        seenPixels = new int[vertices.Length];


        multiviewSeenPixels = new int[vertices.Length];

        focusPixels = new int[vertices.Length, camPositions.Count];

        visualAngles = new float[vertices.Length, camPositions.Count];

        visualDirections = new Vector3[vertices.Length, camPositions.Count];


        for (int i = 0; i < initColors.Length; i++)
        {
            initColors[i] = new Color(1f, 1f, 1f);

            resetColor[i] = new Color(1f, 1f, 1f);

            seenPixels[i] = 0;

            multiviewSeenPixels[i] = 0;

            

            

            for (int j = 0; j < camPositions.Count; j++)
            {
                visualAngles[i, j] = -1;

                visualDirections[i, j] = new Vector3(-1f, -1f, -1f);


                focusPixels[i, j] = 0;
            }
        }

        //resetColor = (Color[])initColors.Clone();


        


        

        Debug.Log(Screen.width + " | " + Screen.height);

        for (int i = 0; i < camPositions.Count; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            cube.transform.position = camPositions[i];

            cube.transform.localScale = new Vector3(0.08f, 0.04f, 0.01f);

            cube.transform.Rotate(new Vector3(0, 180, 0));

            cube.transform.SetParent(camHolder.transform);

            //cube.transform.up = camPerps[i];

            //cube.transform.forward = camNormals[i];

            Quaternion rotation = Quaternion.LookRotation(camNormals[i], camPerps[i]);
            cube.transform.rotation = rotation;

            

            cube.GetComponent<Collider>().enabled = false;




            cube.AddComponent<Camera>();

            //cube.GetComponent<Camera>().rect = new Rect(x,y,1,1);

            cube.GetComponent<Camera>().fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan(sensorHeight / (2 * focalLength));

            cube.GetComponent<Camera>().nearClipPlane = 0.001f;

            cube.GetComponent<Camera>().enabled = false;

            //// set the desired aspect ratio (the values in this example are
            //// hard-coded for 16:9, but you could make them into public
            //// variables instead so you can set them at design time)
            //float targetaspect = 3.0f / 2.0f;

            //// determine the game window's current aspect ratio
            //float windowaspect = (float)Screen.width / (float)Screen.height;

            //// current viewport height should be scaled by this amount
            //float scaleheight = windowaspect / targetaspect;



            //// if scaled height is less than current height, add letterbox
            //if (scaleheight < 1.0f)
            //{
            //    Rect rect = cube.GetComponent<Camera>().rect;

            //    rect.width = 1.0f;
            //    rect.height = scaleheight;
            //    rect.x = 0;
            //    rect.y = (1.0f - scaleheight) / 2.0f;

            //    cube.GetComponent<Camera>().rect = rect;
            //}
            //else // add pillarbox
            //{
            //    float scalewidth = 1.0f / scaleheight;

            //    Rect rect = cube.GetComponent<Camera>().rect;

            //    rect.width = scalewidth;
            //    rect.height = 1.0f;
            //    rect.x = (1.0f - scalewidth) / 2.0f;
            //    rect.y = 0;

            //    cube.GetComponent<Camera>().rect = rect;
            //}


           
            
        }

        Vector3 tweakPos = camHolder.transform.position;
        tweakPos.y += 0.02f;
        tweakPos.x += 0.02f;

        camHolder.transform.position = tweakPos;

        coCNumber /= 1000;
        focalLength /= 1000;

        string allTiePointsDir = "Assets/CameraSeenPixels_things/Bunny/goodTies";
        DirectoryInfo dir = new DirectoryInfo(allTiePointsDir);
        FileInfo[] info = dir.GetFiles("*.txt");
        //foreach (FileInfo f in info)
        //{
        //    Debug.Log(f.Name);
        //}










	}

    void LoadImagePoints(string loadPath, out List<Vector2> loadTo)
    {
        StreamReader reader = new StreamReader(loadPath);

        loadTo = new List<Vector2>();
        string itemStrings = reader.ReadLine();
        char[] delimiter = { ',' };

        while (itemStrings != null)
        //for (int j = 0; j < 1000; j++)

        {
            string[] fields = itemStrings.Split(delimiter);



            Vector2 currVec2 = Vector2.zero;
            for (int i = 0; i < fields.Length; i++)
            {
                //Debug.Log(i + "  " + fields[i]);
               

                    

                    if (i == 0)
                    {
                        currVec2[i] = float.Parse(fields[i],
                      CultureInfo.InvariantCulture);

                    }
                    else
                    {
                        currVec2[i] = float.Parse(fields[i],
                      CultureInfo.InvariantCulture);

                    }

                    //Debug.Log(currVec2);

            }


            loadTo.Add(currVec2);

            itemStrings = reader.ReadLine();
        }
    }



    void LoadStuff(string loadPath, out List<Vector3> loadTo)
    {
        StreamReader reader = new StreamReader(loadPath);

        loadTo = new List<Vector3>();
        string itemStrings = reader.ReadLine();
        char[] delimiter = { ',' };

        while (itemStrings != null)
        {
            string[] fields = itemStrings.Split(delimiter);



            Vector3 currVec3 = Vector3.zero;
            for (int i = 0; i < fields.Length; i++)
            {
                //Debug.Log(i + "  " + fields[i]);
                if (i == 0)
                {
                    currVec3[i] = -float.Parse(fields[i],
                      CultureInfo.InvariantCulture);

                }
                else
                {
                    currVec3[i] = float.Parse(fields[i],
                      CultureInfo.InvariantCulture);

                }
               

               // Debug.Log(currVec3);

            }


            loadTo.Add(currVec3);

            itemStrings = reader.ReadLine();
        }
    }



    void ChangeAspectRatio(Camera camera, float aspX, float aspY)
    {
        // set the desired aspect ratio (the values in this example are
        // hard-coded for 16:9, but you could make them into public
        // variables instead so you can set them at design time)
        float targetaspect = aspX / aspY;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;

        // obtain camera component so we can modify its viewport


        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            camera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }

	
	// Update is called once per frame
	void Update () {

        foreach (Transform trans in camHolder.transform)
        {
            Debug.DrawRay(trans.position, trans.forward * 10000, Color.green);

        }

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    mf.mesh.colors =resetColor;
        //}

        if (Input.GetKeyDown(KeyCode.Space))
        {

            initColors = new Color[vertices.Length];

            bool[] isChecked = new bool[vertices.Length];

            for (int i = 0; i < initColors.Length; i++)
            {
                initColors[i] = new Color(1f, 1f, 1f);

                isChecked[i] = false;
            }


            mf.mesh.colors = initColors;





            string allTiePointsDir = "Assets/CameraSeenPixels_things/Bunny/goodTies";
            DirectoryInfo dir = new DirectoryInfo(allTiePointsDir);
            FileInfo[] info = dir.GetFiles("*.txt");
            
            currTiePoints = new List<Vector2>();

            if (counter>=info.Length)
            {
                counter = 0;
            }

            string currTiePDir = allTiePointsDir + "/" + info[counter].Name;
            Debug.Log(currTiePDir);
            LoadImagePoints(currTiePDir, out currTiePoints);


            Camera currCamera = camHolder.transform.GetChild(counter).gameObject.GetComponent<Camera>();

            currCamera.GetComponent<Camera>().enabled = true;

            camHolder.transform.GetChild(counter).gameObject.GetComponent<Renderer>().material = selected;

            if (counter != 0)
            {
                camHolder.transform.GetChild(counter - 1).gameObject.GetComponent<Renderer>().material = notSelected;
            }
            else
            {
                camHolder.transform.GetChild(info.Length-1).gameObject.GetComponent<Renderer>().material = notSelected;
            }
            

            //ChangeAspectRatio(currCamera, 3, 2);

            Texture2D tex = new Texture2D((int)(imageDim[0] * imageScale), (int)(imageDim[1] * imageScale), TextureFormat.RGB24, false);

            for (int i = 0; i < currTiePoints.Count; i++)
            {
                //float viewPort_x = currTiePoints[i].x / (imageDim[0] * imageScale);
                //float viewPort_y = currTiePoints[i].y / (imageDim[1] * imageScale);

                //Debug.Log(currTiePoints[i].x + " | " + currTiePoints[i].y);
                RaycastHit hit;
                Ray ray = currCamera.ViewportPointToRay(new Vector3(currTiePoints[i].y, 1-currTiePoints[i].x, 0f));
                Debug.DrawRay(ray.origin, ray.direction * 1, Color.red, 60);
                //if (Physics.Raycast(ray, out hit, maxRange))
                //{
                //    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 60);

                //}

                if (Physics.Raycast(ray, out hit, maxRange))
                {

                    for (int m = 0; m < 3; m++)
                    {
                        var test = triangles[hit.triangleIndex * 3 + m];

                        initColors[test] = Color.red;

                        if (isChecked[test] == false)
                        {
                            seenPixels[test] += 1;
                            isChecked[test] = true;
                        }

                        
                    }

                }



                tex.SetPixel((int)(currTiePoints[i].y * (imageDim[1] * imageScale)), (int)(currTiePoints[i].x * (imageDim[0] * imageScale)), new Color(1, 0, 0));

            }

            tex.Apply();

            mf.mesh.colors = initColors;

            byte[] bytes = tex.EncodeToPNG();

            //File.WriteAllBytes(Application.dataPath + "/../Assets/CameraSeenPixels_things/AngeltestImage2.png", bytes);

            Debug.Log("Image " + counter + " Finished at " + Time.deltaTime);
            //currCamera.GetComponent<Camera>().enabled = false;

            counter++;
        }



        //if (Input.GetKeyDown(KeyCode.C))
        //{

        //    StreamWriter streamWriter = new StreamWriter("C:\\Users\\ivan\\Documents\\UnityRaycastTest\\Assets\\CameraSeenPixels_things\\Angel\\seenPixels.txt");
        //    string output = "";

        //    for (int i = 0; i < seenPixels.Length; i++)
        //    {
        //        output = seenPixels[i].ToString();

        //        streamWriter.WriteLine(output);

        //    }

        //    streamWriter.Close();

        //    Debug.Log("SAVED");

        //}

        //Calculate the in/out of focus parts
        if (Input.GetKeyDown(KeyCode.V))
        {


            if (dofCounter >= camPositions.Count)
            {
                dofCounter = 0;
            }

            Camera currCamera = camHolder.transform.GetChild(dofCounter).gameObject.GetComponent<Camera>();

            camHolder.transform.GetChild(dofCounter).gameObject.GetComponent<Renderer>().material = selected;


            if (dofCounter != 0)
            {
                camHolder.transform.GetChild(dofCounter - 1).gameObject.GetComponent<Renderer>().material = notSelected;
            }
            else
            {

                camHolder.transform.GetChild(camPositions.Count - 1).gameObject.GetComponent<Renderer>().material = notSelected;
            }


            currCamera.GetComponent<Camera>().enabled = true;

            float rayDensity_x = 1.0f / (imageDim[0] * imageScale);
            float rayDensity_y = 1.0f / (imageDim[1] * imageScale);



            //Calculate DOF near and far points

            
            float hyperfocalDist = (focalLength * focalLength) / (aperture * (coCNumber) );

            Ray closestDistRay = currCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit closestDistHit;
            float closestDist = 0;

            if (Physics.Raycast(closestDistRay, out closestDistHit, maxRange))
            {
                closestDist = closestDistHit.distance;
                
            }

            if (closestDist == 0)
            {
                for (float i = 0; i < 1; i= i+0.1f)
                {
                    closestDistRay = currCamera.ViewportPointToRay(new Vector3(0.5f, i, 0f));
                    if (Physics.Raycast(closestDistRay, out closestDistHit, maxRange))
                    {
                        closestDist = closestDistHit.distance;
                        break;
                    }
                }


                //closestDist = dofClosestDist_old; 
                
            }

            if (closestDist == 0)
            {
                closestDist = dofClosestDist_old; 
            }




            float nearFocalPoint = (hyperfocalDist * closestDist) / (hyperfocalDist + (closestDist - focalLength));
            float farFocalPoint = (hyperfocalDist * closestDist) / (hyperfocalDist - (closestDist - focalLength));

           // Debug.Log(nearFocalPoint + " | " + farFocalPoint);
            
            bool[] checkedPixels = new bool[vertices.Length];
            float maxTest = 0;
            float minTest = 999999;
            for (float i = 0; i < 1; i += rayDensity_x)
            {
                for (float j = 0; j < 1; j += rayDensity_y)
                {

                    RaycastHit hit;
                    Ray ray = currCamera.ViewportPointToRay(new Vector3(i, j, 0f));
                  //  Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 60);


                    if (Physics.Raycast(ray, out hit, maxRange))
                    {
                        if (maxTest < hit.distance)
                        {
                            maxTest = hit.distance;
                        }

                        if (minTest > hit.distance)
                        {
                            minTest = hit.distance;
                        }
                        //Debug.Log(hit.distance);
                        //if (hit.distance*1000f > farFocalPoint)
                        //{
                        //    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 60);
                        //}
                        //else if (hit.distance * 1000f < nearFocalPoint)
                        //{
                        //    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.blue, 60);
                        //}
                        //else
                        //{
                        //    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 60);
                        //}

                        //if (hit.distance * 1000f < farFocalPoint && hit.distance *1000f > nearFocalPoint)
                        //{
                        //    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 60);
                        //}

                        if (hit.distance > farFocalPoint || hit.distance < nearFocalPoint)
                        {
                            //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 10);

                            Debug.DrawRay(hit.point, hit.normal * 0.0001f, Color.red, 10);

                            for (int m = 0; m < 3; m++)
                            {

                                var test = triangles[hit.triangleIndex * 3 + m];

                                if (checkedPixels[test] == false)
                                {

                                    focusPixels[test, dofCounter] = -1;
                                    checkedPixels[test] = true;
                                }

                            }


                            
                        }
                        else
                        {
                            for (int m = 0; m < 3; m++)
                            {

                                var test = triangles[hit.triangleIndex * 3 + m];

                                if (checkedPixels[test] == false)
                                {

                                    focusPixels[test, dofCounter] = 1;
                                    checkedPixels[test] = true;
                                }

                            }
                        }

                       

                    }

                }
            }

            Debug.Log("Camera " + dofCounter +  " Near focal dist " + nearFocalPoint + " | Far focal dist " + farFocalPoint + " | Minimum seen vertex" + minTest + " | Maximum seen vertex" + maxTest);
            //Debug.Log("Maximum seen vertix " + maxTest);
            //Debug.Log("Minimum seen vertix " + minTest);
            dofCounter++;

            dofClosestDist_old = closestDist;

            //currCamera.GetComponent<Camera>().enabled = false;

        }








        
        //Calculate number of cameras
        if (Input.GetKeyDown(KeyCode.B))
        {

            Debug.Log("Button pressed");
            if (seenCounter >= camPositions.Count)
            {
                seenCounter = 0;
            }

            
            Camera currCamera = camHolder.transform.GetChild(seenCounter).gameObject.GetComponent<Camera>();

            camHolder.transform.GetChild(seenCounter).gameObject.GetComponent<Renderer>().material = selected;
            currCamera.GetComponent<Camera>().enabled = true;

            if (seenCounter != 0)
            {
                camHolder.transform.GetChild(seenCounter - 1).gameObject.GetComponent<Renderer>().material = notSelected;
            }
            else
            {

                camHolder.transform.GetChild(camPositions.Count - 1).gameObject.GetComponent<Renderer>().material = notSelected;
            }


            float rayDensity_x = 1f / (imageDim[0] * imageScale);
            float rayDensity_y = 1f / (imageDim[1] * imageScale);



            bool[] checkedPixels = new bool[vertices.Length];

            for (float i = 0; i < 1; i += rayDensity_x)
            {
                for (float j = 0; j < 1; j += rayDensity_y)
                {
                    
                    RaycastHit hit;
                    Ray ray = currCamera.ViewportPointToRay(new Vector3(i, j, 0f));
                    //Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 60);


                    if (Physics.Raycast(ray, out hit, maxRange))
                    {
                        //Debug.DrawRay(hit.point, hit.normal * 0.0001f, Color.blue, 30);
                        
                        for (int m = 0; m < 3; m++)
                        {

                            var test = triangles[hit.triangleIndex * 3 + m];

                            if (checkedPixels[test] == false)
                            {
                                multiviewSeenPixels[test] += 1;
                                checkedPixels[test] = true;




                                if (multiviewSeenPixels[test] == 0)
                                {
                                    Debug.DrawRay(hit.point, hit.normal * 0.001f, Color.red, 10);
                                }

                                if (multiviewSeenPixels[test] == 1)
                                {
                                    Debug.DrawRay(hit.point, hit.normal * 0.001f, Color.blue, 10);
                                }

                                if (multiviewSeenPixels[test] > 1)
                                {
                                    Debug.DrawRay(hit.point, hit.normal * 0.001f, Color.green, 10);
                                }

                            }

                        }

                        


                    }

                }
            }

            Debug.Log("Finished with camera " + seenCounter);
            seenCounter++;
            //currCamera.GetComponent<Camera>().enabled = false;
        }





        // Save the angles between the normals and the camera
        if (Input.GetKeyDown(KeyCode.N))
        {


            if (normDirCounter >= camPositions.Count)
            {
                normDirCounter = 0;
            }

            

            Camera currCamera = camHolder.transform.GetChild(normDirCounter).gameObject.GetComponent<Camera>();

            camHolder.transform.GetChild(normDirCounter).gameObject.GetComponent<Renderer>().material = selected;

            currCamera.enabled = true;


            if (normDirCounter != 0)
            {
                camHolder.transform.GetChild(normDirCounter - 1).gameObject.GetComponent<Renderer>().material = notSelected;
            }
            else
            {

                camHolder.transform.GetChild(camPositions.Count - 1).gameObject.GetComponent<Renderer>().material = notSelected;
            }


            float rayDensity_x = 1f / (imageDim[0] * imageScale);
            float rayDensity_y = 1f / (imageDim[1] * imageScale);





            bool[] checkedPixels = new bool[vertices.Length];

            Vector3 camForward = currCamera.transform.forward;

            for (float i = 0; i < 1; i += rayDensity_x)
            {
                for (float j = 0; j < 1; j += rayDensity_y)
                {

                    RaycastHit hit;
                    Ray ray = currCamera.ViewportPointToRay(new Vector3(i, j, 0f));
                    //  Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 60);


                    if (Physics.Raycast(ray, out hit, maxRange))
                    {


                        for (int m = 0; m < 3; m++)
                        {

                            if (hit.triangleIndex != -1 && hit.triangleIndex*3 < triangles.Length)
                            {
                                var test = triangles[hit.triangleIndex * 3 + m];

                                if (checkedPixels[test] == false)
                                {

                                    Vector3 hitPointNorm = hit.normal;

                                    float angle = Mathf.Acos(Vector3.Dot(camForward, hitPointNorm) / (camForward.magnitude * hitPointNorm.magnitude));

                                    float angle360 = (angle * Mathf.Rad2Deg + 360f) % 360f;

                                    visualAngles[test, normDirCounter] = angle360;


                                    visualDirections[test, normDirCounter] = Vector3.Cross(camForward, hitPointNorm);


                                    checkedPixels[test] = true;



                                }
                            }
                            

                        }




                    }

                }
            }


            normDirCounter++;

            Debug.Log("Finished camera " + normDirCounter);

            //currCamera.enabled = false;

        }




        //Save system - 1 - save seen verts, 2 - save angle orientation, 3 - save in-focus, 4 - save feature point reprojection

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StreamWriter streamWriter = new StreamWriter("Assets\\SavedFiles\\vertsSeen.txt");
            string output = "";

            for (int i = 0; i < vertices.Length; i++)
            {
                output = multiviewSeenPixels[i].ToString();

                streamWriter.WriteLine(output);

            }

            streamWriter.Close();

            Debug.Log("Saved Seen Verts");
        }


        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StreamWriter streamWriter = new StreamWriter("Assets\\SavedFiles\\vertsAngleOrient.txt");

            

            for (int i = 0; i < vertices.Length; i++)
            {
                string output = "";

                for (int j = 0; j < camPositions.Count; j++)
                {
                    output += " " + visualAngles[i, j].ToString();
                }
                 //+ " " + visualDirections[i, j].x.ToString() + " " + visualDirections[i, j].y.ToString() + " " + visualDirections[i, j].z.ToString()

                streamWriter.WriteLine(output);

            }

            streamWriter.Close();

            Debug.Log("Saved Angle Orientations");
        }


        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StreamWriter streamWriter = new StreamWriter("Assets\\SavedFiles\\vertsInFocus.txt");



            for (int i = 0; i < vertices.Length; i++)
            {
                string output = "";

                for (int j = 0; j < camPositions.Count; j++)
                {
                    output += " " + focusPixels[i, j].ToString();
                }


                streamWriter.WriteLine(output);

            }

            streamWriter.Close();

            Debug.Log("Saved In Focus");
        }


        if (Input.GetKeyDown(KeyCode.Alpha4))
        {

            StreamWriter streamWriter = new StreamWriter("Assets\\SavedFiles\\projKeypoints.txt");
            string output = "";

            for (int i = 0; i < seenPixels.Length; i++)
            {
                output = seenPixels[i].ToString();

                streamWriter.WriteLine(output);

            }

            streamWriter.Close();

            Debug.Log("SAVED");

        }


        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Camera currCamera = camHolder.transform.GetChild(0).gameObject.GetComponent<Camera>();

        //    ChangeAspectRatio(currCamera, 3, 2);

        //    Texture2D tex = new Texture2D((int)(imageDim[0] * imageScale), (int)(imageDim[1] * imageScale), TextureFormat.RGB24, false);

        //    float rayDensity_x = 1f / (imageDim[0] * imageScale);
        //    float rayDensity_y = 1f / (imageDim[1] * imageScale);


        //    for (float i = 0; i < 1; i += rayDensity_x)
        //    {
        //        for (float j = 0; j < 1; j += rayDensity_y)
        //        {

        //            RaycastHit hit;
        //            Ray ray = currCamera.ViewportPointToRay(new Vector3(i, j, 0f));
        //            //Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 60);
        //            if (Physics.Raycast(ray, out hit, maxRange))
        //            {

        //                for (int m = 0; m < 3; m++)
        //                {
        //                    var test = triangles[hit.triangleIndex * 3 + m];

        //                    initColors[test] = Color.red;
        //                }




        //                // Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 60);

                        


        //                tex.SetPixel((int)(i * imageDim[0] * imageScale), (int)(j * imageDim[1] * imageScale), new Color(1, 0, 0));

        //            }

        //        }
        //    }

        //    mf.mesh.colors = initColors;

        //    tex.Apply();

        //    byte[] bytes = tex.EncodeToPNG();

        //    File.WriteAllBytes(Application.dataPath + "/../Assets/CameraSeenPixels_things/AngeltestImage.png", bytes);

        //    Debug.Log("Finished at " + Time.deltaTime);
        //}
        


		
	}
}
