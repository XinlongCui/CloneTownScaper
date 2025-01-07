using System.Collections;
using System.Collections.Generic;
using TS;
using UnityEngine;
using UnityEngine.UIElements;

namespace TS
{
    public class Triangle
    {
        public readonly Coord a, b, c;

        public Triangle(Coord a, Coord b, Coord c)
        {
            this.a = a; this.b = b; this.c = c;
        }

        public static List<Triangle> SingleRing(List<Coord> vertices,int radius)
        {
            List<Triangle> triangles = new List<Triangle>();

            List<Coord> innerVertexs;
            if(radius<=1)
                innerVertexs = vertices.GetRange(0,1);
            else 
                innerVertexs = vertices.GetRange((radius-1) * (radius - 2) * 3 + 1,( radius-1) * 6);
            List<Coord> outerVertexs = vertices.GetRange(radius * (radius - 1) *3+1, radius * 6);

            for(int d = 0; d < 6; d++)
            {
                for(int r = 0; r < radius; r++)
                {
                    //两个外圈顶点一个内圈顶点
                    Coord va = outerVertexs[d * radius + r];//6个方向，每个方向恰好有radius个（0到radius-1）
                    Coord vb = outerVertexs[(d * radius + r+1)%outerVertexs.Count];//下一个，由于最后回到0
                    Coord vc = innerVertexs[(d * (radius - 1) + r)%innerVertexs.Count];
                    triangles.Add(new Triangle(va, vb, vc));

                    //两个内圈顶点一个外圈顶点
                    if(radius>1 && r>0)//从第二圈开始才有，且每个方向上，这种三角形个数比上面少一个，所以要跳过一个（j>0是跳过第一个）
                    {
                        Coord vd = innerVertexs[d * (radius-1) + r - 1];//如果是跳过最后一个(j<radius-1)，则为（d * (radius-1) + r +1）%innerVertex.Count,下面构建三角形也要变化
                        triangles.Add(new Triangle(va, vc, vd));
                    }
                }
            }
            return triangles;
        }
        public static List<Triangle> MultiRings(List<Coord> coords, int radius) {
            List<Triangle> triangles = new List<Triangle>();

            for(int r=1;r<radius;r++)
            {
                triangles.AddRange(SingleRing(coords, r));
            }
            return triangles;
        }

        public static Dictionary<string, List<Triangle>> GetEdgeTrianglesDictionary(List<Triangle> triangles)
        {
            Dictionary<string, List<Triangle>> edgeTriangles = new Dictionary<string, List<Triangle>>();
            foreach (Triangle triangle in triangles) {

                List<Coord> vertexs = new List<Coord> { triangle.a,triangle.b,triangle.c};
                for (int i = 0; i < 3; i++)
                {
                    string edge = vertexs[i].GetCoordString() + vertexs[(i+1)%3].GetCoordString();
                    if (edgeTriangles.ContainsKey(edge))
                    {
                        edgeTriangles[edge].Add(triangle);
                    }
                    else//edge名称可能是反过来的
                    {
                        string edgeRverse = vertexs[(i + 1) % 3].GetCoordString() + vertexs[i].GetCoordString();
                        if (edgeTriangles.ContainsKey(edgeRverse)) edgeTriangles[edgeRverse].Add(triangle);
                        else edgeTriangles[edgeRverse] = new List<Triangle> { triangle };
                    }
                }
            }
            return edgeTriangles;
        }

    }
}

