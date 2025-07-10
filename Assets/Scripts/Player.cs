using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Transform cameraTransform;
    [SerializeField] private Vector2 camSens;

    [SerializeField] CharacterController characterController;
    // private TextMeshProUGUI debugText;
    private bool jumpPressed = false;
    private bool groundedLastTick = false;

    [SerializeField] private float _groundAcceleration;
    [SerializeField] private float _airAcceleration;
    [SerializeField] private float _groundMaxVel;
    [SerializeField] private float _airMaxVel;
    [SerializeField] private float _friction;
    [SerializeField] private float _gravity;
    [SerializeField] private float _jumpVel;
    // Whether to apply friction in air or not
    [SerializeField] private bool _alwaysApplyFriction;

    private Vector3 _moveInputDir = Vector3.zero;
    private Vector3 _rawMoveInputDir = Vector3.zero;

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
        // debugText.text = string.Format(
        //     "CCVel:\t{0}\nInputDir:\t{1}\n{2}\n{3}\n{4}", 
        //     characterController.velocity, 
        //     _moveInputDir, jumpPressed, 
        //     characterController.isGrounded, 
        //     Move(_moveInputDir, characterController.velocity).y
        // );  

        // Player movement
        Vector3 delta = Move(_moveInputDir, characterController.velocity);

        // Apply gravity and jump
        if (characterController.isGrounded && jumpPressed)
        {
            delta.y = _jumpVel * Time.fixedDeltaTime;
            jumpPressed = false;
        }
        delta.y += _gravity * Time.fixedDeltaTime;
        groundedLastTick = characterController.isGrounded;

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
        bool useGroundPhys = groundedLastTick && characterController.isGrounded;
        // Apply friction
        Vector3 lateralVel = Vector3.Scale(currentVel, new Vector3(1, 0, 1));
        if (lateralVel.magnitude != 0 && (useGroundPhys|| _alwaysApplyFriction))
        {
            float d = lateralVel.magnitude * _friction * Time.fixedDeltaTime;
            currentVel.x *= Mathf.Max(lateralVel.magnitude - d, 0) / lateralVel.magnitude;
            currentVel.z *= Mathf.Max(lateralVel.magnitude - d, 0) / lateralVel.magnitude;
        }

        return AddAcceleration(
            inputDir,
            currentVel,
            useGroundPhys ? _groundAcceleration : _airAcceleration,
            useGroundPhys ? _groundMaxVel : _airMaxVel
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
}