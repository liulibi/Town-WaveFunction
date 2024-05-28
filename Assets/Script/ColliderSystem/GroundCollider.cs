using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCollider : MonoBehaviour
{
    public void CreatCollider(Grid grid)
    {
        foreach(SubQuad subQuad in grid.subQuads)
        {
            Vector3[] meshVerties = new Vector3[] 
            {
                subQuad.a.currentPosition, 
                subQuad.b.currentPosition,
                subQuad.c.currentPosition,
                subQuad.d.currentPosition,
            };
            int[] meshTriangle = new int[]
            {
                0,1,2,
                0,2,3,
            };
            Mesh mesh = new Mesh();
            mesh.vertices = meshVerties;

            //mesh.triangles是一个整数数组，用于存储网格的三角形索引。每三个连续的索引表示一个三角形的顶点顺序。
            mesh.triangles = meshTriangle;
            GameObject groundQuadCollider_Quad = new GameObject("QuadCollider_" + grid.subQuads.IndexOf(subQuad), typeof(MeshCollider),typeof(GroundCollider_Quad));
            groundQuadCollider_Quad.transform.SetParent(transform);
            groundQuadCollider_Quad.transform.localPosition += Vector3.up * Grid.cellHeight * (0.5f);
            groundQuadCollider_Quad.GetComponent<MeshCollider>().sharedMesh = mesh;
            groundQuadCollider_Quad.GetComponent<GroundCollider_Quad>().subQuad = subQuad;
            groundQuadCollider_Quad.layer = LayerMask.NameToLayer("GroundCollider");
        }
    }
}


public class GroundCollider_Quad : MonoBehaviour
{
    public SubQuad subQuad;
}
