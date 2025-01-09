using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace TS
{
    public class GridManager:MonoBehaviour
    {
        #region Configs And Attributes
        //Stage1
        public int radius=3;
        public static int cellSize = 1;
        public int relaxTimes = 10;

        public List<Coord> coords;
        public List<Triangle> triangles;
        public List<Quad> quads;
        public List<SubdivideQuad> subdivideQuads;
        //Stage2
        public static Transform worldCenter;//所有的module都挂在这上面

        public Material moduleMaterial;
        public static Material material;
        public GameObject G_Modules;

        public static int cellHeight = 1;
        public int height = 5;
        public List<Cube> cubes;
        //--------Stage2 test---------
        public GameObject activeVertex;
        public GameObject unActiveVertex;

        #endregion
        private void Awake()
        {
            
            //Stage1
            coords = Coord.MultiRings(radius);
            triangles = Triangle.MultiRings(coords, radius);
            quads = Quad.MergeNeighborTriangles(triangles);
            subdivideQuads = SubdivideQuad.GetSubdivideQuads(quads, triangles);

            for (int i = 0; i < relaxTimes; i++)
            {
                foreach (SubdivideQuad subdivideQuad in subdivideQuads)
                {
                    subdivideQuad.CalculateOffsetValue();
                }
                foreach (Vertex vertex in SubdivideQuad.vertices)
                {
                    vertex.Relax();
                }
            }
            //Stage2
            GridManager.material = moduleMaterial;
            if (worldCenter == null) worldCenter = new GameObject().transform;
            worldCenter.transform.position = Vector3.zero;  
            
            Modules.SetAllModules(G_Modules);
            cubes = Cube.GetCubes(subdivideQuads,height);





            GC.Collect();

        }

        private void Update()
        {
            foreach(KeyValuePair<Vertex,List<Vertex>> pair in Cube.verticesOfDifferentY)
            {
                foreach(Vertex vertex in pair.Value)
                {
                    if(vertex.State == false && Vector3.Distance(vertex.currentPosition, activeVertex.transform.position) < 0.5f)
                    {
                        Debug.LogWarning("Active");
                        vertex.State = true;
                    }
                    else if(vertex.State == true && Vector3.Distance(vertex.currentPosition, unActiveVertex.transform.position) < 0.5f)
                    {
                        Debug.LogWarning("UnActive");
                        vertex.State = false;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            //Stage 1
            //if (coords != null)
            //{
            //    foreach (Coord coord in coords)
            //    {
            //        Gizmos.DrawSphere(coord.worldPosition, 0.1f);
            //    }
            //}
            //if (triangles != null)
            //{
            //    foreach (Triangle triangle in triangles)
            //    {
            //        Gizmos.DrawLine(triangle.a.worldPosition, triangle.b.worldPosition);
            //        Gizmos.DrawLine(triangle.b.worldPosition, triangle.c.worldPosition);
            //        Gizmos.DrawLine(triangle.c.worldPosition, triangle.a.worldPosition);
            //    }
            //}
            //if (quads != null)
            //{
            //    foreach (Quad quad in quads)
            //    {
            //        Gizmos.DrawLine(quad.a.worldPosition, quad.b.worldPosition);
            //        Gizmos.DrawLine(quad.b.worldPosition, quad.c.worldPosition);
            //        Gizmos.DrawLine(quad.c.worldPosition, quad.d.worldPosition);
            //        Gizmos.DrawLine(quad.d.worldPosition, quad.a.worldPosition);
            //    }
            //}

            //if (subdivideQuads != null)
            //{
            //    Gizmos.color = Color.yellow;
            //    foreach (SubdivideQuad subdivideQuad in subdivideQuads)
            //    {
                    
            //        Gizmos.DrawLine(subdivideQuad.a.currentPosition, subdivideQuad.b.currentPosition);
            //        Gizmos.DrawLine(subdivideQuad.b.currentPosition, subdivideQuad.c.currentPosition);
            //        Gizmos.DrawLine(subdivideQuad.c.currentPosition, subdivideQuad.d.currentPosition);
            //        Gizmos.DrawLine(subdivideQuad.d.currentPosition, subdivideQuad.a.currentPosition);
            //    }
            //}

            //Stage 2
            if (cubes != null)
            {
                Gizmos.color = Color.blue;
                foreach (Cube cube in cubes)
                {
                    //上面一圈
                    Gizmos.DrawLine(cube.vertices[0].currentPosition, cube.vertices[1].currentPosition);
                    Gizmos.DrawLine(cube.vertices[1].currentPosition, cube.vertices[2].currentPosition);
                    Gizmos.DrawLine(cube.vertices[2].currentPosition, cube.vertices[3].currentPosition);
                    Gizmos.DrawLine(cube.vertices[3].currentPosition, cube.vertices[0].currentPosition);
                    //下面一圈
                    Gizmos.DrawLine(cube.vertices[4].currentPosition, cube.vertices[5].currentPosition);
                    Gizmos.DrawLine(cube.vertices[5].currentPosition, cube.vertices[6].currentPosition);
                    Gizmos.DrawLine(cube.vertices[6].currentPosition, cube.vertices[7].currentPosition);
                    Gizmos.DrawLine(cube.vertices[7].currentPosition, cube.vertices[4].currentPosition);
                    //中间四条
                    Gizmos.DrawLine(cube.vertices[0].currentPosition, cube.vertices[4].currentPosition);
                    Gizmos.DrawLine(cube.vertices[1].currentPosition, cube.vertices[5].currentPosition);
                    Gizmos.DrawLine(cube.vertices[2].currentPosition, cube.vertices[6].currentPosition);
                    Gizmos.DrawLine(cube.vertices[3].currentPosition, cube.vertices[7].currentPosition);

                    Handles.Label(cube.centerPosition, cube.bit);
                }
            }
            foreach (KeyValuePair<Vertex, List<Vertex>> pair in Cube.verticesOfDifferentY)
            {
                foreach (Vertex vertex in pair.Value)
                {
                    if (vertex.State == true)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(vertex.currentPosition, 0.1f);
                    }
                    else
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawSphere(vertex.currentPosition, 0.05f);
                    }
                }
            }

        }
    }
}
