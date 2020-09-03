using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Camera playerCamera;
    [SerializeField] float cameraDistance = 15;
    [SerializeField] float cameraRadicalFalloff = 1;
    [SerializeField] float cameraTangentialSpeed = 20;

    [SerializeField] float fallThreshold = -10;
    [SerializeField] float verticalTorque = 100;
    [SerializeField] float horizontalTorque = 100;

    Rigidbody rb;
    Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
        Debug.Log($"Horizontal: {movement.x}, Vertical: {movement.y}");
    }

    public void OnFire() { }

    public void OnLook() { }

    void FallReset()
    {
        if (transform.position.y < fallThreshold) game.Reset();
    }

    void UpdateCamera()
    {
        var trans = playerCamera.transform;
        var look = transform.position - trans.position;
        trans.rotation = Quaternion.LookRotation(look, Vector3.up);

        var balanceAngle = Vector3.Angle(Vector3.up, transform.right);
        Debug.DrawRay(transform.position, transform.right);
        if (balanceAngle < 10 || balanceAngle > 170) return;

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
            trans.position = Vector3.Lerp(trans.position, target, 1 - Mathf.Exp(-cameraRadicalFalloff * Time.deltaTime));
        }
    }

    void Update()
    {
        UpdateCamera();
        FallReset();
    }

    void FixedUpdate()
    {
        float torque = verticalTorque;
        var verticalAxis = transform.right * movement.y;
        rb.AddTorque(torque * verticalAxis);

        torque = horizontalTorque;
        var horizontalAxis = Vector3.Cross(Vector3.up, transform.right) * movement.x;
        rb.AddTorque(torque * horizontalAxis);
    }
}
