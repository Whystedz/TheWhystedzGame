using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDButtonCooldown : MonoBehaviour
{
    private HUDMainButtons HUDMainButtons;
    [SerializeField] private TMP_Text buttonText;

    private Image imageDarkened;
    private Image imageNormal;

    public float MaxAmount;
    public float CurrentAmount;

    void Start()
    {
        GameObject HUDMainButtonGameObject = GameObject.FindGameObjectWithTag("MainButtons");
        this.HUDMainButtons = HUDMainButtonGameObject.GetComponent<HUDMainButtons>();

        this.imageNormal = GetComponent<Image>();
        this.imageDarkened = transform.parent.GetComponent<Image>();

        SetNormalColors();

        this.imageNormal.sprite = this.imageDarkened.sprite;

        if (this.buttonText == null) Debug.LogError(this.name + ": Set the text for this button in the inspector from the main menu game object.");
    }

    void Update()
    {
        if (MaxAmount != 0f && CurrentAmount / MaxAmount < 1f)
            SetCoolDownColors();
        else
            SetNormalColors();
            
        this.imageNormal.fillAmount = CurrentAmount / MaxAmount;
    } 

    private void SetNormalColors()
    {
        this.buttonText.color = this.HUDMainButtons.CanUseTextColor;
        this.imageNormal.color = this.HUDMainButtons.CanUseButtonColor;
        this.imageDarkened.color = this.HUDMainButtons.CanNotUseButtonColor;
    }

    private void SetCoolDownColors()
    {
        this.buttonText.color = this.HUDMainButtons.CanNotUseTextColor;
        this.imageNormal.color = this.HUDMainButtons.CanNotUseButtonColor;
        this.imageDarkened.color = this.HUDMainButtons.CanUseButtonColor;
    }
}
