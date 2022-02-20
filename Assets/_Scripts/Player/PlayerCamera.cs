using System;
using UnityEngine;

public class PlayerCamera : UnityEngine.MonoBehaviour
{
    public float sensitivity = 300f;
    public PlayerInput playerInput;
    private PlayerObjects objects;

    
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        objects = GetComponent<PlayerObjects>();
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

        var headRotation = objects.head.localRotation;
        var verticalRotation = headRotation.eulerAngles.x;
        var horizontalRotation = headRotation.eulerAngles.y;

        verticalRotation += mouseY;
        horizontalRotation += mouseX;

        if (verticalRotation is > 90 and < 200)
            verticalRotation = 90;
        else if (verticalRotation is < 270 and > 200)
            verticalRotation = 270;
        else
            verticalRotation = verticalRotation;

        // Right lock
        if (horizontalRotation > 50 && horizontalRotation < 200)
        {
            horizontalRotation = 50-10;
            objects.body.Rotate(Vector3.up, 10);
        }
        // Left lock
        else if (horizontalRotation < 310 && horizontalRotation > 200)
        {
            horizontalRotation = 310+10;
            objects.body.Rotate(Vector3.up, -10);
        }

        // verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        headRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        objects.head.localRotation = headRotation;
        
        // body.Rotate(Vector3.up, mouseX);
        
        objects.moveDirection.localRotation = Quaternion.Euler(0, horizontalRotation, 0f);
    }
}