using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class VideoTutorialUI : VideoManager
{
    public static string TUTORIAL_KEY = "TutorialSeen";
    
    [Header("UI")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject closeButton;

    [Header("Video Player")]
    [SerializeField] private VideoClip[] videos;
    [Range(0,1)]
    [SerializeField] private float volume;
    private int currentVideoIndex;
    public int CurrentVideoIndex => this.currentVideoIndex;

    private bool tutorialSeen;

    private void Awake()
    {
        // 1 = player has seen the tutorial before. 0 = player has not seen it.
        this.tutorialSeen = PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 1;
    }

    private void Start()
    {
        this.videoPlayer.SetDirectAudioVolume(0, this.volume);
        
        this.currentVideoIndex = 0;
        
        LoadVideo(this.videos[this.currentVideoIndex]);
        RefreshUI();
    }

    protected override void LoopPointReached(VideoPlayer video)
    {
        base.LoopPointReached(video);
        PlayNextVideo();
    }

    protected override void PrepareCompleted(VideoPlayer video)
    {
        base.PrepareCompleted(video);
        PlayVideo();
    }

    public void PlayNextVideo()
    {
        if (this.currentVideoIndex + 1 >= this.videos.Length)
            return;

        this.currentVideoIndex++;
        LoadVideo(this.videos[this.currentVideoIndex]);

        if (this.currentVideoIndex == this.videos.Length - 1)
        {
            PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
            this.tutorialSeen = true;
        }

        RefreshUI();
    }
    
    public void PlayPreviousVideo()
    {
        if (this.currentVideoIndex - 1 < 0)
            return;

        this.currentVideoIndex--;
        LoadVideo(this.videos[this.currentVideoIndex]);
        
        RefreshUI();
    }

    public void CloseTutorial() => this.gameObject.SetActive(false);
    
    private void RefreshUI()
    {
        if (!tutorialSeen)
        {
            nextButton.SetActive(false);
            previousButton.SetActive(false);
            closeButton.SetActive(false);
            return;
        }
        
        closeButton.SetActive(true);
        if (currentVideoIndex == 0)
        {
            nextButton.SetActive(true);
            previousButton.SetActive(false);
        }
        else if (currentVideoIndex == videos.Length - 1)
        {
            nextButton.SetActive(false);
            previousButton.SetActive(true);
        }
        else
        {
            nextButton.SetActive(true);
            previousButton.SetActive(true);
        }
        
        // Handle event system for controller support.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(nextButton.activeSelf ? nextButton : previousButton);
    }

    private void ResetTutorialSeen()
    {
        PlayerPrefs.SetInt(TUTORIAL_KEY, 0);
        this.tutorialSeen = false;
    }
}
