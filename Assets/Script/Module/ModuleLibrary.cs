using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;

[CreateAssetMenu(menuName ="ScriptableObject/ModuleLibrary")]
public class ModuleLibrary:ScriptableObject
{
    [SerializeField]
    private GameObject importedModules;

    public Material Material;

    private Dictionary<string, List<Module>> moduleLibrary = new Dictionary<string, List<Module>>();

    private void Awake()
    {
        ImportedModule();
    }

    #region 导入模型
    public void ImportedModule()
    {
        //将8个顶点是否激活的所有状态导入字典中
        for (int i = 1; i < 256; i++)
        {
            string binaryString = Convert.ToString(i, 2).PadLeft(8, '0'); // 将整数转换为 8 位的二进制字符串
            List<Module> moduleList = new List<Module>(); // 创建一个空的 Module 列表

            moduleLibrary.Add(binaryString, moduleList); // 将二进制字符串作为键，空的 Module 列表作为值添加到 moduleLibrary 字典中
        }

        //将importedModules中的元素对应名称导入
        foreach (Transform child in importedModules.transform)
        {
            Mesh mesh = child.GetComponent<MeshFilter>().sharedMesh;// 获取子物体的网格
            string name = child.name;
            name = GetStringName(name);
            moduleLibrary[name].Add(new Module(name, mesh, 0, false));
            if (!RotationEqualCheck(name))//如果旋转之后不重合
            {
                moduleLibrary[RotateName(name, 1)].Add(new Module(RotateName(name, 1), mesh, 1, false));
                if (!RotationTwiceEqualCheck(name))//旋转两次不重回
                {
                    moduleLibrary[RotateName(name, 2)].Add(new Module(RotateName(name, 2), mesh, 2, false));
                    moduleLibrary[RotateName(name, 3)].Add(new Module(RotateName(name, 3), mesh, 3, false));

                    if (!FilpRotationEqualCheck(name))//镜像不重回
                    {
                        moduleLibrary[FlipName(name)].Add(new Module(FlipName(name), mesh, 0, true));
                        moduleLibrary[RotateName(FlipName(name), 1)].Add(new Module(RotateName(FlipName(name), 1), mesh, 1, true));
                        moduleLibrary[RotateName(FlipName(name), 2)].Add(new Module(RotateName(FlipName(name), 2), mesh, 2, true));
                        moduleLibrary[RotateName(FlipName(name), 3)].Add(new Module(RotateName(FlipName(name), 3), mesh, 3, true));
                    }
                }
            }
        }
        
    }

    private String RotateName(string name,int time)
    {
        string result = name;
        for (int i = 0;i < time; i++)
        {
            //顺时针旋转
            //将索引为 3 的字符放在首位，然后将索引为 0 到 2 的子字符串放在其后，
            //接着将索引为 7 的字符放在第五位，最后将索引为 4 到 6 的子字符串放在其后
            result = result[3] + result.Substring(0, 3) + result[7] + result.Substring(4, 3);
        }
        return result;
    }

    private string FlipName(string name)
    {
        return name[3].ToString() + name[2] + name[1] + name[0] + name[7] + name[6] + name[5] + name[4];
    }

    private bool RotationEqualCheck(string name)
    {
        //旋转一次
        return name[0] == name[1] && name[1] == name[2] && name[2] == name[3] 
            && name[4] == name[5] && name[5] == name[6] && name[6] == name[7];
    }

    private bool RotationTwiceEqualCheck(string name)
    {
        //旋转两次
        return name[0] == name[2] && name[1] == name[3] // 0 1 __ 2 3   4 5  __ 6 7
            && name[4] == name[6] && name[5] == name[7];// 3 2    1 0   7 6     5 4
    }

    private bool FilpRotationEqualCheck(string name)
    {
        string symmetry_vertical = name[3].ToString() + name[2] + name[1] + name[0] + name[7] + name[6] + name[5] + name[4];
        string symmetry_horizontal = name[1].ToString() + name[0] + name[3] + name[2] + name[5] + name[4] + name[7] + name[6];
        string symmetry_02 = name[0].ToString() + name[3] + name[2] + name[1] + name[4] + name[7] + name[6] + name[5];
        string symmetry_13 = name[2].ToString() + name[1] + name[0] + name[3] + name[6] + name[5] + name[4] + name[7];
        return name == symmetry_vertical || name == symmetry_horizontal || name == symmetry_02 || name == symmetry_13;

    }
    #endregion

    public List<Module> GetModules(string name) 
    {
        List<Module> result;
        if(moduleLibrary.TryGetValue(name, out result))
        {
            return result;
        }
        return null;
    }

    string GetStringName(string input)
    {
        input = input.Remove(4, 1);//移除名称中的空格
        if (input.Length != 8)
        {
            Debug.LogError("输入字符串长度必须为8。");
            return input;
        }

        char[] charArray = input.ToCharArray();

        // 对换第一位和第三位
        char temp = charArray[0];
        charArray[0] = charArray[2];
        charArray[2] = temp;

        // 对换第二位和第四位
        temp = charArray[1];
        charArray[1] = charArray[3];
        charArray[3] = temp;

        // 对换第五位和第七位
        temp = charArray[4];
        charArray[4] = charArray[6];
        charArray[6] = temp;

        // 对换第六位和第八位
        temp = charArray[5];
        charArray[5] = charArray[7];
        charArray[7] = temp;

        return new string(charArray);

    }
}
