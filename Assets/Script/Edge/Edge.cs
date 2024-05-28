using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge 
{
    //提供了高效的存储和检索唯一元素的功能
    public readonly HashSet<Vertex_hex> hexes;//两端顶点
    public Vertex_mid edgeMid;//线段中心点

    public Edge(Vertex_hex a, Vertex_hex b, List<Edge> edges, List<Vertex_mid> edgeMids)
    {
        hexes = new HashSet<Vertex_hex> { a, b };
        edges.Add(this);
        edgeMid = new Vertex_mid(this, edgeMids);
    }

    public static Edge FindEdge(Vertex_hex a, Vertex_hex b, List<Edge> edges)
    {
        foreach(Edge edge in edges)
        {
            if (edge.hexes.Contains(a) && edge.hexes.Contains(b))
            {
                return edge;
            }
        }
        return null;
    }
}
