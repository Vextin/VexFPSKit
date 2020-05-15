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
    MovementStyle movementStyle;

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
    [Tooltip("Walking speed of the player's movement in meters per second. Humans are naturally around 1.4m/s")]
    float walkSpeed;

    [SerializeField]
    [Tooltip("Prone crawling speed of the player's movement in meters per second.")]
    float proneSpeed;

    [SerializeField]
    [Tooltip("Crouch-walking speed of the player's movement in meters per second.")]
    float crouchSpeed;

    [SerializeField]
    [Tooltip("Base speed of the player's movement in meters per second. A defualt of around 7 is nice.")]
    float jogSpeed;

    [SerializeField]
    [Tooltip("Base walking speed of the player's movement in meters per second. Humans can reach around 12m/s")]
    float sprintSpeed;

    float currentSpeed;
    #endregion

    #region player state
    [SerializeField]
    bool toggleSprint, toggleProne, toggleWalk, toggleCrouch;
    bool isCrouching, isProne, isGrounded, isWalking, isSprinting;
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
    }
    void SetProne(bool set)
    {
        ResetPlayerStates();
        isProne = set;
    }    
    void SetSprint(bool set)
    {
        ResetPlayerStates();
        isSprinting = set;
    }
    void SetWalk(bool set)
    {
        ResetPlayerStates();
        isWalking = set;
    }

    void ResetPlayerStates()
    {
        isCrouching = false;
        isWalking = false;
        isProne = false;
        isSprinting = false;
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

    void MoveArcade()
    {
        UpdateSpeed();

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
