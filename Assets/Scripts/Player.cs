using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Transform cameraTransform;
    [SerializeField] private Vector2 camSens;

    [SerializeField] CharacterController characterController;
    [SerializeField] private TextMeshProUGUI debugText;
    private bool jumpPressed = false;
    private bool groundedLastTick = false;
    private bool grounded = false;

    [SerializeField] private float _groundAcceleration;
    [SerializeField] private float _airAcceleration;
    [SerializeField] private float _groundMaxVel;
    [SerializeField] private float _airMaxVel;
    [SerializeField] private float _friction;
    [SerializeField] private float _gravity;
    [SerializeField] private float _jumpVel;
    [SerializeField] private float _airControl;
    // Amount of times you can double jump
    [SerializeField] private int _airJumps;
    // Whether to apply friction in air or not
    [SerializeField] private bool _alwaysApplyFriction;
    [SerializeField] private LayerMask groundMask;

    private Vector3 _moveInputDir = Vector3.zero;
    private Vector3 _rawMoveInputDir = Vector3.zero;
    private int currentAirJumps;
    private Vector3 lateralVector = new(1, 0, 1);

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // debugText = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
        GetComponent<PlayerInput>().enabled = true;

        // Set up camera follower
        cameraTransform = Camera.main.transform;
        cameraTransform.forward = transform.forward;
        // Camera.main.GetComponent<FollowTransform>().SetToFollow(transform);
        // Camera.main.GetComponent<FollowTransform>().enabled = true;
    }

    void FixedUpdate()
    {
        debugText.text = string.Format(
            "CCVel:\t{0}\nInputDir:\t{1}\n{2}\nGrounded: {3}\n{4}", 
            characterController.velocity, 
            _moveInputDir, 
            jumpPressed, 
            grounded, 
            Move(_moveInputDir, characterController.velocity).y
        );

        grounded = GroundCheck();
        if (grounded) currentAirJumps = _airJumps;

        // Player movement
        Vector3 delta = Move(_moveInputDir, characterController.velocity);

        // Apply gravity and jump
        if (jumpPressed && (grounded || currentAirJumps > 0))
        {
            // redirect all lateral movement in jump direction
            delta = Vector3.Scale(delta, lateralVector).magnitude * _moveInputDir;

            // apply vertical jump force
            delta.y = _jumpVel * Time.fixedDeltaTime;
            jumpPressed = false;
            if (!grounded) currentAirJumps--;

        }
        delta.y += _gravity * Time.fixedDeltaTime;
        groundedLastTick = grounded;

        characterController.Move(delta);
    }

    private Vector3 AddAcceleration(Vector3 inputDir, Vector3 currentVel, float acceleration, float maxVel)
    {
        // Apply acceleration based on current velocity and desired velocity
        float projectedVel = Vector3.Dot(currentVel * Time.fixedDeltaTime, inputDir);
        float accelVel = acceleration * Time.fixedDeltaTime;
        maxVel *= Time.fixedDeltaTime;

        // Cap max accel
        if (projectedVel + accelVel > maxVel)
        {
            accelVel = maxVel - projectedVel;
        }
        
        return currentVel * Time.fixedDeltaTime + inputDir * accelVel;
    }

    private Vector3 Move(Vector3 inputDir, Vector3 currentVel)
    {
        bool useGroundPhys = groundedLastTick && grounded;

        if (useGroundPhys)
        {
            return GroundMove(inputDir, currentVel);
        }
        else
        {
            return AirMove(inputDir, currentVel);
        }
    }

    private Vector3 GroundMove(Vector3 inputDir, Vector3 currentVel)
    {
        // Apply friction
        Vector3 lateralVel = Vector3.Scale(currentVel, lateralVector);
        if (lateralVel.magnitude != 0)
        {
            float d = lateralVel.magnitude * _friction * Time.fixedDeltaTime;
            currentVel.x *= Mathf.Max(lateralVel.magnitude - d, 0) / lateralVel.magnitude;
            currentVel.z *= Mathf.Max(lateralVel.magnitude - d, 0) / lateralVel.magnitude;
        }

        return AddAcceleration(
            inputDir,
            currentVel,
            _groundAcceleration,
            _groundMaxVel
            );
    }

    private Vector3 AirMove(Vector3 inputDir, Vector3 currentVel)
    {
        // Air control
        float oldyspeed = currentVel.y;
        currentVel.y = 0;
        float dot = Vector3.Dot(currentVel, inputDir);
        float magnitude = currentVel.magnitude;
        currentVel.Normalize();
        float k = _airControl * dot * dot * Time.fixedDeltaTime;
        if (dot != 0)
        {
            currentVel *= currentVel.magnitude;
            currentVel += inputDir * k;
            currentVel.Normalize();
        }
        currentVel.x *= magnitude;
        currentVel.y = oldyspeed;
        currentVel.z *= magnitude;

        return AddAcceleration(
            inputDir,
            currentVel,
            _airAcceleration,
            _airMaxVel
            );
    }

    public void OnLook(InputValue value)
    {
        if (cameraTransform == null) return;
        Vector2 v = value.Get<Vector2>();
        // Rotate user and cam to with mouse x movement
        transform.Rotate(Vector3.up, v.x * camSens.x, Space.World);
        cameraTransform.Rotate(Vector3.up, v.x * camSens.x, Space.World);

        // Rotate only cam with mouse y movement
        cameraTransform.Rotate(Vector3.right, v.y * camSens.y, Space.Self);
        CalculateMoveInputDir();
    }

    public void OnMove(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        _rawMoveInputDir.x = v.x;
        _rawMoveInputDir.z = v.y;
        _rawMoveInputDir.Normalize();
        CalculateMoveInputDir();
    }

    // Rotate desired move dir with cam
    public void CalculateMoveInputDir()
    {
        if (cameraTransform == null) return;
        _moveInputDir = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * _rawMoveInputDir;
    }

    public void OnJump(InputValue value)
    {
        jumpPressed = value.isPressed;
    }

    public void OnRestart(InputValue value)
    {
        characterController.enabled = false;
        transform.position = new Vector3(0, 2, 0);
        characterController.enabled = true;
    }

    private bool GroundCheck()
    {
        return Physics.Raycast(transform.position + characterController.center, Vector3.down, characterController.height * 0.55f, groundMask);
    }
}