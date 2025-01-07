using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TS
{
    public class Quad
    {
        public readonly Coord a, b, c, d;

        public Quad(Coord a, Coord b, Coord c, Coord d)
        {
            this.a = a;this.b = b;this.c = c;this.d = d;
        }

        public static List<Quad> MergeNeighborTriangles(List<Triangle> triangles)
        {
            List<Quad> quads = new List<Quad>();

            Dictionary<string, List<Triangle>> edgeTriangles = Triangle.GetEdgeTrianglesDictionary(triangles);
            List<string> edgesKeys = edgeTriangles.Keys.ToList();

            while (edgesKeys.Count > 0) { 
                int randomEdgeIndex =  Random.Range(0, edgesKeys.Count);

                List<Triangle> neighborTriangles = edgeTriangles[edgesKeys[randomEdgeIndex]];
                if(neighborTriangles.Count < 2) edgesKeys.Remove(edgesKeys[randomEdgeIndex]);

                else //neighborTriangles.Count ==2
                {   
                    Triangle ta = neighborTriangles[0];
                    Triangle tb = neighborTriangles[1];
                    //移除两个三角形的所有edge（6-1=5个）
                    List<string> edges =  new List<string>
                    {
                        ta.a.GetCoordString()+ta.b.GetCoordString(),ta.b.GetCoordString()+ta.a.GetCoordString(),
                        ta.b.GetCoordString()+ta.c.GetCoordString(),ta.c.GetCoordString()+ta.b.GetCoordString(),
                        ta.c.GetCoordString()+ta.a.GetCoordString(),ta.a.GetCoordString()+ta.c.GetCoordString(),

                        tb.a.GetCoordString()+tb.b.GetCoordString(),tb.b.GetCoordString()+tb.a.GetCoordString(),
                        tb.b.GetCoordString()+tb.c.GetCoordString(),tb.c.GetCoordString()+tb.b.GetCoordString(),
                        tb.c.GetCoordString()+tb.a.GetCoordString(),tb.a.GetCoordString()+tb.c.GetCoordString(),
                    };
                    foreach (string edge in edges)
                    {
                        if (edgeTriangles.ContainsKey(edge)) edgesKeys.Remove(edge);
                    }
                    //
                    List<Coord> unique = new List<Coord> { ta.a,ta.b,ta.c};
                    List<Coord> repeat = new List<Coord> { tb.a,tb.b,tb.c};
                    Coord taUniqueCoord = null;
                    Coord tbUniqueCoord = null;//仅仅赋个初值
                    for (int i = 0; i < repeat.Count; i++) {
                        if (unique.Contains(repeat[i]))//那就是重复的
                        {
                            unique.Remove(repeat[i]);
                        }
                        else
                        {
                            tbUniqueCoord = repeat[i];//这里才是真正的
                            unique.Add(repeat[i]);
                            repeat.Remove(repeat[i]);
                            i--;
                        }
                    }
                    taUniqueCoord = (tbUniqueCoord == unique[0]) ? unique[1] : unique[0];
                    List<Coord> tbCoords = new List<Coord> { tb.a, tb.b, tb.c };
                    int tbCoordsIndex = 0;
                    for (int i = 0; i < tbCoords.Count; i++)
                    {
                        if (tbCoords[i] == tbUniqueCoord) tbCoordsIndex = i;
                    }
                    quads.Add(new Quad(tbUniqueCoord, tbCoords[(tbCoordsIndex + 1) % 3], taUniqueCoord, tbCoords[(tbCoordsIndex + 2) % 3]));
                    triangles.Remove(ta);
                    triangles.Remove(tb);
                }
            }
            return quads;
        }
    }
}
