using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TS
{
    public class BuildAndDemolish_Indicator : MonoBehaviour
    {
        Vertex vertex;
        public int neighborHasBuildingCounts = 0;
        public void Init(Vertex vertex)
        {
            Renderer rend = GetComponent<Renderer>();
            rend.material = BuildAndDemolish.s_indicatorNotChoosingMaterial;
            this.vertex = vertex;
            vertex.indicator = this;
        }

        public void Build() {
            if (vertex.State == false)
            {
                vertex.State = true;
                if (vertex.neighborVertices.Count == 0) vertex.SetNeighborVertices();

                foreach (Vertex v in vertex.neighborVertices)
                {
                    BuildAndDemolish_Indicator indicator = v.indicator;
                    if (indicator == null)
                    {
                        GameObject indicatorGameObject = Instantiate(BuildAndDemolish.s_indicatorGameObject);
                        indicatorGameObject.transform.parent = BuildAndDemolish.s_indicatorAttachGameObject.transform;
                        indicatorGameObject.transform.localPosition = v.currentPosition;

                        indicator = indicatorGameObject.AddComponent<BuildAndDemolish_Indicator>();
                        indicator.Init(v);
                    }
                    indicator.neighborHasBuildingCounts++;
                }
            }
            else
            {
                //如果已经建造好了东西那么，考虑重新坍缩或什么
            }
        }

        public void Demolish() 
        {
            if (vertex.State == true)
            {
                vertex.State = false;
                if (vertex.neighborVertices.Count == 0) vertex.SetNeighborVertices();

                foreach (Vertex v in vertex.neighborVertices)
                {
                    BuildAndDemolish_Indicator indicator = v.indicator;
                    if (indicator != null && --indicator.neighborHasBuildingCounts <= 0)
                    {
                        Destroy(indicator.gameObject);
                    }
                }
            }
        }
        public void ChangeToChoosingState()
        { 
            gameObject.transform.localScale *= 2f;
            Renderer rend = GetComponent<Renderer>();
            rend.material = BuildAndDemolish.s_indicatorChoosingMaterial;
        }
        public void ChangeToNotChoosingState()
        {
            gameObject.transform.localScale /= 2f;
            Renderer rend = GetComponent<Renderer>();
            rend.material = BuildAndDemolish.s_indicatorNotChoosingMaterial;
        }

    }
}
