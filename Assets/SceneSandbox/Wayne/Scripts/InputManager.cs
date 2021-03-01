using UnityEngine;

public class InputManager : MonoBehaviour
{

    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private PlayerActions playerActions;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }

        playerActions = new PlayerActions();
        
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerActions.PlayerControls.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetCameraLook()
    {
        return playerActions.PlayerControls.Look.ReadValue<Vector2>();
    }

}
