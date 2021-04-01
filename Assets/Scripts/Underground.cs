using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underground : MonoBehaviour
{
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private int undergroundYLevel = -68;
    [SerializeField] private Vector3 respawnPosition;
    private float timer;
    private PlayerMovement playerMovement;
    // Start is called before the first frame update
    void Start()
    {
        this.timer = 0;
        this.playerMovement = this.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (this.playerMovement.isMovementDisabled)
            return;
        if (this.transform.position.y <= undergroundYLevel)
        {

            timer += Time.deltaTime;
            if (timer >= timeToDie)
                Die();

        } else
            timer = 0;

    }

    void Die()
    {
        this.transform.position = respawnPosition;
    }
}
