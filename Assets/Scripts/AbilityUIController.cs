using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class AbilityUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text mutagenCountText;
    [SerializeField] private Transform upgradeButtonContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Button purchaseButton;

    [Header("Details Panel")]
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailCostText;

    [Header("Data")]
    [SerializeField] private List<AbilityUpgradeSO> availableUpgrades;

    private AbilityUpgradeSO selectedUpgrade;
    private PlayerInventory playerInventory;
    private playerController playerController;

    void Start()
    {
        playerController = gamemanager.instance.playerScript;
        playerInventory = playerController.GetComponent<PlayerInventory>();
        PopulateUpgradeList();
    }

    void OnEnable()
    {
        UpdateMutagenCountDisplay();
        Deselect();
    }

    void PopulateUpgradeList()
    {
        foreach (Transform child in upgradeButtonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (AbilityUpgradeSO upgrade in availableUpgrades)
        {
            GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);

            UpgradeButton buttonScript = buttonObj.GetComponent<UpgradeButton>();

            buttonScript.SetUp(upgrade, this);
        }
    }

    public void SelectUpgrade(AbilityUpgradeSO upgrade)
    {
        selectedUpgrade = upgrade;
        detailNameText.text = upgrade.upgradeName;
        detailDescriptionText.text = upgrade.description;
        detailCostText.text = "Cost: " + upgrade.mutagenCost + " Mutagens";

        if (upgrade.requiredItem != null)
        {
            detailCostText.text += " + " + upgrade.requiredItem.itemName;
        }

        purchaseButton.interactable = true;
    }

    public void Deselect()
    {
        selectedUpgrade = null;
        detailNameText.text = "Select an Upgrade";
        detailDescriptionText.text = "View details here.";
        detailCostText.text = "";
        purchaseButton.interactable = false;
    }

    public void OnPurchaseButtonPressed()
    {
        if (selectedUpgrade == null) return;

        bool hasEnoughMutagens = playerInventory.TrySpendMutagens(selectedUpgrade.mutagenCost);

        if (hasEnoughMutagens)
        {
            ApplyUpgradeEffect(selectedUpgrade);
            Debug.Log("Upgrade purchased: " + selectedUpgrade.upgradeName);
            UpdateMutagenCountDisplay();
        }
        else
        {
            Debug.Log("Not enough mutagens!");
        }
    }

    private void ApplyUpgradeEffect(AbilityUpgradeSO upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.Health:
                playerController.Heal(upgrade.quantity, upgrade.increaseMax);
                break;
            case UpgradeType.Shield:
                playerController.GainShield(upgrade.quantity, upgrade.increaseMax);
                break;
            case UpgradeType.Armor:
                playerController.GainArmor(upgrade.quantity, upgrade.increaseMax);
                break;
            case UpgradeType.Damage:
                playerController.IncreaseDamage(upgrade.quantity, upgrade.magnitude);
                break;
            case UpgradeType.Speed:
                playerController.IncreaseSpeed(upgrade.quantity, upgrade.magnitude);
                break;
            case UpgradeType.Jump:
                playerController.IncreaseJumpMaxCount(upgrade.quantity, upgrade.magnitude);
                break;
        }
    }

    private void UpdateMutagenCountDisplay()
    {
        mutagenCountText.text = "Mutagens: " + playerInventory.GetMutagenCount();
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);

        if (gamemanager.instance != null)
        {
            gamemanager.instance.statePause();
            gamemanager.instance.menuActive = this.gameObject;
        }
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);

        {
            gamemanager.instance.stateUnpause();
        }
    }
}