using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour
{
    [Header("Menu Navigation")]
    [SerializeField] private GameObject weaponUpgradePanel;
    [SerializeField] private AbilityUIController abilityUIController;

    [Header("UI References")]
    [SerializeField] private TMP_Text componentCountText;
    [SerializeField] private Transform upgradeButtonContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Button purchaseButton;

    [Header("Details Panel")]
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailCostText;

    [Header("Data")]
    [Tooltip("A list of ALL possible weapon upgrades in the game.")]
    [SerializeField] private List<WeaponUpgradeSO> allPossibleUpgrades;

    private WeaponUpgradeSO selectedUpgrade;
    private PlayerInventory playerInventory;
    private WeaponSO currentWeapon;

    void Start()
    {
        playerInventory = gamemanager.instance.playerScript.GetComponent<PlayerInventory>();
    }

    public void OpenWeaponMenu()
    {
        abilityUIController.gameObject.SetActive(false);
        weaponUpgradePanel.SetActive(true);

        currentWeapon = playerInventory.equippedWeapon;

        PopulateUpgrades();
        UpdateComponentCount();
        Deselect();
    }

    public void ShowAbilityMenu()
    {
        weaponUpgradePanel.SetActive(false);
        abilityUIController.gameObject.SetActive(true);
    }

    void PopulateUpgrades()
    {
        foreach (Transform child in upgradeButtonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (WeaponUpgradeSO upgrade in allPossibleUpgrades)
        {
            if (!currentWeapon.appliedUpgrades.Contains(upgrade))
            {
                GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
                UpgradeButton buttonScript = buttonObj.GetComponent<UpgradeButton>();

                buttonObj.GetComponentInChildren<TMP_Text>().text = upgrade.upgradeName;
                buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectUpgrade(upgrade));
            }
        }
    }

    public void SelectUpgrade(WeaponUpgradeSO upgrade)
    {
        selectedUpgrade = upgrade;
        detailNameText.text = upgrade.upgradeName;
        detailDescriptionText.text = upgrade.description;
        detailCostText.text = "Cost: " + upgrade.componentCost + " Components";
        purchaseButton.interactable = true;
    }

    public void Deselect()
    {
        selectedUpgrade = null;
        detailNameText.text = "Select an Upgrade";
        detailDescriptionText.text = "View details for the selected weapon upgrade.";
        detailCostText.text = "";
        purchaseButton.interactable = false;
    }

    public void OnPurchaseButtonPressed()
    {
        if (selectedUpgrade == null || currentWeapon == null) return;

        if (playerInventory.TrySpendWeaponComponents(selectedUpgrade.componentCost))
        {

            currentWeapon.ApplyUpgrade(selectedUpgrade);

            Debug.Log("Purchased '" + selectedUpgrade.upgradeName + "' for " + currentWeapon.weaponName);

            playerInventory.EquipWeapon();

            PopulateUpgrades();
            UpdateComponentCount();
            Deselect();
        }
        else
        {
            Debug.Log("Not enough Weapon Components!");
        }
    }

    void UpdateComponentCount()
    {
        if (componentCountText != null)
        {
            componentCountText.text = "Components: " + playerInventory.GetWeaponComponentCount();
        }
    }
}