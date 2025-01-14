using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TS
{
    public class Cube
    {
        public List<Vertex> vertices;//8个顶点
        public Vector3 centerPosition=Vector3.zero;
        public string bit = "00000000";
        public GameObject G_Module;

        public static  Dictionary<Vertex, List<Vertex>> verticesOfDifferentY = new Dictionary<Vertex, List<Vertex>>();
        public Cube(List<Vertex> vertices)
        {
            this.vertices = vertices;
            foreach (Vertex vertex in vertices)
            {
                centerPosition += vertex.currentPosition;
                vertex.SwitchState += UpdateBit;

                //Stage3
                vertex.AddNeighborVertices += AddNeighborVertices;

                //Stage4
                vertex.SameVertex += CountingSameVertex;
                vertex.SameVertexCount += ResetSameVertexCount;
                vertex.GoingToCollapseCubes += AddGoingToCollapseCubes;
            }
            centerPosition /= 8;//可换成右移

        }



        public static List<Cube> GetCubes(List<SubdivideQuad> subdivideQuads,int height)
        {

            List<Cube> cubes = new List<Cube>();

            foreach(Vertex vertex in SubdivideQuad.vertices)
            {    
                verticesOfDifferentY[vertex] = new List<Vertex> { vertex};
                for (int h = 1; h <= height; h++)
                {
                    verticesOfDifferentY[vertex].Add(new Vertex(vertex.currentPosition, h));
                }
            }

            foreach (SubdivideQuad subdivideQuad in subdivideQuads) 
            {
                Vertex a= subdivideQuad.a, b= subdivideQuad.b,c= subdivideQuad.c,d=subdivideQuad.d;
                for (int h = 1; h <= height; h++)
                {
                    cubes.Add(new Cube(new List<Vertex>
                    {
                        verticesOfDifferentY[a][h],verticesOfDifferentY[b][h],verticesOfDifferentY[c][h],verticesOfDifferentY[d][h],
                        verticesOfDifferentY[a][h-1],verticesOfDifferentY[b][h-1],verticesOfDifferentY[c][h-1],verticesOfDifferentY[d][h-1]
                    }));
                } 
            }
            return cubes;
        }

        public void UpdateBit()
        {
            string new_bit = "";
            for(int i = 0; i < 8; i++)
                new_bit += System.Convert.ToInt32(vertices[i].State);

            bit = new_bit;
            possibleModules = Modules.GetPossiableModules(bit);
            //移入Stage4 SetG_Module()中
            //if (bit == "00000000" || bit == "11111111")
            //{
            //    if (G_Module != null) Object.Destroy(G_Module);
            //}
            //else
            //{
            //    List<Module> possiableModules = Modules.GetPossiableModules(bit);//获取bit对应Module
            //    Mesh mesh = Object.Instantiate(possiableModules[0].mesh);//复制一个，否则更改会影响指向同一个的mesh

            //    Modules.DeformMesh(mesh, this);//变形

            //    if (G_Module != null)
            //    {
            //        G_Module.GetComponent<MeshFilter>().mesh = mesh;
            //        G_Module.GetComponent<MeshRenderer>().material = GridManager.s_moduleMaterial;
            //    }
            //    else
            //    {
            //        G_Module = new GameObject(bit, typeof(MeshFilter), typeof(MeshRenderer));
            //        G_Module.transform.SetParent(GridManager.s_worldCenter);
            //        G_Module.transform.localPosition = centerPosition;
            //        G_Module.GetComponent<MeshFilter>().mesh = mesh;
            //        G_Module.GetComponent<MeshRenderer>().material = GridManager.s_moduleMaterial;
            //    }
            //}
        }
        //Stage3
        private Vertex GetNextVertex(Vertex vertex)
        {
            //差1
            int vertexIndex = vertices.IndexOf(vertex);
            return vertices[(vertexIndex+1)%vertices.Count];
        }
        private Vertex GetUpDownVertex(Vertex vertex)
        {
            //上下差4
            int vertexIndex = vertices.IndexOf(vertex);
            return vertices[(vertexIndex+4)%vertices.Count];
        }
        private void AddNeighborVertices(Vertex vertex,HashSet<Vertex> neighborVertices)
        {
            neighborVertices.Add(GetNextVertex(vertex));
            neighborVertices.Add(GetUpDownVertex(vertex));
        }

        //Stage4
        public Module module = null;
        public List<Module> possibleModules = new List<Module>();

        public static Dictionary<int, List<int>> faceVertices = new Dictionary<int, List<int>>
        {
            //详见Module socket 规定部分(上下 然后顺序一圈)
            {0 ,new List<int>{0,1,2,3 }},
            {1 ,new List<int>{4,5,6,7 }},

            {2 ,new List<int>{0,1,4,5 }},
            {3 ,new List<int>{1,2,5,6 }},

            {4 ,new List<int>{2,3,6,7 }},
            {5 ,new List<int>{0,3,4,7 }},
            
        };
        
        public Dictionary<int,Cube> neighborCubes = new Dictionary<int, Cube>();//6个方向，每个方向有一个8bit值（顶点激活为1反之为0）

        public Cube GetNeighborCube(int face)
        {
            Cube neighborCube = null;
            HashSet<Cube> hasFourSameVertexCubes = new HashSet<Cube>();
            foreach(int vertexIndex in faceVertices[face])
            {
                vertices[vertexIndex].HasSameVertex(hasFourSameVertexCubes);
            }

            foreach(Cube cube in hasFourSameVertexCubes)
            {
                 if(cube!=this) neighborCube = cube;
            }

            //记得重置为零
            foreach (int vertexIndex in faceVertices[face])
            {
                vertices[vertexIndex].RestSameVertexCount();
            }
            return neighborCube;
        }
        public void SetNeighborCubes()
        {
            for(int face=0; face < 6; face++)
            {
                neighborCubes[face] = GetNeighborCube(face);
            }
        }
        
        private int sameVertexCount = 0;
        private void CountingSameVertex(HashSet<Cube> hasFourSameVertexCubes)
        {
            if (++sameVertexCount == 4)
            {
                hasFourSameVertexCubes.Add(this);
            }
        }
        private void ResetSameVertexCount() { sameVertexCount = 0; }

        private void AddGoingToCollapseCubes(HashSet<Cube> goingToCollapseCubes)
        {
            goingToCollapseCubes.Add(this );
        }

        public void SetG_Module(Module module)
        {
            this.module = module;
            //当波函数坍缩之后确定是哪个module时进行设置
            if (bit == "00000000" || bit == "11111111")
            {
                if (G_Module != null) Object.Destroy(G_Module);
            }
            else
            {
                Mesh mesh = Object.Instantiate(module.mesh);//复制一个，否则更改会影响指向同一个的mesh

                Modules.DeformMesh(mesh, this);//变形

                if (G_Module != null)
                {
                    G_Module.GetComponent<MeshFilter>().mesh = mesh;
                    G_Module.GetComponent<MeshRenderer>().material = GridManager.s_moduleMaterial;
                }
                else
                {
                    G_Module = new GameObject(bit, typeof(MeshFilter), typeof(MeshRenderer));
                    G_Module.transform.SetParent(GridManager.s_worldCenter);
                    G_Module.transform.localPosition = centerPosition;
                    G_Module.GetComponent<MeshFilter>().mesh = mesh;
                    G_Module.GetComponent<MeshRenderer>().material = GridManager.s_moduleMaterial;
                }
            }
        }

        public int index = 0;
    }
}
