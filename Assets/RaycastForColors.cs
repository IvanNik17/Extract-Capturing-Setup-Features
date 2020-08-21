using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class RaycastForColors : MonoBehaviour {


    public float maxRange = 100;

    float rayDensity_x = 0.005f;
    float rayDensity_y = 0.005f;

    GameObject[] camList;

    public float sensorHeight = 35;
    public float focalLength = 110;

    public Vector2 imageDim = new Vector2(8736, 5856);

    bool test = true;

    //Texture2D imageTex;

    
	// Use this for initialization
	void Start () {
        

        camList = GameObject.FindGameObjectsWithTag("cam");

        float numCams = (float)camList.Length;
        for (int i = 0; i < numCams; i++)
        {
            Debug.Log(Mathf.Rad2Deg * 2f * Mathf.Atan(sensorHeight / (2f * focalLength)));
            camList[i].GetComponent<Camera>().fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan(sensorHeight / (2 * focalLength));
        }


        //imageTex = new Texture2D((int)imageDim.x, (int)imageDim.y);

        //visualPlane.GetComponent<Renderer>().material.mainTexture = imageTex;

        // Color fillColor = new Color(1.0f, 0.0f, 0.0f);
        // var fillColorArray = imageTex.GetPixels();
 
        // for(var i = 0; i < fillColorArray.Length; ++i)
        // {
        //     fillColorArray[i] = fillColor;
        // }

        // imageTex.SetPixels(fillColorArray);

        // imageTex.Apply();


         //for (int y = 0; y < imageTex.height; y++)
         //{
         //    for (int x = 0; x < imageTex.width; x++)
         //    {

         //        imageTex.SetPixel(x, y, Color.green);
         //    }
         //}

         Camera currCam = camList[0].GetComponent<Camera>();

         //int[,] imageCoords = new int[(int)imageDim.x, (int)imageDim.y];

         List<Vector2> seenPixels = new List<Vector2>();



         for (float i = 0; i < 1; i += rayDensity_x)
         {
             for (float j = 0; j < 1; j += rayDensity_y)
             {
                 RaycastHit hit;
                 Ray ray = currCam.ViewportPointToRay(new Vector3(i, j, 0f));

                 if (Physics.Raycast(ray, out hit, maxRange))
                 {

                     seenPixels.Add(new Vector2((int)(i * (int)imageDim.x), (int)(j * (int)imageDim.y)));
                         
                     // imageCoords[(int)(i * (int)imageDim.x), (int)(j * (int)imageDim.y)] = 1;
                     Debug.Log((int)(i * (int)imageDim.x) + " | " + (int)(j * (int)imageDim.y));
                     //imageTex.SetPixel((int)(i * (int)imageDim.x), (int)(j * (int)imageDim.y), Color.green);
                     Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 60);
                 }

             }
         }

       //  imageTex.Apply();


        StreamWriter streamWriter = new StreamWriter("C:\\Users\\ivan\\Documents\\UnityRaycastTest\\Assets\\CameraSeenPixels\\seenPixels.txt");
        string output = "";

        for (int i = 0; i < seenPixels.Count; i++)
        {
            output = seenPixels[i].x.ToString() + " " + seenPixels[i].y.ToString();

            streamWriter.WriteLine(output);
            
        }

        streamWriter.Close();

        


	}
	
	// Update is called once per frame
	void Update () {





        //if (test)
        //{
        //    StreamWriter streamWriter = new StreamWriter("testImage.txt");
        //    string output = "";
        //    for (int i = 0; i < imageCoords.GetUpperBound(0); i++)
        //    {
        //        for (int j = 0; j < imageCoords.GetUpperBound(1); j++)
        //        {
        //            output += imageCoords[i, j].ToString();
        //        }
        //        streamWriter.WriteLine(output);
        //        output = "";
        //    }
        //    streamWriter.Close();

        //    test = false;
        //}





	}
}
