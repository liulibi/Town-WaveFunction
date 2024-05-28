using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid 
{
    public static int radius;
    public static int height;

    public static float cellSize;
    public static float cellHeight;

    public readonly List<Vertex_hex> hexes = new List<Vertex_hex>();//顶点集合
    public readonly List<Vertex_mid> edgeMids = new List<Vertex_mid>();//边上终点集合
    public readonly List<Vertex_center> vertex_Centers = new List<Vertex_center>();//中心点集合
    public readonly List<Vertex> vertices = new List<Vertex>();//所有点集合

    public readonly List<Edge> edges = new List<Edge>();//边集合

    public readonly List<Triangle> triangles = new List<Triangle>();//三角形集合

    public readonly List<Quad> quads = new List<Quad>();//四边形集合

    public readonly List<SubQuad> subQuads = new List<SubQuad>();//细分四边形集合

    public Grid(int radius, int height, float cellSize, float cellHeight, int relaxTimes)
    {
        Grid.cellSize = cellSize;
        Grid.height = height;
        Grid.radius = radius;
        Grid.cellHeight = cellHeight;

        Vertex_hex.Hex(hexes,radius);
        Triangle.Triangles_Hex(hexes, vertex_Centers, edgeMids, edges, triangles);

        while (Triangle.HasNeighborTriangle(triangles))
        {
            Triangle.RandomlyMergeTriangles(vertex_Centers,edgeMids, edges, triangles, quads);
        }
        vertices.AddRange(edgeMids);
        vertices.AddRange(vertex_Centers);
        vertices.AddRange(hexes);

        //细分三角形
        foreach(Triangle triangle in triangles)
        {
            triangle.Subdivide(subQuads);
        }

        //细分四边形
        foreach(Quad quad in quads)
        {
            quad.Subdivide(subQuads);
        }

        for(int i = 0; i < relaxTimes; i++)
        {
            foreach(SubQuad subQuad in subQuads)
            {
                subQuad.CalculateRelaxOffset();

                foreach(Vertex vertex in vertices)
                {
                    vertex.Relax();
                }
            }
        }

        foreach (Vertex vertex in vertices)
        {
            vertex.index = vertices.IndexOf(vertex);
            vertex.BoundaryCheck();
            for (int i = 0; i < Grid.height+1; i++)
            {
                vertex.vertex_Ys.Add(new Vertex_Y(vertex, i));
            }
        }

        foreach(SubQuad subQuad in subQuads)
        {
            for (int i = 0; i < Grid.height; i++) 
            {
                subQuad.subQuad_Cubes.Add(new SubQuad_Cube(subQuad, i)); 
            }
        }
    }
}
