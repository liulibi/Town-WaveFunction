using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField]
    private int radius;
    [SerializeField] 
    private int height;
    [SerializeField]
    public float cellSize;
    [SerializeField]
    private float cellHeight;
    
    private Grid grid;
    [SerializeField]
    public int relaxTimes;

    [SerializeField]
    private Transform addSphere;
    //private float addNumber = 2f;
    [SerializeField]
    public Transform deleteSphere;

    [SerializeField]
    private ModuleLibrary moduleLibrary;
    [SerializeField]
    private Material moduleMaterial;


    private void Awake()
    {
        grid = new Grid(radius, height, cellSize, cellHeight, relaxTimes);
        moduleLibrary = Instantiate(moduleLibrary);

        if (moduleLibrary == null)
            Debug.LogError("moduleLibrary == null ");

    }

    private void Update()
    {
        #region debug 
        //foreach(Vertex v in grid.vertices)
        //{
        //    foreach(Vertex_Y vertex_Y in v.vertex_Ys)
        //    {
        //        if (!vertex_Y.isActive && Vector3.Distance(vertex_Y.worldPosition, addSphere.position) < addNumber && !vertex_Y.isBoundary) 
        //        {
        //            vertex_Y.isActive = true;
        //        }
        //        else if(vertex_Y.isActive && Vector3.Distance(vertex_Y.worldPosition, deleteSphere.position) < addNumber)
        //        {
        //            vertex_Y.isActive = false;
        //        }
        //    }
        //}
        #endregion

        foreach (SubQuad subQuad in grid.subQuads)
        {
            foreach(SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
            {
                subQuad_Cube.UpdateBit();
                if (subQuad_Cube.pre_bit != subQuad_Cube.bit) 
                {
                    UpdateSlot(subQuad_Cube);
                }
            }
        }
    }

    public void ToggleSlot(Vertex_Y vertex_Y)
    {
        vertex_Y.isActive = !vertex_Y.isActive;

        foreach(SubQuad_Cube subQuad_Cube in vertex_Y.subQuad_Cubes)
        {
            subQuad_Cube.UpdateBit();

            UpdateSlot(subQuad_Cube);
        }
    }

    private void UpdateSlot(SubQuad_Cube subQuad_Cube)
    {
        string parentName = "SlotParent" + grid.subQuads.IndexOf(subQuad_Cube.subQuad) + "_" + subQuad_Cube.y;

        GameObject slot_GameObjectParent;
        if(transform.Find(parentName))
        {
           slot_GameObjectParent = transform.Find(parentName).gameObject;
        }
        else
        {
            slot_GameObjectParent = null;
        }

        if (slot_GameObjectParent == null) 
        {
            if (subQuad_Cube.bit != "00000000" && subQuad_Cube.bit != "11111111")
            {
                slot_GameObjectParent = new GameObject(parentName);
                slot_GameObjectParent.transform.SetParent(transform);
                slot_GameObjectParent.transform.localPosition = subQuad_Cube.centerPostion;

                string name = "Slot" + grid.subQuads.IndexOf(subQuad_Cube.subQuad) + "_" + subQuad_Cube.y;
                GameObject slot_GameObject = new(name, typeof(Slot));
                slot_GameObject.transform.SetParent(slot_GameObjectParent.transform);
                slot_GameObject.transform.localPosition = Vector3.zero;

                Slot slot = slot_GameObject.GetComponent<Slot>();
                slot.Initialized(moduleLibrary, subQuad_Cube, moduleMaterial);
                slot.UpdateModule(slot.possiableModules[0]);
            }
        }
        else
        {
            Slot slot = slot_GameObjectParent.GetComponentInChildren<Slot>();
            if (subQuad_Cube.bit == "00000000" || subQuad_Cube.bit == "11111111")
            {
                Destroy(slot_GameObjectParent);
                Resources.UnloadUnusedAssets();
            }
            else
            {
                slot.ResetPossiableModules(moduleLibrary);
                slot.UpdateModule(slot.possiableModules[0]); 
            }

        }
    }

    public Grid GetGrid()
    {
        return grid;
    }

    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            //foreach (Vertex_hex vertex in grid.hexes)
            //{
            //    Gizmos.DrawSphere(vertex.coord.worldPosition, 0.03f);
            //}

            //Gizmos.color = Color.red;
            //foreach(Triangle triangle in grid.triangles)
            //{
            //    Gizmos.DrawLine(triangle.a.currentPosition, triangle.b.currentPosition);
            //    Gizmos.DrawLine(triangle.b.currentPosition, triangle.c.currentPosition);
            //    Gizmos.DrawLine(triangle.c.currentPosition, triangle.a.currentPosition);
            //    Gizmos.DrawSphere((triangle.a.currentPosition + triangle.b.currentPosition + triangle.c.currentPosition) / 3, 0.05f);
            //}

            //Gizmos.color= Color.green;
            //foreach(Quad quad in grid.quads)
            //{
            //    Gizmos.DrawLine(quad.a.currentPosition, quad.b.currentPosition);
            //    Gizmos.DrawLine(quad.b.currentPosition, quad.c.currentPosition);
            //    Gizmos.DrawLine(quad.c.currentPosition, quad.d.currentPosition);
            //    Gizmos.DrawLine(quad.d.currentPosition, quad.a.currentPosition);
            //}

            //Gizmos.color = Color.white;
            //foreach(Vertex_mid edgeMid in grid.edgeMids)
            //{
            //    Gizmos.DrawSphere(edgeMid.currentPosition, 0.1f);
            //}

            //Gizmos.color = Color.white;
            //foreach(Vertex_center center in grid.vertex_Centers)
            //{
            //    Gizmos.DrawSphere(center.currentPosition, 0.1f);
            //}

            foreach(SubQuad subQuad in grid.subQuads)
            {
                Gizmos.DrawLine(subQuad.a.currentPosition, subQuad.b.currentPosition);
                Gizmos.DrawLine(subQuad.b.currentPosition, subQuad.c.currentPosition);
                Gizmos.DrawLine(subQuad.c.currentPosition, subQuad.d.currentPosition);
                Gizmos.DrawLine(subQuad.d.currentPosition, subQuad.a.currentPosition);
            }

            //foreach(Vertex vertex in grid.vertices)
            //{
            //    foreach(Vertex_Y vertex_Y in vertex.vertex_Ys)
            //    {
            //        if (vertex_Y.isActive == true)
            //            Gizmos.color = Color.red;
            //        else
            //            Gizmos.color = Color.white;

            //        Gizmos.DrawSphere(vertex_Y.worldPosition, 0.1f);
            //    }
            //}

            //foreach(SubQuad subQuad in grid.subQuads)
            //{
            //    foreach(SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
            //    {
            //        Gizmos.color = Color.gray;
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[0].worldPosition, subQuad_Cube.vertex_Ys[1].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[1].worldPosition, subQuad_Cube.vertex_Ys[2].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[2].worldPosition, subQuad_Cube.vertex_Ys[3].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[3].worldPosition, subQuad_Cube.vertex_Ys[0].worldPosition);

            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[4].worldPosition, subQuad_Cube.vertex_Ys[5].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[5].worldPosition, subQuad_Cube.vertex_Ys[6].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[6].worldPosition, subQuad_Cube.vertex_Ys[7].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[7].worldPosition, subQuad_Cube.vertex_Ys[4].worldPosition);

            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[0].worldPosition, subQuad_Cube.vertex_Ys[4].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[1].worldPosition, subQuad_Cube.vertex_Ys[5].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[2].worldPosition, subQuad_Cube.vertex_Ys[6].worldPosition);
            //        Gizmos.DrawLine(subQuad_Cube.vertex_Ys[3].worldPosition, subQuad_Cube.vertex_Ys[7].worldPosition);
            //    }
            //}
        }
    }
}
