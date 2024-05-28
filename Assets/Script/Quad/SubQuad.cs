using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SubQuad
{
    public readonly Vertex_hex a;
    public readonly Vertex_mid b;
    public readonly Vertex_center c;
    public readonly Vertex_mid d;
    public readonly float offsetNumber = 0.1f;//偏移系数
    public List<SubQuad_Cube> subQuad_Cubes = new List<SubQuad_Cube>();

    public SubQuad(Vertex_hex a, Vertex_mid b, Vertex_center c, Vertex_mid d, List<SubQuad> subQuads)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        subQuads.Add(this);
    }

    public void CalculateRelaxOffset()
    {
        Vector3 center = (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition) / 4;

        Vector3 vector_a = (a.currentPosition
                          + Quaternion.AngleAxis(-90, Vector3.up) * (b.currentPosition - center) + center
                          + Quaternion.AngleAxis(-180, Vector3.up) * (c.currentPosition - center) + center
                          + Quaternion.AngleAxis(-270, Vector3.up) * (d.currentPosition - center) + center) / 4;

        Vector3 vector_b = Quaternion.AngleAxis(90, Vector3.up) * (vector_a - center) + center;
        Vector3 vector_c = Quaternion.AngleAxis(180, Vector3.up) * (vector_a - center) + center;
        Vector3 vector_d = Quaternion.AngleAxis(270, Vector3.up) * (vector_a - center) + center;

        a.offset += (vector_a - a.currentPosition) * offsetNumber;
        b.offset += (vector_b - b.currentPosition) * offsetNumber;
        c.offset += (vector_c - c.currentPosition) * offsetNumber;
        d.offset += (vector_d - d.currentPosition) * offsetNumber;
    }

    public Vector3 GetCenterPosition()
    {
        return (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition) / 4;
    }
    public Vector3 GetMid_ab()
    {
        return (a.currentPosition + b.currentPosition) / 2;
    }
    public Vector3 GetMid_bc()
    {
        return (b.currentPosition + c.currentPosition) / 2;
    }
    public Vector3 GetMid_cd()
    {
        return (c.currentPosition + d.currentPosition) / 2;
    }
    public Vector3 GetMid_da()
    {
        return (d.currentPosition + a.currentPosition) / 2; 
    }
}
public class SubQuad_Cube
{
    public readonly SubQuad subQuad;

    public readonly int y;

    public readonly Vector3 centerPostion;

    public readonly Vertex_Y[] vertex_Ys = new Vertex_Y[8];

    public string bit = "00000000";

    public string pre_bit = "00000000";

    public SubQuad_Cube(SubQuad subQuad, int y)
    {
        this.subQuad = subQuad;
        this.y = y;
        centerPostion = subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight * (y + 0.5f);

        vertex_Ys[0] = subQuad.a.vertex_Ys[y + 1];
        vertex_Ys[1] = subQuad.b.vertex_Ys[y + 1];
        vertex_Ys[2] = subQuad.c.vertex_Ys[y + 1];
        vertex_Ys[3] = subQuad.d.vertex_Ys[y + 1];
        vertex_Ys[4] = subQuad.a.vertex_Ys[y];
        vertex_Ys[5] = subQuad.b.vertex_Ys[y];
        vertex_Ys[6] = subQuad.c.vertex_Ys[y];
        vertex_Ys[7] = subQuad.d.vertex_Ys[y];

        foreach(Vertex_Y vertex_Y in vertex_Ys)
        {
            vertex_Y.subQuad_Cubes.Add(this);
        }
    }

    public void UpdateBit()
    {
        pre_bit = bit;//存储上一次bit值
        string result = "";
        if (vertex_Ys[0].isActive) result += "1";
        else result += "0";

        if (vertex_Ys[1].isActive) result += "1";
        else result += "0";

        if (vertex_Ys[2].isActive) result += "1";
        else result += "0";

        if (vertex_Ys[3].isActive) result += "1";
        else result += "0";

        if (vertex_Ys[4].isActive) result += "1";
        else result += "0";

        if (vertex_Ys[5].isActive) result += "1";
        else result += "0";

        if (vertex_Ys[6].isActive) result += "1";
        else result += "0";

        if (vertex_Ys[7].isActive) result += "1";
        else result += "0";

        bit = result;
    }
}
