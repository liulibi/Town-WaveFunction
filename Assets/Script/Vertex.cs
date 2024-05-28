using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Vertex 
{
    public int index;
    
    public Vector3 initialPosition;//初始位置
    public Vector3 currentPosition;
    public Vector3 offset = Vector3.zero;
    public List<SubQuad> subQuads = new List<SubQuad>();
    public List<Vertex_Y> vertex_Ys = new List<Vertex_Y>();
    public bool isBoundary;

    public void BoundaryCheck()
    {
        //判断是否为边缘Hex
        bool isBoundaryHex = this is Vertex_hex && ((Vertex_hex)this).coord.radius == Grid.radius;

        //判断是否为边缘mid
        bool isBoundaryMid = this is Vertex_mid && ((Vertex_mid)this).edge.hexes.ToArray()[0].coord.radius == Grid.radius
            && ((Vertex_mid)this).edge.hexes.ToArray()[1].coord.radius == Grid.radius;

        this.isBoundary = isBoundaryHex || isBoundaryMid;
    }

    public Mesh CreatMesh()
    {
        List<Vector3> meshVerties = new List<Vector3>();
        List<int> meshTriangles = new List<int>();

        foreach (SubQuad subQuad in subQuads)
        {
            if (this is Vertex_center)//中心点
            {
                meshVerties.Add(currentPosition);
                meshVerties.Add(subQuad.GetMid_cd());
                meshVerties.Add(subQuad.GetCenterPosition());
                meshVerties.Add(subQuad.GetMid_bc());
            }
            else if (this is Vertex_mid)//中点
            {
                if (subQuad.b == this)
                {
                    meshVerties.Add(currentPosition);
                    meshVerties.Add(subQuad.GetMid_bc());
                    meshVerties.Add(subQuad.GetCenterPosition());
                    meshVerties.Add(subQuad.GetMid_ab());
                }
                else
                {
                    meshVerties.Add(currentPosition);
                    meshVerties.Add(subQuad.GetMid_da());
                    meshVerties.Add(subQuad.GetCenterPosition());
                    meshVerties.Add(subQuad.GetMid_cd());
                }
            }
            else//顶点
            {
                meshVerties.Add(currentPosition);
                meshVerties.Add(subQuad.GetMid_ab());
                meshVerties.Add(subQuad.GetCenterPosition());
                meshVerties.Add(subQuad.GetMid_da());
            }

        }

        for (int i = 0; i < meshVerties.Count; i++)
        {
            meshVerties[i] -= currentPosition;
        }
        for (int i = 0; i < subQuads.Count; i++)
        {
            meshTriangles.Add(i * 4);
            meshTriangles.Add(i * 4 + 1);
            meshTriangles.Add(i * 4 + 2);
            meshTriangles.Add(i * 4);
            meshTriangles.Add(i * 4 + 2);
            meshTriangles.Add(i * 4 + 3);
        }
        Mesh mesh = new Mesh();
        mesh.vertices = meshVerties.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        return mesh;
    }


    public void Relax()
    {
        currentPosition = initialPosition + offset;
    }
}

public class Coord//坐标
{
    //readonly 关键字应用于字段时，
    //表示该字段的值只能在声明时或构造函数中进行初始化，
    //并且在初始化后不能再被修改。
    //这意味着只读字段的值在对象的整个生命周期中保持不变。
    public readonly int q;
    public readonly int r;
    public readonly int s;

    public readonly int radius;
    public readonly Vector3 worldPosition;

    public Coord(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        this.radius = Mathf.Max(Mathf.Abs(r), Mathf.Abs(s), Mathf.Abs(q));
        worldPosition = WorldPosition();
    }
    
    public Vector3 WorldPosition()
    {
        return new Vector3(q * Mathf.Sqrt(3) / 2, 0, -(float)r - ((float)q / 2)) * 2 * Grid.cellSize;
    }

    static public Coord[] directions = new Coord[]
    {
        new Coord(0,+1,-1),
        new Coord(-1,+1,0),
        new Coord(-1,0,+1),
        new Coord(0,-1,+1),
        new Coord(+1,-1,0),
        new Coord(+1,0,-1),
    };

    static public Coord Direction(int direction)//返回6个角上所输入方向
    {
        return Coord.directions[direction];
    }

    public Coord Add(Coord coord)
    {
        return new Coord(q + coord.q, r + coord.r, s + coord.s);
    }

    public Coord Scale(int k)
    {
        return new Coord(q * k, r * k, s * k);
    }

    public Coord Neighbor(int direction)
    {
        return Add(Direction(direction));
    }

    //创建环
    public static List<Coord> Coord_Ring(int radius)
    {
        List<Coord> result = new List<Coord>();
        if (radius == 0)
        {
            //如果radius==0，则返回本身,并且加入list中
            result.Add(new Coord(0,0,0));
        }
        else
        {
            //如果半径大于0，则形成环list
            Coord coord = Coord.Direction(4).Scale(radius);
            for (int i = 0; i < 6; i++) 
            {
                for (int j = 0; j < radius; j++)
                {
                    result.Add(coord);
                    coord = coord.Neighbor(i);
                }
            }
        }
        return result;
    }

    //创建点阵
    public static List<Coord> Coord_Hex(int radius)
    {
        List<Coord> result = new List<Coord>();
        for (int i = 0; i <= radius; i++)
        {
            //AddRange 是一个用于向列表（List）或数组（Array）中添加多个元素的方法。
            //它可以将一个集合中的元素添加到目标列表或数组的末尾。
            result.AddRange(Coord_Ring(i));
        }
        return result;
    }
}

public class Vertex_hex : Vertex //Vertex顶点
{
    public readonly Coord coord;

    public Vertex_hex(Coord coord)
    {
        this.coord = coord;
        initialPosition = coord.worldPosition;
        currentPosition = initialPosition;
    }

    public static void Hex(List<Vertex_hex> vertices,int radius)
    {
        foreach(Coord coord in Coord.Coord_Hex(radius))
        {
            vertices.Add(new Vertex_hex(coord));
        }
    }

    public static List<Vertex_hex> GrabRing(int  radius,List<Vertex_hex> vertices)
    {
        if (radius == 0)
        {
            // 是一个用于从列表（List）中获取指定范围的元素的方法
            return vertices.GetRange(0, 1);
        }
        //求出每一环的开始元素为radius*（radius-1）*3+1，个数为radius*6
        return vertices.GetRange(radius * (radius - 1) * 3 + 1, radius * 6);

    }

    //如果是顶点，创建meshcollider
    //subQuad不是为顺序添加，需要判断是否为相邻点，
    //如果第一个subQuad的d，与另一个subQuad的b点相同，相邻
    //一条边的组成就为第一个subQuad中心点，第一个subQuad的ab边中点，第二个subQuad的中心点
    public List<Mesh> CreatSideMesh()
    {
        int n = this.subQuads.Count;
        List<Mesh> meshes = new List<Mesh>();

        for (int i = 0; i < n; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();

            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.down * Grid.cellHeight / 2);

            foreach(SubQuad subQuad in subQuads)
            {
                if (subQuad.d == subQuads[i].b)
                {
                    meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                    meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                    break;
                }
            }

            for (int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }

            meshTriangles.Add(0);
            meshTriangles.Add(2);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(4);
            meshTriangles.Add(5);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(3);

            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;
    }
}

public class Vertex_mid : Vertex//边中点
{
    public readonly Edge edge;
    public Vertex_mid(Edge edge,List<Vertex_mid> edgeMids)
    {
        this.edge = edge;
        //将hasset中的点取出
        Vertex_hex a = edge.hexes.ToArray()[0];
        Vertex_hex b = edge.hexes.ToArray()[1];

        edgeMids.Add(this);

        initialPosition = (a.initialPosition + b.initialPosition) / 2;
        currentPosition = initialPosition;
    }

    //中点创建meshcollider
    //判断相邻
    //当c点或者a点为同一点时相邻，且一条边为中心点，subquad的bc边中点，下一个subquad中心点
    //首先判断subQuad的b点是否为this，如果是则取subQuad中心点，subQuad的bc边中点，下一个与subQuad的c点相同的subQuad中心点
    //再判断subQuad的d点是否为this，如果是则取subQuad中心点，subQuad的da边中点，下一个与subQuad的c点相同的subQuad中心点
    public List<Mesh> CreatSideMesh()
    {
        List<Mesh> meshes = new List<Mesh>();

        for (int i = 0; i < 4; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();

            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);

            if (subQuads[i].b == this)
            {
                meshVertices.Add(subQuads[i].GetMid_bc() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_bc()+Vector3.down * Grid.cellHeight / 2);

                foreach(SubQuad subQuad in subQuads)
                {
                    if (subQuad.c == subQuads[i].c && subQuad != subQuads[i])
                    {
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                        break;
                    }
                }
            }
            else
            {
                meshVertices.Add(subQuads[i].GetMid_da() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_da() + Vector3.down * Grid.cellHeight / 2);

                foreach (SubQuad subQuad in subQuads)
                {
                    if (subQuad.a == subQuads[i].a && subQuad != subQuads[i])
                    {
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
                        break;
                    }
                }
            }


            for (int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }

            meshTriangles.Add(0);
            meshTriangles.Add(2);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(4);
            meshTriangles.Add(5);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(3);

            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;
    }
}

public class Vertex_center : Vertex 
{
    //中心点创建meshcollider
    //如果是中心点，subQuad由顺序添加
    //一条边的组成就为第一个subQuad中心点，第一个subQuad的cd边中点，第二个subQuad的中心点
    public List<Mesh> CreatSideMesh()
    {
        int n = this.subQuads.Count;
        List<Mesh> meshes = new List<Mesh>();

        for(int i = 0; i < n; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();

            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[(i + n - 1) % n].GetCenterPosition() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.down * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[(i + n - 1) % n].GetCenterPosition() + Vector3.down * Grid.cellHeight / 2);

            for(int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }

            meshTriangles.Add(0);
            meshTriangles.Add(1);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(4);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(1);
            meshTriangles.Add(5);
            meshTriangles.Add(4);

            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;
    }
}

public class Vertex_triangleCenter : Vertex_center
{ 
    public Vertex_triangleCenter(Triangle triangle)
    {
        initialPosition = (triangle.a.initialPosition + triangle.b.initialPosition + triangle.c.initialPosition) / 3;
        currentPosition = initialPosition;
    }
}

public class Vertex_quadCenter : Vertex_center
{
    public Vertex_quadCenter(Quad quad)
    {
        initialPosition = (quad.a.initialPosition + quad.b.initialPosition + quad.c.initialPosition + quad.d.initialPosition) / 4;
        currentPosition = initialPosition;
    }
}

public class Vertex_Y 
{
    public readonly string name;

    public readonly Vertex vertex;
    public readonly int y;
    public readonly Vector3 worldPosition;

    public readonly bool isBoundary;

    public bool isActive;
    public List<SubQuad_Cube> subQuad_Cubes = new List<SubQuad_Cube>();

    public Vertex_Y(Vertex vertex, int y)
    {
        this.vertex = vertex;
        this.y = y;
        name = "Vertex_" + vertex.index + "_" + y;
        isBoundary = vertex.isBoundary || y == Grid.height || y == 0;
        worldPosition = vertex.currentPosition + Vector3.up * (y * Grid.cellHeight);
    }
}


