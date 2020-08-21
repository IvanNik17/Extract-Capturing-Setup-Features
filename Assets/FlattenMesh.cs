using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlattenMesh : MonoBehaviour {

    public GameObject smoothMesh;

    public GameObject dirObject;


	// Use this for initialization
	void Start () {




	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.H))
        {
            //smoothMesh.transform.rotation = Quaternion.identity;

            //transform.rotation = Quaternion.identity;

            Debug.Log(transform.forward);
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            MeshFilter mf = GetComponent<MeshFilter>();

            Vector3[] vertices = mf.mesh.vertices;

            

            MeshFilter mf_smooth = smoothMesh.GetComponent<MeshFilter>();

            Vector3[] vertices_smooth = mf_smooth.mesh.vertices;

            Vector3[] normals_smooth = mf_smooth.mesh.normals;


            float countDiff = Mathf.Min(vertices.Length, vertices_smooth.Length);

            Vector3 heading = dirObject.transform.position - transform.position;

            var distance = heading.magnitude;
            var direction = heading / distance;

            



            for (int i = 0; i < countDiff; i++)
            {

                vertices[i].x = vertices[i].x - vertices_smooth[i].x * transform.forward.x;
                vertices[i].y = vertices[i].y - vertices_smooth[i].y * transform.forward.y;
                vertices[i].z = vertices[i].z - vertices_smooth[i].z * transform.forward.z;



            }

            mf.mesh.vertices = vertices;
            mf.mesh.RecalculateBounds();
            mf.mesh.RecalculateNormals();

            DestroyImmediate(gameObject.GetComponent<MeshCollider>());
            var collider = gameObject.AddComponent<MeshCollider>();

            collider.sharedMesh = mf.mesh;
        }

	}
}
