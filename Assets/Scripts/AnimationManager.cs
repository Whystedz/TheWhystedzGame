using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    private InputManager inputManager;
    private PlayerMovement playerMovement;
    private Animator animator;

    private bool fallingTrigger = false;

    private void Awake()
    {
        this.animator = GetComponent<Animator>();
        if (this.characterController == null)
            this.characterController = transform.parent.GetComponent<CharacterController>();
    }
    
    void Start()
    {
        this.inputManager = InputManager.GetInstance();
        this.playerMovement = transform.parent.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!this.playerMovement.IsInUnderground && (animator.GetCurrentAnimatorStateInfo(0).IsName("IdleUnderGround") || animator.GetCurrentAnimatorStateInfo(0).IsName("RunningUnderGround")))
            this.animator.SetTrigger("Reset");
            

        if (this.playerMovement.IsFalling() && !this.fallingTrigger && this.playerMovement.IsInUnderground)
        {
            this.fallingTrigger = true;
            this.animator.SetTrigger("Fall");
        }
        else if (!this.playerMovement.IsFalling())
            this.fallingTrigger = false;


        if (this.playerMovement.IsClimbing)
            animator.SetBool("isClimbing", true);
        else
            this.animator.SetBool("isClimbing", false);


        if (this.inputManager.GetDigging() && !this.playerMovement.IsInUnderground)
            this.animator.SetTrigger("Shoot");
        

        if (Mathf.Abs(this.characterController.velocity.x) > 0 || Mathf.Abs(this.characterController.velocity.z) > 0)
            this.animator.SetBool("isRunning", true);
        else
            this.animator.SetBool("isRunning", false);
    }
}
