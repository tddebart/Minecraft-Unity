using System.Collections;
using Popcron;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

[ExecuteAlways]
public abstract class BaseEntity : MonoBehaviour
{
    public float crouchSpeed = 1.31f;
    public float walkSpeed = 4.317f;
    public float sprintSpeed = 5.612f;
    public float flySpeed = 10.92f;
    public float flySprintSpeed = 21.6f;
    public float jumpForce = 10.15f;
    public float gravity = -32f;
    
    [Space]
    
    public float entityWidth = 0.32f;
    public float entityHeight = 1.8f;
    public float eyeHeight =1.578f;

    [Space] 
    
    public bool isGrounded;
    public bool isSprinting;
    public bool isCrouching;
    public bool isFlying;
    public bool drawBounds;
    
    [Space]
    
    protected World world;
    
    protected float horizontal;
    protected float vertical;
    
    protected float mouseX;
    protected float mouseY;
    
    protected Vector3 velocity;
    protected float verticalMomentum;
    protected bool jumpRequest;
    protected bool isWaitingOnJumpDelay;


    public virtual void Start()
    {
        world = World.Instance;
    }

    public virtual void FixedUpdate()
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

        Move();
    }

    public virtual void Move()
    {
        transform.Rotate(Vector3.up * mouseX);
        transform.Translate(velocity, Space.World);
    }

    public virtual void Update()
    {
        Gizmos.Enabled = drawBounds;
        Gizmos.Draw<CubeDrawer>(Color.white, false, transform.position + (Vector3.up*entityHeight/2),Quaternion.identity, new Vector3(entityWidth * 2, entityHeight, entityWidth * 2));
        Gizmos.Draw<CubeDrawer>(Color.red, false, transform.position + (Vector3.up*eyeHeight),Quaternion.identity, new Vector3(entityWidth * 2, 0.01f, entityWidth * 2));
    }
    
    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
        StartCoroutine(WaitOnJumpDelay());
    }
    
    private IEnumerator WaitOnJumpDelay()
    {
        isWaitingOnJumpDelay = true;
        yield return new WaitForSeconds(0.5f);
        isWaitingOnJumpDelay = false;
    }

    public void CalculateVelocity()
    {
        // Apply gravity
        if (!isGrounded && !isFlying)
        {
            verticalMomentum += gravity * 0.98f * Time.deltaTime;
            // verticalMomentum *= 0.99f;
        }
        else if(verticalMomentum < 0 && !isFlying)
        {
            verticalMomentum = 0;
        }

        // Calculate movement velocity
        if (isCrouching && !isFlying)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * (Time.fixedDeltaTime * crouchSpeed);
        }
        else if (isSprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * (Time.fixedDeltaTime * (isFlying ? flySprintSpeed : sprintSpeed));
        }
        else
        { 
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * (Time.fixedDeltaTime * (isFlying ? flySpeed : walkSpeed));
        }
        
        // Apply vertical momentum (falling & jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        // Check collisions
        if(velocity.z > 0 && front())
        {
            transform.SetZPosition(Mathf.Floor(transform.position.z)+(0.5f-entityWidth-0.005f));
            velocity.z = 0;
        }

        if (velocity.z < 0 && back())
        {
            transform.SetZPosition(Mathf.Floor(transform.position.z)+(0.5f+entityWidth));
            velocity.z = 0;
        }
        
        if(velocity.x > 0 && right())
        {
            transform.SetXPosition(Mathf.Floor(transform.position.x)+(0.5f-entityWidth-0.005f));
            velocity.x = 0;
        }

        if (velocity.x < 0 && left())
        {
            transform.SetXPosition(Mathf.Floor(transform.position.x)+(0.5f+entityWidth+0.005f));
            velocity.x = 0;
        }

        if (velocity.y == 0 && !isFlying)
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
    
    
    #region Collision checks
    
    private float CheckDownCollision(float downSpeed)
    {
        if (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y-0.11f + downSpeed, transform.position.z - entityWidth)).type].generateCollider && !left() && !back() ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y-0.11f + downSpeed, transform.position.z - entityWidth)).type].generateCollider && !right() && !back() ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y-0.11f + downSpeed, transform.position.z + entityWidth)).type].generateCollider && !right() && !front()||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y-0.11f + downSpeed, transform.position.z + entityWidth)).type].generateCollider && !left() && !front()
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
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + entityHeight + upSpeed, transform.position.z - entityWidth)).type].generateCollider && !left() && !back() ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + entityHeight + upSpeed, transform.position.z - entityWidth)).type].generateCollider && !right() && !back()||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + entityHeight + upSpeed, transform.position.z + entityWidth)).type].generateCollider && !right() && !front()||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + entityHeight + upSpeed, transform.position.z + entityWidth)).type].generateCollider && !left() && !front()
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
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y, transform.position.z + entityWidth+0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y + 1, transform.position.z + entityWidth+0.03f)).type].generateCollider ||
            
            // Blocks to the right and left
            extra && (
            !right(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y, transform.position.z + entityWidth+0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + 1, transform.position.z + entityWidth+0.03f)).type].generateCollider) || 
            !left(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y, transform.position.z + entityWidth+0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + 1, transform.position.z + entityWidth+0.03f)).type].generateCollider))
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
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y, transform.position.z - entityWidth-0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x,  transform.position.y + 1, transform.position.z - entityWidth-0.03f)).type].generateCollider ||
            
            // Blocks to the right and left
            extra && (
            !right(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y, transform.position.z - entityWidth-0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + 1, transform.position.z - entityWidth-0.03f)).type].generateCollider) ||
            !left(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y, transform.position.z - entityWidth-0.03f)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + 1, transform.position.z - entityWidth-0.03f)).type].generateCollider))
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
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y, transform.position.z)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y + 1, transform.position.z)).type].generateCollider ||
            
            // Blocks to the front and back
            extra && (
            !front(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y, transform.position.z + entityWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y + 1, transform.position.z + entityWidth)).type].generateCollider) ||
            !back(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y, transform.position.z - entityWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y + 1, transform.position.z - entityWidth)).type].generateCollider))
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
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y, transform.position.z)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y + 1, transform.position.z)).type].generateCollider ||
            
            // Blocks to the front and back
            extra && (
            !front(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y, transform.position.z + entityWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y + 1, transform.position.z + entityWidth)).type].generateCollider) ||
            !back(false) && (
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y, transform.position.z - entityWidth)).type].generateCollider ||
            BlockDataManager.textureDataDictionary[(int)world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y + 1, transform.position.z - entityWidth)).type].generateCollider))
        )
            return true;
        else
        {
            return false;
        }
    }
    
    #endregion
}