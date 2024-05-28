using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class SlotColliderSystem : MonoBehaviour
{
    private string GetSlotColliderName(Vertex_Y vertex_Y)
    {
        return "SlotCollider" + vertex_Y.name;
    }

    public void CreatCollider(Vertex_Y vertex_Y)
    {
        //创建slot碰撞体父对象
        GameObject slotCollider = new GameObject(GetSlotColliderName(vertex_Y), typeof(SlotCollider));
        slotCollider.GetComponent<SlotCollider>().vertex_Y = vertex_Y;
        slotCollider.transform.SetParent(transform);
        slotCollider.transform.localPosition = vertex_Y.worldPosition;

        //创建顶部碰撞面
        //与创建cursor相同
        GameObject top = new GameObject("top_to_" + (vertex_Y.y + 1), typeof(MeshCollider), typeof(SlotCollider_Top));
        top.layer = LayerMask.NameToLayer("SlotCollider");
        top.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreatMesh();
        top.transform.SetParent(slotCollider.transform);
        top.transform.localPosition = Vector3.up * Grid.cellHeight * (0.5f);

        //创建底部碰撞面
        //与创建CursorUI相同
        GameObject bottom = new GameObject("bottom_to_" + (vertex_Y.y - 1), typeof(MeshCollider), typeof(SlotCollider_Bottom));
        bottom.layer = LayerMask.NameToLayer("SlotCollider");
        bottom.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreatMesh();
        bottom.GetComponent<MeshCollider>().sharedMesh.triangles = bottom.GetComponent<MeshCollider>().sharedMesh.triangles.Reverse().ToArray();
        bottom.transform.SetParent(slotCollider.transform);
        bottom.transform.localPosition = Vector3.down * Grid.cellHeight * (0.5f);

        //创建侧面碰撞面，并计算相邻块

        //如果是中心点，subQuad由顺序添加
        //一条边的组成就为第一个subQuad中心点，第一个subQuad的cd边中点，第二个subQuad的中心点
        if (vertex_Y.vertex is Vertex_center)
        {
            List<Mesh> meshes = ((Vertex_center)vertex_Y.vertex).CreatSideMesh();
            for (int i = 0; i < vertex_Y.vertex.subQuads.Count; i++)
            {
                Vertex_Y neighbor = vertex_Y.vertex.subQuads[i].d.vertex_Ys[vertex_Y.y];
                GameObject side = new GameObject("side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
            }
        }

        //如果是顶点，创建meshcollider
        //subQuad不是为顺序添加，需要判断是否为相邻点，
        //如果第一个subQuad的d，与另一个subQuad的b点相同，相邻
        //一条边的组成就为第一个subQuad中心点，第一个subQuad的ab边中点，第二个subQuad的中心点
        if (vertex_Y.vertex is Vertex_hex)
        {
            List<Mesh> meshes = ((Vertex_hex)vertex_Y.vertex).CreatSideMesh();

            for (int i = 0; i < vertex_Y.vertex.subQuads.Count; i++)
            {
                Vertex_Y neighbor = vertex_Y.vertex.subQuads[i].b.vertex_Ys[vertex_Y.y];
                GameObject side = new GameObject("side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
            }
        }

        //如果是中点
        //中点创建meshcollider
        //判断相邻
        //当c点或者a点为同一点时相邻，且一条边为中心点，subquad的bc边中点，下一个subquad中心点
        //首先判断subQuad的b点是否为this，如果是则取subQuad中心点，subQuad的bc边中点，下一个与subQuad的c点相同的subQuad中心点
        //再判断subQuad的d点是否为this，如果是则取subQuad中心点，subQuad的da边中点，下一个与subQuad的c点相同的subQuad中心点
        if (vertex_Y.vertex is Vertex_mid)
        {
            List<Mesh> meshes = ((Vertex_mid)vertex_Y.vertex).CreatSideMesh();
            for (int i = 0; i < 4; i++)
            {
                Vertex_Y neighbor;
                if (vertex_Y.vertex == vertex_Y.vertex.subQuads[i].b)
                {
                    neighbor = vertex_Y.vertex.subQuads[i].c.vertex_Ys[vertex_Y.y];
                }
                else
                {
                    neighbor = vertex_Y.vertex.subQuads[i].a.vertex_Ys[vertex_Y.y];
                }
                GameObject side = new GameObject("side_to" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void DestroyCollider(Vertex_Y vertex_Y)
    {
        Destroy(transform.Find(GetSlotColliderName(vertex_Y)).gameObject);
        Resources.UnloadUnusedAssets();
    }
}

public class SlotCollider : MonoBehaviour 
{
    public Vertex_Y vertex_Y; 
}

public class SlotCollider_Top : MonoBehaviour
{

}

public class SlotCollider_Bottom : MonoBehaviour { }

public class SlotCollider_Side : MonoBehaviour
{
    public Vertex_Y neighbor;
}

