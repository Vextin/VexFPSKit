using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FPSCamera : MonoBehaviour
{
    [SerializeField]
    //Sensitivity 
    float sens = 10;
    //Whether the player wants inverted camera controls
    bool invert = false;
    //The maximum (and negative minimum) angle the camera should be allowed to look vertically. 
    float maximumAngle = 90;
    //Mouse movement this frame
    Vector2 mouseDelta;
    //The current total X rotation (vertical rotation) of the camera
    float currentXRotation;
    //Same, but Y (horizontal)
    float currentYRotation;
    //Player object - should always be the parent. 
    GameObject player;
    //InputActions instance 
    Controls ctrl;

    private void Awake()
    {
        //Assign the player object to whatever the camera's parent is
        player = transform.parent.gameObject;

        //Get the InputActions instance from the FPS Manager
        ctrl = FPSManager.controls;

        //Make sure the InputActions instance is reading input
        ctrl.Gameplay.Enable();
    }

    private void Update()
    {
        //read mouse delta from InputSystem
        mouseDelta = ctrl.Gameplay.MouseDelta.ReadValue<Vector2>();

        //Make the rotation independent from frame rate and add sensitivity
        mouseDelta = mouseDelta * sens * Time.deltaTime;

        //Apply inversion
        if (invert) mouseDelta *= -1;

        //Add this frame's rotation to the total
        currentXRotation -= mouseDelta.y;
        currentYRotation += mouseDelta.x;

        //Clamp the rotation between the max and min values.
        if(currentXRotation > maximumAngle)
        {
            currentXRotation = maximumAngle;
        } else 
        if(currentXRotation < -maximumAngle)
        {
            currentXRotation = -maximumAngle;
        }

        //Perform the vertical rotation
        transform.localRotation = Quaternion.AngleAxis(currentXRotation, Vector3.right);

        //Perform the horizontal rotation
        player.transform.localRotation = Quaternion.AngleAxis(currentYRotation, Vector3.up);
    }
}
