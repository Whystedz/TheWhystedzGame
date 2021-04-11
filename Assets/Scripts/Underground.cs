using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Underground : MonoBehaviour
{
    [SerializeField] private int timeToDie = 15;
    [SerializeField] private Transform respawnPoint;
    private float timer;
    private PlayerMovement playerMovement;
    private LoseCrystals loseCrystals;
    private AnimationManager animationManager;

    private GameObject surface;
    private GameObject underground;

    private CharacterController characterController;
    private DiggingAndRopeInteractions diggingAndRopeInteractions;

    [SerializeField] private GameObject undergroundCanvas;
    private Image undergroundBackground;
    private Image undergroundImage;
    [SerializeField] private Color fullColor;
    [SerializeField] private Color halfColor;
    [SerializeField] private Color urgentColor;
    private Transform mainCameraTransform;

    private void Awake()
    {
        this.playerMovement = this.GetComponent<PlayerMovement>();
        this.loseCrystals = this.GetComponent<LoseCrystals>();
        this.animationManager = this.GetComponentInChildren<AnimationManager>();
        this.underground = GameObject.FindGameObjectWithTag("Underground");
        this.surface = GameObject.FindGameObjectWithTag("Surface");
        this.characterController = this.GetComponent<CharacterController>();
        this.diggingAndRopeInteractions = GetComponent<DiggingAndRopeInteractions>();
        this.mainCameraTransform = Camera.main.transform;
        this.undergroundCanvas.SetActive(false);
        this.undergroundBackground = this.undergroundCanvas.transform.GetChild(0).GetComponent<Image>();
        this.undergroundImage = this.undergroundBackground.transform.GetChild(0).GetComponent<Image>();
    }

    void Update()
    {
        UpdateUndergroundBar();

        if (this.playerMovement.IsMovementDisabled)
            return;

        if (this.playerMovement.IsInUnderground && !this.playerMovement.IsClimbing)
        {
            this.timer += Time.deltaTime;
            if (this.timer >= this.timeToDie)
                StartCoroutine(Die());
        } else
            this.timer = 0;
    }

    private IEnumerator Die()
    {
        var initialPosition = this.transform.position;

        this.loseCrystals.LoseCrystal();

        this.characterController.enabled = false;
        this.playerMovement.DisableMovement();
        this.animationManager.TriggerDeath(); // TODO
        
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(this.playerMovement.FadeOut(2f));
        yield return new WaitForSeconds(0.5f);
       
        this.characterController.enabled = true;
        var offset = initialPosition.y - this.underground.transform.position.y;
        var revivedPosition = new Vector3(
            respawnPoint.position.x,
            this.surface.transform.position.y + offset,
            respawnPoint.position.z);
        this.characterController.enabled = false;
        this.transform.position = revivedPosition;
        this.characterController.enabled = true;
        this.playerMovement.IsInUnderground = false;
        StartCoroutine(this.playerMovement.FadeIn(1f));
        yield return new WaitForSeconds(0.1f);
        this.playerMovement.EnableMovement();
        

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
            this.undergroundCanvas.transform.LookAt(this.mainCameraTransform);
        }

        if (fillAmount < 0.5f)
            this.undergroundBackground.color = this.fullColor;
        else if (fillAmount < 0.75)
            this.undergroundBackground.color = this.halfColor;
        else
            this.undergroundBackground.color = this.urgentColor;
    }


}
