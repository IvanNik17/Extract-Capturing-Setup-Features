using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasPlaneCode : MonoBehaviour {

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    Color[] initColors;

    MeshFilter mf;


    public GameObject raycastObj;

    public GameObject raycastObj2;

    public GameObject viewObj;

    Vector3 centerPos;

    Vector3 centerPos2;

    Vector3 squareSize;

    float rayDensity = 0.04f;

    float maxRange = 100f;

    float colorPaint = 0.1f;

    bool startScan = false;

    float maxScans = 50;
    float currScan = 0;

    public bool normOrRef = true;


    List<RaycastHit> hitPositions1 = new List<RaycastHit>();
    List<RaycastHit> hitPositions2 = new List<RaycastHit>();

    List<Vector3> deltaNormList = new List<Vector3>();

    List<float> normMagList = new List<float>();

    List<Vector3> reflectList1 = new List<Vector3>();

    List<Vector3> reflectList2 = new List<Vector3>();

    Vector3 initPos = Vector3.zero;
    

	// Use this for initialization
	void Start () {

        initPos = raycastObj.transform.position;

        squareSize = raycastObj.transform.localScale;

        mesh = this.GetComponent<MeshCollider>().sharedMesh;

        vertices = mesh.vertices;

        triangles = mesh.triangles;

        initColors = new Color[vertices.Length];

        mf = this.GetComponent<MeshFilter>();

        for (int i = 0; i < initColors.Length; i++)
        {
            initColors[i] = new Color(1f, 1f, 1f);

        }

        
		
	}


    bool rayCastFromObj(GameObject rayCasterObj, Vector3 centerPos, Vector3 squareSize, float curr_i, float curr_j, Color rayColor, out RaycastHit hit, out Vector3 refRay)
    {
        Vector3 currPos = centerPos;
        currPos += rayCasterObj.transform.right * curr_i + rayCasterObj.transform.up * curr_j;

        Vector3 currForward = rayCasterObj.transform.forward;



       // currForward += rayCasterObj.transform.right * (curr_i / squareSize.x) + rayCasterObj.transform.up * (curr_j / squareSize.y);

        refRay = new Vector3(0, 0, 0);

        //Debug.DrawRay(currPos, currForward * 100, Color.green);

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
            

            Debug.DrawRay(hit.point, (reflectiveRay) * 100, rayColor);

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
            initColors[i] = new Color(1f, 1f, 1f);

        }
        mf.mesh.colors = initColors;

    }


    void scanObj(float colorGrad, GameObject raycastObj, int whichObj)
    {


        if (whichObj == 1)
        {
            hitPositions1 = new List<RaycastHit>();
        }
        else if (whichObj == 2)
        {
            hitPositions2 = new List<RaycastHit>();
        }


        centerPos = raycastObj.transform.position;


        squareSize = raycastObj.transform.localScale;
        
        Color paintColor = new Color(1, 0f, 0f, 1f);

        float H, S, V;

        Color.RGBToHSV(paintColor, out H, out S, out V);
        
        H = Mathf.Clamp(colorGrad,0,1);
        
        paintColor = Color.HSVToRGB(H, S, V);



        for (float i = -squareSize.x / 2; i <= squareSize.x / 2; i += rayDensity)
        {
            for (float j = -squareSize.y / 2; j <= squareSize.y / 2; j += rayDensity)
            {
                RaycastHit hit_it;
                Vector3 refRay;


                

                if (rayCastFromObj(raycastObj, centerPos, squareSize, i, j, Color.blue, out hit_it, out refRay))
                {
                    RaycastHit hitSecond;


                    
                    if (Physics.Raycast(hit_it.point, refRay, out hitSecond, maxRange) && hitSecond.transform.tag == "CanvasObj")
                    {
                        //Vector3 p0 = vertices[triangles[hitSecond.triangleIndex * 3 + 0]];
                        //Vector3 p1 = vertices[triangles[hitSecond.triangleIndex * 3 + 1]];
                        //Vector3 p2 = vertices[triangles[hitSecond.triangleIndex * 3 + 2]];

                        //Debug.DrawRay(hitSecond.point, (refRay) * 100, Color.green);

                        if (whichObj == 1)
                        {
                            hitPositions1.Add(hitSecond);


                            reflectList1.Add(refRay);
                        }
                        else if (whichObj == 2)
                        {
                            hitPositions2.Add(hit_it);
                            reflectList2.Add(refRay);
                        }
                        

                        initColors[triangles[hitSecond.triangleIndex * 3 + 0]] = paintColor;
                        //initColors[triangles[hitSecond.triangleIndex * 3 + 1]] = paintColor;
                        //initColors[triangles[hitSecond.triangleIndex * 3 + 2]] = paintColor;
                    }


                }



            }

        }

        mf.mesh.colors = initColors;
    }


    void calculateDiff()
    {
        
        Vector3 meanPos = Vector3.zero;

        Vector3 variancePos = Vector3.zero;

        Vector3 sumOfDerivation = Vector3.zero;

        Vector3 stdDev = Vector3.zero;

        for (int i = 0; i < hitPositions1.Count; i++)
        {
            meanPos += hitPositions1[i].point;

            sumOfDerivation += Vector3.Scale(hitPositions1[i].point, hitPositions1[i].point);


        }

        meanPos /= hitPositions1.Count;

        for (int i = 0; i < hitPositions1.Count; i++)
        {
            variancePos += Vector3.Scale( hitPositions1[i].point - meanPos, hitPositions1[i].point - meanPos);



        }

        variancePos /= hitPositions1.Count;

        Vector3 stdDevPos = new Vector3(Mathf.Sqrt(variancePos.x), Mathf.Sqrt(variancePos.y), Mathf.Sqrt(variancePos.z));

        //Debug.Log("Mean pos " + meanPos);
        
        //Debug.Log("Variance " + variancePos);

        Debug.Log("Std.Dev. " + stdDevPos.y + " from num points " + hitPositions1.Count);


        //sumOfDerivation /= hitPositions1.Count;

        



        //float countDiff = Mathf.Min(hitPositions1.Count, hitPositions2.Count);

        //normMagList = new List<float>();

        //float avgMag = 0;
        //for (int i = 0; i < countDiff; i++)
        //{

        //    Vector3 deltaNorm = (reflectList1[i] - reflectList2[i]) / 2;

        //    float currNormMag = Mathf.Sqrt(Vector3.Dot(deltaNorm, deltaNorm));

        //    float angleBetween = Vector3.SignedAngle(reflectList1[i], reflectList2[i]);

        //    normMagList.Add(currNormMag);

        //    avgMag += currNormMag;

        //    Debug.DrawRay(hitPositions1[i].point, deltaNorm * 2, Color.red);

        //}

        //Debug.Log("AVERAGE MAG " + avgMag / countDiff);

    }

	
	// Update is called once per frame
	void Update () {


        Debug.DrawRay(raycastObj.transform.position, raycastObj.transform.forward * 100, Color.red);

       // Debug.DrawRay(raycastObj2.transform.position, raycastObj2.transform.forward * 100, Color.blue);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            clearCanvas();

           
            scanObj(colorPaint, raycastObj,1);

           // scanObj(colorPaint, raycastObj2,2);

            calculateDiff();

            Debug.Log("Done");

            colorPaint += 0.02f;
        }

        //if (hitPositions1.Count != 0)
        //{
        //    calculateDiff();
            
        //    Debug.Log("Vals = " + string.Join(" ",
        //     new List<float>(normMagList)
        //     .ConvertAll(i => i.ToString())
        //     .ToArray()));
        //}
        


        if (Input.GetKeyDown(KeyCode.Q))
        {
            viewObj.transform.Rotate(0, -1, 0);
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            startScan = true;

            clearCanvas();

            raycastObj.transform.position = initPos;

            currScan = 0;

        }

        if (startScan)
        {


            scanObj(colorPaint, raycastObj,1);

            calculateDiff();

            //Debug.Log("Finished scan number " + currScan.ToString());

            raycastObj.transform.Translate(0, squareSize.y, 0);

            currScan += squareSize.y;
        }

        if (currScan >= maxScans)
        {
            startScan = false;
        }

        
		
	}
}
