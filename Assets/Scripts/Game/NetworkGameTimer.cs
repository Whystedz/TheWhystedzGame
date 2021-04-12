using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

public class NetworkGameTimer : MonoBehaviour
{
    [SerializeField] private double gameTime;
    private double startTime;

    private double timeLeft;
    private bool gameEnded;

    // sync timer after some frames
    private const int timerSyncIntervalFrameNumber = 100;

    private int framePassed = 0;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        this.gameEnded = false;
        this.timeLeft = this.gameTime; // this line is to give time for matchController to load
        StartCoroutine(DoHalfTimeEvent((float) gameTime / 2));
    }

    public void StartTimerFrom(double start)
    {
        this.startTime = start;
        this.timeLeft = this.gameTime + this.startTime - NetworkTime.time;
    }

    void Update()
    {
        this.framePassed++;
        if (!this.gameEnded)
        {
            if (this.timeLeft > 0)
            {
                if (this.framePassed > timerSyncIntervalFrameNumber)
                {
                    this.timeLeft = this.gameTime + this.startTime - NetworkTime.time;
                }
                else
                {
                    this.timeLeft -= Time.deltaTime;
                }
                DisplayTimer((float) this.timeLeft);
            }
            else
            {
                Debug.Log("Time ran out. Game ended!");
                this.timeLeft = 0;
                DisplayTimer((float) this.gameTime);
                this.gameEnded = true;
                AudioManager.StopSpeedupMusic();
            }
        }
    }

    private IEnumerator DoHalfTimeEvent(float time)
    {
        yield return new WaitForSeconds(time);
        AudioManager.PlaySpeedupMusic();
    }

    private void DisplayTimer(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        this.timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
