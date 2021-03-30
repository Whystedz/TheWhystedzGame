using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;

    private PlayerInput playerInput;

    public static InputManager GetInstance() => instance;
    public Vector2 GetInputCamera() => this.playerInput.PlayerControls.Camera.ReadValue<Vector2>();
    public Vector2 GetInputMovement() => this.playerInput.PlayerControls.Movement.ReadValue<Vector2>();
    public bool GetDigging() => this.playerInput.PlayerControls.Digging.triggered;
    public bool GetRope() => this.playerInput.PlayerControls.DropRope.triggered;
    public bool GetMuteSelf() => this.playerInput.PlayerControls.MuteSelf.triggered;
    public bool GetInitiateCombo() => this.playerInput.PlayerControls.InitiateCombo.triggered;
    public bool GetMainMenu() => this.playerInput.PlayerControls.MainMenu.triggered;
    public PlayerInput GetPlayerInput() => this.playerInput;
    
    private void Awake()
    {
        MaintainSingleInstance();
        this.playerInput = new PlayerInput();
    }

    private void OnEnable() => this.playerInput.Enable();
    private void OnDisable() => this.playerInput.Disable();
    private void OnDestroy() => this.playerInput = null;

    private void MaintainSingleInstance()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }
}
