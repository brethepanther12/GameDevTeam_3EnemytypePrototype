using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Image buttonIcon;

    private AbilityUpgradeSO myUpgrade;
    private AbilityUIController uiController;

    public void SetUp(AbilityUpgradeSO upgrade, AbilityUIController controller)
    {
        myUpgrade = upgrade;
        uiController = controller;

        if (buttonText != null)
        {
            buttonText.text = myUpgrade.upgradeName;
        }
        if (buttonIcon != null && myUpgrade.icon != null)
        {
            buttonIcon.enabled = true;
            buttonIcon.sprite = myUpgrade.icon;
        }

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        uiController.SelectUpgrade(myUpgrade);
    }
}