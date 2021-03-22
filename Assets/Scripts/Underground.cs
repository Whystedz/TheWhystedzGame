using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underground : MonoBehaviour
{
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private int undergroundYLevel = -68;
    [SerializeField] private Vector3 respawnPosition;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
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
