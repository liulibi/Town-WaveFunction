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
        //����slot��ײ�常����
        GameObject slotCollider = new GameObject(GetSlotColliderName(vertex_Y), typeof(SlotCollider));
        slotCollider.GetComponent<SlotCollider>().vertex_Y = vertex_Y;
        slotCollider.transform.SetParent(transform);
        slotCollider.transform.localPosition = vertex_Y.worldPosition;

        //����������ײ��
        //�봴��cursor��ͬ
        GameObject top = new GameObject("top_to_" + (vertex_Y.y + 1), typeof(MeshCollider), typeof(SlotCollider_Top));
        top.layer = LayerMask.NameToLayer("SlotCollider");
        top.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreatMesh();
        top.transform.SetParent(slotCollider.transform);
        top.transform.localPosition = Vector3.up * Grid.cellHeight * (0.5f);

        //�����ײ���ײ��
        //�봴��CursorUI��ͬ
        GameObject bottom = new GameObject("bottom_to_" + (vertex_Y.y - 1), typeof(MeshCollider), typeof(SlotCollider_Bottom));
        bottom.layer = LayerMask.NameToLayer("SlotCollider");
        bottom.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreatMesh();
        bottom.GetComponent<MeshCollider>().sharedMesh.triangles = bottom.GetComponent<MeshCollider>().sharedMesh.triangles.Reverse().ToArray();
        bottom.transform.SetParent(slotCollider.transform);
        bottom.transform.localPosition = Vector3.down * Grid.cellHeight * (0.5f);

        //����������ײ�棬���������ڿ�

        //��������ĵ㣬subQuad��˳�����
        //һ���ߵ���ɾ�Ϊ��һ��subQuad���ĵ㣬��һ��subQuad��cd���е㣬�ڶ���subQuad�����ĵ�
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

        //����Ƕ��㣬����meshcollider
        //subQuad����Ϊ˳����ӣ���Ҫ�ж��Ƿ�Ϊ���ڵ㣬
        //�����һ��subQuad��d������һ��subQuad��b����ͬ������
        //һ���ߵ���ɾ�Ϊ��һ��subQuad���ĵ㣬��һ��subQuad��ab���е㣬�ڶ���subQuad�����ĵ�
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

        //������е�
        //�е㴴��meshcollider
        //�ж�����
        //��c�����a��Ϊͬһ��ʱ���ڣ���һ����Ϊ���ĵ㣬subquad��bc���е㣬��һ��subquad���ĵ�
        //�����ж�subQuad��b���Ƿ�Ϊthis���������ȡsubQuad���ĵ㣬subQuad��bc���е㣬��һ����subQuad��c����ͬ��subQuad���ĵ�
        //���ж�subQuad��d���Ƿ�Ϊthis���������ȡsubQuad���ĵ㣬subQuad��da���е㣬��һ����subQuad��c����ͬ��subQuad���ĵ�
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

