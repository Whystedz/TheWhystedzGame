using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkHUDMainButtons : MonoBehaviour
{
    private GameObject player;
    private NetworkPlayerMovement playerMovement;
    private NetworkComboPlayer comboPlayer;

    [SerializeField] private Image digImage;
    [SerializeField] private Image digImageCooldown;
    [SerializeField] private Image digBackground;
    [SerializeField] private Image comboImage;
    [SerializeField] private Image comboImageCooldown;
    [SerializeField] private Image comboBackground;

    private NetworkHUDButtonCooldown digButtonCooldown;
    private NetworkHUDButtonCooldown comboButtonCooldown;

    [SerializeField] private TMP_Text digTMP_Text;
    [SerializeField] private string textDig;
    [SerializeField] private string textClimb;

    public Color CanUseButtonColor;
    public Color CanNotUseButtonColor;

    public Color CanUseTextColor;
    public Color CanNotUseTextColor;

    [Header("Keyboard Images")]
    [SerializeField] private GameObject keyboardDig;
    [SerializeField] private Sprite keyboardDigBackground;
    [SerializeField] private GameObject keyboardCombo;
    [SerializeField] private Sprite keyboardComboBackground;

    [Header("PS Images")]
    [SerializeField] private Sprite psDig;
    [SerializeField] private Sprite psDigBackground;
    [SerializeField] private Sprite psCombo;
    [SerializeField] private Sprite psComboBackground;

    [Header("XBOX Images")]
    [SerializeField] private Sprite xboxDig;
    [SerializeField] private Sprite xboxDigBackground;
    [SerializeField] private Sprite xboxCombo;
    [SerializeField] private Sprite xboxComboBackground;

    public ControllerSchemeInUse ControllerSchemeInUse { get; private set; }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
        this.playerMovement = this.player.GetComponent<NetworkPlayerMovement>();
        this.comboPlayer = this.player.GetComponent<NetworkComboPlayer>();
    }

    private void Update()
    {
        if (this.player is null)
            return;
        
        this.comboButtonCooldown = NetworkInputManager.Instance.IsUsingKeyboard ? 
            this.keyboardCombo.GetComponentInChildren<NetworkHUDButtonCooldown>() : 
            this.comboImage.GetComponentInChildren<NetworkHUDButtonCooldown>();

        this.comboButtonCooldown.MaxAmount = this.comboPlayer.GetCooldownMax();
        this.comboButtonCooldown.CurrentAmount = this.comboPlayer.GetCooldownProgress();
        
        if (this.playerMovement.IsInUnderground)
            this.digTMP_Text.text = this.textClimb;
        else
            this.digTMP_Text.text = this.textDig;
    }

    public void DisplayPlayStationControls()
    {
        ControllerSchemeInUse = ControllerSchemeInUse.PlayStation;

        digImage.gameObject.SetActive(true);
        digImageCooldown.gameObject.SetActive(true);

        comboImage.gameObject.SetActive(true);
        comboImageCooldown.gameObject.SetActive(true);

        keyboardDig.SetActive(false);
        keyboardCombo.SetActive(false);

        this.digImage.sprite = psDig;
        this.digImageCooldown.sprite = psDig;
        this.digBackground.sprite = psDigBackground;

        this.comboImage.sprite = psCombo;
        this.comboImageCooldown.sprite = psCombo;
        this.comboBackground.sprite = psComboBackground;
    }

    public void DisplayXBOXControls()
    {
        ControllerSchemeInUse = ControllerSchemeInUse.XBox;

        digImage.gameObject.SetActive(true);
        digImageCooldown.gameObject.SetActive(true);

        comboImage.gameObject.SetActive(true);
        comboImageCooldown.gameObject.SetActive(true);

        keyboardDig.SetActive(false);
        keyboardCombo.SetActive(false);

        this.digImage.sprite = xboxDig;
        this.digImageCooldown.sprite = xboxDig;
        this.digBackground.sprite = xboxDigBackground;

        this.comboImage.sprite = xboxCombo;
        this.comboImageCooldown.sprite = xboxCombo;
        this.comboBackground.sprite = xboxComboBackground;
    }

    public void DisplayKeyboardControls()
    {
        ControllerSchemeInUse = ControllerSchemeInUse.PC;
        
        digImage.gameObject.SetActive(false);
        digImageCooldown.gameObject.SetActive(false);

        comboImage.gameObject.SetActive(false);
        comboImageCooldown.gameObject.SetActive(false);

        keyboardDig.SetActive(true);
        keyboardCombo.SetActive(true);

        this.digBackground.sprite = keyboardDigBackground;
        this.comboBackground.sprite = keyboardComboBackground;
    }
}
