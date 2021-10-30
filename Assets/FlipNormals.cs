using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FlipNormals can be attached to any GameObject with a MeshFilter to have it render the inside faces of the object rather than the outside.
// Taken from: https://www.youtube.com/watch?v=HEHn4EUUyBk&

public class FlipNormals : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;

        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++){
            normals[i] = -1 * normals[i];
        }   

        mesh.normals = normals;

        for (int i = 0; i < mesh.subMeshCount; i++){
            int[] tris = mesh.GetTriangles(i);
            for (int j = 0; j < tris.Length; j += 3){
                int temp = tris[j];
                tris[j] = tris[j + 1];
                tris[j + 1] = temp;
            }

            mesh.SetTriangles(tris, i);
        }
    }
}
