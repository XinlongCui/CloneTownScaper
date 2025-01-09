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
        public bool State { get { return _state; } set { _state = value; SwitchState?.Invoke(); } }

        public Vertex(Vector3 initialPosition, int y=0)
        {
            this.initialPosition = initialPosition + Vector3.up*y*GridManager.cellHeight;
            currentPosition = this.initialPosition;
            this.y = y;
        }

        public void Relax()
        {
            currentPosition = initialPosition + offset;//ͬһ�����ܵ�����ı��ε�Ӱ�죬����offset�� += ���ۻ�����֮��
        }

        
    }
}
