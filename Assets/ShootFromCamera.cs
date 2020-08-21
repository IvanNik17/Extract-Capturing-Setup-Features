using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFromCamera : MonoBehaviour {

    float rayDensity = 0.005f;

    public float maxRange = 100;

    public bool normOrRef = true;

    float colorGrad = 1f;

    public GameObject scannedObj;


    public float sensorHeight = 35;
    public float focalLength = 110;

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    Color[] initColors;

    MeshFilter mf;


    GameObject[] camList;


    int[] isChecked;

    float numCams;


    bool rayCastFromObj(Camera rayCasterObj, float curr_i, float curr_j, Color rayColor, out RaycastHit hit, out Vector3 refRay)
    {

        Ray ray = rayCasterObj.ViewportPointToRay(new Vector3(curr_i, curr_j, 0f));

        

        refRay = new Vector3(0, 0, 0);

        //Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.green);

        if (Physics.Raycast(ray, out hit, maxRange) && hit.transform.tag == "ScanObj")
        {

            Vector3 reflectiveRay = Vector3.Normalize(Vector3.Reflect(hit.point - rayCasterObj.transform.position, hit.normal));


            if (normOrRef)
            {
                refRay = reflectiveRay;
            }
            else
            {
                refRay = hit.normal;
            }

            float H, S, V;

            for (int i = 0; i < 3; i++)
            {

                if (isChecked[triangles[hit.triangleIndex * 3 + i]] != 1)
                {
                    Color paintColor = initColors[triangles[hit.triangleIndex * 3 + i]];



                    Color.RGBToHSV(paintColor, out H, out S, out V);

                    //Debug.Log("Before " + H);

                    V = 1f;
                    S = 1f;
                    H = Mathf.Clamp(H + colorGrad / numCams, 0f, 1f);

                    //Debug.Log("After " + H);

                    paintColor = Color.HSVToRGB(H, S, V);



                    initColors[triangles[hit.triangleIndex * 3 + i]] = paintColor;

                    isChecked[triangles[hit.triangleIndex * 3 + i]] = 1;
                }

                

            }

            

            
            //Debug.DrawRay(hit.point, (reflectiveRay) * 10, rayColor);

            return true;
        }
        else
        {
            return false;
        }


    }

    void clearCanvas()
    {
        for (int i = 0; i < initColors.Length; i++)
        {
            initColors[i] = new Color(1f, 1f, 1f, 1f);

        }
        mf.mesh.colors = initColors;

    }

	// Use this for initialization
	void Start () {

        mesh = scannedObj.GetComponent<MeshCollider>().sharedMesh;

        vertices = mesh.vertices;

        triangles = mesh.triangles;

        initColors = new Color[vertices.Length];

        mf = scannedObj.GetComponent<MeshFilter>();

        for (int i = 0; i < initColors.Length; i++)
        {
            initColors[i] = new Color(0.1f, 0.1f, 0.1f, 1f);

        }


        camList = GameObject.FindGameObjectsWithTag("cam");

        numCams = (float)camList.Length;

        
        for (int i = 0; i < camList.Length; i++)
        {
            Debug.Log(Mathf.Rad2Deg * 2f * Mathf.Atan(sensorHeight / (2f * focalLength)));
            camList[i].GetComponent<Camera>().fieldOfView = Mathf.Rad2Deg* 2 * Mathf.Atan(sensorHeight / (2 * focalLength));
        }
		
	}
	
	// Update is called once per frame
	void Update () {



        clearCanvas();

        for (int k = 0; k < camList.Length; k++)
        {
            isChecked = new int[vertices.Length];
            for (int n = 0; n < isChecked.Length; n++)
            {
                isChecked[n] = 0;
            }

            for (float i = 0; i < 1; i += rayDensity)
            {
                for (float j = 0; j < 1; j += rayDensity)
                {

                    RaycastHit hit_it;
                    Vector3 refRay;

                    rayCastFromObj(camList[k].GetComponent<Camera>(), i, j, Color.blue, out hit_it, out refRay);

                    //initColors[triangles[hit_it.triangleIndex * 3 + 0]] = paintColor;
                    //initColors[triangles[hit_it.triangleIndex * 3 + 1]] = paintColor;
                    //initColors[triangles[hit_it.triangleIndex * 3 + 2]] = paintColor;

                }
            }
        }
        mf.mesh.colors = initColors;


        

	}
}
