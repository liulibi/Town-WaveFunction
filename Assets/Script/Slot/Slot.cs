using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class Slot :MonoBehaviour
{
    public List<Module> possiableModules;
    public SubQuad_Cube subQuad_Cube;

    public Material material;


    private void Awake()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
    }

    public void Initialized(ModuleLibrary moduleLibrary, SubQuad_Cube subQuad_Cube, Material material)
    {
        this.subQuad_Cube = subQuad_Cube;
        ResetPossiableModules(moduleLibrary);
        this.material = material;
    }

    public void ResetPossiableModules(ModuleLibrary moduleLibrary)
    {
        possiableModules = moduleLibrary.GetModules(subQuad_Cube.bit);
    }

    //旋转模型
    private void RotationModule(Mesh mesh, int rotation)
    {
        if(rotation!=0)
        {
            Vector3[] vertices = mesh.vertices;
            for(int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Quaternion.AngleAxis(90 * rotation, Vector3.up) * vertices[i];
            }
            mesh.vertices = vertices;
        }
    }

    //镜像模型
    private void FlipModule(Mesh mesh,bool flip)
    {
        if (flip)
        {
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(-vertices[i].x, vertices[i].y, vertices[i].z);
            }
            mesh.vertices = vertices;
            mesh.triangles = mesh.triangles.Reverse().ToArray();
        }
    }

    //模型网格点与网格变形对齐
    private void DeformModule(Mesh mesh,SubQuad_Cube subQuad_Cube)
    {
        Vector3[] vertices = mesh.vertices;
        SubQuad subQuad = subQuad_Cube.subQuad;
        for (int i = 0; i < vertices.Length; i++) 
        {
            Vector3 ad_x = Vector3.Lerp(subQuad.a.currentPosition, subQuad.d.currentPosition, (vertices[i].x + 0.5f));
            Vector3 bc_x = Vector3.Lerp(subQuad.b.currentPosition, subQuad.c.currentPosition, (vertices[i].x + 0.5f));
            vertices[i] = Vector3.Lerp(ad_x, bc_x, (vertices[i].z + 0.5f)) + Vector3.up * vertices[i].y * Grid.cellHeight - subQuad.GetCenterPosition();
        }
        mesh.vertices = vertices;
    }

    public void UpdateModule(Module module)
    {
        gameObject.GetComponent<MeshFilter>().mesh = module.mesh;
        FlipModule(gameObject.GetComponent<MeshFilter>().mesh, module.flip);
        RotationModule(gameObject.GetComponent<MeshFilter>().mesh, module.rotation);
        DeformModule(gameObject.GetComponent<MeshFilter>().mesh, subQuad_Cube);
        gameObject.GetComponent<MeshRenderer>().material = material;
        gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        gameObject.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }
}
