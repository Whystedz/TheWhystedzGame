using UnityEngine;

public class Underground : MonoBehaviour
{
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private Transform respawnPoint;
    private float timer;
    private PlayerMovement playerMovement;
    private LoseCrystals loseCrystals;
    private GameObject underground;
    [SerializeField] private float undergroundOffset;

    void Start()
    {
        this.timer = 0;
        this.playerMovement = this.GetComponent<PlayerMovement>();
        this.loseCrystals = this.GetComponent<LoseCrystals>();
        this.underground = GameObject.FindGameObjectWithTag("Underground");
    }

    // Update is called once per frame
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
