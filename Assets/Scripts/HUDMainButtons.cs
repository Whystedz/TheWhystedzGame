using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDMainButtons : MonoBehaviour
{
    private InputManager inputManager;

    private GameObject player;
    private PlayerMovement playerMovement;
    private ComboPlayer comboPlayer;
    private DiggingAndRopeInteractions diggingAndRopeInteractions;

    public GameObject northButton;
    public GameObject southButton;
    public GameObject eastButton;
    public GameObject westButton;

    private HUDButtonCooldown eastButtonCooldown;

    [SerializeField] private TMP_Text southButtonText;
    [SerializeField] private string southButtonText_Dig;
    [SerializeField] private string southButtonText_Rope;
    [SerializeField] private string southButtonText_RemoveRope;
    [SerializeField] private string southButtonText_Climb;

    [SerializeField] private TMP_Text westButtonText;
    [SerializeField] private string westButtonText_ToggleTrue;
    [SerializeField] private string westButtonText_ToggleFalse;
    private bool muteToggle;

    public Color CanUseButtonColor;
    public Color CanNotUseButtonColor;

    private void Start()
    {
        this.inputManager = InputManager.GetInstance();
        this.playerMovement = this.player.GetComponent<PlayerMovement>();
        this.comboPlayer = this.player.GetComponent<ComboPlayer>();
        this.diggingAndRopeInteractions = this.player.GetComponent<DiggingAndRopeInteractions>();

        this.eastButtonCooldown = this.eastButton.GetComponentInChildren<HUDButtonCooldown>();
        this.eastButtonCooldown.MaxAmount = this.comboPlayer.GetCooldownMax();
    }

    public void SetPlayer(GameObject player) => this.player = player;

    private void Update()
    {
        this.eastButtonCooldown.CurrentAmount = this.comboPlayer.GetCooldownProgress();

        if (inputManager.GetMuteSelf())
        {
            muteToggle = !muteToggle;
            westButtonText.text = muteToggle ? westButtonText_ToggleTrue : westButtonText_ToggleFalse;
        }
        
        if (this.playerMovement.IsInUnderground)
            this.southButtonText.text = this.southButtonText_Climb;
        else if (this.diggingAndRopeInteractions.tile != null && this.diggingAndRopeInteractions.tile.tileState == TileState.Respawning)
            this.southButtonText.text = this.southButtonText_Rope;
        else if (this.diggingAndRopeInteractions.tile != null && this.diggingAndRopeInteractions.tile.tileState == TileState.Rope)
            this.southButtonText.text = this.southButtonText_RemoveRope;
        else
            this.southButtonText.text = this.southButtonText_Dig;
    }

}
