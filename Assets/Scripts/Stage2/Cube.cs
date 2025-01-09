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
            foreach (Vertex vertex in vertices) {
                centerPosition += vertex.currentPosition;
                vertex.SwitchState += UpdateBit;
            }
            centerPosition /=  8;//可换成右移

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
            Debug.Log(bit);

            if (bit == "00000000" || bit == "11111111")
            {
                if (G_Module != null) Object.Destroy(G_Module);
            }
            else
            {
                List<Mesh> possiableModules = Modules.GetPossiableModules(bit);//获取bit对应Module
                Mesh M_Module = Object.Instantiate(possiableModules[0]);//复制一个，否则更改会影响指向同一个的mesh

                Modules.DeformModule(M_Module, this);//变形

                if (G_Module != null)
                {
                    G_Module.GetComponent<MeshFilter>().mesh = M_Module;
                    G_Module.GetComponent<MeshRenderer>().material = GridManager.material;
                }
                else
                {
                    G_Module = new GameObject(bit, typeof(MeshFilter), typeof(MeshRenderer));
                    G_Module.transform.SetParent(GridManager.worldCenter);
                    G_Module.transform.localPosition = centerPosition;
                    G_Module.GetComponent<MeshFilter>().mesh = M_Module;
                    G_Module.GetComponent<MeshRenderer>().material = GridManager.material;
                }
            }


        }
    }
}
