using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Camera mainCam;
    private PlayerInput playerInput;
    private PlayerController playerController;
    
    public float playerReach = 4.5f;

    public bool isFlying;

    private bool waitForJumpDelay;

    private PlayerObjects objects;

    private World world;

    private void Awake()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
        objects = GetComponent<PlayerObjects>();
        world = FindObjectOfType<World>();
    }

    private void Start()
    {
        playerInput.OnMouseClick += OnMouseClick;
        playerInput.OnFly += OnFly;
    }
    
    private void OnMouseClick()
    {
        Ray playerRay = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(playerRay, out hit, playerReach, objects.groundMask))
        {
            world.SetBlock(hit, BlockType.Air);
        }
    }

    private void OnFly()
    {
        isFlying = !isFlying;
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            OnMouseClick();
        }
        
        if (isFlying)
        {
            playerController.Fly(playerInput.MovementInput, playerInput.IsJumpPressed, playerInput.IsRunPressed);
        }
        else
        {
            if (playerController.IsGrounded && playerInput.IsJumpPressed && !waitForJumpDelay)
            {
                waitForJumpDelay = true;
                StopAllCoroutines();
                StartCoroutine(WaitForJumpDelay());
            }

            playerController.HandleGravity(playerInput.IsJumpPressed);
            playerController.Move(playerInput.MovementInput, playerInput.IsRunPressed);
        }
    }

    private IEnumerator WaitForJumpDelay()
    {
        yield return new WaitForSeconds(0.1f);
        waitForJumpDelay = false;
    }
}
