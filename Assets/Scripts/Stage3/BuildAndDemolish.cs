using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static UnityEngine.UI.Image;

namespace TS
{
    public class BuildAndDemolish : MonoBehaviour
    {
        public static GameObject s_indicatorAttachGameObject;
        public GameObject indicatorGameObject;
        public static GameObject s_indicatorGameObject;

        private IS_InputHandler inputHandler;
        public float radius = 0.2f;
        public float maxDistance = 10f;
        BuildAndDemolish_Indicator preIndicator, nowIndicator;

        public Material indicatorChoosingMaterial;
        public static Material s_indicatorChoosingMaterial;
        public Material indicatorNotChoosingMaterial;
        public static Material s_indicatorNotChoosingMaterial;

        public AudioClip soundEffect;
        private AudioSource as_soundEffect;
        public AudioClip backgroundSound;
        private AudioSource as_backgroundSound;
        private void Awake()
        {
            s_indicatorGameObject = indicatorGameObject;
            s_indicatorChoosingMaterial = indicatorChoosingMaterial;
            s_indicatorNotChoosingMaterial = indicatorNotChoosingMaterial;



            inputHandler = new IS_InputHandler();
            inputHandler.Enable();
            inputHandler.AM_BuildAndDemolish.Build.performed += Build;
            inputHandler.AM_BuildAndDemolish.Demolish.performed += Demolish;

            s_indicatorAttachGameObject = new GameObject("BuildAndDemolishIndicators");
            s_indicatorAttachGameObject.transform.position = Vector3.zero;

            SetGroundBuildAndDemolishIndicators();

            {
                as_soundEffect = gameObject.AddComponent<AudioSource>();
                as_soundEffect.volume = 2f;

                as_backgroundSound = gameObject.AddComponent<AudioSource>();
                as_backgroundSound.volume = 0.2f;
                as_backgroundSound.clip = backgroundSound;
                as_backgroundSound.loop = true;
                as_backgroundSound.Play();
            }

        }
        private void SetGroundBuildAndDemolishIndicators()
        {
            foreach (Vertex vertex in SubdivideQuad.vertices)
            {
                GameObject indicatorGameObject = Instantiate(s_indicatorGameObject);
                indicatorGameObject.transform.parent = s_indicatorAttachGameObject.transform;
                indicatorGameObject.transform.localPosition = vertex.currentPosition;

                BuildAndDemolish_Indicator indicator = indicatorGameObject.AddComponent<BuildAndDemolish_Indicator>();
                indicator.Init(vertex);
                indicator.neighborHasBuildingCounts = 1;//保证地面上的指示器永远存在
            }
        }

        private void UpdateIndicator()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3 p1 = Camera.main.transform.position;
            Vector3 direction = ray.direction.normalized;
            Vector3 p2 = p1 + direction * 0.1f; // 将两个点设置得更近一些??
            LayerMask layerMask = 1 << 6;

            if (Physics.CapsuleCast(p1, p2, radius, ray.direction, out RaycastHit hitInfo, layerMask))
            {
                //Debug.Log(hitInfo.transform.ToString());
                //DrawCapsule(p1, p2, radius, Color.red);
                BuildAndDemolish_Indicator indicator = hitInfo.transform.gameObject.GetComponent<BuildAndDemolish_Indicator>();
                if (indicator != null)
                {
                    nowIndicator = indicator;
                    if (preIndicator != nowIndicator)
                    {
                        if (preIndicator != null)
                        {
                            preIndicator.ChangeToNotChoosingState();
                        }
                        preIndicator = nowIndicator;
                        nowIndicator.ChangeToChoosingState();
                    }
                }
            }
            //else DrawCapsule(p1, p2, radius, Color.blue);
        }
        private void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color)
        {
            // 绘制胶囊体的两个端点
            Debug.DrawLine(start, end, color);

            // 绘制两个端点球
            Debug.DrawLine(start + Vector3.forward * radius, end + Vector3.forward * radius, color);
            Debug.DrawLine(start - Vector3.forward * radius, end - Vector3.forward * radius, color);
        }

        private void Build(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (nowIndicator != null)
            {
                nowIndicator.Build();
                as_soundEffect.PlayOneShot(soundEffect);
            }
        }        
        
        private void Demolish(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (nowIndicator != null)
            {
                nowIndicator.Demolish();
                as_soundEffect.PlayOneShot(soundEffect);
                //AudioSource.PlayClipAtPoint(soundEffect, nowIndicator.transform.position);
            }
        }        

        private void Update()
        {
            UpdateIndicator();
        }
    }
}
