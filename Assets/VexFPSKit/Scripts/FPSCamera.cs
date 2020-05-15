using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    [SerializeField]
    //Sensitivity 
    float sens;
    //Mouse movement this frame
    Vector2 mouseDelta;
    //Player object - should always be the parent. 
    GameObject player;
    //InputActions instance 
    Controls ctrl;

    private void Awake()
    {
        player = transform.parent.gameObject;
        ctrl = FPSManager.controls;
        ctrl.Gameplay.Enable();
    }

    private void Update()
    {
        //read mouse delta from InputSystem
        mouseDelta = ctrl.Gameplay.MouseDelta.ReadValue<Vector2>();
        mouseDelta = mouseDelta * sens * Time.deltaTime;
        float mouseY = mouseDelta.y;
        transform.localEulerAngles = new Vector3(-(Mathf.Clamp(mouseY, -85f, 85f)), transform.localEulerAngles.y, 0);
    }
}
