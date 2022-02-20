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

    private void Awake()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        playerInput.OnMouseClick += OnMouseClick;
        playerInput.OnFly += OnFly;
    }
    
    private void OnMouseClick()
    {
        
    }
    
    private void OnFly()
    {
        isFlying = !isFlying;
    }

    private void FixedUpdate()
    {
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
