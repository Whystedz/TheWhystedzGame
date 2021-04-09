using UnityEngine;

public class NetworkUnderground : MonoBehaviour
{
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private Transform respawnPoint;
    private float timer;
    private NetworkPlayerMovement playerMovement;
    private NetworkLoseCrystals loseCrystals;

    private GameObject surface;
    private GameObject underground;
    [SerializeField] private float undergroundOffset;

    private CharacterController characterController;

    void Start()
    {
        this.timer = 0;
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();
        this.loseCrystals = this.GetComponent<NetworkLoseCrystals>();

        this.respawnPoint = this.transform;
        this.surface = GameObject.FindGameObjectWithTag("Surface");
        this.characterController = this.GetComponent<CharacterController>();
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
        var initialPosition = this.transform.position;

        this.loseCrystals.LoseCrystal();

        var offset = initialPosition.y - this.underground.transform.position.y;
        var revivedPosition = new Vector3(
            respawnPoint.position.x,
            this.surface.transform.position.y + offset,
            respawnPoint.position.z);

        this.characterController.enabled = false;
        this.transform.position = revivedPosition;
        this.characterController.enabled = true;

        this.playerMovement.IsInUnderground = false;

        this.playerMovement.RefreshTileCurrentlyOn();
    }
}
