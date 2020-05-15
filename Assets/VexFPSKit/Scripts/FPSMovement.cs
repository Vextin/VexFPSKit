using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSMovement : MonoBehaviour
{
    #region Run the right code depending on the user's movement mode choice. 
    private enum MovementStyle
    {
        Arcade = 0
    }
    [SerializeField] [Tooltip("What style of movement should the controller use? Arcade, milsim, etc.")]
    MovementStyle movementStyle = 0;

    delegate void MoveDelegate();

    MoveDelegate[] methods;
    MoveDelegate move;
    #endregion

    #region input definitions
    CharacterController cc;
    Controls ctrl;
    InputActionMap actions;
    #endregion

    #region speeds
    [Header("Speed")]

    [SerializeField]
    [Range(.5f, 15f)]
    [Tooltip("Walking speed of the player's movement in meters per second. Humans are naturally around 1.4m/s")]
    float walkSpeed = 1.4f;

    [SerializeField]
    [Range(.5f, 15f)]
    [Tooltip("Prone crawling speed of the player's movement in meters per second.")]
    float proneSpeed = 1f;

    [SerializeField]
    [Range(.5f, 15f)]
    [Tooltip("Crouch-walking speed of the player's movement in meters per second.")]
    float crouchSpeed = 1.4f;

    [SerializeField]
    [Range(.5f, 15f)]
    [Tooltip("Base speed of the player's movement in meters per second. A defualt of around 7 is nice.")]
    float jogSpeed = 7f;

    [SerializeField]
    [Range(.5f, 15f)]
    [Tooltip("Base walking speed of the player's movement in meters per second. Humans can reach around 12m/s")]
    float sprintSpeed = 12f;

    float currentSpeed;
    #endregion

    #region heights
    [Space] [Header("Heights")]
    [SerializeField]
    [Range(.75f, 2f)]
    float normalHeight = 2f;
    [SerializeField]
    [Range(.75f, 2f)]
    float crouchHeight = 1.5f;
    [SerializeField]
    [Range(.75f, 2f)]
    float proneHeight = .75f;
    #endregion

    GameObject cam;

    #region player state
    [Space][Header("Control options")]
    [SerializeField]
    bool toggleSprint = false;
    [SerializeField]
    bool toggleProne = true;
    [SerializeField]
    bool toggleWalk = false;
    [SerializeField]
    bool toggleCrouch = false;
    bool isCrouching = false, isProne = false, isGrounded = true, isWalking = false, isSprinting = false;
    #endregion

    private void Awake()
    {
        //Setup delegate
        methods = new MoveDelegate[3];
        methods[0] = MoveArcade;
        move = methods[(int)movementStyle];

        //Input setup
        ctrl = FPSManager.controls;
        ctrl.Gameplay.Enable();

        ConfigureControlsDelegates();

        cc = GetComponent<CharacterController>();

        cam = Camera.main.gameObject;
    }

    void ConfigureControlsDelegates()
    {
        if (!toggleCrouch) ctrl.Gameplay.Crouch.canceled += ctx => SetCrouch(false);
        ctrl.Gameplay.Crouch.performed += ctx => SetCrouch(!isCrouching);

        if (!toggleProne) ctrl.Gameplay.Prone.canceled += ctx => SetProne(false);
        ctrl.Gameplay.Prone.performed += ctx => SetProne(!isProne);

        if (!toggleWalk) ctrl.Gameplay.Walk.canceled += ctx => SetWalk(false);
        ctrl.Gameplay.Walk.performed += ctx => SetWalk(!isWalking);

        if (!toggleSprint) ctrl.Gameplay.Sprint.canceled += ctx => SetSprint(false);
        ctrl.Gameplay.Sprint.performed += ctx => SetSprint(!isSprinting);
    }

    private void Update()
    {
        print(currentSpeed);
        UpdateSpeed();
        //move();
        MoveArcade();
    }

    void SetCrouch(bool set)
    {
        ResetPlayerStates();
        isCrouching = set;

        if (isCrouching)
        {
            cc.height = crouchHeight;
            cc.center = new Vector3(0, (crouchHeight-2)*0.5f, 0);
        }
        else
        {
            cc.height = normalHeight;
            cc.center = Vector3.zero;
        }
    }
    void SetProne(bool set)
    {
        ResetPlayerStates();
        isProne = set;
        if (isProne)
        {
            cc.height = proneHeight;
            cc.center = new Vector3(0, (proneHeight - 2) * 0.5f, 0);
        }
        else
        {
            cc.height = normalHeight;
            cc.center = Vector3.zero;
        }
    }    
    void SetSprint(bool set)
    {
        ResetPlayerStates();
        isSprinting = set;
        if (isSprinting)
        {
            cc.height = normalHeight;
            cc.center = Vector3.zero;
        }
    }
    void SetWalk(bool set)
    {
        ResetPlayerStates();
        isWalking = set;
        if (isWalking)
        {
            cc.height = normalHeight;
            cc.center = Vector3.zero;
        }
    }

    void ResetPlayerStates()
    {
        isCrouching = false;
        isSprinting = false;
        isProne = false;
        isWalking = false;
        cc.center = Vector3.zero;
        cc.height = normalHeight;
    }

    void UpdateSpeed()
    {
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        } else
        if (isProne)
        {
            currentSpeed = proneSpeed;
        }
        else
        if (isWalking)
        {
            currentSpeed = walkSpeed;
        } else
        if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = jogSpeed;
        }
    }

    void UpdateCameraHeight()
    {
        cam.transform.localPosition = new Vector3(0, cc.height-1, 0);
    }

    void MoveArcade()
    {
        UpdateSpeed();
        UpdateCameraHeight();

        //Read the movement from the InputSystem
        Vector2 moveInput = ctrl.Gameplay.Move.ReadValue<Vector2>();

        //New movement vector for this frame's movement.
        Vector3 movement = new Vector3();

        //Set the axes to the right values from the input.
        movement.x = moveInput.x;
        movement.z = moveInput.y;

        //Framerate independence & multiply by speed.
        movement *= currentSpeed * Time.deltaTime;

        //Transform to worldspace so the player moves in the CAMERA'S direction, not world forward.
        movement = transform.TransformVector(movement);

        //Add gravity.
        movement.y = Physics.gravity.y * Time.deltaTime;

        //Tell the CharacterController component to move.
        cc.Move(movement);
    }
}
