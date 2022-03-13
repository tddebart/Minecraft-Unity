using System.Collections;
using Popcron;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Gizmos = Popcron.Gizmos;

public class Player : BaseEntity
{
    private Transform _camera;

    public override void Start()
    {
        base.Start();
        Cursor.lockState = CursorLockMode.Locked;
        _camera = GameObject.Find("MainCamera").transform;
    }

    private void OnEnable()
    {
        Gizmos.CameraFilter += cam => cam.transform == _camera;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _camera.Rotate(Vector3.left * mouseY);
    }

    public override void Update()
    {
        base.Update();
        GetPlayerInput();
    }

    private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFlying = !isFlying;
            verticalMomentum = 0;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = true;
            if (!isFlying)
            {
                isSprinting = false;
            }
        }
        
        if (Input.GetButtonUp("Crouch"))
        {
            isCrouching = false;
        }


        if (isFlying)
        {
            if (Input.GetButton("Jump"))
            {
                verticalMomentum = flySpeed;
                if (isCrouching)
                {
                    verticalMomentum = 0;
                }
            }
            else if (isCrouching)
            {
                verticalMomentum = -flySpeed;
            }
            else
            {
                verticalMomentum *= 0.9f;
            }
        }

        if(isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                jumpRequest = true;
            }
            if(Input.GetButtonDown("Jump"))
            {
                isWaitingOnJumpDelay = false;
                StopAllCoroutines();
            }
            if(Input.GetButtonUp("Jump"))
            {
                jumpRequest = false;
            }
        }
    }
    
    
}