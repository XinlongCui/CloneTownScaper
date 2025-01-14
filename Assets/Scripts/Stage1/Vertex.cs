using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS
{
    public class Vertex
    {
        public Vector3 initialPosition;
        public Vector3 currentPosition;
        public Vector3 offset = Vector3.zero;

        public int y;
        public event Action SwitchState;
        bool _state = false;
        public bool State { get { return _state; } set { _state = value; SwitchState?.Invoke(); NeedCollapse?.Invoke(this); } }

        public Vertex(Vector3 initialPosition, int y=0)
        {
            this.initialPosition = initialPosition + Vector3.up*y*GridManager.s_cellHeight;
            currentPosition = this.initialPosition;
            this.y = y;
        }

        public void Relax()
        {
            currentPosition = initialPosition + offset;//同一个点受到多个四边形的影响，所以offset是 += ；累积起来之后
        }

        //stage3
        public BuildAndDemolish_Indicator indicator;

        public HashSet<Vertex> neighborVertices = new HashSet<Vertex>();
        public event Action<Vertex,HashSet<Vertex>> AddNeighborVertices;
        public void SetNeighborVertices()
        {
            AddNeighborVertices?.Invoke(this,neighborVertices);
        }

        //Stage4
        public event Action<HashSet<Cube>> SameVertex;
        public void HasSameVertex(HashSet<Cube> hasSameVertex)
        {
            SameVertex?.Invoke(hasSameVertex);
        }
        public event Action SameVertexCount;
        public void RestSameVertexCount()
        {
            SameVertexCount?.Invoke();
        }

        public event Action<HashSet<Cube>> GoingToCollapseCubes;
        public void GetGoingToCollapseCube(HashSet<Cube> goingToCollapseCubes)
        {
            GoingToCollapseCubes?.Invoke(goingToCollapseCubes);
        }

         public static event Action<Vertex> NeedCollapse;//通知waveCollapseFunction可以开始了
    }
}
