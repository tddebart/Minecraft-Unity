using System.Collections;
using System.Collections.Generic;
using Mirror;
using Popcron;
using Steamworks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Gizmos = Popcron.Gizmos;

public class Player : BaseEntity
{
    [Space]
    public float sensitivity = 10f;
    public float checkIncrement = 0.1f;
    public float reach = 4.5f;
    
    [HideInInspector]
    public Transform cam;
    private PlayerObjects objects;
    private Animator animator;
    private PlayerInventory inventory;
    
    private bool leftMouseDown;
    private bool rightMouseDown;
    private const float breakDelay = 0.3f;
    private const float placeDelay = 0.21f;
    private IEnumerator breakCoroutine;
    private IEnumerator placeCoroutine;
    [HideInInspector]
    public bool f3KeyComboUsed;

    private MeshFilter[] meshFilters;
    private int blockLightLastFrame;
    private int skyLightLastFrame;
    
    [SyncVar] 
    public string PlayerName;
    
    private static readonly int Speed = Animator.StringToHash("speed");
    private static readonly int Sneaking = Animator.StringToHash("sneaking");

    private bool networkStarted => NetworkClient.active;
    
    // INFO:
    // 530ms from jumping to landing on the ground
    // 400ms to jump up one block

    public override void Start()
    {
        base.Start();
        
        Cursor.lockState = CursorLockMode.Locked;
        objects = GetComponent<PlayerObjects>();
        cam = objects.cam;
        inventory = GetComponent<PlayerInventory>();
        direction = objects.moveDirection;
        animator = GetComponent<Animator>();
        if (!Application.isPlaying) return;
        
        meshFilters = GetComponentsInChildren<MeshFilter>();
        SetMeshLight(15,15);

        if (isLocalPlayer)
        {
            CmdSetPlayerName(SteamClient.Name);
        }
        
        objects.playerName.text = PlayerName;

        if (!isLocalPlayer && networkStarted)
        {
            inventory.enabled = false;
            cam.gameObject.SetActive(false);
            foreach (var obj in GetComponentsInChildren<Transform>())
            {
                obj.gameObject.layer = LayerMask.NameToLayer("Default");
            }
            return;
        }

        if (SettingsManager.instance != null)
        {
            cam.GetComponent<Camera>().fieldOfView = SettingsManager.instance.fov;
        }

        GameManager.Instance.localPlayer = this;
        GameManager.Instance.playerSpawned = true;
        GameManager.Instance.StartCheckingForChunks();
        
        GameObject.Find("Rendering/Camera").SetActive(false);
        
        GameObject.Find("Rendering").transform.GetChild(2).gameObject.SetActive(false);
        this.ExecuteAfterFrames(1, () =>
        {
            GameObject.Find("Rendering").transform.GetChild(2).gameObject.SetActive(true);
        });

    }

    public void SetMeshLight(int skyLight, int blockLight)
    {
        foreach (var meshFilter in meshFilters)
        {
            var orgMesh = meshFilter.mesh;
            var vertexCount = orgMesh.vertexCount;
            var lightArray = new Vector2[vertexCount];
            for (var i = 0; i < vertexCount; i++)
            {
                lightArray[i] = new Vector2(skyLight,blockLight);
            }

            var mesh = new Mesh
            {
                vertices = orgMesh.vertices,
                uv = orgMesh.uv,
                triangles = orgMesh.triangles
            };
            mesh.SetUVs(1,lightArray);
            meshFilter.mesh = mesh;
        }
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        PlayerName = name;
        RpcUpdatePlayerNameText();
    }

    [ClientRpc]
    public void RpcUpdatePlayerNameText()
    {
        objects.playerName.text = PlayerName;
    }

    private void OnEnable()
    {
        Gizmos.CameraFilter += cam => cam.transform == this.cam;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public bool BreakBlock()
    {
        var targetedBlock = TargetedBlock(reach, out _);
        if (targetedBlock != null)
        {
            var blockPos = targetedBlock.section.dataRef.GetGlobalBlockCoords(targetedBlock.position);
            blockPos.y += targetedBlock.section.yOffset;
            inventory.AddItem(targetedBlock.type);
            if (networkStarted)
            {
                NetworkClient.Send(new WorldServer.SetBlockMessage(blockPos,BlockType.Air));
            }
            else
            {
                world.SetBlock(blockPos, BlockType.Air);
            }
            return true;
        }
        else
        {
            return false;
        }
        
        // if (hit.distance > reach) return;
        // if (block.breakSound != null)
        // {
        //     AudioSource.PlayClipAtPoint(block.breakSound, objects.transform.position);
        // }
        // targetedBlock.section.dataRef.SetBlock(blockPos, null);
        // if (block.breakParticles != null)
        // {
        //     var particles = Instantiate(block.breakParticles, blockPos, Quaternion.identity);
        //     Destroy(particles, particles.main.duration);
        // }
    }
    
    public bool PlaceBlock()
    {
        var type = inventory.slots[inventory.selectedSlotIndex].type;
        if (type != BlockType.Nothing)
        {
            var targetedBlock = TargetedBlock(reach, out var blockPos);
            if (targetedBlock != null)
            {
                // Check if the block is not in the player
                if (!IsPlayerStandingIn(blockPos))
                {
                    if (networkStarted)
                    {
                        NetworkClient.Send(new WorldServer.SetBlockMessage(blockPos, type));
                    }
                    else
                    {
                        world.SetBlock(blockPos, type);
                    }
                    inventory.RemoveHeldItem();
                    return true;
                }
                
            }
        }
        return false;
    }

    public bool IsPlayerStandingIn(Vector3 posU)
    {
        var pos = Vector3Int.FloorToInt(posU);
        var blockPoss = new List<Vector3Int>();

        var position = transform.position;
        blockPoss.Add(Vector3Int.FloorToInt(new Vector3(position.x - entityWidth, position.y, position.z - entityWidth)));
        blockPoss.Add( Vector3Int.FloorToInt(new Vector3(position.x - entityWidth, position.y + 1, position.z - entityWidth)));
        
        blockPoss.Add(Vector3Int.FloorToInt(new Vector3(position.x + entityWidth, position.y, position.z - entityWidth)));
        blockPoss.Add(Vector3Int.FloorToInt(new Vector3(position.x + entityWidth, position.y + 1, position.z - entityWidth)));
        
        blockPoss.Add(Vector3Int.FloorToInt(new Vector3(position.x - entityWidth, position.y, position.z + entityWidth)));
        blockPoss.Add(Vector3Int.FloorToInt(new Vector3(position.x - entityWidth, position.y + 1, position.z + entityWidth)));
        
        blockPoss.Add(Vector3Int.FloorToInt(new Vector3(position.x + entityWidth, position.y, position.z + entityWidth)));
        blockPoss.Add(Vector3Int.FloorToInt(new Vector3(position.x + entityWidth, position.y + 1, position.z + entityWidth)));
        
        return blockPoss.Contains(pos);
    }

    public Block TargetedBlock(float tReach,out Vector3Int lastGlobalPos)
    {
        lastGlobalPos = Vector3Int.zero;
        var step = checkIncrement;
        while (step < tReach)
        {
            var pos = cam.position + cam.forward * step;
            var block = world.GetBlock(pos);
            if (block == null) return null;
            if (BlockDataManager.blockTypeDataDictionary[(int)block.type].generateCollider)
            {
                return block;
            }

            // lastGlobalPos = Vector3Int.FloorToInt(pos + FindNormal(pos, lastGlobalPos));
            lastGlobalPos = Vector3Int.FloorToInt(pos);
            
            step += checkIncrement;
        }
        return null;
    }

    // I am a fucking genius, I just fully came up with this shit
    // Loop through all vector directions and return the one that has less then 90 degrees between the camera and the block
    public Vector3 FindNormal(Vector3 pos, Vector3 lastPos)
    {
        var directions = new Vector3Int[]
        {
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.down
        };
        
        var angle = Vector3.Angle(pos, lastPos);
        
        var normal = Vector3Int.zero;
        
        foreach (var dir in directions)
        {
            var angleToDir = Vector3.Angle(lastPos - pos, dir);
            if (angleToDir < angle)
            {
                normal = dir;
                angle = angleToDir;
            }
        }
        
        return normal;
    }

    public override void Move()
    {
        base.Move();
        DoBodyRotation();
    }

    private void DoBodyRotation()
    {
        if (!Application.isPlaying) return;
        
        // if we move forward or backwards, we want to rotate the body to face the direction we are moving
        if (vertical != 0 && objects.body.localRotation.eulerAngles.x != Vector3.SignedAngle(objects.body.forward, objects.moveDirection.forward, Vector3.up))
        {
            RotateBody(Vector3.SignedAngle(objects.body.forward, objects.moveDirection.forward, Vector3.up)*(Time.deltaTime*6));
        }
        
        // if we move to the left, we want to rotate the body 45 degrees to the left
        if (horizontal < 0 && objects.body.localRotation.eulerAngles.x != Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(-objects.moveDirection.right,objects.moveDirection.forward,0.5f), Vector3.up))
        {
            RotateBody(Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(-objects.moveDirection.right,objects.moveDirection.forward,0.46f), Vector3.up)*(Time.deltaTime*10));
        }
        
        // if we move to the right, we want to rotate the body 45 degrees to the right
        if (horizontal > 0 && objects.body.localRotation.eulerAngles.x != Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(objects.moveDirection.right,objects.moveDirection.forward,0.5f), Vector3.up))
        {
            RotateBody(Vector3.SignedAngle(objects.body.forward, Vector3.Lerp(objects.moveDirection.right,objects.moveDirection.forward,0.46f), Vector3.up)*(Time.deltaTime*10));
        }
    }

    private void DoHeadRotation()
    {
        var headRotation = objects.head.localRotation;
        var verticalRotation = headRotation.eulerAngles.x;
        var horizontalRotation = headRotation.eulerAngles.y;

        verticalRotation += -mouseY*sensitivity;
        horizontalRotation += mouseX*sensitivity;

        verticalRotation = verticalRotation switch
        {
            > 89 and < 200 => 89,
            < 271 and > 200 => 271,
            _ => verticalRotation
        };

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
        objects.cam.localRotation = headRotation;

        // body.Rotate(Vector3.up, mouseX);
        
        objects.moveDirection.localRotation = Quaternion.Euler(0, horizontalRotation, 0);
    }

    public override void Update()
    {
        base.Update();
        if (Application.isPlaying)
        {
            var blockLight = World.Instance.GetBlock(transform.position).GetBlockLight();
            var skyLight = World.Instance.GetBlock(transform.position).GetSkyLight();
            
            // Do mesh lighting
            if (blockLightLastFrame != blockLight || skyLightLastFrame != skyLight)
            {
                SetMeshLight(skyLight,blockLight);
            }
            blockLightLastFrame = blockLight;
            skyLightLastFrame = skyLight;   
        }

        
        if (!Application.isPlaying || !isLocalPlayer && networkStarted) return;
        GetPlayerInput();
        var vel = new Vector3(velocity.x, 0, velocity.z);
        animator.SetFloat(Speed, (vel.magnitude/Time.fixedDeltaTime)/walkSpeed);
        if (!isFlying)
        {
            animator.SetBool(Sneaking, isCrouching);
        }
        DoHeadRotation();

        var targetedBlock = TargetedBlock(reach, out _);
        if (targetedBlock != null)
        {
            var blockPos = targetedBlock.section.dataRef.GetGlobalBlockCoords(targetedBlock.position);
            blockPos.y += targetedBlock.section.yOffset;
            Gizmos.Draw<CubeDrawer>(Color.black, false, blockPos+ new Vector3(0.5f,0.5f,0.5f),Quaternion.identity, Vector3.one*1.01f);
        }
    }

    // When the body is rotated, the head needs to be rotated as well to keep the head in the same position
    private void RotateBody(float bodyYawRotation)
    {
        objects.body.Rotate(objects.body.up, bodyYawRotation);

        var headRotation = objects.head.localRotation.eulerAngles;
        headRotation += new Vector3(0, -bodyYawRotation, 0);
        objects.head.localRotation = Quaternion.Euler(headRotation);
    }
    
    public IEnumerator BreakLoop()
    {
        while (true)
        {
            if (!Input.GetMouseButton(0))
            {
                break;
            }
            if (BreakBlock())
            {
                yield return new WaitForSeconds(breakDelay);
            }
            else
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
    
    public IEnumerator PlaceLoop()
    {
        while (true)
        {
            if (!Input.GetMouseButton(1))
            {
                break;
            }
            if (PlaceBlock())
            {
                yield return new WaitForSeconds(placeDelay);
            } else
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private void GetPlayerInput()
    {
        if (inventory.isOpen)
        {
            horizontal = 0;
            vertical = 0;
            mouseX = 0;
            mouseY = 0;
            
            isCrouching = false;
            jumpRequest = false;
            
            return;
        }
        
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
        
        if (Input.GetMouseButtonUp(0))
        {
            if (breakCoroutine != null)
            {
                StopCoroutine(breakCoroutine);
                breakCoroutine = null;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit ray;
            if (Physics.Raycast(objects.cam.position, objects.cam.forward, out ray, reach, LayerMask.GetMask("Default")))
            {
                ray.transform.GetComponentInParent<Player>().CmdAddForce(ray.transform.position -transform.position + Vector3.up, 12);
            }
            else if(breakCoroutine == null)
            {
                breakCoroutine = BreakLoop();
                StartCoroutine(breakCoroutine);
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (placeCoroutine != null)
            {
                StopCoroutine(placeCoroutine);
                placeCoroutine = null;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (placeCoroutine == null)
            {
                placeCoroutine = PlaceLoop();
                StartCoroutine(placeCoroutine);
            }
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

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (Input.GetKey(KeyCode.F3))
            {
                foreach (var entity in FindObjectsOfType<BaseEntity>())
                {
                    entity.drawBounds = !entity.drawBounds;
                }
                f3KeyComboUsed = true;
            }
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
                StopCoroutine(nameof(WaitOnJumpDelay));
            }
            if(Input.GetButtonUp("Jump"))
            {
                jumpRequest = false;
            }
        }
    }
    
    
}