using UnityEngine;

public class AnimationAudio : MonoBehaviour
{
    private NetworkPlayerMovement playerMovement;
    
    private PlayerAudio playerAudio;

    void Awake()
    {
        playerMovement = transform.parent.GetComponent<NetworkPlayerMovement>();
        this.playerAudio = transform.parent.GetComponent<PlayerAudio>();
    }

    public void PlayFootstepAudio() => this.playerAudio.PlayFootstepAudio();

    public void DisableMovement() => this.playerMovement.DisableMovement();
    public void EnableMovement() => this.playerMovement.EnableMovement();

    public void EnableInput() => NetworkInputManager.Instance.EnableInput();
    public void DisableInput() => NetworkInputManager.Instance.DisableInput();

    public void EnableInputAndMovement()
    {
        EnableInput();
        EnableMovement();
    }

    public void DisableInputAndMovement()
    {
        DisableInput();
        DisableMovement();
    }
}
