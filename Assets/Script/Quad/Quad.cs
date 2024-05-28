using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Quad 
{
    public readonly Vertex_hex a;
    public readonly Vertex_hex b;
    public readonly Vertex_hex c;
    public readonly Vertex_hex d;

    public readonly Vertex_quadCenter center;

    public readonly Edge ab;
    public readonly Edge bc;
    public readonly Edge cd;
    public readonly Edge da;

    public Quad(Vertex_hex a, Vertex_hex b, Vertex_hex c, Vertex_hex d, List<Vertex_center> center, List<Edge> edges, List<Quad> quads)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        this.center = new Vertex_quadCenter(this);
        this.ab = Edge.FindEdge(a, b, edges);
        this.bc = Edge.FindEdge(b, c, edges);
        this.cd = Edge.FindEdge(c, d, edges);
        this.da = Edge.FindEdge(d, a, edges);
        quads.Add(this);
        center.Add(this.center);
    }

    public void Subdivide(List<SubQuad> subQuads)
    {
        SubQuad subQuad_a = new SubQuad(a, ab.edgeMid, center, da.edgeMid, subQuads);
        SubQuad subQuad_b = new SubQuad(b, bc.edgeMid, center, ab.edgeMid, subQuads);
        SubQuad subQuad_c = new SubQuad(c, cd.edgeMid, center, bc.edgeMid, subQuads);
        SubQuad subQuad_d = new SubQuad(d, da.edgeMid, center, cd.edgeMid, subQuads);

        //顶点存入四边形
        a.subQuads.Add(subQuad_a);
        b.subQuads.Add(subQuad_b);
        c.subQuads.Add(subQuad_c);
        d.subQuads.Add(subQuad_d);

        //中心点存入四边形
        center.subQuads.Add(subQuad_a);
        center.subQuads.Add(subQuad_b);
        center.subQuads.Add(subQuad_c);
        center.subQuads.Add(subQuad_d);

        //各边中点存入四边形
        ab.edgeMid.subQuads.Add(subQuad_a);
        ab.edgeMid.subQuads.Add(subQuad_b);

        bc.edgeMid.subQuads.Add(subQuad_b);
        bc.edgeMid.subQuads.Add(subQuad_c);

        cd.edgeMid.subQuads.Add(subQuad_c);
        cd.edgeMid.subQuads.Add(subQuad_d);

        da.edgeMid.subQuads.Add(subQuad_d);
        da.edgeMid.subQuads.Add(subQuad_a);
    }
}
