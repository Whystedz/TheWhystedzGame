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
    
    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup playerGroup;
    [SerializeField] private AudioSource audioSource;

    private void Awake() => audioSource.outputAudioMixerGroup = playerGroup;

    // [ClientRpc]
    public void PlayFootstepAudio()
    {
        if (audioSource.isPlaying)
            return;

        var index = Random.Range(0, footstepClips.Length);

        audioSource.clip = footstepClips[index];
        audioSource.Play();
    }
    
    // [ClientRpc]
    public void PlayLaserAudio()
    {
        var index = Random.Range(0, laserClips.Length);

        audioSource.PlayOneShot(laserClips[index]);
    }
    
    // [ClientRpc]
    public void PlayCollectAudio()
    {
        var index = Random.Range(0, collectClips.Length);

        audioSource.PlayOneShot(collectClips[index]);
    }
    
    // [ClientRpc]
    public void PlayRopeAudio() => audioSource.PlayOneShot(ropeClip);
    
    // [ClientRpc]
    public void PlayFallingAudio() => audioSource.PlayOneShot(fallingClip);
    
    // [ClientRpc]
    public void PlayLandAudio() => audioSource.PlayOneShot(landClip);
}
