using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace TS
{
    public struct S_RotationInfo
    {
        public Vector3 axis;//绕那个轴旋转
        public int rotationDirection;//顺时针还是逆时针
        public int times;//旋转次数
    }
    public class Modules
    {
        public GameObject G_Modules;
        public static Dictionary<string,List<Mesh>> modules = new Dictionary<string,List<Mesh>>();

        public static List<S_RotationInfo> s_RotationInfos = new List<S_RotationInfo>
        {
            new S_RotationInfo {axis=Vector3.up,rotationDirection = 1,times=0},
            new S_RotationInfo {axis=Vector3.right,rotationDirection = 1,times=2},

            new S_RotationInfo {axis=Vector3.right,rotationDirection = 1,times=1},
            new S_RotationInfo {axis=Vector3.right,rotationDirection = -1,times=1},

            new S_RotationInfo {axis=Vector3.forward,rotationDirection = 1,times=1},
            new S_RotationInfo {axis=Vector3.forward,rotationDirection = -1,times=1},
        };
        
        public static List<Mesh> GetPossiableModules(string bit)
        {
            return modules[bit];
        }
        public static void DeformModule(Mesh M_Module,Cube cube)
        {
            //      3d-->0a
            //         |
            //      2c<--1b    ！！！同时这里也完成了坐标的映射（0对应a，1对应b。。。）
            //横坐标为x轴，纵为z轴,
            Vector3[] vertices = M_Module.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                //这里mesh的顶点坐标均为局部坐标系，我们的mesh大小此时为1，这样的话mesh所有点的局部坐标范围为[-0.5至0.5];
                Vector3 x_da = Vector3.Lerp(cube.vertices[3].currentPosition, cube.vertices[0].currentPosition, (vertices[i].x + 0.5f));//!!!!!!均是从小到大，d-->a为增大方向
                Vector3 x_cb = Vector3.Lerp(cube.vertices[2].currentPosition, cube.vertices[1].currentPosition, (vertices[i].x + 0.5f));//c-->b为增大方向

                //可以看到上面使用的currentPostion是世界坐标，我们人需要变回局部坐标，则需要减去；
                Vector3 centerPosition = (cube.vertices[0].currentPosition + cube.vertices[1].currentPosition + cube.vertices[2].currentPosition + cube.vertices[3].currentPosition) / 4;
                vertices[i] = Vector3.Lerp(x_cb, x_da, (vertices[i].z + 0.5f)) + Vector3.up * vertices[i].y * GridManager.s_cellHeight - centerPosition;//cb-->da为增大方向
                //                                                               前面都是平面上，加上这部分才是立体的 
            }
            M_Module.vertices = vertices;

            M_Module.RecalculateBounds();
            M_Module.RecalculateNormals();
        }

        public static void SetAllModules(GameObject G_Modules)
        {
            for(int i = 1; i < 256; i++)
            {
                string bit = Convert.ToString(i, 2).PadLeft(8, '0');
                modules[bit] = new List<Mesh>();
            }

            foreach(Transform childTransform in G_Modules.transform)
            {
                string bit = childTransform.name;
                Mesh M_Module = childTransform.GetComponent<MeshFilter>().sharedMesh;

                HashSet<string> allBits = GetModulesByRotation(bit, M_Module);

                if (!allBits.Contains(FlipName(bit)))
                {
                    //Debug.Log("has mirror " + bit);
                    bit = FlipName(bit);
                    M_Module = DeriveFromMirror(M_Module);
                    allBits = GetModulesByRotation(bit, M_Module);
                }
            }
        }

        public static string RotationName(string name,S_RotationInfo s_RotationInfo) 
        {
            //以UNity中坐标系为基础，旋转符合左手系(大拇指所指方向为轴，四指弯曲方向为旋转方向)
            //      3-->0
            //         |
            //      2<--1
            for (int i = 0; i < s_RotationInfo.times; i++)
            {
                if (s_RotationInfo.axis == Vector3.right)//X轴
                {
                    if(s_RotationInfo.rotationDirection==1) name = name[1].ToString() + name[5] + name[6] + name[2] + name[0] + name[4] + name[7] + name[3];
                    else name = name[4].ToString() + name[0] + name[3] + name[7] + name[5] + name[1] + name[2] + name[6];
                }
                else if (s_RotationInfo.axis == Vector3.forward)//Z轴
                {
                    if (s_RotationInfo.rotationDirection == 1) name = name[4].ToString() + name[5] + name[1] + name[0] + name[7] + name[6] + name[2] + name[3];
                    else name = name[3].ToString() + name[2] + name[6] + name[7] + name[0] + name[1] + name[5] + name[4];
                }
                else if (s_RotationInfo.axis == Vector3.up)//Y轴
                {
                    if (s_RotationInfo.rotationDirection == 1) name = name[3].ToString() + name[0] + name[1] + name[2] + name[7] + name[4] + name[5] + name[6];
                    else name = name[1].ToString() + name[2] + name[3] + name[0] + name[5] + name[6] + name[7] + name[4];
                }
            }
            return name;
        }
        public static string FlipName(string name) 
        {
            //仅按照 plane="YoZ" 进行镜像
            //      3-->0
            //         |
            //      2<--1
            return name[3].ToString() + name[2] + name[1] + name[0] + name[7]  + name[6] + name[5] + name[4];
        }
        public static Mesh DeriveFromRotation(Mesh M_Module,S_RotationInfo s_RotationInfo, S_RotationInfo s_YRotation)
        {
            Quaternion baseRotation = Quaternion.AngleAxis(90f*s_RotationInfo.rotationDirection*s_RotationInfo.times, s_RotationInfo.axis);
            Quaternion yRotation = Quaternion.AngleAxis(90f*s_YRotation.times, s_YRotation.axis);
            Quaternion rotation = yRotation * baseRotation;

            // 获取原始Mesh的顶点
            Vector3[] originalVertices = M_Module.vertices;
            // 创建一个新的顶点数组
            Vector3[] rotatedVertices = new Vector3[originalVertices.Length];
            // 对每个顶点应用旋转
            for (int i = 0; i < originalVertices.Length; i++)
            {
                rotatedVertices[i] = rotation * originalVertices[i];
            }
            // 创建一个新的Mesh
            Mesh M_NewModule = new Mesh
            {
                vertices = rotatedVertices,
                triangles = M_Module.triangles, // 三角形索引不变
                //uv = M_Module.uv // UV坐标保持不变
            };

            M_NewModule.RecalculateBounds();
            M_NewModule.RecalculateNormals();
            return M_NewModule;

        }
        public static Mesh DeriveFromMirror(Mesh M_Module)
        {
            // 获取原始Mesh的顶点
            Vector3[] originalVertices = M_Module.vertices;
            // 创建一个新的顶点数组
            Vector3[] mirrorVertices = new Vector3[originalVertices.Length];
            // 对每个顶点获取镜像
            for (int i = 0; i < originalVertices.Length; i++)
            {
                mirrorVertices[i] = new Vector3(-originalVertices[i].x, originalVertices[i].y, originalVertices[i].z);
            }

            // 创建一个新的Mesh
            Mesh M_NewModule = new Mesh
            {
                vertices = mirrorVertices,
                triangles = M_Module.triangles.Reverse().ToArray(), //镜像操作会导致面片的法线方向反转 
                //uv = M_Module.uv // UV坐标保持不变
            };
            M_NewModule.RecalculateBounds();
            M_NewModule.RecalculateNormals();
            return M_NewModule;
        }

        public static HashSet<string> GetModulesByRotation(string bit,Mesh M_Module)
        {
            HashSet<string> allBits = new HashSet<string>();

            foreach (S_RotationInfo s_RotationInfo in s_RotationInfos)//6个面
            {
                S_RotationInfo s_YRotation = new S_RotationInfo { axis = Vector3.up, rotationDirection = 1, times = 0 };
                string initial_bit = RotationName(bit, s_RotationInfo);
                for (int t = 0; t < 4; t++)//转4次
                {
                    s_YRotation.times = t;
                    string new_bit = RotationName(initial_bit, s_YRotation);

                    if (!allBits.Contains(new_bit))
                    {
                        allBits.Add(new_bit);
                        if (modules[new_bit].Count > 0) Debug.Log("Repeated " + new_bit);//不可能有重复的
                        modules[new_bit].Add(DeriveFromRotation(M_Module, s_RotationInfo, s_YRotation));

                        //To see所有可能
                        //GameObject G_Module = new GameObject(new_bit, typeof(MeshFilter), typeof(MeshRenderer));
                        //G_Module.transform.SetParent(GridManager.s_worldCenter);
                        //G_Module.transform.localPosition = Vector3.up*(i++)*2;
                        //G_Module.GetComponent<MeshFilter>().mesh = DeriveFromRotation(M_Module, s_RotationInfo, s_YRotation);
                        //G_Module.GetComponent<MeshRenderer>().s_moduleMaterial = GridManager.s_moduleMaterial;
                    }
                }
            }

            return allBits;
        }

    }
}
