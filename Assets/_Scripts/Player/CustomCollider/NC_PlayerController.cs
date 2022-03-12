using System.Collections;
using UnityEditor;
using UnityEngine;

public class NC_PlayerController : MonoBehaviour
{
    public float walkSpeed = 8f;
    public float sprintSpeed = 12f;
    public float jumpForce = 8f;
    public float gravity = -32f;
    
    [Space]
    
    public float playerWidth = 0.25f;
    public float playerHeight = 0.5f;

    [Space] 
    
    public bool isGrounded;
    public bool isSprinting;
    
    
    private Transform camera;
    private World world;
    
    private float horizontal;
    private float vertical;
    
    private float mouseX;
    private float mouseY;
    
    private Vector3 velocity;
    private float verticalMomentum;
    private bool jumpRequest;
    private bool isWaitingOnJumpDelay;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camera = GameObject.Find("Camera").transform;
        world = World.Instance;
    }

    private void FixedUpdate()
    {
        CalculateVelocity();

        if (Mathf.Abs(velocity.x) <= 0 && Mathf.Abs(velocity.z) <= 0)
        {
            isSprinting = false;
        }

        if (jumpRequest && !isWaitingOnJumpDelay)
        {
            Jump();
        }

        transform.Rotate(Vector3.up * mouseX);
        camera.Rotate(Vector3.left * mouseY);
        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
        StartCoroutine(WaitOnJumpDelay());
    }
    
    private void CalculateVelocity()
    {
        // Apply gravity
        if (!isGrounded)
        {
            verticalMomentum += gravity * Time.deltaTime;
            verticalMomentum *= 0.98f;
        }
        else if(verticalMomentum < 0)
        {
            verticalMomentum = 0;
        }

        // if we are sprinting
        if (isSprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        }
        else
        { 
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }
        
        // Apply vertical momentum (falling & jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        // Check collisions
        if(velocity.z > 0 && front())
        {
            transform.SetZPosition(Mathf.Floor(transform.position.z)+(0.5f-playerWidth-0.005f));
            velocity.z = 0;
        }

        if (velocity.z < 0 && back())
        {
            transform.SetZPosition(Mathf.Floor(transform.position.z)+(0.5f+playerWidth));
            velocity.z = 0;
        }
        
        if(velocity.x > 0 && right())
        {
            transform.SetXPosition(Mathf.Floor(transform.position.x)+(0.5f-playerWidth-0.005f));
            velocity.x = 0;
        }

        if (velocity.x < 0 && left())
        {
            transform.SetXPosition(Mathf.Floor(transform.position.x)+(0.5f+playerWidth+0.005f));
            velocity.x = 0;
        }

        if (velocity.y == 0)
        {
            this.ExecuteAfterFrames(1, () =>
            {
                transform.SetYPosition(Mathf.Floor(transform.position.y)+0.51f);
            });
        }
        
        if((velocity.y <= 0))
        {
            velocity.y = CheckDownCollision(velocity.y);
        } else if((velocity.y > 0))
        {
            velocity.y = CheckUpCollision(velocity.y);
        }
        
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
    
    private IEnumerator WaitOnJumpDelay()
    {
        isWaitingOnJumpDelay = true;
        yield return new WaitForSeconds(0.5f);
        isWaitingOnJumpDelay = false;
    }


    private float CheckDownCollision(float downSpeed)
    {
        if (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y-0.11f + downSpeed, transform.position.z - playerWidth)).type].generateCollider && !left() && !back() ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y-0.11f + downSpeed, transform.position.z - playerWidth)).type].generateCollider && !right() && !back() ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y-0.11f + downSpeed, transform.position.z + playerWidth)).type].generateCollider && !right() && !front()||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y-0.11f + downSpeed, transform.position.z + playerWidth)).type].generateCollider && !left() && !front()
        )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }

    }
    
    private float CheckUpCollision(float upSpeed)
    {
        if (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)).type].generateCollider && !left() && !back() ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y + playerHeight + upSpeed, transform.position.z - playerWidth)).type].generateCollider && !right() && !back()||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)).type].generateCollider && !right() && !front()||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y + playerHeight + upSpeed, transform.position.z + playerWidth)).type].generateCollider && !left() && !front()
        )
        {
            verticalMomentum = 0;
            return 0;
        }
        else
        {
            return upSpeed;
        }

    }

    public bool front(bool extra = true)
    {
        if(
            // Two blocks directly in front
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y, transform.position.z + playerWidth+0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y + 1, transform.position.z + playerWidth+0.03f)).type].generateCollider ||
            
            // Blocks to the right and left
            extra && (
            !right(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y, transform.position.z + playerWidth+0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y + 1, transform.position.z + playerWidth+0.03f)).type].generateCollider) || 
            !left(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y, transform.position.z + playerWidth+0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y + 1, transform.position.z + playerWidth+0.03f)).type].generateCollider))
        ) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool back(bool extra = true)
    {
        if(
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y, transform.position.z - playerWidth-0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y + 1, transform.position.z - playerWidth-0.03f)).type].generateCollider ||
            
            // Blocks to the right and left
            extra && (
            !right(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y, transform.position.z - playerWidth-0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth,  transform.position.y + 1, transform.position.z - playerWidth-0.03f)).type].generateCollider) ||
            !left(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y, transform.position.z - playerWidth-0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth,  transform.position.y + 1, transform.position.z - playerWidth-0.03f)).type].generateCollider))
            )
            return true;
        else
        {
            return false;
        }
    }
    
    public bool left(bool extra = true)
    {

        if(
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth-0.03f,  transform.position.y, transform.position.z)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth-0.03f,  transform.position.y + 1, transform.position.z)).type].generateCollider ||
            
            // Blocks to the front and back
            extra && (
            !front(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth-0.03f,  transform.position.y, transform.position.z + playerWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth-0.03f,  transform.position.y + 1, transform.position.z + playerWidth)).type].generateCollider) ||
            !back(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth-0.03f,  transform.position.y, transform.position.z - playerWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - playerWidth-0.03f,  transform.position.y + 1, transform.position.z - playerWidth)).type].generateCollider))
        )
            return true;
        else
        {
            return false;
        }
        
    }
    
    public bool right(bool extra = true)
    {
        if(
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth+0.03f,  transform.position.y, transform.position.z)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth+0.03f,  transform.position.y + 1, transform.position.z)).type].generateCollider ||
            
            // Blocks to the front and back
            extra && (
            !front(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth+0.03f,  transform.position.y, transform.position.z + playerWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth+0.03f,  transform.position.y + 1, transform.position.z + playerWidth)).type].generateCollider) ||
            !back(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth+0.03f,  transform.position.y, transform.position.z - playerWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + playerWidth+0.03f,  transform.position.y + 1, transform.position.z - playerWidth)).type].generateCollider))
        )
            return true;
        else
        {
            return false;
        }
    }
    
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + (Vector3.up*playerHeight/2), new Vector3(playerWidth * 2, playerHeight, playerWidth * 2));
    }
    #endif
}