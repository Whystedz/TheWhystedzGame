using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUnderground : MonoBehaviour
{
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private Transform respawnPoint;
    private float timer;
    private NetworkPlayerMovement playerMovement;

    private GameObject underground;
    [SerializeField] private float undergroundOffset;

    void Start()
    {
        this.timer = 0;
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();

        this.underground = GameObject.FindGameObjectWithTag("Underground");
        this.respawnPoint = GameObject.FindGameObjectWithTag("RespawnPoint").transform;
    }

    void Update()
    {
        if (this.playerMovement.IsMovementDisabled)
            return;

        if (this.transform.position.y <= this.underground.transform.position.y + undergroundOffset)
        {
            timer += Time.deltaTime;
            if (timer >= timeToDie)
                Die();

        } else
            timer = 0;

    }

    void Die()
    {
        this.transform.position = respawnPoint.position;
    }
}
