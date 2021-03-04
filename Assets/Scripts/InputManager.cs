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

    private static InputManager Instance;

    private PlayerInput PlayerInput;

    public static InputManager GetInstance() => Instance;
    public Vector2 GetInputCamera() => PlayerInput.PlayerControls.Camera.ReadValue<Vector2>();
    public Vector2 GetInputMovement() => PlayerInput.PlayerControls.Movement.ReadValue<Vector2>();

    private void Awake()
    {
        MaintainSingleInstance();

        PlayerInput = new PlayerInput();

    }

    private void OnEnable()
    {
        PlayerInput.Enable();
    }

    private void OnDisable()
    {
        PlayerInput.Disable();
    }

    private void MaintainSingleInstance()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
}
