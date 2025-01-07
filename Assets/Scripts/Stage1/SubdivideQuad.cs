using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TS
{
    public class SubdivideQuad
    {
        public readonly Vertex a, b, c,d;
        public static List<Vertex> vertices = new List<Vertex>();
        public SubdivideQuad(Vertex a, Vertex b, Vertex c, Vertex d)
        {
            this.a = a;this.b = b;this.c = c;this.d = d;
        }
        private static Dictionary<string,Vertex> keyValuePairs = new Dictionary<string,Vertex>();
        public static List<SubdivideQuad> GetSubdivideQuads(List<Quad> quads, List<Triangle> triangles)
        {
            List<SubdivideQuad> subdivideQuads = new List<SubdivideQuad>();

            foreach (Quad quad in quads)
            {
                Vertex a = FindVertex(new List<Coord> { quad.a });
                Vertex b = FindVertex(new List<Coord> { quad.b });
                Vertex c = FindVertex(new List<Coord> { quad.c });
                Vertex d = FindVertex(new List<Coord> { quad.d });
                Vertex center = FindVertex(new List<Coord> { quad.a ,quad.b,quad.c,quad.d});
                Vertex mid_ab = FindVertex(new List<Coord> { quad.a ,quad.b});
                Vertex mid_bc = FindVertex(new List<Coord> { quad.b ,quad.c});
                Vertex mid_cd = FindVertex(new List<Coord> { quad.c ,quad.d});
                Vertex mid_da = FindVertex(new List<Coord> { quad.d ,quad.a });
   
                subdivideQuads.AddRange(new List<SubdivideQuad> {
                    new SubdivideQuad(a,mid_ab,center,mid_da),
                    new SubdivideQuad(b,mid_bc,center,mid_ab),
                    new SubdivideQuad(c,mid_cd,center,mid_bc),
                    new SubdivideQuad(d,mid_da,center,mid_cd),
                });
            }
            foreach(Triangle triangle in triangles)
            {
                Vertex a = FindVertex(new List<Coord> { triangle.a });
                Vertex b = FindVertex(new List<Coord> { triangle.b });
                Vertex c = FindVertex(new List<Coord> { triangle.c });
                Vertex center = FindVertex(new List<Coord> { triangle.a, triangle.b, triangle.c});
                Vertex mid_ab = FindVertex(new List<Coord> { triangle.a, triangle.b });
                Vertex mid_bc = FindVertex(new List<Coord> { triangle.b, triangle.c });
                Vertex mid_ca = FindVertex(new List<Coord> { triangle.c, triangle.a });


                subdivideQuads.AddRange(new List<SubdivideQuad> {
                    new SubdivideQuad(a,mid_ab,center,mid_ca),
                    new SubdivideQuad(b,mid_bc,center,mid_ab),
                    new SubdivideQuad(c,mid_ca,center,mid_bc),
                });
            }

            return subdivideQuads;
        }
        private static Vertex FindVertex(List<Coord> coords)
        {
            Vertex result=null;
            string key = "666";
            if (coords.Count == 1)
            {
                key = coords[0].GetCoordString();
                if (keyValuePairs.ContainsKey(key)) return keyValuePairs[key];
                else result = new Vertex(coords[0].worldPosition);

                keyValuePairs[key] = result;
            }
            else if (coords.Count == 2) { 
                key = coords[0].GetCoordString() + coords[1].GetCoordString();
                if (keyValuePairs.ContainsKey(key)) return keyValuePairs[key];
                else if(keyValuePairs.ContainsKey(key = coords[1].GetCoordString() + coords[0].GetCoordString())) return keyValuePairs[key];
                else result = new Vertex((coords[0].worldPosition + coords[1].worldPosition)/2);

                keyValuePairs[key] = result;
            }
            else if(coords.Count==3)//一定不会重复
            {
                result = new Vertex((coords[0].worldPosition + coords[1].worldPosition + coords[2].worldPosition ) /3);
            }
            else if (coords.Count == 4)//一定不会重复
            {
                result = new Vertex((coords[0].worldPosition + coords[1].worldPosition + coords[2].worldPosition + coords[3].worldPosition) / 4);
            }
            
            vertices.Add(result);
            return result;
        }
        public void CalculateOffsetValue()
        {
            Vector3 center = (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition)/4;

            //保证你的细分四边形为顺时针创建的，则为逆时针旋转
            Vector3 vaNew = (a.currentPosition
                + Quaternion.AngleAxis(-90, Vector3.up) * (b.currentPosition - center) + center
                + Quaternion.AngleAxis(-180, Vector3.up) * (c.currentPosition - center) + center
                + Quaternion.AngleAxis(-270, Vector3.up) * (d.currentPosition - center) + center)/4;

            Vector3 vbNew = Quaternion.AngleAxis(90, Vector3.up) * (vaNew - center) + center;
            Vector3 vcNew = Quaternion.AngleAxis(180, Vector3.up) * (vaNew - center) + center;
            Vector3 vdNew = Quaternion.AngleAxis(270, Vector3.up) * (vaNew - center) + center;

            a.offset += (vaNew - a.currentPosition)*0.1f;
            b.offset += (vbNew - b.currentPosition)*0.1f;
            c.offset += (vcNew - c.currentPosition)*0.1f;
            d.offset += (vdNew - d.currentPosition)*0.1f;
        }
    }
}
