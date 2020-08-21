using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootFromLight : MonoBehaviour {


    public GameObject raycasterObj;

    public float maxRange = 100;

    public bool normOrRef = true;


    float rayDensity = 0.5f;


    Vector3 centerPos;

    Vector3 squareSize;

    float colorGrad = 0.1f;

    public int numRef = 5;

    Vector3[] verticesInfo;
    int[] triangles;


    int rayCastFromObj(GameObject rayCasterObj, Vector3 centerPos, Vector3 squareSize, Vector3 currForward, float curr_i, float curr_j, Color rayColor, out RaycastHit hit, out Vector3 refRay)
    {
        Vector3 currPos = centerPos;
        currPos += rayCasterObj.transform.right * curr_i + rayCasterObj.transform.up * curr_j;

        //Vector3 currForward = rayCasterObj.transform.forward;



        // currForward += rayCasterObj.transform.right * (curr_i / squareSize.x) + rayCasterObj.transform.up * (curr_j / squareSize.y);

        refRay = new Vector3(0, 0, 0);

        

        if (Physics.Raycast(currPos, currForward, out hit, maxRange) && hit.transform.tag == "ScanObj")
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

            Debug.DrawRay(currPos, currForward * (currPos - hit.point).magnitude, Color.green);

            Debug.DrawRay(hit.point, (reflectiveRay) * 10, Color.green);

            return 2;
        }
        else if (Physics.Raycast(currPos, currForward, out hit, maxRange) && hit.transform.tag == "Wall")
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

            Debug.DrawRay(currPos, currForward * (currPos - hit.point).magnitude, rayColor);
            //Debug.DrawRay(hit.point, (reflectiveRay) * 10, rayColor);

            return 1;
        }
        else
        {
            return 0;
        }


    }

	// Use this for initialization
	void Start () {

        Mesh mc = GameObject.FindGameObjectWithTag("ScanObj").GetComponent<MeshCollider>().sharedMesh;
        
        verticesInfo = new Vector3[mc.vertices.Length];
        triangles = mc.triangles;

        for (int i = 0; i < verticesInfo.Length; i++)
        {
            verticesInfo[i] = Vector3.zero;
        }
	}
	
	// Update is called once per frame
	void Update () {

        centerPos = raycasterObj.transform.position;


        squareSize = raycasterObj.transform.localScale;

        //RaycastHit hit_it;
        //Vector3 refRay;

        GameObject currCasterObj = raycasterObj;

        Vector3 currDir = currCasterObj.transform.forward;

        Color paintColor = new Color(1, 0f, 0f, 1f);

        float H, S, V;

        Color.RGBToHSV(paintColor, out H, out S, out V);

        H = Mathf.Clamp(colorGrad, 0, 1);

        paintColor = Color.HSVToRGB(H, S, V);

        for (float i = -squareSize.x / 2; i <= squareSize.x / 2; i += rayDensity)
        {

            for (float j = -squareSize.y / 2; j <= squareSize.y / 2; j += rayDensity)
            {
                centerPos = raycasterObj.transform.position;
                currCasterObj = raycasterObj;
                currDir = currCasterObj.transform.forward;

                paintColor = new Color(1, 0f, 0f, 1f);

                colorGrad = 0.1f;

                

                RaycastHit hit_it;
                Vector3 refRay;

                for (int k = 1; k < numRef; k++)
                {

                    if (k == 1)
                    {
                        centerPos = raycasterObj.transform.position;
                        centerPos += currCasterObj.transform.right * i + currCasterObj.transform.up * j;
                        currDir += currCasterObj.transform.right * (i / squareSize.x) + currCasterObj.transform.up * (j / squareSize.y);
                    }

                    int castResult = rayCastFromObj(currCasterObj, centerPos, squareSize, currDir, 0, 0, paintColor, out hit_it, out refRay);

                    if (castResult == 1)
                    {
                        Debug.Log("wall");

                        currCasterObj = hit_it.transform.gameObject;
                        centerPos = hit_it.point;

                        currDir = refRay;

                        Color.RGBToHSV(paintColor, out H, out S, out V);

                        H = Mathf.Clamp(colorGrad, 0, 1);

                        paintColor = Color.HSVToRGB(H, S, V);

                        // currI = 0;
                        // currJ = 0;

                        colorGrad += 0.9f / (float)numRef;


                    }
                    else if (castResult == 2)
                    {
                        Debug.Log("object");

                        //verticesInfo[triangles[hit_it.triangleIndex * 3 + 0]] = new Vector3(0, verticesInfo[triangles[hit_it.triangleIndex * 3 + 0]].y + 1/k,0);

                        break;
                    }
                }
            }
        }


        //for (int i = 0; i < 5; i++)
        //{
        //    for (int k = 0; k < 2; k++)
        //    {
        //        int castResult = rayCastFromObj(currCasterObj, centerPos, squareSize, currDir, 0, 0, Color.blue, out hit_it, out refRay);

        //        if (castResult == 1)
        //        {
        //            Debug.Log("wall");

        //            currCasterObj = hit_it.transform.gameObject;
        //            centerPos = hit_it.point;

        //            currDir = refRay;


        //        }
        //        else if (castResult == 2)
        //        {
        //            Debug.Log("object");

        //            break;
        //        }
        //    }
        //}
        //for (int k = 0; k < 2; k++)
        //{
        //    int castResult = rayCastFromObj(currCasterObj, centerPos, squareSize, currDir, 0, 0, Color.blue, out hit_it, out refRay);

        //    if (castResult == 1)
        //    {
        //        Debug.Log("wall");

        //        currCasterObj = hit_it.transform.gameObject;
        //        centerPos = hit_it.point;

        //        currDir = refRay;


        //    }
        //    else if (castResult == 2)
        //    {
        //        Debug.Log("object");

        //        break;
        //    }
        //}

        


        //for (float i = -squareSize.x / 2; i <= squareSize.x / 2; i += rayDensity)
        //{
        //    for (float j = -squareSize.y / 2; j <= squareSize.y / 2; j += rayDensity)
        //    {
        //        RaycastHit hit_it;
        //        Vector3 refRay;




        //        if (rayCastFromObj(raycasterObj, centerPos, squareSize, i, j, Color.blue, out hit_it, out refRay))
        //        {
        //            //RaycastHit hitSecond;



        //            //if (Physics.Raycast(hit_it.point, refRay, out hitSecond, maxRange) && hitSecond.transform.tag == "CanvasObj")
        //            //{


        //            //    if (whichObj == 1)
        //            //    {
        //            //        hitPositions1.Add(hitSecond);


        //            //        reflectList1.Add(refRay);
        //            //    }
        //            //    else if (whichObj == 2)
        //            //    {
        //            //        hitPositions2.Add(hit_it);
        //            //        reflectList2.Add(refRay);
        //            //    }



        //            //}


        //        }



        //    }

        //}

	}
}
