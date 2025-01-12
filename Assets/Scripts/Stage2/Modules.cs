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
        public Vector3 axis;//���Ǹ�����ת
        public int rotationDirection;//˳ʱ�뻹����ʱ��
        public int times;//��ת����
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
            //      2c<--1b    ������ͬʱ����Ҳ����������ӳ�䣨0��Ӧa��1��Ӧb��������
            //������Ϊx�ᣬ��Ϊz��,
            Vector3[] vertices = M_Module.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                //����mesh�Ķ��������Ϊ�ֲ�����ϵ�����ǵ�mesh��С��ʱΪ1�������Ļ�mesh���е�ľֲ����귶ΧΪ[-0.5��0.5];
                Vector3 x_da = Vector3.Lerp(cube.vertices[3].currentPosition, cube.vertices[0].currentPosition, (vertices[i].x + 0.5f));//!!!!!!���Ǵ�С����d-->aΪ������
                Vector3 x_cb = Vector3.Lerp(cube.vertices[2].currentPosition, cube.vertices[1].currentPosition, (vertices[i].x + 0.5f));//c-->bΪ������

                //���Կ�������ʹ�õ�currentPostion���������꣬��������Ҫ��ؾֲ����꣬����Ҫ��ȥ��
                Vector3 centerPosition = (cube.vertices[0].currentPosition + cube.vertices[1].currentPosition + cube.vertices[2].currentPosition + cube.vertices[3].currentPosition) / 4;
                vertices[i] = Vector3.Lerp(x_cb, x_da, (vertices[i].z + 0.5f)) + Vector3.up * vertices[i].y * GridManager.s_cellHeight - centerPosition;//cb-->daΪ������
                //                                                               ǰ�涼��ƽ���ϣ������ⲿ�ֲ�������� 
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
            //��UNity������ϵΪ��������ת��������ϵ(��Ĵָ��ָ����Ϊ�ᣬ��ָ��������Ϊ��ת����)
            //      3-->0
            //         |
            //      2<--1
            for (int i = 0; i < s_RotationInfo.times; i++)
            {
                if (s_RotationInfo.axis == Vector3.right)//X��
                {
                    if(s_RotationInfo.rotationDirection==1) name = name[1].ToString() + name[5] + name[6] + name[2] + name[0] + name[4] + name[7] + name[3];
                    else name = name[4].ToString() + name[0] + name[3] + name[7] + name[5] + name[1] + name[2] + name[6];
                }
                else if (s_RotationInfo.axis == Vector3.forward)//Z��
                {
                    if (s_RotationInfo.rotationDirection == 1) name = name[4].ToString() + name[5] + name[1] + name[0] + name[7] + name[6] + name[2] + name[3];
                    else name = name[3].ToString() + name[2] + name[6] + name[7] + name[0] + name[1] + name[5] + name[4];
                }
                else if (s_RotationInfo.axis == Vector3.up)//Y��
                {
                    if (s_RotationInfo.rotationDirection == 1) name = name[3].ToString() + name[0] + name[1] + name[2] + name[7] + name[4] + name[5] + name[6];
                    else name = name[1].ToString() + name[2] + name[3] + name[0] + name[5] + name[6] + name[7] + name[4];
                }
            }
            return name;
        }
        public static string FlipName(string name) 
        {
            //������ plane="YoZ" ���о���
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

            // ��ȡԭʼMesh�Ķ���
            Vector3[] originalVertices = M_Module.vertices;
            // ����һ���µĶ�������
            Vector3[] rotatedVertices = new Vector3[originalVertices.Length];
            // ��ÿ������Ӧ����ת
            for (int i = 0; i < originalVertices.Length; i++)
            {
                rotatedVertices[i] = rotation * originalVertices[i];
            }
            // ����һ���µ�Mesh
            Mesh M_NewModule = new Mesh
            {
                vertices = rotatedVertices,
                triangles = M_Module.triangles, // ��������������
                //uv = M_Module.uv // UV���걣�ֲ���
            };

            M_NewModule.RecalculateBounds();
            M_NewModule.RecalculateNormals();
            return M_NewModule;

        }
        public static Mesh DeriveFromMirror(Mesh M_Module)
        {
            // ��ȡԭʼMesh�Ķ���
            Vector3[] originalVertices = M_Module.vertices;
            // ����һ���µĶ�������
            Vector3[] mirrorVertices = new Vector3[originalVertices.Length];
            // ��ÿ�������ȡ����
            for (int i = 0; i < originalVertices.Length; i++)
            {
                mirrorVertices[i] = new Vector3(-originalVertices[i].x, originalVertices[i].y, originalVertices[i].z);
            }

            // ����һ���µ�Mesh
            Mesh M_NewModule = new Mesh
            {
                vertices = mirrorVertices,
                triangles = M_Module.triangles.Reverse().ToArray(), //��������ᵼ����Ƭ�ķ��߷���ת 
                //uv = M_Module.uv // UV���걣�ֲ���
            };
            M_NewModule.RecalculateBounds();
            M_NewModule.RecalculateNormals();
            return M_NewModule;
        }

        public static HashSet<string> GetModulesByRotation(string bit,Mesh M_Module)
        {
            HashSet<string> allBits = new HashSet<string>();

            foreach (S_RotationInfo s_RotationInfo in s_RotationInfos)//6����
            {
                S_RotationInfo s_YRotation = new S_RotationInfo { axis = Vector3.up, rotationDirection = 1, times = 0 };
                string initial_bit = RotationName(bit, s_RotationInfo);
                for (int t = 0; t < 4; t++)//ת4��
                {
                    s_YRotation.times = t;
                    string new_bit = RotationName(initial_bit, s_YRotation);

                    if (!allBits.Contains(new_bit))
                    {
                        allBits.Add(new_bit);
                        if (modules[new_bit].Count > 0) Debug.Log("Repeated " + new_bit);//���������ظ���
                        modules[new_bit].Add(DeriveFromRotation(M_Module, s_RotationInfo, s_YRotation));

                        //To see���п���
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
