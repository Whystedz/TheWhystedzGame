using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Vector3 cameraInput;

    [SerializeField]
    private float cameraSpeed = 6f;

    [SerializeField]
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnCameraAdjust(InputAction.CallbackContext value)
    {
        Vector2 cameraValue = value.ReadValue<Vector2>();

        cameraInput = new Vector3(cameraValue.x * Time.deltaTime * cameraSpeed, 0f, cameraValue.y * Time.deltaTime * cameraSpeed);


    }
}
