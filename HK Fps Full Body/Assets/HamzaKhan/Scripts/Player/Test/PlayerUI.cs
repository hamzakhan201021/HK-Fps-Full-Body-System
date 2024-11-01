using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    
    // CHANGES (
    [Space]
    [Header("Main")]
    [SerializeField] private ItemTypeData _systemItemData;

    // CHANGES )

    [Space]
    [Header("Weapon UI")]
    [SerializeField] private GameObject _crossHairUI;
    [SerializeField] private GameObject _interactUI;

    [SerializeField] private TextMeshProUGUI _ammoText;

    [SerializeField] private Image _weaponImage;
    [SerializeField] private Image _crossHairImage;



    // CHANGES (
    // CHANGES ITEM FIELD )
    [Space]
    [Header("Item UI")]
    [SerializeField] private GameObject _cancelPromptUI;
    [SerializeField] private ItemWheelUIManager _itemWheel;
    [Space(5)]
    [SerializeField] private Image _currentItemImage;
    [SerializeField] private float _sizeDeltaMultiplier = 1;
    [Space(3)]
    [SerializeField] private TextMeshProUGUI _currentItemAmountText;
    // CHANGES )
    // CHANGES )



    private TextMeshProUGUI _interactUIText;

    private PlayerController _controller;

    private Vector2 _weaponIconSize = Vector2.zero;

    void Start()
    {
        _interactUIText = _interactUI.GetComponentInChildren<TextMeshProUGUI>();

        // CHANGES (
        //_itemWheel.Initialize(this);

        _controller = GetComponent<PlayerController>();
        // )
    }

    public void SetCrossHairShow(bool show)
    {
        _crossHairUI.SetActive(show);
    }

    public void SetInteractUIShow(bool show, IInteractable interactable = null)
    {
        _interactUI.SetActive(show);

        if (interactable != null)
        {
            _interactUIText.text = interactable.GetMessage();
        }
    }

    public void SetCurrentWeaponUI(WeaponBase currentWeapon)
    {
        // Set the Cross Hair and Weapon Images.
        _crossHairImage.sprite = currentWeapon.WeaponData.CrossHairSprite;
        _weaponImage.sprite = currentWeapon.WeaponData.WeaponIconSprite;

        // Icon Vector
        _weaponIconSize.x = currentWeapon.WeaponData.WeaponIconImageWidth;
        _weaponIconSize.y = currentWeapon.WeaponData.WeaponIconImageHeight;

        // Image Rect
        _weaponImage.rectTransform.sizeDelta = _weaponIconSize;
        _weaponImage.rectTransform.anchoredPosition = currentWeapon.WeaponData.WeaponUIPositionOffset;
        _weaponImage.rectTransform.localRotation = Quaternion.Euler(currentWeapon.WeaponData.WeaponUIRotationOffset);

        // Set the ammoText.
        _ammoText.text = currentWeapon.CurrentAmmo + " / " + currentWeapon.TotalAmmo.ToString();
    }

    // CHANGES NEW FUNCTIONS BELOW (
    public void SetActiveItemWheel(bool active = true)
    {
        _itemWheel.ShowItemWheel(active);
    }

    public void SetActiveCancelPromptUI(bool active = true)
    {
        _cancelPromptUI.SetActive(active);
    }

    public void SelectItem(InventoryItem item)
    {
        SetCurrentItem(item);

    }

    public void UpdateItemWheel(List<InventoryItem> inventoryItems, InventoryItem selectedItem)
    {
        _itemWheel.UpdateItemWheel(inventoryItems, selectedItem, _systemItemData);

        SetCurrentItem(selectedItem);
    }

    private void SetCurrentItem(InventoryItem selectedItem)
    {
        var iconData = _systemItemData.GetItemTypeEntry(selectedItem.Item.ItemType);

        _currentItemAmountText.text = selectedItem.Amount.ToString();
        _currentItemImage.sprite = iconData.Icon;
        _currentItemImage.rectTransform.sizeDelta = iconData.IconSize * _sizeDeltaMultiplier;
    }

    // CHANGES )
}
