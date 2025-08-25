using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour
{
    [Header("Menu Navigation")]
    [SerializeField] public GameObject weaponUpgradePanel;
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

    private WeaponUpgradeSO selectedUpgrade;
    private PlayerInventory playerInventory;
    private WeaponSO currentWeapon;

    void Start()
    {
        playerInventory = gamemanager.instance.playerScript.GetComponent<PlayerInventory>();

        if (playerInventory != null)
        {
            playerInventory.OnWeaponComponentsChanged += HandleComponentChanged;

            HandleComponentChanged(playerInventory.GetWeaponComponentCount());
        }
    }

    public void OpenWeaponMenu()
    {
        playerInventory = gamemanager.instance.playerScript.GetComponent<PlayerInventory>();

        if (playerInventory == null)
        {
          //  Debug.LogError("WeaponUIController could not find PlayerInventory!");
            return;
        }

        UpdateComponentCount();

        gamemanager.instance.OpenMenu(weaponUpgradePanel);


        UpdateForNewWeapon(playerInventory.equippedWeapon);
    }

    public void ShowAbilityMenu()
    {

        AbilityUIController abilityUI = FindAnyObjectByType<AbilityUIController>();
        if (abilityUI != null)
        {
            gamemanager.instance.OpenMenu(abilityUI.upgradePanel);
        }
    }

    public void CloseMenu()
    {
        gamemanager.instance.CloseActiveMenu();
    }

    void PopulateUpgrades()
    {
        foreach (Transform child in upgradeButtonContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentWeapon == null)
        {
          //  Debug.LogWarning("No weapon equipped. Cannot show upgrades.");

            detailNameText.text = "No Weapon Equipped";
            detailDescriptionText.text = "Equip a weapon to see its available upgrades.";
            return; 
        }

        // this should make weapon upgrades dynamic. if there is a problem with that for anyone else check this area.
        foreach (WeaponUpgradeSO upgrade in currentWeapon.availableUpgrades)
        {
            if (!playerInventory.HasPurchasedUpgrade(currentWeapon, upgrade))
            {
                GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
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
        if (selectedUpgrade == null) return;

        Weapon activeWeapon = playerInventory.GetActiveWeapon();
        if (activeWeapon == null) return;

        if (playerInventory.TrySpendWeaponComponents(selectedUpgrade.componentCost))
        {
            activeWeapon.ApplyUpgrade(selectedUpgrade);

            playerInventory.RecordUpgradeForWeapon(currentWeapon, selectedUpgrade);

          //  Debug.Log($"Purchased '{selectedUpgrade.upgradeName}' for {activeWeapon.weaponData.weaponName}");

            PopulateUpgrades();
            UpdateComponentCount();
            Deselect();
        }
        else
        {
           // Debug.Log("Not enough Weapon Components!");
        }
    }

    void UpdateComponentCount()
    {
        if (playerInventory == null)
        {
            playerInventory = gamemanager.instance.playerScript.GetComponent<PlayerInventory>();
        }

        int count = playerInventory.GetWeaponComponentCount();
       // Debug.Log("UpdateComponentCount called. Player has " + count + " components.");

        if (componentCountText != null)
        {
            componentCountText.text = "Components: " + count;
        }
        else
        {
           // Debug.LogWarning("componentCountText is not assigned in the inspector!");
        }
    }

    public void UpdateForNewWeapon(WeaponSO newWeapon)
    {
        if (playerInventory == null)
        {
            playerInventory = gamemanager.instance.playerScript.GetComponent<PlayerInventory>();
        }

        currentWeapon = newWeapon;

        UpdateComponentCount();
        PopulateUpgrades();
        Deselect();
    }

    private void HandleComponentChanged(int newCount)
    {
        if (componentCountText != null)
        {
            componentCountText.text = "Components: " + newCount;
        }
    }
}