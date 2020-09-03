using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, InputActions.IPlayerActions
{
    [SerializeField] float verticalTorque;
    [SerializeField] float horizontalTorque;

    InputActions inputActions;

    Rigidbody rb;
    Vector2 movement;

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Awake()
    {
        inputActions = new InputActions();
        inputActions.Player.Enable();

        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        inputActions.Player.SetCallbacks(this);
        Debug.Log("Started");
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        Debug.Log($"Horizontal: {movement.x}, Vertical: {movement.y}");
    }

    public void OnFire(InputAction.CallbackContext context) { }

    public void OnLook(InputAction.CallbackContext context) { }

    void FixedUpdate()
    {
        float torque = verticalTorque;
        var verticalAxis = transform.right * movement.y;
        rb.AddTorque(torque * verticalAxis);

        torque = horizontalTorque;
        var horizontalAxis = -transform.forward * movement.x;
        rb.AddTorque(torque * horizontalAxis);
    }
}
