using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Camera playerCamera = default;
    [SerializeField] float cameraDistance = 15;
    [SerializeField] float cameraFollowFalloff = 1;

    [Header("Misc")]
    [SerializeField] float fallThreshold = -10;
    [SerializeField] float balanceLimit = 30;

    [Header("Move")]
    [SerializeField] float maxAxialPower = 3000;
    [SerializeField] float maxAxialTorque = 1000;
    [SerializeField] float inclineAngleSensitivity = 60;
    [SerializeField] float inclineAngleFalloff = 0.1f;
    [SerializeField] float targetInclineAngleFalloff = 10;

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

    [SerializeField] Vector2 movement;

    float targetInclineAngle = 0;

    bool balanced = true;

    bool grounded = false;

    bool jump = false;
    float jumpedTime = 0;

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
        UpdateTargetInclineAngle();
        FallReset();
    }

    void FixedUpdate()
    {
        ApplyControl();
        ApplyJump();
    }

    void OnTriggerEnter(Collider collider)
    {
        var obj = collider.gameObject;
        var collectable = collider.gameObject.GetComponent<Collectable>();
        if ((collectable && (1 << obj.layer & collectableLayers) != 0))
        {
            GameManager.instance.IncreaseScore(collectable.score);
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
        if (transform.position.y < fallThreshold) GameManager.instance.Reset();
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
        balanced = balanceAngle > balanceLimit && balanceAngle < 180 - balanceLimit;
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
        if (distance > cameraDistance)
        {
            var lookDir = planeLook.normalized;
            var delta = lookDir * (distance - cameraDistance);
            var target = trans.position + new Vector3(delta.x, 0, delta.y);
            target.y = transform.position.y + 5;
            trans.position = Vector3.Lerp(trans.position, target, 1 - Mathf.Exp(-cameraFollowFalloff * Time.deltaTime));
        }
    }

    void UpdateTargetInclineAngle()
    {
        if (movement.x != 0)
        {
            var inclineAngleLimit = 90 - balanceLimit;
            targetInclineAngle += movement.x * inclineAngleSensitivity * Time.deltaTime;
            targetInclineAngle = Mathf.Clamp(targetInclineAngle, -inclineAngleLimit, inclineAngleLimit);
        }
        else
        {
            targetInclineAngle = Mathf.Lerp(targetInclineAngle, 0, 1 - Mathf.Exp(-targetInclineAngleFalloff * Time.deltaTime));
        }
    }

    void ApplyControl()
    {
        var primaryAxis = transform.right;                              // right
        var secondaryAxis = Vector3.Cross(Vector3.up, primaryAxis);     // forward
        var tertiaryAxis = Vector3.Cross(primaryAxis, secondaryAxis);   // up

        var angularVelocity = rb.angularVelocity;

        /*
        Vector3 deltaAxialAngularVelocity = Vector3.zero;
        if (movement.y != 0)
        {
            var axialAxis = primaryAxis * movement.y;
            var axialSpeed = Vector3.Dot(angularVelocity, axialAxis);
            var targetAxialSpeed = Mathf.Lerp(axialSpeed, maxAxialSpeed, 1 - Mathf.Exp(-axialFalloff * Time.fixedDeltaTime));
            var deltaAxialSpeed = targetAxialSpeed - axialSpeed;
            deltaAxialAngularVelocity = axialAxis * deltaAxialSpeed;
        }
        angularVelocity += deltaAxialAngularVelocity;
        */

        float axialTorque = 0;
        if (movement.y != 0)
        {
            var axialAxis = primaryAxis * movement.y;
            var axialSpeed = Vector3.Dot(angularVelocity, axialAxis);
            if (axialSpeed < maxAxialPower / maxAxialTorque) axialTorque = maxAxialTorque;
            else axialTorque = maxAxialPower / axialSpeed;

            rb.AddTorque(axialTorque * axialAxis);
        }

        if (grounded)
        {
            var inclineAngle = Vector3.SignedAngle(Vector3.up, tertiaryAxis, secondaryAxis);
            var nextInclineAngle = Mathf.Lerp(inclineAngle, targetInclineAngle, 1 - Mathf.Exp(-inclineAngleFalloff * Time.fixedDeltaTime));
            var inclineAngularSpeed = (nextInclineAngle - inclineAngle) / Time.fixedDeltaTime;

            var secondaryProjection = Vector3.Dot(angularVelocity, secondaryAxis);
            angularVelocity += secondaryAxis * (-secondaryProjection + inclineAngularSpeed);
        }
        else
        {
            rb.AddTorque(axialTorque * movement.x * secondaryAxis);
        }

        rb.angularVelocity = angularVelocity;
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
