using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Triangle
{
    public readonly Vertex_hex a;
    public readonly Vertex_hex b;
    public readonly Vertex_hex c;
    public readonly Vertex_triangleCenter center;

    public readonly Edge ab;
    public readonly Edge bc;
    public readonly Edge ca;

    public readonly Vertex_hex[] vertex_HexeArry;
    public readonly Edge[] edgeArry;

    public Triangle(Vertex_hex a, Vertex_hex b, Vertex_hex c, List<Vertex_center> centers, List<Vertex_mid> edgeMids, List<Edge> edges, List<Triangle> triangles)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        center = new Vertex_triangleCenter(this);

        this.vertex_HexeArry = new Vertex_hex[] { a, b, c };

        //创建边线，在edges边集合中寻找是否已经存在两点之间连线
        ab = Edge.FindEdge(a, b, edges);
        bc = Edge.FindEdge(b, c, edges);
        ca = Edge.FindEdge(c, a, edges);

        if (ab == null)
        {
            ab = new Edge(a, b, edges, edgeMids);
        }
        if(bc == null)
        {
            bc = new Edge(b, c, edges, edgeMids);
        }
        if (ca == null)
        {
            ca = new Edge(c, a, edges, edgeMids);
        }

        //为三角形自身的边集合添加元素
        this.edgeArry = new Edge[] { ab, bc, ca };

        triangles.Add(this);
        centers.Add(center);
    }

    //参数半径，点集合，边集合，三角形集合
    public static void Triangles_Ring(int radius, List<Vertex_hex> vertices, List<Vertex_center> centers, List<Vertex_mid> edgeMids, List<Edge> edges, List<Triangle> triangles)
    {
        List<Vertex_hex> inner = Vertex_hex.GrabRing(radius - 1, vertices);//内圈点
        List<Vertex_hex> outer = Vertex_hex.GrabRing(radius, vertices);//外圈点

        for (int i = 0; i < 6; i++) 
        {
            for(int j = 0; j < radius; j++)
            {
                //创建俩个顶点在外圈，一个在内圈的三角形
                Vertex_hex a = outer[i * radius + j];
                Vertex_hex b = outer[(i * radius + j + 1) % outer.Count];//在顶点转一圈后会回到原点
                Vertex_hex c = inner[(i * (radius - 1) + j) % inner.Count];

                new Triangle(a, b, c, centers, edgeMids, edges, triangles);
                if (j > 0) 
                {
                    //创建俩个顶点在内圈，一个在外圈的三角形
                    Vertex_hex d = inner[i * (radius - 1) + j - 1];
                    new Triangle(a, c, d, centers, edgeMids, edges, triangles);
                }
            }
        }
    }

    public static void Triangles_Hex(List<Vertex_hex> vertices, List<Vertex_center> centers, List<Vertex_mid> edgeMids, List<Edge> edges, List<Triangle> triangles)
    {
        for(int i = 1; i <= Grid.radius; i++)
        {
            Triangles_Ring(i, vertices, centers, edgeMids, edges, triangles);
        }
    }

    
    #region 合并相邻三角形
    //判断三角形是否相邻 
    public bool IsNeighbor(Triangle target)
    {
        HashSet<Edge> intersection = new HashSet<Edge>(edgeArry);

        //IntersectWith 是 HashSet 类的一个方法，用于计算两个 HashSet 集合的交集
        intersection.IntersectWith(target.edgeArry);
        return intersection.Count == 1;
    }

    public List<Triangle> FindAllNeighborTrangles(List<Triangle> triangles)
    {
        List<Triangle> result = new List<Triangle>();

        foreach (Triangle triangle in triangles)
        {
            if (this.IsNeighbor(triangle))
            {
                result.Add(triangle);
            }
        }

        return result;
    }

    //返回相交的那条边
    public Edge NeighborEdge(Triangle neighbor)
    {
        HashSet<Edge> intersection = new HashSet<Edge>(edgeArry);

        //IntersectWith 是 HashSet 类的一个方法，用于计算两个 HashSet 集合的交集
        intersection.IntersectWith(neighbor.edgeArry);
        return intersection.Single();
    }

    //获取本身三角形的不相交点
    public Vertex_hex IsolatedVertex_Slef(Triangle neighbor)
    {
        HashSet<Vertex_hex> exception = new HashSet<Vertex_hex>(vertex_HexeArry);
        //用于从调用方法的 HashSet 中移除与另一个集合的元素相同的元素。
        exception.ExceptWith(NeighborEdge(neighbor).hexes);
        return exception.Single();
    }

    //获取neighbor三角形中不相交的点
    public Vertex_hex IsolatedVertex_Neighbor(Triangle neighbor)
    {
        HashSet<Vertex_hex> exception = new HashSet<Vertex_hex>(neighbor.vertex_HexeArry);
        exception.ExceptWith(NeighborEdge(neighbor).hexes);
        return exception.Single();
    }

    //将相交的三角形简化为四边形
    public void MergeNeighborTrangle(Triangle neighbor, List<Vertex_center> centers, List<Vertex_mid> edgeMids, List<Edge> edges, List<Triangle> triangles,List<Quad> quads)
    {
        Vertex_hex a = IsolatedVertex_Slef(neighbor);

        //IndexOf(vertex_HexeArry, a)获取顶点 a 在数组中的索引
        Vertex_hex b = vertex_HexeArry[(Array.IndexOf(vertex_HexeArry, a) + 1) % 3];

        Vertex_hex c = IsolatedVertex_Neighbor(neighbor);

        //IndexOf(neighbor.vertex_HexeArry, c)获取顶点 c 在数组中的索引
        Vertex_hex d = neighbor.vertex_HexeArry[(Array.IndexOf(neighbor.vertex_HexeArry, c) + 1) % 3];

        Quad quad = new Quad(a, b, c, d, centers, edges, quads);

        //在边集合中去除相交边
        edges.Remove(NeighborEdge(neighbor));

        //在线段中点集合中去去除相交边的中点
        edgeMids.Remove(NeighborEdge(neighbor).edgeMid);

        //在中心点集合中移除被合并的三角形中心点
        centers.Remove(neighbor.center);
        centers.Remove(this.center);

        triangles.Remove(neighbor);
        triangles.Remove(this);
    }

    //判断是否还存在相邻三角形
    public static bool HasNeighborTriangle(List<Triangle> triangles)
    {
        foreach(Triangle a in triangles)
        {
             foreach(Triangle b in triangles)
            {
                if(a.IsNeighbor(b)) return true;
            }
        }
        return false;
    }

    //随机合并相邻三角形
    public static void RandomlyMergeTriangles(List<Vertex_center> centers, List<Vertex_mid> edgeMids, List<Edge> edges,List<Triangle> triangles,List<Quad> quads)
    {
        int randomIndex = UnityEngine.Random.Range(0, triangles.Count);
        List<Triangle> neighbors = triangles[randomIndex].FindAllNeighborTrangles(triangles);
        if(neighbors.Count != 0) 
        {
            int randomNeighborIndex = UnityEngine.Random.Range(0, neighbors.Count);
            triangles[randomIndex].MergeNeighborTrangle(neighbors[randomNeighborIndex], centers, edgeMids, edges, triangles, quads);
        }

    }

    #endregion

    #region 细分三角形
    public void Subdivide(List<SubQuad> subQuads)
    {
        SubQuad subQuad_a = new SubQuad(a, ab.edgeMid, center, ca.edgeMid, subQuads);
        SubQuad subQuad_b = new SubQuad(b, bc.edgeMid, center, ab.edgeMid, subQuads);
        SubQuad subQuad_c = new SubQuad(c, ca.edgeMid, center, bc.edgeMid, subQuads);

        //顶点存入四边形
        a.subQuads.Add(subQuad_a);
        b.subQuads.Add(subQuad_b);
        c.subQuads.Add(subQuad_c);

        //中心点存入四边形
        center.subQuads.Add(subQuad_a);
        center.subQuads.Add(subQuad_b);
        center.subQuads.Add(subQuad_c);

        //各边中点存入四边形
        ab.edgeMid.subQuads.Add(subQuad_a);
        ab.edgeMid.subQuads.Add(subQuad_b);

        bc.edgeMid.subQuads.Add(subQuad_b);
        bc.edgeMid.subQuads.Add(subQuad_c);

        ca.edgeMid.subQuads.Add(subQuad_c);
        ca.edgeMid.subQuads.Add(subQuad_a);
    }
    #endregion
}

