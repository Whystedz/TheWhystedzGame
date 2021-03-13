using UnityEngine;

public class InputManager : MonoBehaviour
{
    /* To use InputManager include the following in your script, along with the InputManager prefab in your scene:
     
    private InputManager InputManager;
        
    private void Start()
        {
            InputManager = InputManager.Instance;
        }

    example call:
    Vector3 movementInput = new Vector3(InputManager.GetInputMovement().x, 0f, InputManager.GetInputMovement().y);

    */

    private static InputManager instance;

    private PlayerInput playerInput;

    public static InputManager GetInstance() => instance;
    public Vector2 GetInputCamera() => this.playerInput.PlayerControls.Camera.ReadValue<Vector2>();
    public Vector2 GetInputMovement() => this.playerInput.PlayerControls.Movement.ReadValue<Vector2>();
    public bool GetDigging() => this.playerInput.PlayerControls.Digging.triggered;
    private void Awake()
    {
        MaintainSingleInstance();
        this.playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        this.playerInput.Enable();
    }

    private void OnDisable()
    {
        this.playerInput.Disable();
    }

    private void MaintainSingleInstance()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

}
