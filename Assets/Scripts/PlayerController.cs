using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameManager game = default;

    [Header("Camera")]
    [SerializeField] Camera playerCamera = default;
    [SerializeField] float cameraDistance = 15;
    [SerializeField] float cameraRadicalFalloff = 1;
    [SerializeField] float cameraTangentialSpeed = 20;

    [Header("Move")]
    [SerializeField] float fallThreshold = -10;
    [SerializeField] float verticalTorque = 100;
    [SerializeField] float horizontalTorque = 100;

    [Header("Jump")]
    [SerializeField] float maxJumpTime = 0.5f;
    [SerializeField] float minJumpSpeed = 5;
    [SerializeField] float maxJumpSpeed = 15;
    [SerializeField] LayerMask groundLayers = default;
    [SerializeField] float rayLength = 1.2f;

    [Header("Collectable")]
    [SerializeField] LayerMask collectableLayers = default;

    PlayerControls controls;
    Rigidbody rb;

    Vector2 movement;
    bool grounded = false;

    bool jump = false;
    float jumpedTime = 0;

    bool balanced = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        RegisterControlCallbacks();
    }

    void OnEnable()
    {
        if (controls == null) controls = new PlayerControls();
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    {
        GroundedCheck();
        BalanceCheck();
        UpdateCamera();
        FallReset();
    }

    void FixedUpdate()
    {
        ApplyTorque();
        ApplyJump();
    }

    void OnTriggerEnter(Collider collider)
    {
        var obj = collider.gameObject;
        var collectable = collider.gameObject.GetComponent<Collectable>();
        if ((collectable && (1 << obj.layer & collectableLayers) != 0))
        {
            game.IncreaseScore(collectable.score);
            Destroy(obj);
        }
    }

    void RegisterControlCallbacks()
    {
        controls.Player.Move.performed += context => OnMove(context.ReadValue<Vector2>());
        controls.Player.Move.canceled += context => OnMove(context.ReadValue<Vector2>());

        controls.Player.Jump.performed += context => OnJumpPerformed();
        controls.Player.Jump.canceled += context => OnJumpCanceled();
    }

    public void OnMove(Vector2 movement)
    {
        this.movement = movement;
        Debug.Log($"Horizontal: {movement.x}, Vertical: {movement.y}");
    }

    public void OnJumpPerformed()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (grounded)
        {
            jump = true;
            jumpedTime = 0;
        }
    }

    public void OnJumpCanceled()
    {
        jump = false;
        jumpedTime = 0;
    }
    void FallReset()
    {
        if (transform.position.y < fallThreshold) game.Reset();
    }

    void GroundedCheck()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(ray, rayLength, groundLayers);
    }

    void BalanceCheck()
    {
        var balanceAngle = Vector3.Angle(Vector3.up, transform.right);
        Debug.DrawRay(transform.position, transform.right);
        balanced = balanceAngle > 30 && balanceAngle < 150;
    }

    void UpdateCamera()
    {
        var trans = playerCamera.transform;
        var look = transform.position - trans.position;
        trans.rotation = Quaternion.LookRotation(look, Vector3.up);

        if (grounded && !balanced) return;

        var planeLook = new Vector2(look.x, look.z);
        var velocity = new Vector2(rb.velocity.x, rb.velocity.z);
        var distance = planeLook.magnitude;
        var angle = Vector2.SignedAngle(velocity, planeLook);
        if (distance > cameraDistance && Mathf.Abs(angle) < 90)
        {
            var lookDir = planeLook.normalized;
            var radical = lookDir * (distance - cameraDistance);
            var tangent = new Vector2(-lookDir.y, lookDir.x) * angle * Mathf.Deg2Rad * cameraTangentialSpeed;
            var target = trans.position + new Vector3(radical.x, 0, radical.y) + new Vector3(tangent.x, 0, tangent.y);
            target.y = transform.position.y + 5;
            trans.position = Vector3.Lerp(trans.position, target, 1 - Mathf.Exp(-cameraRadicalFalloff * Time.deltaTime));
        }
    }

    void ApplyTorque()
    {
        var verticalAxis = transform.right * movement.y;
        rb.AddTorque(verticalTorque * verticalAxis);

        var horizontalAxis = Vector3.Cross(Vector3.up, transform.right) * movement.x;
        rb.AddTorque(horizontalTorque * horizontalAxis);
    }

    void ApplyJump()
    {
        if (!jump) return;

        jumpedTime += Time.fixedDeltaTime;
        if (jumpedTime > maxJumpTime) jump = false;

        var speed = Mathf.Lerp(minJumpSpeed, maxJumpSpeed, jumpedTime / maxJumpTime);
        speed = Mathf.Min(speed, maxJumpSpeed);

        var velocity = rb.velocity;
        velocity.y = speed;
        rb.velocity = velocity;
    }
}
