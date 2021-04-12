using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip[] laserClips;
    [SerializeField] private AudioClip[] collectClips;
    [SerializeField] private AudioClip ropeClip;
    [SerializeField] private AudioClip fallingClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip dieClip;
    [SerializeField] private AudioClip climbClip;
    
    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup playerGroup;
    [SerializeField] private AudioSource audioSource;

    private void Awake() => this.audioSource.outputAudioMixerGroup = this.playerGroup;

    // [ClientRpc]
    public void PlayFootstepAudio()
    {
        if (this.audioSource.isPlaying)
            return;

        var index = Random.Range(0, this.footstepClips.Length);

        this.audioSource.clip = this.footstepClips[index];
        this.audioSource.Play();
    }
    
    // [ClientRpc]
    public void PlayLaserAudio()
    {
        var index = Random.Range(0, this.laserClips.Length);

        this.audioSource.PlayOneShot(this.laserClips[index]);
    }
    
    // [ClientRpc]
    public void PlayCollectAudio()
    {
        var index = Random.Range(0, this.collectClips.Length);

        this.audioSource.PlayOneShot(this.collectClips[index]);
    }
    
    // [ClientRpc]
    public void PlayRopeAudio() => this.audioSource.PlayOneShot(this.ropeClip);
    
    // [ClientRpc]
    public void PlayFallingAudio() => this.audioSource.PlayOneShot(this.fallingClip);
    
    // [ClientRpc]
    public void PlayLandAudio() => this.audioSource.PlayOneShot(this.landClip);
    
    // [ClientRpc]
    public void PlayDieAudio() => this.audioSource.PlayOneShot(this.dieClip);
    
    // [ClientRpc]
    public void PlayClimbAudio() => this.audioSource.PlayOneShot(this.climbClip);
}
