using PixelRestaurant.Data;
using PixelRestaurant.Gacha;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaWeaponInventoryUI : MonoBehaviour
{
    [Header("Weapon Inventory")]
    [SerializeField]
    private GameObject weaponInventoryPopup;

    [SerializeField]
    private Transform weaponInventoryGrid;

    [SerializeField]
    private GameObject itemCardPrefab;

    [Header("Player Weapon")]
    [SerializeField]
    private PlayerWeaponEquipment weaponEquipment;

    [Header("Gacha Data")]
    [SerializeField]
    private GachaPoolData weaponGachaPool;

    private void Awake()
    {
        if (weaponEquipment == null)
        {
            weaponEquipment =
                FindObjectOfType<PlayerWeaponEquipment>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleWeaponInventory();
        }
    }

    public void ToggleWeaponInventory()
    {
        if (weaponInventoryPopup == null)
            return;

        bool isOpen =
            weaponInventoryPopup.activeSelf;

        if (isOpen)
        {
            CloseWeaponInventory();
        }
        else
        {
            OpenWeaponInventory();
        }
    }

    public void OpenWeaponInventory()
    {
        if (weaponInventoryPopup == null)
            return;

        weaponInventoryPopup.SetActive(true);

        RefreshWeaponInventory();
    }

    public void CloseWeaponInventory()
    {
        if (weaponInventoryPopup == null)
            return;

        weaponInventoryPopup.SetActive(false);
    }

    public void RefreshWeaponInventory()
    {
        ClearCards();

        if (weaponInventoryGrid == null)
        {
            Debug.LogError(
                "[무기 인벤토리] weaponInventoryGrid가 없습니다."
            );

            return;
        }

        if (itemCardPrefab == null)
        {
            Debug.LogError(
                "[무기 인벤토리] itemCardPrefab이 없습니다."
            );

            return;
        }

        if (weaponGachaPool == null)
        {
            Debug.LogError(
                "[무기 인벤토리] weaponGachaPool이 없습니다."
            );

            return;
        }

        if (GameManager.instance == null ||
            GameManager.instance.PlayerData == null)
        {
            Debug.LogError(
                "[무기 인벤토리] PlayerData가 없습니다."
            );

            return;
        }

        PlayerData playerData =
            GameManager.instance.PlayerData;

        if (playerData.ownedWeapons == null)
            return;

        List<OwnedWeaponData> ownedWeapons =
            new List<OwnedWeaponData>(
                playerData.ownedWeapons
            );

        // GachaPool의 무기 정보 기준으로 정렬
        ownedWeapons.Sort(
            (a, b) =>
            {
                GachaItem itemA =
                    FindGachaWeapon(a.weaponId);

                GachaItem itemB =
                    FindGachaWeapon(b.weaponId);

                if (itemA == null)
                    return 1;

                if (itemB == null)
                    return -1;

                int rarityCompare =
                    GetRarityOrder(itemA.Rarity)
                    .CompareTo(
                        GetRarityOrder(itemB.Rarity)
                    );

                if (rarityCompare != 0)
                    return rarityCompare;

                return itemA.Grade.CompareTo(
                    itemB.Grade
                );
            }
        );

        foreach (OwnedWeaponData owned in ownedWeapons)
        {
            if (owned == null ||
                owned.count <= 0)
                continue;

            GachaItem item =
                FindGachaWeapon(
                    owned.weaponId
                );

            if (item == null)
                continue;

            CreateWeaponCard(
                item,
                owned.count
            );
        }
    }

    private void CreateWeaponCard(
        GachaItem item,
        int count)
    {
        GameObject card =
            Instantiate(
                itemCardPrefab,
                weaponInventoryGrid
            );

        if (card == null)
            return;

        card.name =
            $"{item.ItemName}_WeaponCard";

        GachaResultItemDisplay display =
            card.GetComponent<GachaResultItemDisplay>();

        if (display != null)
        {
            display.SetItemInfo(
                item,
                false
            );

            display.SetCount(
                count
            );
        }

        Button button =
            card.GetComponent<Button>();

        if (button == null)
        {
            button =
                card.AddComponent<Button>();
        }

        string weaponId =
            item.ItemId;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            () =>
            {
                EquipWeapon(
                    weaponId
                );
            }
        );
    }

    private void EquipWeapon(
        string weaponId)
    {
        if (weaponEquipment == null)
        {
            Debug.LogError(
                "[무기 인벤토리] " +
                "PlayerWeaponEquipment가 없습니다."
            );

            return;
        }

        bool success =
            weaponEquipment.EquipWeapon(
                weaponId
            );

        if (success)
        {
            Debug.Log(
                $"[무기 인벤토리] {weaponId} 장착 완료"
            );
        }

        RefreshWeaponInventory();
    }

    private GachaItem FindGachaWeapon(
        string weaponId)
    {
        if (weaponGachaPool == null)
            return null;

        return weaponGachaPool.Items.Find(
            item =>
                item != null &&
                item.ItemId == weaponId
        );
    }

    private int GetRarityOrder(
        GachaRarity rarity)
    {
        switch (rarity)
        {
            case GachaRarity.Common:
                return 0;

            case GachaRarity.Rare:
                return 1;

            case GachaRarity.Unique:
                return 2;

            case GachaRarity.Epic:
                return 3;

            default:
                return 99;
        }
    }

    private void ClearCards()
    {
        if (weaponInventoryGrid == null)
            return;

        for (int i =
                 weaponInventoryGrid.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                weaponInventoryGrid
                    .GetChild(i)
                    .gameObject
            );
        }
    }
}