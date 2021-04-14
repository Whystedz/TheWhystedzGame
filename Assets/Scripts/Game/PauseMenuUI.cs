using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    private bool isPaused;

    [SerializeField] private GameObject pauseUI;
    
    private void Start() => this.isPaused = false;

    void Update()
    {
        if (NetworkInputManager.Instance.GetMainMenu())
        {
            if(this.isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        this.pauseUI.SetActive(true);
        this.isPaused = true;
    }

    public void Resume()
    {
        this.pauseUI.SetActive(false);
        this.isPaused = false;
    }

    public void ExitGame() => Application.Quit();
}
