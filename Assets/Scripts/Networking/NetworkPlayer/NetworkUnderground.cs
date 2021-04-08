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

    private NetworkLoseCrystals loseCrystals;

    void Start()
    {
        this.timer = 0;
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();

        this.respawnPoint = this.transform;
        this.loseCrystals = this.GetComponent<NetworkLoseCrystals>();
        this.underground = GameObject.FindGameObjectWithTag("Underground");
    }

    void Update()
    {
        if (this.playerMovement.IsMovementDisabled)
            return;

        if (this.transform.position.y <= this.underground.transform.position.y + undergroundOffset)
        {
            this.timer += Time.deltaTime;
            if (this.timer >= this.timeToDie)
                Die();

        } else
            this.timer = 0;

    }

    void Die()
    {
        this.loseCrystals.LoseCrystal();
        this.transform.position = respawnPoint.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Underground"))
            this.loseCrystals.LoseCrystal();
    }
}
