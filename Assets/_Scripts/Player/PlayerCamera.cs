using System;
using UnityEngine;

public class PlayerCamera : UnityEngine.MonoBehaviour
{
    public float sensitivity = 300f;
    public Transform head;
    public Transform body;
    public Transform moveDirection;
    public PlayerInput playerInput;

    
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    //NOTE: body moves only if head moves more than 50degree from the body rotation
    private void Update()
    {
        var mouseX = playerInput.MousePosition.x * sensitivity * Time.deltaTime;
        var mouseY = playerInput.MousePosition.y * sensitivity * Time.deltaTime;

        var headRotation = head.localRotation;
        var verticalRotation = headRotation.eulerAngles.x;
        var horizontalRotation = headRotation.eulerAngles.y;

        verticalRotation += mouseY;
        horizontalRotation += mouseX;

        verticalRotation = verticalRotation switch
        {
            > 90 and < 200 => 90,
            < 270 and > 200 => 270,
            _ => verticalRotation
        };

        // Right lock
        if (horizontalRotation > 50 && horizontalRotation < 200)
        {
            horizontalRotation = 50-10;
            body.Rotate(Vector3.up, 10);
        }
        // Left lock
        else if (horizontalRotation < 310 && horizontalRotation > 200)
        {
            horizontalRotation = 310+10;
            body.Rotate(Vector3.up, -10);
        }

        // verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        headRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        head.localRotation = headRotation;
        
        // body.Rotate(Vector3.up, mouseX);
        
        moveDirection.localRotation = Quaternion.Euler(0, horizontalRotation, 0f);
    }
}