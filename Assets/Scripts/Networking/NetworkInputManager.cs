using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkInputManager : MonoBehaviour
{
    public static NetworkInputManager Instance;

    [SerializeField] private PlayerInput playerInput;

    private NetworkHUDMainButtons HUDMainButtons;
    public Vector2 GetInputMovement() => this.playerInput.PlayerControls.Movement.ReadValue<Vector2>();
    public bool GetDigging() => this.playerInput.PlayerControls.Digging.triggered;
    public bool GetInitiateCombo() => this.playerInput.PlayerControls.InitiateCombo.triggered;
    public bool GetLadder() => this.playerInput.PlayerControls.Ladder.triggered;
    public bool GetMainMenu() => this.playerInput.PlayerControls.MainMenu.triggered;
    public PlayerInput GetPlayerInput() => this.playerInput;

    public bool IsUsingKeyboard = false;

    private void Awake()
    {
        MaintainSingleInstance();
        this.playerInput = new PlayerInput();
    }

    private void Start()
    {
        GameObject HUDMainButtonGameObject = GameObject.FindGameObjectWithTag("MainButtons");
        this.HUDMainButtons = HUDMainButtonGameObject.GetComponent<NetworkHUDMainButtons>();
        OnControlsChanged();
    }

    public void OnControlsChanged()
    {
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        if(gamepad == null)
        {
            IsUsingKeyboard = true;
            this.HUDMainButtons.DisplayKeyboardControls();
        }
        else
        {
            string device = gamepad.device.ToString();

            switch (device)
            {
                case "DualShock4GamepadHID:/DualShock4GamepadHID":
                    IsUsingKeyboard = false;
                    this.HUDMainButtons.DisplayPlayStationControls();
                    break;
                case "SwitchProControllerHID:/SwitchProControllerHID": // no UI support for switch/generic game pad
                case "Gamepad:/Gamepad":
                case "XInputControllerWindows:/XInputControllerWindows":
                default:
                    IsUsingKeyboard = false;
                    this.HUDMainButtons.DisplayXBOXControls();
                    break;
            }
        }
    }

    private void OnEnable() => this.playerInput.Enable();
    private void OnDisable() => this.playerInput.Disable();
    private void OnDestroy() => this.playerInput = null;
    public void EnableInput() => this.playerInput.Enable();
    public void DisableInput() => this.playerInput.Disable();

    private void MaintainSingleInstance()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
}
