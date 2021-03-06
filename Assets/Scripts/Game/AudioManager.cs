using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager current;

    public bool DebugPlayOnStart = false;
    
    [Header("Audio FX")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float initialCutoff = 22000f;
    [SerializeField] private float muffledCutoff = 780f;
    [SerializeField] private float tenseMusicSpeed = 1.1f;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip mainMusicClip;
    [SerializeField] private AudioClip lobbyClip;
    [SerializeField] private AudioClip winClip;
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup ambientGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    private AudioSource ambientSource;
    private AudioSource musicSource;

    private void Awake()
    {
        // Keep 1 instance of the Audio Manager at all times.
        if (current != null && current != this)
            Destroy(gameObject);

        current = this;
        //DontDestroyOnLoad(gameObject); TODO
        
        // Generate the Audio source channels.
        this.ambientSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        this.musicSource = gameObject.AddComponent<AudioSource>() as AudioSource;

        this.ambientSource.volume = 0f;
        this.musicSource.volume = 0f;
        
        this.ambientSource.outputAudioMixerGroup = ambientGroup;
        this.musicSource.outputAudioMixerGroup = musicGroup;

        // Set initial values.
        this.mainMixer.GetFloat("musicCutoff", out initialCutoff);
        this.musicSource.pitch = 1f;
        
        this.ambientSource.clip = this.ambientClip;
        this.ambientSource.loop = true;
        this.ambientSource.Play();
        StopAmbientAudio();
        
        if (this.DebugPlayOnStart)
            PlayMainMusic();
    }

    public static void PlayAmbientAudio()
    {
        FadeAudio(current.ambientSource, current.fadeDuration, 1f);
    }
    
    public static void StopAmbientAudio()
    {
        FadeAudio(current.ambientSource, current.fadeDuration, 0f);
    }

    public static void PlayMainMusic() => PlayMusic(current.mainMusicClip);
    
    public static void PlaySpeedupMusic() => current.musicSource.pitch = current.tenseMusicSpeed;
    
    public static void StopSpeedupMusic() => current.musicSource.pitch = 1f;

    public static void PlayLobbyMusic() => PlayMusic(current.lobbyClip);

    public static void PlayWinMusic()
    {
        PlayMusic(current.winClip);
        current.musicSource.loop = false;
    }

    private static void PlayMusic(AudioClip musicClip)
    {
        if (current == null)
            return;

        if (current.musicSource.isPlaying)
            ChangeMusic(musicClip);
        else
        {
            current.musicSource.clip = musicClip;
            current.musicSource.loop = true;
            current.musicSource.Play();
            FadeAudio(current.musicSource, current.fadeDuration, 1f);
        }
    }

    public static void PlayUndergroundFX() 
        => current.StartCoroutine(MuffleMusicCo(current.fadeDuration, current.muffledCutoff));
    
    public static void StopUndergroundFX() 
        => current.StartCoroutine(MuffleMusicCo(current.fadeDuration, current.initialCutoff));

    private static IEnumerator MuffleMusicCo(float duration, float targetCutoff)
    {
        var currentTime = 0f;
        current.mainMixer.GetFloat("musicCutoff", out var startCutoff);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            current.mainMixer.SetFloat("musicCutoff", 
                Mathf.Lerp(startCutoff, targetCutoff, currentTime / duration));
            yield return null;
        }
    }
    
    private static void ChangeMusic(AudioClip newMusic) => current.StartCoroutine(ChangeMusicCo(newMusic));
    
    private static IEnumerator ChangeMusicCo(AudioClip newMusic)
    {
        FadeAudio(current.musicSource, current.fadeDuration, 0f);

        // Wait for fade out to finish.
        while (current.musicSource.volume > 0)
            yield return new WaitForSeconds(0.01f);
        
        current.musicSource.Stop();
        current.musicSource.clip = newMusic;
        current.musicSource.loop = true;
        current.musicSource.Play();
        FadeAudio(current.musicSource, current.fadeDuration, 1f);
    }
    
    private static void FadeAudio(AudioSource audioSource, float fadeDuration, float targetVolume) 
        => current.StartCoroutine(FadeAudioCo(audioSource, fadeDuration, targetVolume));
    
    private static IEnumerator FadeAudioCo(AudioSource audioSource, float fadeDuration, float targetVolume)
    {
        var currentTime = 0f;
        var startVolume = audioSource.volume;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeDuration);
            yield return null;
        }
    }
    
    public static void ChangeAmbientVolume(float volume) => current.mainMixer.SetFloat("ambientVolume", volume);
    
    public static void ChangeMusicVolume(float volume) => current.mainMixer.SetFloat("ambientVolume", volume);
}
