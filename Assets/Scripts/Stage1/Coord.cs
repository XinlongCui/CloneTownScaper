using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TS
{
    public class Coord
    {
        #region Attributes
        public readonly int q, r, s;
        public readonly Vector3 worldPosition;
        public static Coord[] directions = new Coord[]
        {
            new Coord(0,1,-1),
            new Coord(-1,1,0),
            new Coord(-1,0,1),
            new Coord(0,-1,1),
            new Coord(1,-1,0),
            new Coord(1,0,-1),
        };
        #endregion
        public Coord(int q, int r, int s) 
        { 
            this.q = q; this.r = r; this.s = s; 
            worldPosition = new Vector3(q + 0.5f * r, 0, -Mathf.Sqrt(3) / 2 * r) * 2*GridManager.s_cellSize; //new Vector3(q * Mathf.Sqrt(3) / 2, 0, -(float)r - ((float)q / 2)) * 2 * GridManager.s_cellSize; //new Vector3(q+0.5f*r, 0, Mathf.Sqrt(3) /2*r) * GridManager.s_cellSize;
        }

        public static Coord Direction(int dirIndex)
        {
            return directions[dirIndex];  
        }
        public Coord Add(Coord vertex)
        {
            return new Coord(q+vertex.q,r+vertex.r,s+vertex.s);//new 创建出新的
        }
        public Coord Neighbor(int dirIndex)
        {
            return Add(directions[dirIndex]);
        }

        public Coord Scale(int factor) 
        { 
            return new Coord(q*factor,r*factor,s*factor);
        }

        public static List<Coord> SingleRing(int radius)
        {
            List<Coord> vertices = new List<Coord>();

            Coord startVertex = Direction(4).Scale(radius);//在direction起始确定情况下，保证你开始的vertex开始要绘制的方向与direction中顺序一致
            if(radius == 0) vertices.Add(startVertex);

            for (int d = 0; d < directions.Length; d++)//6个方向
            {
                for(int r = 0; r < radius; r++)//每圈每个方向恰好有radius个
                {
                    vertices.Add(startVertex);
                    startVertex = startVertex.Neighbor(d);
                }
            }
            return vertices;
        }
        public static List<Coord> MultiRings(int radius) 
        {
            List<Coord> vertices = new List<Coord>();

            for (int r = 0;r<radius;r++)
            {
                vertices.AddRange(SingleRing(r));
            }
            return vertices;
        }

        //
        public string GetCoordString()
        {
            return q.ToString()+r.ToString()+s.ToString(); 
        }
    }
}
