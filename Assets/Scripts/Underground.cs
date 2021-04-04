using UnityEngine;

public class Underground : MonoBehaviour
{
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private Transform respawnPoint;
    private float timer;
    private PlayerMovement playerMovement;

    private GameObject underground;
    [SerializeField] private float undergroundOffset;

    void Start()
    {
        this.timer = 0;
        this.playerMovement = this.GetComponent<PlayerMovement>();

        this.underground = GameObject.FindGameObjectWithTag("Underground");
    }

    // Update is called once per frame
    void Update()
    {
        if (this.playerMovement.IsMovementDisabled)
            return;

        if (this.underground != null && this.transform.position.y <= this.underground.transform.position.y + undergroundOffset)
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
