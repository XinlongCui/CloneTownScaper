using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TS
{
    public class CameraController : MonoBehaviour
    {
        private IS_InputHandler inputHandler;
        // Start is called before the first frame update
        void Start()
        {
            inputHandler = new IS_InputHandler();
            inputHandler.Enable();
            inputHandler.AM_CameraMovement.CameraViewportMovement.performed += inputActionCallbackContext => MoveViewport(inputActionCallbackContext.ReadValue<Vector2>());
        }


        float yaw = 0, pitch = 0;
        public void MoveViewport(Vector2 mouseXY)
        {
            float sensitity = 0.1f;
            float pitchMin = -80;
            float pitchMax = 80;
            yaw += mouseXY.x * sensitity;//绕着y轴转
            pitch += -mouseXY.y * sensitity;//绕着x轴转
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            Camera.main.transform.rotation = Quaternion.Euler(pitch, yaw, 0);//Quaternion.Euler(x,y,z)分别绕着对应轴转
        }
        public void Move(Vector2 horizontalAndVertical)
        {
            //前后左右移动
            //Vector2 horizontalAndVertical = MovementActions.Movement.ReadValue<Vector2>();
            float speed = 3;
            Vector3 RightDirection = Camera.main.transform.right;
            Vector3 ForwardDirection = Camera.main.transform.forward;
            Vector3 velocity = (horizontalAndVertical.x * RightDirection + horizontalAndVertical.y * ForwardDirection) * speed;

            Camera.main.transform.position += velocity * Time.deltaTime;
        }
        private void Update()
        {
            Move(inputHandler.AM_CameraMovement.CameraMovement.ReadValue<Vector2>());
        }
    }
}