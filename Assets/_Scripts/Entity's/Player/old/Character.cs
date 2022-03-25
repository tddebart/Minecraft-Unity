using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Camera mainCam;
    private PlayerInput playerInput;
    private OLD_PlayerController _oldPlayerController;
    
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
        _oldPlayerController = GetComponent<OLD_PlayerController>();
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
        if (Physics.Raycast(playerRay, out hit, playerReach))
        {
            // world.SetBlock(hit, BlockType.Air);
        }
    }

    private void OnFly()
    {
        isFlying = !isFlying;
    }

    private void FixedUpdate()
    {

        if (isFlying)
        {
            _oldPlayerController.Fly(playerInput.MovementInput, playerInput.IsJumpPressed, playerInput.IsRunPressed);
        }
        else
        {
            if (_oldPlayerController.IsGrounded && playerInput.IsJumpPressed && !waitForJumpDelay)
            {
                waitForJumpDelay = true;
                StopAllCoroutines();
                StartCoroutine(WaitForJumpDelay());
            }

            _oldPlayerController.HandleGravity(playerInput.IsJumpPressed);
            _oldPlayerController.Move(playerInput.MovementInput, playerInput.IsRunPressed);
        }
    }

    private IEnumerator WaitForJumpDelay()
    {
        yield return new WaitForSeconds(0.1f);
        waitForJumpDelay = false;
    }
}
