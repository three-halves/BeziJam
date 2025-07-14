using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Transform cameraTransform;
    [SerializeField] CharacterController characterController;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private Animator swordAnimator;
    [SerializeField] private GameObject meterParent;
    [SerializeField] private Image meterFill;
    [SerializeField] private Sprite airjumpIndicatorFull;
    [SerializeField] private Sprite airjumpIndicatorEmpty;
    [SerializeField] private Image airjumpIndicator;
    private bool jumpPressed = false;
    private bool groundedLastTick = false;
    private bool grounded = false;

    [SerializeField] private float _groundAcceleration;
    [SerializeField] private float _airAcceleration;
    [SerializeField] private float _groundMaxVel;
    [SerializeField] private float _airMaxVel;
    [SerializeField] private float _friction;
    [SerializeField] private float _maxFallSpeed;
    [SerializeField] private float _gravity;
    [SerializeField] private float _jumpVel;
    [SerializeField] private float _airControl;
    [SerializeField] private float _wallRunTime;
    [SerializeField] private float _wallRunSpeed;
    [SerializeField] private float _attackCooldown;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _deathPlaneY;
    // Wall run cooldown and reduced air control after walljump
    [SerializeField] private float _wallRunRecoveryTime;
    [SerializeField] private float _wallJumpLatVel;
    // Amount of times you can double jump
    [SerializeField] private int _airJumps;
    // Whether to apply friction in air or not
    [SerializeField] private bool _alwaysApplyFriction;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask attackableMask;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip[] wallRunSFX;

    private Vector3 _moveInputDir = Vector3.zero;
    private Vector3 _rawMoveInputDir = Vector3.zero;
    private int currentAirJumps;
    private Vector3 lateralVector = new(1, 0, 1);
    private float wallRunTimer = 0f;
    private Vector3 wallRunDirection;
    private Vector3 WallRunNormal;
    private float attackTimer = 0f;
    private AttackableBase highlightedObject;
    private Vector3 wallRunCastVector;

    private float disableGravityTimer = 0f;

    // Extra force caused by other objects, to be applied next tick
    private Vector3 applyForce = Vector3.zero;

    private float targetCameraTilt = 0f;
    private float camTiltVel = 0f;
    
    [SerializeField] private GameObject collectableDisplay;
    private float collectableDisplayTimer = 0f;
    [SerializeField] private TextMeshProUGUI collectableText;
    private Vector3 spawnPosition;
    private Vector3 spawnRotation;
    private Vector2 targetCamPosition = Vector2.zero;

    private bool respawnedThisTick;

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
        spawnPosition = transform.position;
        spawnRotation = transform.forward;
    }

    void FixedUpdate()
    {
        debugText.text = string.Format(
            "CCVel:\t{0}\nInputDir:\t{1}\n{2}\nGrounded: {3}\n{4}", 
            characterController.velocity, 
            _moveInputDir, 
            jumpPressed, 
            grounded, 
            wallRunTimer
        );

        grounded = GroundCheck();
        bool inWallrun = wallRunTimer > 0;
        if (grounded) currentAirJumps = _airJumps;

        // update timers
        wallRunTimer -= Time.fixedDeltaTime;
        attackTimer -= Time.fixedDeltaTime;
        disableGravityTimer -= Time.fixedDeltaTime;
        collectableDisplayTimer -= Time.fixedDeltaTime;

        CheckHighlightableObjects();

        float scaledWallTimer = Mathf.Floor(wallRunTimer / _wallRunTime * 16) / 16;
        if (inWallrun && meterFill.fillAmount != scaledWallTimer)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(wallRunSFX[Math.Abs((int)(scaledWallTimer * 16) % wallRunSFX.Count())]);
        }

        // UI
        meterParent.SetActive(inWallrun);
        meterFill.fillAmount = scaledWallTimer;
        airjumpIndicator.sprite = currentAirJumps > 0 ? airjumpIndicatorFull : airjumpIndicatorEmpty;
        collectableDisplay.SetActive(collectableDisplayTimer > 0);

        // Player movement
        Vector3 delta = Move(_moveInputDir, characterController.velocity + applyForce);
        applyForce = Vector3.zero;

        // Jump logic
        if (jumpPressed && (grounded || currentAirJumps > 0 || inWallrun))
        {
            // Redirect all lateral movement in jump direction
            if(_moveInputDir != Vector3.zero)
                delta = Vector3.Scale(delta, lateralVector).magnitude * _moveInputDir;

            // Give some extra vel for double jumping out of a vertical jump
            if (Vector3.Scale(delta, lateralVector).magnitude < _groundMaxVel * Time.fixedDeltaTime / 3f) 
                delta = _groundMaxVel * Time.fixedDeltaTime / 3f * _moveInputDir;

            // Apply lateral jump force while in wallrun
            if (inWallrun)
            {
                wallRunTimer = 0f;
                delta = _wallRunSpeed * Time.deltaTime * wallRunDirection + WallRunNormal * _wallJumpLatVel;
            }

            // Apply vertical jump force
            delta.y = _jumpVel * Time.fixedDeltaTime;

            jumpPressed = false;

            // Remove airjump if appropriate
            if (!grounded && !inWallrun) currentAirJumps--;

            // sound effect
            audioSource.pitch = 0.85f + (1 - currentAirJumps) * 0.24f;
            audioSource.PlayOneShot(jumpSFX);

        }

        // Apply gravity
        if (disableGravityTimer <= 0) delta.y += _gravity * Time.fixedDeltaTime;
        groundedLastTick = grounded;

        // Cap fall speed
        delta.y = Math.Max(delta.y, inWallrun ? 0 : _maxFallSpeed);

        // Check if below death plane
        if (transform.position.y < _deathPlaneY) Respawn();

        // Camera tilt logic
        cameraTransform.eulerAngles = new Vector3(
            targetCamPosition.y,
            cameraTransform.eulerAngles.y,
            Mathf.SmoothDampAngle(cameraTransform.eulerAngles.z, targetCameraTilt, ref camTiltVel, 0.1f)
        );

        if (respawnedThisTick) delta = Vector3.zero;
        respawnedThisTick = false;

        characterController.Move(delta);
    }

    public void ApplyForce(Vector3 f)
    {
        applyForce += f;
    }

    /// <summary>
    /// Apply acceleration based on current velocity and desired velocity
    /// </summary>
    /// <param name="inputDir"></param>
    /// <param name="currentVel"></param>
    /// <param name="acceleration"></param>
    /// <param name="maxVel"></param>
    /// <returns></returns>
    private Vector3 AddAcceleration(Vector3 inputDir, Vector3 currentVel, float acceleration, float maxVel)
    {
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

    /// <summary>
    /// Calculates current player state and returns an approprate movement vector based on state & input
    /// </summary>
    /// <param name="inputDir"></param>
    /// <param name="currentVel"></param>
    /// <returns></returns>
    private Vector3 Move(Vector3 inputDir, Vector3 currentVel)
    {
        // Determine current state movement
        bool useGroundPhys = groundedLastTick && grounded;
        bool shouldWallrun = CheckWallrun(inputDir);
        // stop wallruning early if we stop hitting a wall
        if (!shouldWallrun && wallRunTimer > 0) 
            wallRunTimer = 0;

        // Initiate wallrun
        if (shouldWallrun && wallRunTimer <= -_wallRunRecoveryTime)
        {
            wallRunTimer = _wallRunTime;
            currentAirJumps = _airJumps;
            wallRunCastVector = transform.right;

            wallRunDirection = Vector3.Cross(Vector3.up, WallRunNormal);
            float s = Mathf.Sign(Vector3.Dot(wallRunDirection, transform.forward));
            wallRunDirection *= s;
            camTiltVel = 18 * s;
        }

        // Run current state movement
        if (wallRunTimer > 0f)
        {
            return WallrunMove(inputDir, currentVel);
        }
        else if (useGroundPhys)
        {
            return GroundMove(inputDir, currentVel);
        }
        else
        {
            return AirMove(inputDir, currentVel);
        }

    }

    /// <summary>
    /// Check if we should initiate (or maintain) a wall run this frame
    /// </summary>
    private bool CheckWallrun(Vector3 inputDir)
    {
        Physics.Raycast(
            transform.position + characterController.center,
            wallRunTimer <= 0 ? transform.right : wallRunCastVector,
            out RaycastHit hit,
            characterController.radius * 1.5f,
            groundMask
        );
        // check left wall if no right wall found
        if (hit.normal == Vector3.zero)
            Physics.Raycast(
            transform.position + characterController.center,
            (wallRunTimer <= 0 ? transform.right : wallRunCastVector) * -1,
            out hit,
            characterController.radius * 1.5f,
            groundMask
        );

        // Don't wallrun if no wall is found, input is not perpendicular to wall, or we are grounded, or not fast enough
        if (hit.normal == Vector3.zero || 
            Vector3.Dot(hit.normal, inputDir) == 0 || 
            grounded || 
            Vector3.Scale(characterController.velocity, lateralVector).magnitude < _groundMaxVel * Time.fixedDeltaTime * 0.75) 
        {
            camTiltVel = 0f;
            return false;
        }

        // Otherwise, initiate/continue wallrun
        WallRunNormal = hit.normal;
        return true;
    }

    private Vector3 WallrunMove(Vector3 inputDir, Vector3 currentVel)
    {
        float speed = Mathf.Max(Vector3.Scale(currentVel, lateralVector).magnitude, _wallRunSpeed);
        return speed * Time.fixedDeltaTime * wallRunDirection 
            + Vector3.Scale(currentVel, new Vector3(0, 1, 0)) * Time.fixedDeltaTime;
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

        float dot = Vector3.Dot(currentVel.normalized, inputDir);

        float magnitude = currentVel.magnitude;
        currentVel.Normalize();

        // reduce air control while in walljump cooldown
        float control = _airControl * Mathf.Min(-wallRunTimer / _wallRunRecoveryTime, 1f);

        float k = control * dot * dot * Time.fixedDeltaTime;
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
        Vector2 sens = new (
            PlayerPrefs.GetFloat("xsens", 0.1f) * (PlayerPrefs.GetInt("xinvert", 0) == 0 ? 1 : -1),
            PlayerPrefs.GetFloat("ysens", 0.1f) * (PlayerPrefs.GetInt("yinvert", 1) == 0 ? 1 : -1)
        );
        // Rotate user and cam to with mouse x movement
        targetCamPosition.x += v.x * sens.x;
        transform.Rotate(Vector3.up, v.x * sens.x, Space.World);

        // Rotate only cam with mouse y movement
        // cameraTransform.Rotate(Vector3.right, v.y * sens.y, Space.Self);
        targetCamPosition.y = Mathf.Clamp(targetCamPosition.y + v.y * sens.y, -85, 85);
        // Vector3 ea = cameraTransform.eulerAngles;
        // cameraTransform.eulerAngles = new Vector3(Mathf.Clamp(-Mathf.DeltaAngle(ea.x,0),-75,55), ea.y, ea.z);
        CalculateMoveInputDir();
    }

    /// <summary>
    /// Check if there are any attackable objects in attack range to highlight
    /// </summary>
    private void CheckHighlightableObjects()
    {
        // check if there are any objects we can highlight
        Physics.Raycast(
            transform.position + characterController.center,
            cameraTransform.forward,
            out RaycastHit hit,
            _attackRange + characterController.velocity.magnitude * Time.fixedDeltaTime,
            attackableMask
        );

        if (hit.collider == null)
        {
            highlightedObject?.SetHighlight(false);
            return;
        }
        
        if (hit.collider.TryGetComponent<AttackableBase>(out var found))
        {
            if (found != highlightedObject && highlightedObject != null) 
                highlightedObject.SetHighlight(false);
            
            found.SetHighlight(true);
            highlightedObject = found;
        }
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
        Respawn();
    }

    public void Respawn()
    {
        characterController.enabled = false;
        transform.position = spawnPosition;
        transform.forward = spawnRotation;
        respawnedThisTick = true;
        CalculateMoveInputDir();
        characterController.enabled = true;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || attackTimer > 0) return;

        attackTimer = _attackCooldown;
        swordAnimator.ResetTrigger("Attack");
        swordAnimator.SetTrigger("Attack");

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(attackSFX);

        // raycast forward and check if hit
        Physics.Raycast(
            transform.position + characterController.center,
            cameraTransform.forward,
            out RaycastHit hit,
            _attackRange + characterController.velocity.magnitude * Time.fixedDeltaTime,
            attackableMask
        );
        if (hit.collider == null) return;

        if (hit.collider.TryGetComponent<AttackableBase>(out var attackedObj)) 
            attackedObj.OnAttacked(this);
    }

    public Vector3 GetLookVector()
    {
        return cameraTransform.forward;
    }

    public void DisableGravityForSeconds(float s)
    {
        disableGravityTimer = s;
    }

    public void GetCollectable()
    {
        WorldState.Instance.GetCollectable();
        collectableDisplayTimer = 3f;
        collectableText.text = "x" + WorldState.Instance.collectableCount;
    }

    public void SetSpawn(Vector3 pos, Vector3 rot)
    {
        spawnPosition = pos;
        spawnRotation = rot;
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed) 
            FindObjectsByType<PauseMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0].gameObject.SetActive(true);
    }

    private bool GroundCheck()
    {
        return Physics.Raycast(transform.position + characterController.center, Vector3.down, characterController.height * 0.56f, groundMask);
    }
}