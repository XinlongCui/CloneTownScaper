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

        public Vertex(Vector3 initialPosition)
        {
            this.initialPosition = initialPosition;
            currentPosition = initialPosition;
        }

        public void Relax()
        {
            currentPosition = initialPosition + offset;//同一个点受到多个四边形的影响，所以offset是 += ；累积起来之后
        }

    }
}
