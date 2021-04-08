using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkHUDButtonCooldown : MonoBehaviour
{
    private NetworkHUDMainButtons HUDMainButtons;
    [SerializeField] private TMP_Text buttonText;

    private Image imageDarkened;
    private Image imageNormal;

    public float MaxAmount;
    public float CurrentAmount;

    void Start()
    {
        GameObject HUDMainButtonGameObject = GameObject.FindGameObjectWithTag("MainButtons");
        this.HUDMainButtons = HUDMainButtonGameObject.GetComponent<NetworkHUDMainButtons>();

        this.imageNormal = GetComponent<Image>();
        this.imageNormal.color = this.HUDMainButtons.CanUseButtonColor;

        this.imageDarkened = transform.parent.GetComponent<Image>();
        this.imageDarkened.color = this.HUDMainButtons.CanNotUseButtonColor;

        this.imageNormal.sprite = this.imageDarkened.sprite;

        if (this.buttonText == null) Debug.LogError(this.name + ": Set the text for this button in the inspector from the main menu game object.");
    }

    void Update()
    {
        this.buttonText.color = (MaxAmount != 0f && CurrentAmount / MaxAmount < 1f) ? this.imageDarkened.color : this.imageNormal.color;

        this.imageNormal.fillAmount = CurrentAmount / MaxAmount;
    } 
}
