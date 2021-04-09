using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkUnderground : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private Transform respawnPoint;
    private float timer;
    private NetworkPlayerMovement playerMovement;
    private NetworkLoseCrystals loseCrystals;

    private GameObject surface;
    private GameObject underground;
    [SerializeField] private float undergroundOffset;

    private CharacterController characterController;

    [SerializeField] private GameObject undergroundCanvas;
    private Image undergroundBackground;
    private Image undergroundImage;
    [SerializeField] private Color fullColor;
    [SerializeField] private Color halfColor;
    [SerializeField] private Color urgentColor;

    void Start()
    {
        this.timer = 0;
        this.playerMovement = this.GetComponent<NetworkPlayerMovement>();
        this.loseCrystals = this.GetComponent<NetworkLoseCrystals>();

        this.respawnPoint = this.transform;
        this.surface = GameObject.FindGameObjectWithTag("Surface");
        this.characterController = this.GetComponent<CharacterController>();
        this.underground = GameObject.FindGameObjectWithTag("Underground");

        this.undergroundCanvas.SetActive(false);
        this.undergroundBackground = this.undergroundCanvas.transform.GetChild(0).GetComponent<Image>();
        this.undergroundImage = this.undergroundBackground.transform.GetChild(0).GetComponent<Image>();
    }

    void Update()
    {
        if (base.hasAuthority)
        {
            UpdateUndergroundBar();

            if (this.playerMovement.IsMovementDisabled)
                return;

            if (this.transform.position.y <= this.underground.transform.position.y + undergroundOffset)
            {
                this.timer += Time.deltaTime;
                if (this.timer >= this.timeToDie)
                    StartCoroutine(Die());
            } 
            else
                this.timer = 0;
        }
    }

    private IEnumerator Die()
    {
        var initialPosition = this.transform.position;

        this.characterController.enabled = false;
        this.playerMovement.DisableMovement();
        this.animator.SetTrigger("Dies");
        yield return new WaitForSecondsRealtime(3f);
        this.playerMovement.EnableMovement();
        this.characterController.enabled = true;

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

    private void UpdateUndergroundBar()
    {
        float fillAmount = this.timer / (float)this.timeToDie;
        if (this.playerMovement.IsInUnderground && !this.undergroundCanvas.activeSelf)
            this.undergroundCanvas.SetActive(true);
        else if (!this.playerMovement.IsInUnderground && this.undergroundCanvas.activeSelf)
            this.undergroundCanvas.SetActive(false);

        if (this.undergroundCanvas.activeSelf)
        {
            this.undergroundImage.fillAmount = fillAmount;
            this.undergroundCanvas.transform.LookAt(Camera.main.transform);
        }

        if (fillAmount < 0.5f)
            this.undergroundBackground.color = this.fullColor;
        else if (fillAmount < 0.75)
            this.undergroundBackground.color = this.halfColor;
        else
            this.undergroundBackground.color = this.urgentColor;
    }
}
