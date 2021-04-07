using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    private InputManager inputManager;
    private PlayerMovement playerMovement;
    private PlayerAudio playerAudio;
    private Animator animator;

    private bool fallingTrigger = false;
    private bool isRunning = false;

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
        this.playerAudio = transform.parent.GetComponent<PlayerAudio>();
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


        this.animator.SetBool("isClimbing", this.playerMovement.IsClimbing);


        if (this.inputManager.GetDigging() && !this.playerMovement.IsInUnderground)
            this.animator.SetTrigger("Shoot");
        

        this.isRunning = (Mathf.Abs(this.characterController.velocity.x) > 0 || Mathf.Abs(this.characterController.velocity.z) > 0) ? true : false;
        this.animator.SetBool("isRunning", isRunning);

    }

    public void PlayFootstepAudio() => this.playerAudio.PlayFootstepAudio();
}