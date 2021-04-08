using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [SerializeField] protected VideoPlayer videoPlayer;
    public VideoPlayer VideoPlayer => this.videoPlayer;

    protected bool isDonePlaying;
    public bool IsDonePlaying => this.isDonePlaying;
    public bool IsPlaying => this.videoPlayer.isPlaying;
    public bool IsLooping => this.videoPlayer.isLooping;
    public bool IsPrepared => this.videoPlayer.isPrepared;
    public double Time => this.videoPlayer.time;
    public ulong Duration => (ulong) (this.videoPlayer.frameCount / this.videoPlayer.frameRate);
    public double TimeProgress => Time / Duration;

    public void LoadVideo(VideoClip videoClip)
    {
        if(this.videoPlayer.clip == videoClip)
            return;

        this.videoPlayer.clip = videoClip;
        this.videoPlayer.Prepare();
    }
    
    public void PlayVideo()
    {
        if(!IsPrepared)
            return;
        
        this.videoPlayer.Play();
    }
    
    public void PauseVideo()
    {
        if (!IsPlaying)
            return;
        
        this.videoPlayer.Pause();
    }
    
    public void RestartVideo()
    {
        if (!IsPlaying)
            return;

        PauseVideo();
        Seek(0);
    }
    
    public void LoopVideo(bool loopEnabled)
    {
        if (!IsPrepared)
            return;

        this.videoPlayer.isLooping = loopEnabled;
    }
    
    public void Seek(float timeProgress)
    {
        if (!IsPrepared || !this.videoPlayer.canSetTime)
            return;

        timeProgress = Mathf.Clamp(timeProgress, 0, 1);
        this.videoPlayer.time = timeProgress * Duration;
    }
    
    public void IncrementPlaybackSpeed()
    {
        if (!videoPlayer.canSetPlaybackSpeed)
            return;

        var playbackSpeed = this.videoPlayer.playbackSpeed;
        playbackSpeed += 1;
        this.videoPlayer.playbackSpeed = playbackSpeed;
        this.videoPlayer.playbackSpeed = Mathf.Clamp(playbackSpeed,0,10);
    }
    
    public void DecrementPlaybackSpeed()
    {
        if (!videoPlayer.canSetPlaybackSpeed)
            return;

        var playbackSpeed = this.videoPlayer.playbackSpeed;
        playbackSpeed -= 1;
        this.videoPlayer.playbackSpeed = playbackSpeed;
        this.videoPlayer.playbackSpeed = Mathf.Clamp(playbackSpeed,0,10);
    }
    
    protected virtual void ErrorReceived(VideoPlayer video, string message) => Debug.Log($"VIDEO PLAYER ERROR: {message}");
    
    protected virtual void LoopPointReached(VideoPlayer video)
    {
        Debug.Log("Video player loop point reached.");
        this.isDonePlaying = true;
    }
    
    protected virtual void PrepareCompleted(VideoPlayer video)
    {
        Debug.Log("Video player preparation completed.");
        this.isDonePlaying = false;
    }
    
    protected virtual void SeekCompleted(VideoPlayer video)
    {
        Debug.Log("Video player seeking completed.");
        this.isDonePlaying = false;
    }
    
    protected virtual void Started(VideoPlayer video) => Debug.Log("Video player started.");
    
    protected void OnEnable()
    {
        this.videoPlayer.errorReceived += ErrorReceived;
        this.videoPlayer.loopPointReached += LoopPointReached;
        this.videoPlayer.prepareCompleted += PrepareCompleted;
        this.videoPlayer.seekCompleted += SeekCompleted;
        this.videoPlayer.started += Started;
    }

    protected void OnDisable()
    {
        this.videoPlayer.errorReceived -= ErrorReceived;
        this.videoPlayer.loopPointReached -= LoopPointReached;
        this.videoPlayer.prepareCompleted -= PrepareCompleted;
        this.videoPlayer.seekCompleted -= SeekCompleted;
        this.videoPlayer.started -= Started;
    }
}
