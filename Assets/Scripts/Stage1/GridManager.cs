using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TS
{
    public class GridManager:MonoBehaviour
    {
        #region Configs And Attributes
        public int radius=3;
        public static int cellSize = 1;
        public int relaxTimes = 10;

        public List<Coord> coords;
        public List<Triangle> triangles;
        public List<Quad> quads;
        public List<SubdivideQuad> subdivideQuads;
        #endregion
        private void Awake()
        {
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
        }

        private void OnDrawGizmos()
        {
            if (coords != null)
            {
                foreach (Coord vertex in coords)
                {
                    Gizmos.DrawSphere(vertex.worldPosition, 0.2f);
                }
            }
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

            if (subdivideQuads != null)
            {
                foreach (SubdivideQuad subdivideQuad in subdivideQuads)
                {
                    Gizmos.DrawLine(subdivideQuad.a.currentPosition, subdivideQuad.b.currentPosition);
                    Gizmos.DrawLine(subdivideQuad.b.currentPosition, subdivideQuad.c.currentPosition);
                    Gizmos.DrawLine(subdivideQuad.c.currentPosition, subdivideQuad.d.currentPosition);
                    Gizmos.DrawLine(subdivideQuad.d.currentPosition, subdivideQuad.a.currentPosition);
                }                                                                
            }

        }
    }
}
