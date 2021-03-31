// GENERATED AUTOMATICALLY FROM 'Assets/Input/PlayerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""PlayerControls"",
            ""id"": ""8ccacc50-e4b6-4834-83f2-c51e90d6981a"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Button"",
                    ""id"": ""2ae19b24-8427-4e66-94a8-cf3e8550bca8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Camera"",
                    ""type"": ""Value"",
                    ""id"": ""34899ac1-5df0-43ab-94e6-8efa3d6bd928"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Digging"",
                    ""type"": ""Button"",
                    ""id"": ""2b7555a4-280b-4dd6-84cd-129ab948669d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""DropRope"",
                    ""type"": ""Button"",
                    ""id"": ""d45e5497-923a-4726-83a8-d1b359123056"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MuteSelf"",
                    ""type"": ""Button"",
                    ""id"": ""3d6349d7-198f-489f-a82b-bc52c4471b2e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InitiateCombo"",
                    ""type"": ""Button"",
                    ""id"": ""767a5bd7-2b5f-463b-a8c3-679bd7233bb9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MainMenu"",
                    ""type"": ""Button"",
                    ""id"": ""98344ab9-9143-40b4-8bb1-681868d7472b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""7cfa449a-b56a-4013-a044-55f4cb8dcf10"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""65fc7cc7-4984-478e-9fb7-3635c3e96016"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""1379b68a-2f0c-45b0-8765-560ee46788ba"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""c2dc2617-db02-42e9-9dde-94eeda1ecf30"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b56a4a67-9d2a-4779-8073-76c76813f3ec"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad/Left Stick"",
                    ""id"": ""815d8046-7e46-4e10-bafa-89da8ec1c590"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""ee89e70d-bf99-467d-be8f-ddd3678f7321"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""adf26a65-a83f-463b-8e1f-69244e81d557"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""86b7aa3e-ed1f-41c2-b1bf-ef12522352f7"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""58df7d72-cea3-4cbe-a0df-5b84cee31e8a"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""fce320fa-3672-4721-aedc-baa91d541d42"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""db6a15c9-6d1c-4012-8b81-2be6384cceab"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7e8237c2-858b-4a05-9f62-0dbb49942189"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Digging"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3175f016-2e22-489d-a7c5-2a6a0a6516fc"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Digging"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1d39b373-832e-4b1d-9ef7-24409be3d479"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DropRope"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""47b810ee-9ef4-4e11-b150-0a19eb4214ad"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DropRope"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""69832e5e-0d35-49d9-bd2a-89d2b0e9437b"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MuteSelf"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ce80a824-3be4-4ea0-a8b5-81b8a6f331f5"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MuteSelf"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a0cd5632-2eb9-4213-bf71-5e5650b7b818"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InitiateCombo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9722928c-cf48-4a78-8c6d-bc89c981a17d"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InitiateCombo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6e013af3-55d1-4702-8587-c3984cf6ad5d"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MainMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a173e90a-01b0-424c-b5dc-939d0f4af2cf"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MainMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // PlayerControls
        m_PlayerControls = asset.FindActionMap("PlayerControls", throwIfNotFound: true);
        m_PlayerControls_Movement = m_PlayerControls.FindAction("Movement", throwIfNotFound: true);
        m_PlayerControls_Camera = m_PlayerControls.FindAction("Camera", throwIfNotFound: true);
        m_PlayerControls_Digging = m_PlayerControls.FindAction("Digging", throwIfNotFound: true);
        m_PlayerControls_DropRope = m_PlayerControls.FindAction("DropRope", throwIfNotFound: true);
        m_PlayerControls_MuteSelf = m_PlayerControls.FindAction("MuteSelf", throwIfNotFound: true);
        m_PlayerControls_InitiateCombo = m_PlayerControls.FindAction("InitiateCombo", throwIfNotFound: true);
        m_PlayerControls_MainMenu = m_PlayerControls.FindAction("MainMenu", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // PlayerControls
    private readonly InputActionMap m_PlayerControls;
    private IPlayerControlsActions m_PlayerControlsActionsCallbackInterface;
    private readonly InputAction m_PlayerControls_Movement;
    private readonly InputAction m_PlayerControls_Camera;
    private readonly InputAction m_PlayerControls_Digging;
    private readonly InputAction m_PlayerControls_DropRope;
    private readonly InputAction m_PlayerControls_MuteSelf;
    private readonly InputAction m_PlayerControls_InitiateCombo;
    private readonly InputAction m_PlayerControls_MainMenu;
    public struct PlayerControlsActions
    {
        private @PlayerInput m_Wrapper;
        public PlayerControlsActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_PlayerControls_Movement;
        public InputAction @Camera => m_Wrapper.m_PlayerControls_Camera;
        public InputAction @Digging => m_Wrapper.m_PlayerControls_Digging;
        public InputAction @DropRope => m_Wrapper.m_PlayerControls_DropRope;
        public InputAction @MuteSelf => m_Wrapper.m_PlayerControls_MuteSelf;
        public InputAction @InitiateCombo => m_Wrapper.m_PlayerControls_InitiateCombo;
        public InputAction @MainMenu => m_Wrapper.m_PlayerControls_MainMenu;
        public InputActionMap Get() { return m_Wrapper.m_PlayerControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerControlsActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerControlsActions instance)
        {
            if (m_Wrapper.m_PlayerControlsActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMovement;
                @Camera.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnCamera;
                @Camera.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnCamera;
                @Camera.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnCamera;
                @Digging.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnDigging;
                @Digging.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnDigging;
                @Digging.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnDigging;
                @DropRope.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnDropRope;
                @DropRope.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnDropRope;
                @DropRope.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnDropRope;
                @MuteSelf.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMuteSelf;
                @MuteSelf.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMuteSelf;
                @MuteSelf.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMuteSelf;
                @InitiateCombo.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnInitiateCombo;
                @InitiateCombo.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnInitiateCombo;
                @InitiateCombo.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnInitiateCombo;
                @MainMenu.started -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMainMenu;
                @MainMenu.performed -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMainMenu;
                @MainMenu.canceled -= m_Wrapper.m_PlayerControlsActionsCallbackInterface.OnMainMenu;
            }
            m_Wrapper.m_PlayerControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @Camera.started += instance.OnCamera;
                @Camera.performed += instance.OnCamera;
                @Camera.canceled += instance.OnCamera;
                @Digging.started += instance.OnDigging;
                @Digging.performed += instance.OnDigging;
                @Digging.canceled += instance.OnDigging;
                @DropRope.started += instance.OnDropRope;
                @DropRope.performed += instance.OnDropRope;
                @DropRope.canceled += instance.OnDropRope;
                @MuteSelf.started += instance.OnMuteSelf;
                @MuteSelf.performed += instance.OnMuteSelf;
                @MuteSelf.canceled += instance.OnMuteSelf;
                @InitiateCombo.started += instance.OnInitiateCombo;
                @InitiateCombo.performed += instance.OnInitiateCombo;
                @InitiateCombo.canceled += instance.OnInitiateCombo;
                @MainMenu.started += instance.OnMainMenu;
                @MainMenu.performed += instance.OnMainMenu;
                @MainMenu.canceled += instance.OnMainMenu;
            }
        }
    }
    public PlayerControlsActions @PlayerControls => new PlayerControlsActions(this);
    public interface IPlayerControlsActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnCamera(InputAction.CallbackContext context);
        void OnDigging(InputAction.CallbackContext context);
        void OnDropRope(InputAction.CallbackContext context);
        void OnMuteSelf(InputAction.CallbackContext context);
        void OnInitiateCombo(InputAction.CallbackContext context);
        void OnMainMenu(InputAction.CallbackContext context);
    }
}
