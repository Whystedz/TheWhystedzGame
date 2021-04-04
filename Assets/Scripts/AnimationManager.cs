using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    private Animator animator;

    private void Awake()
    {
        this.animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        if (this.characterController == null)
            this.characterController = transform.parent.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Mathf.Abs(characterController.velocity.x) > 0 || Mathf.Abs(characterController.velocity.z) > 0)
            animator.SetBool("isRunning", true);
        else
            animator.SetBool("isRunning", false);
    }
}
