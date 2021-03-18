using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;

public class NetworkPlayerCamera : NetworkBehaviour
{
    private InputManager inputManager;

    [Header("Camera")]
    [SerializeField] private Vector2 maxFollowOffset = new Vector2(-1f, 6f);
    [SerializeField] private Vector2 cameraVelocity = new Vector2(4f, 0.25f);
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
    private CinemachineTransposer transposer;

    public override void OnStartAuthority()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        virtualCamera.gameObject.SetActive(true);
        this.inputManager = InputManager.GetInstance();

        inputManager.GetPlayerInput().PlayerControls.Camera.performed += ctx => Look(ctx.ReadValue<Vector2>());
    }

    private void Look(Vector2 lookAxis)
    {
        float deltaTime = Time.deltaTime;

        float followOffset = Mathf.Clamp(
            transposer.m_FollowOffset.y - (lookAxis.y * cameraVelocity.y * deltaTime),
            maxFollowOffset.x,
            maxFollowOffset.y);

        transposer.m_FollowOffset.y = followOffset;

        transform.Rotate(0f, lookAxis.x * cameraVelocity.x * deltaTime, 0f);
    }
}
