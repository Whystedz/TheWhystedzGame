using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float gameTime;
    private bool gameEnded;
    
    [SerializeField] private TextMeshProUGUI timerText;

    private EndScreenUI endScreen;

    private void Awake() => this.endScreen = GameObject.FindWithTag("EndScreen").GetComponent<EndScreenUI>();

    private void Start()
    {
        this.gameEnded = false;
        StartCoroutine(DoHalfTimeEvent(gameTime / 2));
    }

    void Update()
    {
        if (!this.gameEnded)
        {
            if (this.gameTime > 0)
            {
                this.gameTime -= Time.deltaTime;
                DisplayTimer(this.gameTime);
            }
            else
            {
                Debug.Log("Time ran out. Game ended!");
                this.gameTime = 0;
                DisplayTimer(this.gameTime);
                this.gameEnded = true;
                this.endScreen.EndGame();
            }
        }
    }

    private IEnumerator DoHalfTimeEvent(float time)
    {
        yield return new WaitForSeconds(time);
        AudioManager.PlayTenseMusic();
    }

    private void DisplayTimer(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        float milliseconds = (time % 1) * 1000;

        this.timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
