using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class GenerateMeshForWarping : MonoBehaviour
{
    public float minDistance;
    public Transform trackedObject;

    public bool holeMakerUsingCoord;
    public Vector3 holeLocalCenter;
    public Vector3 holeSize;
    public float holeRadius;

    MeshFilter filter;

    public void Remesh()
    {
        filter.mesh = holeMakerUsingCoord ? GenerateBoredMesh(holeLocalCenter, holeSize) : GenerateMeshWithHoles();
    }

    Mesh mesh;
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;
    int[] triangles;
    bool[] trianglesDisabled;
    List<int>[] trisWithVertex;

    Vector3[] origvertices;
    Vector3[] orignormals;
    Vector2[] origuvs;
    int[] origtriangles;

    void Start()
    {

        mesh = new Mesh();
        filter = GetComponent<MeshFilter>();
        orignormals = filter.mesh.normals;
        origvertices = filter.mesh.vertices;
        origuvs = filter.mesh.uv;
        origtriangles = filter.mesh.triangles;

        vertices = new Vector3[origvertices.Length];
        normals = new Vector3[orignormals.Length];
        uvs = new Vector2[origuvs.Length];
        triangles = new int[origtriangles.Length];
        trianglesDisabled = new bool[origtriangles.Length];

        orignormals.CopyTo(normals, 0);
        origvertices.CopyTo(vertices, 0);
        origtriangles.CopyTo(triangles, 0);
        origuvs.CopyTo(uvs, 0);

        trisWithVertex = new List<int>[origvertices.Length];
        for (int i = 0; i < origvertices.Length; ++i)
        {
            trisWithVertex[i] = origtriangles.IndexOf(i);

        }
        //filter.mesh = GenerateMeshWithHoles();
    }

    Mesh GenerateMeshWithHoles()
    {
        Vector3 trackPos = trackedObject.position;
        for (int i = 0; i < origvertices.Length; ++i)
        {
            Vector3 v = new Vector3(origvertices[i].x * transform.localScale.x, origvertices[i].y * transform.localScale.y, origvertices[i].z * transform.localScale.z);
            Debug.Log(PointInsideAVolume(trackPos, holeLocalCenter, holeSize));
            if ((v + transform.position - trackPos).magnitude < minDistance)
            {
                for (int j = 0; j < trisWithVertex[i].Count; ++j)
                {
                    int value = trisWithVertex[i][j];
                    int remainder = value % 3;
                    trianglesDisabled[value - remainder] = true;
                    trianglesDisabled[value - remainder + 1] = true;
                    trianglesDisabled[value - remainder + 2] = true;
                }
            }
        }
        triangles = origtriangles;
        triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled).ToArray();

        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        for (int i = 0; i < trianglesDisabled.Length; ++i)
            trianglesDisabled[i] = false;
        return mesh;
    }

    Mesh GenerateBoredMesh(Vector3 holeLocation, Vector3 holeSize, bool worldHole = false)//float holeSize)
    {
        for (int i = 0; i < origvertices.Length; ++i)
        {
            Vector3 verticeRealPosition = new Vector3(origvertices[i].x * transform.localScale.x, origvertices[i].y * transform.localScale.y, origvertices[i].z * transform.localScale.z);
            if (PointInsideAVolume(verticeRealPosition, holeLocation, holeSize, worldHole))//if ((verticeRealPosition + transform.position - holeLocation).magnitude < holeSize)
            {
                for (int j = 0; j < trisWithVertex[i].Count; ++j)
                {
                    int value = trisWithVertex[i][j];
                    int remainder = value % 3;
                    trianglesDisabled[value - remainder] = true;
                    trianglesDisabled[value - remainder + 1] = true;
                    trianglesDisabled[value - remainder + 2] = true;
                }
            }
        }
        
        triangles = origtriangles;
        triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled).ToArray();

        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        for (int i = 0; i < trianglesDisabled.Length; ++i)
            trianglesDisabled[i] = false;
        return mesh;
    }

    public bool PointInsideAVolume(Vector3 point, Vector3 centerVolume, Vector3 sizeVolume, bool worldHole = false)
    {
        Vector3 centerToPoint = worldHole ? point + transform.position - centerVolume : point-centerVolume;
        return Mathf.Abs(centerToPoint.x) < sizeVolume.x && Mathf.Abs(centerToPoint.y) < sizeVolume.y && Mathf.Abs(centerToPoint.z) < sizeVolume.z;
    }

    /*Mesh GenerateMeshWithFakeHoles()
    {
        Vector3 trackPos = trackedObject.position;
        for (int i = 0; i < origvertices.Length; ++i)
        {
            if ((origvertices[i] + transform.position - trackPos).magnitude < minDistance)
            {
                normals[i] = -orignormals[i];
            }
            else
            {
                normals[i] = orignormals[i];
            }
        }
        mesh.SetVertices(vertices.ToList<Vector3>());
        mesh.SetNormals(normals.ToList());
        mesh.SetUVs(0, uvs.ToList());
        mesh.SetTriangles(triangles, 0);
        return mesh;
    }*/
    void Update()
    {
        Remesh();
    }
}
