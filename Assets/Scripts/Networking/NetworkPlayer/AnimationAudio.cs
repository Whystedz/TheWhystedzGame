using UnityEngine;

public class AnimationAudio : MonoBehaviour
{
    private PlayerAudio playerAudio;

    void Awake() => this.playerAudio = transform.parent.GetComponent<PlayerAudio>();

    public void PlayFootstepAudio() => this.playerAudio.PlayFootstepAudio();
}
