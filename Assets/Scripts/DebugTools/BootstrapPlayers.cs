using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootstrapPlayers : MonoBehaviour
{
    private float cooldown = 1f;
    private bool hasBootstrapped;

    [SerializeField] private Transform playersFolder;
    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private bool onlyEnableMainPlayer;

    void Update()
    {
        if (hasBootstrapped) return;

        UpdateCooldown();

        if (cooldown < 0)
            BootstrapPlayersInFolder();
    }

    private void UpdateCooldown()
    {
        cooldown -= Time.deltaTime;
    }

    private void BootstrapPlayersInFolder()
    {
        if (this.onlyEnableMainPlayer)
            EnableOnlyMainPlayer();
        else
            EnablePlayers();

        hasBootstrapped = true;
    }

    private void EnablePlayers()
    {
        if (this.playersFolder != null)
            foreach (Transform player in this.playersFolder)
                player.gameObject.SetActive(true);
    }

    private void EnableOnlyMainPlayer()
    {
        foreach (Transform player in this.playersFolder)
            player.gameObject.SetActive(false);
        mainPlayer.SetActive(true);
    }
}
