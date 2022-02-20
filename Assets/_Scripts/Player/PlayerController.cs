using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;

    public float playerSpeed = 4.317f;
    public float playerRunSpeed = 5.612f;
    public float gravity = -9.81f;
    public float flySpeed = 10.92f;
    public float flyRunSpeed = 21.6f;

    private Vector3 playerVelocity;
    
    [Header("Grounded check parameters")]
    public LayerMask groundMask;
    public float rayDistance = 0.1f;
    [field: SerializeField]
    public bool IsGrounded { get; private set; }

    private long startTime;
    private long endTime;

    private float maxHeightReached;
    
    private PlayerObjects objects;
    
    
    // INFO:
    // 530ms from jumping to landing on the ground
    // 400ms to jump up one block
    
    
    //TODO:
    // make the collider a box instead of a capsule
    // make it so when you let go of the movement keys, you stop moving instead of sliding
    // rotate the body when you move in direction

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        objects = GetComponent<PlayerObjects>();
    }

    private Vector3 GetMovementDirection(Vector3 movementInput)
    {
        return-(objects.moveDirection.right * movementInput.normalized.x + objects.moveDirection.transform.forward * movementInput.normalized.z);
    }
    
    public void Fly(Vector3 movementInput, bool isJumpPressed, bool isRunPressed)
    {
        var movementDirection = GetMovementDirection(movementInput);
        
        if (isJumpPressed)
        {
            movementDirection += Vector3.up;
        }
        else if(isRunPressed)
        {
            movementDirection -= Vector3.up;
        }

        controller.Move(movementDirection * Time.deltaTime * flySpeed);
    }
    
    public void Move(Vector3 movementInput, bool isRunPressed)
    {
        var movementDirection = GetMovementDirection(movementInput);
        var movementSpeed = isRunPressed ? playerRunSpeed : playerSpeed;

        controller.Move(movementDirection * Time.deltaTime * movementSpeed);

        // if we move forward or backwards, we want to rotate the body to face the direction we are moving
        if (movementInput.z != 0 && objects.body.localRotation.eulerAngles.x != Vector3.SignedAngle(objects.body.forward, objects.moveDirection.forward, Vector3.up))
        {
            RotateBody(Vector3.SignedAngle(objects.body.forward, objects.moveDirection.forward, Vector3.up)*(Time.deltaTime*6));
        }
        
        // if we move to the left, we want to rotate the body 45 degrees to the left
        if (movementInput.x < 0 && objects.body.localRotation.eulerAngles.x != Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(-objects.moveDirection.right,objects.moveDirection.forward,0.5f), Vector3.up))
        {
            RotateBody(Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(-objects.moveDirection.right,objects.moveDirection.forward,0.46f), Vector3.up)*(Time.deltaTime*10));
        }
        
        // if we move to the right, we want to rotate the body 45 degrees to the right
        if (movementInput.x > 0 && objects.body.localRotation.eulerAngles.x != Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(objects.moveDirection.right,objects.moveDirection.forward,0.5f), Vector3.up))
        {
            RotateBody(Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(objects.moveDirection.right,objects.moveDirection.forward,0.46f), Vector3.up)*(Time.deltaTime*10));
        }
    }

    public void HandleGravity(bool isJumpPressed)
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        if (isJumpPressed && IsGrounded)
        {
            AddJumpForce();
        }

        ApplyGravityForce();
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void AddJumpForce()
    {
        playerVelocity.y = 10.15f;
        startTime = DateTime.Now.Ticks;
    }

    private void ApplyGravityForce()
    {
        playerVelocity.y += gravity * Time.deltaTime;
        playerVelocity.y *= 0.98f;
    }

    private void FixedUpdate()
    {
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundMask);
        if(startTime != 0 && endTime == 0 && IsGrounded)
        {
            endTime = DateTime.Now.Ticks;
            Debug.Log((endTime - startTime) / TimeSpan.TicksPerMillisecond);
            Debug.Log("maxHeightReached: " + maxHeightReached);
            startTime = 0;
            endTime = 0;
            maxHeightReached = transform.position.y;
        }

        if (!IsGrounded)
        {
            maxHeightReached = Mathf.Max(maxHeightReached, transform.position.y);
        }

        Debug.Log(controller.velocity.x);
    }

    // When the body is rotated, the head needs to be rotated as well to keep the head in the same position
    private void RotateBody(float bodyYawRotation)
    {
        objects.body.Rotate(objects.body.up, bodyYawRotation);

        var headRotation = objects.head.localRotation.eulerAngles;
        headRotation += new Vector3(0, -bodyYawRotation, 0);
        objects.head.localRotation = Quaternion.Euler(headRotation);
    }
    
    // private void SetBodyRotation(float bodyYawRotation)
    // {
    //     objects.body.localRotation = Quaternion.Euler(bodyYawRotation, 90, 90);
    //     objects.head.localRotation = Quaternion.Euler(objects.head.localRotation.x, objects.head.localRotation.y-bodyYawRotation, 0);
    // }
}