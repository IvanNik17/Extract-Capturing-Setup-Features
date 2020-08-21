using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour {

    public float power = 3.0f;
    public float scale = 1.0f;
    private Vector2 v2SampleStart = new Vector2(0f, 0f);

    
    void Start()
    {
       // Noise();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            v2SampleStart = new Vector2(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f));
            Noise();
        }
    }

    void Noise()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Vector3[] vertices = mf.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float xCoord = v2SampleStart.x + vertices[i].x * scale;
            float yCoord = v2SampleStart.y + vertices[i].z * scale;
            vertices[i].y = (Mathf.PerlinNoise(xCoord, yCoord) - 0.5f) * power;
        }
        mf.mesh.vertices = vertices;
        mf.mesh.RecalculateBounds();
        mf.mesh.RecalculateNormals();

        DestroyImmediate(gameObject.GetComponent<MeshCollider>());
        var collider = gameObject.AddComponent<MeshCollider>();

        collider.sharedMesh = mf.mesh;
        //GameObject.Find("Manager").GetComponent<RayCast_code>().calculateRoughness();
       
        //var collider = this.AddComponent<MeshCollider>();
        //collider.sharedMesh = myMesh;


        //gameObject.GetComponent<MeshCollider>().enabled = false;
        //gameObject.GetComponent<MeshCollider>().enabled = true;
    }
}
