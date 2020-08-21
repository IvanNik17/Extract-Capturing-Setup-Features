using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//execute in editor plz add
public class RayCast_code : MonoBehaviour {

    public GameObject raycastObj;

    public GameObject raycastObj2;

    public GameObject bladeObj;
    RaycastHit hit;
    float maxRange = 100;

    float rayDensity = 0.04f;

    Vector3 centerPos;
    Vector3 centerPos2;

    Vector3 squareSize;


    List<RaycastHit> hitPositions1 = new List<RaycastHit>();
    List<RaycastHit> hitPositions2 = new List<RaycastHit>();

    List<Vector3> deltaNormList = new List<Vector3>();
    List<Vector3> deltaPosList = new List<Vector3>();

    List<float> normMagList = new List<float>();


    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    Color[] initColors;

    MeshFilter mf;

    Vector3 scanScale; 

    

	// Use this for initialization
	void Start () {


        //calculateRoughness();

        
        
	}


    public void calculateRoughness()
    {

        hitPositions1 = new List<RaycastHit>();
        hitPositions2 = new List<RaycastHit>();

        deltaNormList = new List<Vector3>();
        deltaPosList = new List<Vector3>();


        normMagList = new List<float>();


        centerPos = raycastObj.transform.position;

        centerPos2 = raycastObj2.transform.position;

        squareSize = raycastObj.transform.localScale;

        mesh = bladeObj.GetComponent<MeshCollider>().sharedMesh;

        vertices = mesh.vertices;

        triangles = mesh.triangles;

        initColors = new Color[vertices.Length];

        mf = bladeObj.GetComponent<MeshFilter>();

        scanScale = bladeObj.GetComponent<Renderer>().bounds.size;


        for (int i = 0; i < initColors.Length; i++)
        {
            initColors[i] = new Color(1f, 1f, 1f);

        }



        for (float i = -squareSize.x / 2; i <= squareSize.x / 2; i += rayDensity)
        {
            for (float j = -squareSize.y / 2; j <= squareSize.y / 2; j += rayDensity)
            {
                //Vector3 currPos = centerPos;
                //currPos += raycastObj.transform.right*i + raycastObj.transform.up*j;

                //Vector3 currForward = raycastObj.transform.forward;



                //currForward += raycastObj.transform.right * (i / squareSize) + raycastObj.transform.up * (j / squareSize);

                //Debug.DrawRay(currPos, currForward * 100, Color.green);

                //if (Physics.Raycast(currPos, currForward, out hit, maxRange))
                //{
                //    Debug.DrawRay(hit.point, hit.normal * 5, Color.blue);
                //}

                RaycastHit hit_it;
                if (rayCastFromObj(raycastObj, centerPos, squareSize, i, j, Color.blue, out hit_it))
                {
                    hitPositions1.Add(hit_it);
                }


                if (rayCastFromObj(raycastObj2, centerPos2, squareSize, i, j, Color.red, out hit_it))
                {
                    hitPositions2.Add(hit_it);
                }


                //hitPositions1.Add(rayCastFromObj(raycastObj, centerPos, squareSize, i, j, Color.blue));
                //rayCastFromObj (raycastObj, centerPos, squareSize, i, j, Color.blue );
                //hitPositions2.Add(rayCastFromObj(raycastObj2, centerPos2, squareSize, i, j, Color.red));
                //rayCastFromObj(raycastObj2, centerPos2, squareSize, i, j, Color.red);

            }

        }
        Debug.Log(hitPositions1.Count);

        foreach (RaycastHit rayHit in hitPositions1)
        {
            float minDist = 999999;
            RaycastHit closestHit2 = hitPositions2[0];
            bool closeEnough = false;
            foreach (RaycastHit rayHit2 in hitPositions2)
            {
                float dist = Vector3.Distance(rayHit.point, rayHit2.point);
                if (dist < minDist && dist < scanScale.x * 0.01)
                {
                    minDist = dist;

                    closestHit2 = rayHit2;

                    closeEnough = true;
                }

            }

            if (!closeEnough)
            {
                continue;
            }

            Vector3 firstNormal = rayHit.normal;
            //if (Vector3.Dot(rayHit.normal, closestHit2.normal) > Mathf.PI)
            //{
            //    firstNormal = -rayHit.normal;
            //}

            Vector3 reflectiveRay_1 = Vector3.Normalize(Vector3.Reflect(rayHit.point - raycastObj.transform.position, firstNormal));

            Vector3 reflectiveRay_2 = Vector3.Normalize(Vector3.Reflect(closestHit2.point - raycastObj2.transform.position, closestHit2.normal));

            Vector3 deltaNorm = (reflectiveRay_1 - reflectiveRay_2) / 2;
            Vector3 deltaPos = (rayHit.point - closestHit2.point) / 2;



            float currNormMag = Mathf.Sqrt(Vector3.Dot(deltaNorm, deltaNorm));

            float angleBetween = Vector3.Angle(reflectiveRay_1, reflectiveRay_2);

            normMagList.Add(currNormMag);
            deltaNormList.Add(deltaNorm);

            deltaPosList.Add(rayHit.point);

            Vector3 p0 = vertices[triangles[rayHit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[rayHit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[rayHit.triangleIndex * 3 + 2]];

            if ((angleBetween >= 0 && angleBetween < 10) || (angleBetween < 180 && angleBetween > 170))
            {
                initColors[triangles[rayHit.triangleIndex * 3 + 0]] = new Color(currNormMag, currNormMag, currNormMag, 1);
                initColors[triangles[rayHit.triangleIndex * 3 + 1]] = new Color(currNormMag, currNormMag, currNormMag, 1);
                initColors[triangles[rayHit.triangleIndex * 3 + 2]] = new Color(currNormMag, currNormMag, currNormMag, 1);

            }



        }
        mf.mesh.colors = initColors;
    }


    public bool rayCastFromObj (GameObject rayCasterObj, Vector3 centerPos, Vector3 squareSize, float curr_i, float curr_j, Color rayColor, out RaycastHit hit )
    {
        Vector3 currPos = centerPos;
        currPos += rayCasterObj.transform.right * curr_i + rayCasterObj.transform.up * curr_j;

        Vector3 currForward = rayCasterObj.transform.forward;



        currForward += rayCasterObj.transform.right * (curr_i / squareSize.x) + rayCasterObj.transform.up * (curr_j / squareSize.y);

        //Debug.DrawRay(currPos, currForward * 100, Color.green);

        if (Physics.Raycast(currPos, currForward, out hit, maxRange))
        {
            Vector3 reflectiveRay = Vector3.Normalize(Vector3.Reflect(hit.point - rayCasterObj.transform.position, hit.normal));


            
            Debug.DrawRay(hit.point, (reflectiveRay) * 100, rayColor);

            return true;
        }
        else
        {
            return false;
        }

        
    }

	
	// Update is called once per frame
	void Update () {


        
        //for (int i = 0; i < deltaNormList.Count; i++)
        //{
        //    Debug.DrawRay(deltaPosList[i], deltaNormList[i] * 2, Color.black);


        //}

        

        //centerPos = raycastObj.transform.position;

        //centerPos2 = raycastObj2.transform.position;

        //squareSize = raycastObj.transform.localScale;

        //for (float i = -squareSize.x / 2; i <= squareSize.x / 2; i += rayDensity)
        //{
        //    for (float j = -squareSize.y / 2; j <= squareSize.y / 2; j += rayDensity)
        //    {
        //        RaycastHit hit_it;

        //        rayCastFromObj(raycastObj, centerPos, squareSize, i, j, Color.blue, out hit_it);
                
        //        //rayCastFromObj(raycastObj2, centerPos2, squareSize, i, j, Color.red, out hit_it);
                

        //    }

        //}





        //for (int i = 0; i < hitPositions1.Count; i++)
        //{
        //    float minDist = 999999;
        //    RaycastHit closestHit2 = hitPositions2[0];
        //    for (int j = 0; j < hitPositions2.Count; j++)
        //    {

        //        if (Vector3.Distance(hitPositions1[i].point, hitPositions2[j].point) < minDist)
        //        {
        //            minDist = Vector3.Distance(hitPositions1[i].point, hitPositions2[j].point);

        //            closestHit2 = hitPositions2[j];
        //        }


        //    }
        //    Vector3 deltaNorm = (hitPositions1[i].normal - closestHit2.normal) / 2;
        //    Vector3 deltaPos = (hitPositions1[i].point - closestHit2.point) / 2;

        //    deltaNormList.Add(deltaNorm);
        //    //Debug.DrawRay(hitPositions1[i].point, deltaNorm * 30, Color.red);

        //}
        


	}
}
