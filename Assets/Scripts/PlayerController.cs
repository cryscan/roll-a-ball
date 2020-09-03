using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Camera playerCamera;

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

    void Update()
    {
        FallReset();
    }

    void FixedUpdate()
    {
        float torque = verticalTorque;
        var verticalAxis = transform.right * movement.y;
        rb.AddTorque(torque * verticalAxis);

        torque = horizontalTorque;
        var horizontalAxis = Vector3.Cross(Vector3.up, verticalAxis) * movement.x;
        rb.AddTorque(torque * horizontalAxis);
    }
}
