using System;
using System.Collections;
using Mirror;
using Popcron;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

[ExecuteAlways]
public abstract class BaseEntity : NetworkBehaviour
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
    public float eyeHeight = 1.578f;

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
    protected Transform direction;


    public virtual void Start()
    {
        world = World.Instance;
        direction = transform;
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
        transform.Translate(velocity, Space.World);
    }

    public virtual void Update()
    {
        if (drawBounds)
        {
            Gizmos.Draw<CubeDrawer>(Color.white, false, transform.position + (Vector3.up*entityHeight/2),Quaternion.identity, new Vector3(entityWidth * 2, entityHeight, entityWidth * 2));
            Gizmos.Draw<CubeDrawer>(Color.red, false, transform.position + (Vector3.up*eyeHeight),Quaternion.identity, new Vector3(entityWidth * 2, 0.01f, entityWidth * 2));
        }
    }
    
    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
        StartCoroutine(WaitOnJumpDelay());
    }
    
    public IEnumerator WaitOnJumpDelay()
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
            // verticalMomentum *= 0.98f;
        }
        else if(verticalMomentum < 0 && !isFlying)
        {
            verticalMomentum = 0;
        }

        // Calculate movement velocity
        if (isCrouching && !isFlying)
        {
            velocity = ((direction.forward * vertical) + (direction.right * horizontal)) * (Time.fixedDeltaTime * crouchSpeed);
        }
        else if (isSprinting)
        {
            velocity = ((direction.forward * vertical) + (direction.right * horizontal)) * (Time.fixedDeltaTime * (isFlying ? flySprintSpeed : sprintSpeed));
        }
        else
        { 
            velocity = ((direction.forward * vertical) + (direction.right * horizontal)) * (Time.fixedDeltaTime * (isFlying ? flySpeed : walkSpeed));
        }
        
        // Apply vertical momentum (falling & jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        // Check collisions
        if(velocity.z > 0 && front())
        {
            if (!(isCrouching && isGrounded && CheckDownCollision(velocity.y == 0 ? 0.1f : velocity.y, false, new Vector3(0,0,0.03f)) != 0
                /*only false if we are on the edge of a block*/ &&
                transform.position.z%1 > (Mathf.Floor(transform.position.z)+entityWidth-0.02f)%1 &&
                transform.position.z%1 < (Mathf.Floor(transform.position.z)+entityWidth+0.02f)%1))
            {
                transform.SetZPosition(Mathf.Floor(transform.position.z)+1-entityWidth);
            }
            else
            {
                transform.SetZPosition(Mathf.Floor(transform.position.z)+entityWidth-0.001f);
            }
            velocity.z = 0;
        }

        if (velocity.z < 0 && back())
        {
            if (!(isCrouching && isGrounded &&
                  CheckDownCollision(velocity.y == 0 ? 0.1f : velocity.y, false, new Vector3(0, 0, -0.03f)) != 0 &&
                  transform.position.z % 1 > (Mathf.Floor(transform.position.z) - entityWidth - 0.02f) % 1 &&
                  transform.position.z % 1 < (Mathf.Floor(transform.position.z) - entityWidth + 0.02f) % 1))
            {
                transform.SetZPosition(Mathf.Floor(transform.position.z)+entityWidth);
            }
            else
            {
                transform.SetZPosition(Mathf.Floor(transform.position.z)+1-entityWidth+0.001f);
            }

            velocity.z = 0;
        }
        
        if(velocity.x > 0 && right())
        {
            if (!(isCrouching && isGrounded &&
                  CheckDownCollision(velocity.y == 0 ? 0.1f : velocity.y, false, new Vector3(0.03f, 0, 0)) != 0 &&
                  transform.position.x % 1 > (Mathf.Floor(transform.position.x) + entityWidth - 0.02f) % 1 &&
                  transform.position.x % 1 < (Mathf.Floor(transform.position.x) + entityWidth + 0.02f) % 1))
            {
                transform.SetXPosition(Mathf.Floor(transform.position.x)+1-entityWidth);
            }
            else
            {
                transform.SetXPosition(Mathf.Floor(transform.position.x)+entityWidth-0.001f);
            }
            velocity.x = 0;
        }

        if (velocity.x < 0 && left())
        {
            if (!(isCrouching && isGrounded &&
                  CheckDownCollision(velocity.y == 0 ? 0.1f : velocity.y, false, new Vector3(-0.03f, 0, 0)) != 0 &&
                  transform.position.x % 1 > (Mathf.Floor(transform.position.x) - entityWidth - 0.02f) % 1 &&
                  transform.position.x % 1 < (Mathf.Floor(transform.position.x) - entityWidth + 0.02f) % 1))
            {
                transform.SetXPosition(Mathf.Floor(transform.position.x)+entityWidth);
            }
            else
            {
                transform.SetXPosition(Mathf.Floor(transform.position.x)+1-entityWidth+0.001f);
            }
            velocity.x = 0;
        }

        if (velocity.y == 0 && !isFlying)
        {
            var yLerp = Mathf.Lerp(transform.position.y, Mathf.Floor(transform.position.y), Time.deltaTime * -gravity);
            transform.SetYPosition(yLerp);
        }

        // If velocity y has more than 3 decimals, round it to 0
        if (Mathf.Abs(velocity.y) < 0.001f)
        {
            velocity.y = 0;
        }

        if((velocity.y) <= 0)
        {
            velocity.y = CheckDownCollision(velocity.y);
        } else if((velocity.y > 0))
        {
            velocity.y = CheckUpCollision(velocity.y);
        }
        
        // If our feet are in a block we we shouldn't fall
        if (world.GetBlock(transform.position).BlockData.generateCollider)
        {
            velocity.y = 0;
            isGrounded = true;
            verticalMomentum = 0;
        }
        
    }
    
    
    #region Collision checks
    
    private float CheckDownCollision(float downSpeed, bool setGrounded = true, Vector3? extraMovementNull = null)
    {
        extraMovementNull ??= Vector3.zero;
        var extraMovement = extraMovementNull.Value;

        if (
            world.GetBlock(new Vector3(transform.position.x - entityWidth+extraMovement.x,  transform.position.y-0.11f + downSpeed + extraMovement.y, transform.position.z - entityWidth + extraMovement.z )).BlockData.generateCollider && !left(false) && !back(false) ||
            world.GetBlock(new Vector3(transform.position.x + entityWidth+extraMovement.x,  transform.position.y-0.11f + downSpeed + extraMovement.y, transform.position.z - entityWidth + extraMovement.z )).BlockData.generateCollider && !right(false) && !back(false) ||
            world.GetBlock(new Vector3(transform.position.x + entityWidth+extraMovement.x,  transform.position.y-0.11f + downSpeed + extraMovement.y, transform.position.z + entityWidth + extraMovement.z -0.001f)).BlockData.generateCollider && !right(false) && !front(false)||
            world.GetBlock(new Vector3(transform.position.x - entityWidth+extraMovement.x,  transform.position.y-0.11f + downSpeed + extraMovement.y, transform.position.z + entityWidth + extraMovement.z -0.001f)).BlockData.generateCollider && !left(false) && !front(false)
        )
        {
            if(setGrounded)
            {
                isGrounded = true;
            }
            return 0;
        }
        else
        {
            if(setGrounded)
            {
                isGrounded = false;
            }
            return downSpeed;
        }
    }

    private float CheckUpCollision(float upSpeed)
    {
        if (
            world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + entityHeight-0.15f + upSpeed, transform.position.z - entityWidth)).BlockData.generateCollider && !left() && !back() ||
            world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + entityHeight-0.15f + upSpeed, transform.position.z - entityWidth)).BlockData.generateCollider && !right() && !back()||
            world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + entityHeight-0.15f + upSpeed, transform.position.z + entityWidth)).BlockData.generateCollider && !right() && !front()||
            world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + entityHeight-0.15f + upSpeed, transform.position.z + entityWidth)).BlockData.generateCollider && !left() && !front()
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
            world.GetBlock(new Vector3(transform.position.x,  transform.position.y, transform.position.z + entityWidth+0.03f)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x,  transform.position.y + 1, transform.position.z + entityWidth+0.03f)).BlockData.generateCollider ||
            
            // Blocks to the right and left
            extra && (
            !right(false) && (
            world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y, transform.position.z + entityWidth+0.03f)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + 1, transform.position.z + entityWidth+0.03f)).BlockData.generateCollider) || 
            !left(false) && (
            world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y, transform.position.z + entityWidth+0.03f)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + 1, transform.position.z + entityWidth+0.03f)).BlockData.generateCollider) ||
            
            // If we are crouching make sure we don't fall off the edge
            isCrouching && isGrounded && CheckDownCollision(0.1f, false, new Vector3(0,0,0.03f)) != 0 && 
            transform.position.z%1 > (Mathf.Floor(transform.position.z)+entityWidth-0.02f)%1 &&
            transform.position.z%1 < (Mathf.Floor(transform.position.z)+entityWidth+0.02f)%1
            
            ) 
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
            world.GetBlock(new Vector3(transform.position.x,  transform.position.y, transform.position.z - entityWidth-0.03f)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x,  transform.position.y + 1, transform.position.z - entityWidth-0.03f)).BlockData.generateCollider ||
            
            // Blocks to the right and left
            extra && (
            !right(false) && (
            world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y, transform.position.z - entityWidth-0.03f)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x + entityWidth,  transform.position.y + 1, transform.position.z - entityWidth-0.03f)).BlockData.generateCollider) ||
            !left(false) && (
            world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y, transform.position.z - entityWidth-0.03f)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x - entityWidth,  transform.position.y + 1, transform.position.z - entityWidth-0.03f)).BlockData.generateCollider) ||
            
            // If we are crouching make sure we don't fall off the edge
            isCrouching && isGrounded && CheckDownCollision(0.1f, false, new Vector3(0,0,-0.03f)) != 0 && 
            transform.position.z%1 > (Mathf.Floor(transform.position.z)-entityWidth-0.02f)%1 &&
            transform.position.z%1 < (Mathf.Floor(transform.position.z)-entityWidth+0.02f)%1
            )
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool right(bool extra = true)
    {
        if(
            world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y, transform.position.z)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y + 1, transform.position.z)).BlockData.generateCollider ||
            
            // Blocks to the front and back
            extra && (
                !front(false) && (
                    world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y, transform.position.z + entityWidth)).BlockData.generateCollider ||
                    world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y + 1, transform.position.z + entityWidth)).BlockData.generateCollider) ||
                !back(false) && (
                    world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y, transform.position.z - entityWidth)).BlockData.generateCollider ||
                    world.GetBlock(new Vector3(transform.position.x + entityWidth+0.03f,  transform.position.y + 1, transform.position.z - entityWidth)).BlockData.generateCollider) ||
                
                // If we are crouching make sure we don't fall off the edge
                isCrouching && isGrounded && CheckDownCollision(0.1f, false, new Vector3(0.03f,0,0)) != 0 && 
                transform.position.x%1 > (Mathf.Floor(transform.position.x)+entityWidth-0.02f)%1 &&
                transform.position.x%1 < (Mathf.Floor(transform.position.x)+entityWidth+0.02f)%1
                
                
                )
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
            world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y, transform.position.z)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y + 1, transform.position.z)).BlockData.generateCollider ||
            
            // Blocks to the front and back
            extra && (
            !front(false) && (
            world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y, transform.position.z + entityWidth)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y + 1, transform.position.z + entityWidth)).BlockData.generateCollider) ||
            !back(false) && (
            world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y, transform.position.z - entityWidth)).BlockData.generateCollider ||
            world.GetBlock(new Vector3(transform.position.x - entityWidth-0.03f,  transform.position.y + 1, transform.position.z - entityWidth)).BlockData.generateCollider) ||
            
            // If we are crouching make sure we don't fall off the edge
            isCrouching && isGrounded && CheckDownCollision(0.1f, false, new Vector3(-0.03f, 0, 0)) != 0 &&
            transform.position.x % 1 > (Mathf.Floor(transform.position.x) - entityWidth - 0.02f) % 1 &&
            transform.position.x % 1 < (Mathf.Floor(transform.position.x) - entityWidth + 0.02f) % 1
            
            
            )
        )
            return true;
        else
        {
            return false;
        }
        
    }
    #endregion
}