using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedSetterUI : MonoBehaviour
{
    [SerializeField] private GameObject uiElement;

    private void OnEnable()
    {
        SetSelected();
    }

    private void Start()
    {
        SetSelected();
    }

    private void SetSelected()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(uiElement);
    }
}
