using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ControllerSchemeInUse
{
    PC,
    XBox,
    PlayStation,
}

public class HUDMainButtons : MonoBehaviour
{
    private InputManager inputManager;

    private GameObject player;
    private PlayerMovement playerMovement;
    private ComboPlayer comboPlayer;
    private SimpleDigging simpleDigging;

    [Header("Controller Images")]
    [SerializeField] private Image digImage;
    [SerializeField] private Image digImageCooldown;
    [SerializeField] private Image digBackground;
    [SerializeField] private Image comboImage;
    [SerializeField] private Image comboImageCooldown;
    [SerializeField] private Image comboBackground;

    [Header("Keyboard Images")]
    [SerializeField] private Image digImage_keyboard;
    [SerializeField] private Image digImageCooldown_keyboard;
    [SerializeField] private Image comboImage_keyboard;
    [SerializeField] private Image comboImageCooldown_keyboard;

    private HUDButtonCooldown digButtonCooldown;
    private HUDButtonCooldown comboButtonCooldown;


    [SerializeField] private TMP_Text comboTMP_Text;
    [SerializeField] private TMP_Text digTMP_Text;
    [SerializeField] private string textDig;
    [SerializeField] private string textClimb;

    public Color CanUseButtonColor;
    public Color CanNotUseButtonColor;

    public Color CanUseTextColor;
    public Color CanNotUseTextColor;

    [Header("Keyboard Images")]
    [SerializeField] private Sprite keyboardDig;
    [SerializeField] private Sprite keyboardDigBackground;
    [SerializeField] private Sprite keyboardCombo;
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

    private void Start()
    {
        this.inputManager = InputManager.GetInstance();
        this.playerMovement = this.player.GetComponent<PlayerMovement>();
        this.comboPlayer = this.player.GetComponent<ComboPlayer>();
        this.simpleDigging = this.player.GetComponent<SimpleDigging>();
    }

    public void SetPlayer(GameObject player) => this.player = player;

    private void Update()
    {
        this.comboButtonCooldown = this.inputManager.IsUsingKeyboard ? 
            this.comboImage_keyboard.GetComponentInChildren<HUDButtonCooldown>() : 
            this.comboImage.GetComponentInChildren<HUDButtonCooldown>();

        this.comboButtonCooldown.MaxAmount = this.comboPlayer.GetCooldownMax();
        this.comboButtonCooldown.CurrentAmount = this.comboPlayer.GetCooldownProgress();
        
        // Setting dig button visibility and text
        if (this.playerMovement.IsInUnderground)
            this.digTMP_Text.text = this.textClimb;
        else
            this.digTMP_Text.text = this.textDig;

        if((this.playerMovement.IsInUnderground 
            && this.playerMovement.CanClimb)
            || (!this.playerMovement.IsInUnderground 
            && this.simpleDigging.TileToDig != null 
            && this.simpleDigging.TileToDig.tileState == TileState.Normal))
            CanUseDigButton();
        else
            CannotUseDigButton();

        // Setting combo button visibility
        if(!this.playerMovement.IsInUnderground && this.comboPlayer.Combos.Count > 0)
            CanUseComboButton();
        else
            CannotUseComboButton();
        
    }

    private void CanUseComboButton()
    {
        this.comboTMP_Text.color = this.CanUseTextColor;

        this.comboImage_keyboard.color = this.CanNotUseButtonColor;
        this.comboImage.color = this.CanNotUseButtonColor;

        this.comboImageCooldown_keyboard.color = this.CanUseButtonColor;
        this.comboImageCooldown.color = this.CanUseButtonColor;
    }

    private void CannotUseComboButton()
    {
        this.comboTMP_Text.color = this.CanNotUseTextColor;

        this.comboImage_keyboard.color = this.CanUseButtonColor;
        this.comboImage.color = this.CanUseButtonColor;

        this.comboImageCooldown_keyboard.color = this.CanNotUseButtonColor;
        this.comboImageCooldown.color = this.CanNotUseButtonColor;
    }

    private void CanUseDigButton()
    {
        this.digTMP_Text.color = this.CanUseTextColor;

        this.digImage_keyboard.color = this.CanNotUseButtonColor;
        this.digImage.color = this.CanNotUseButtonColor;

        this.digImageCooldown_keyboard.color = this.CanUseButtonColor;
        this.digImageCooldown.color = this.CanUseButtonColor;
    }

    private void CannotUseDigButton()
    {
        this.digTMP_Text.color = this.CanNotUseTextColor;

        this.digImage_keyboard.color = this.CanUseButtonColor;
        this.digImage.color = this.CanUseButtonColor;

        this.digImageCooldown_keyboard.color = this.CanNotUseButtonColor;
        this.digImageCooldown.color = this.CanNotUseButtonColor;
    }

    public void DisplayPlayStationControls()
    {
        this.ControllerSchemeInUse = ControllerSchemeInUse.PlayStation;

        this.digImage.gameObject.transform.localScale = new Vector3(1, 1, 1);
        this.comboImage.gameObject.transform.localScale = new Vector3(1, 1, 1);

        this.digImage_keyboard.gameObject.transform.localScale = new Vector3(0, 0, 0);
        this.comboImage_keyboard.gameObject.transform.localScale = new Vector3(0, 0, 0);

        this.digImage.sprite = psDig;
        this.digImageCooldown.sprite = psDig;
        this.digBackground.sprite = psDigBackground;

        this.comboImage.sprite = psCombo;
        this.comboImageCooldown.sprite = psCombo;
        this.comboBackground.sprite = psComboBackground;
    }

    public void DisplayXBOXControls()
    {
        this.ControllerSchemeInUse = ControllerSchemeInUse.XBox;

        this.digImage.gameObject.transform.localScale = new Vector3(1, 1, 1);
        this.comboImage.gameObject.transform.localScale = new Vector3(1, 1, 1);

        this.digImage_keyboard.gameObject.transform.localScale = new Vector3(0, 0, 0);
        this.comboImage_keyboard.gameObject.transform.localScale = new Vector3(0, 0, 0);

        this.digImage.sprite = xboxDig;
        this.digImageCooldown.sprite = xboxDig;
        this.digBackground.sprite = xboxDigBackground;

        this.comboImage.sprite = xboxCombo;
        this.comboImageCooldown.sprite = xboxCombo;
        this.comboBackground.sprite = xboxComboBackground;
    }

    public void DisplayKeyboardControls()
    {
        this.ControllerSchemeInUse = ControllerSchemeInUse.PC;

        this.digImage.gameObject.transform.localScale = new Vector3(0, 0, 0);
        this.comboImage.gameObject.transform.localScale = new Vector3(0, 0, 0);

        this.digImage_keyboard.gameObject.transform.localScale = new Vector3(1, 1, 1);
        this.comboImage_keyboard.gameObject.transform.localScale = new Vector3(1, 1, 1);

        this.digBackground.sprite = keyboardDigBackground;
        this.comboBackground.sprite = keyboardComboBackground;
    }

}
