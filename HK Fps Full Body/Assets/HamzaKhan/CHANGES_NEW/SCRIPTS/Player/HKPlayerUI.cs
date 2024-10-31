using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HKPlayerUI : MonoBehaviour
{

    [Space]
    [Header("Main")]
    [SerializeField] private float _sizeDeltaMultiplier = 1;
    [Space]
    [SerializeField] private HKPlayerUIDirector _hKPlayerUIDirector;

    [Space]
    [Header("Game Objects")]
    [SerializeField] private GameObject _itemWheelUI;
    [Space]
    [SerializeField] private GameObject _interactUIGameObject;

    [Space]
    [Header("UI Elements")]
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _ammoText;
    [Space]
    [SerializeField] private TextMeshProUGUI _currentItemAmountText;
    [Space]
    [SerializeField] private TextMeshProUGUI _interactUIText;

    [Space]
    [Header("Image")]
    [SerializeField] private Image _weaponImage;
    [SerializeField] private Image _crossHairImage;
    [Space]
    [SerializeField] private Image _currentItemImage;
    [Space]
    [SerializeField] private Image _healthBar;
    [Space]
    [SerializeField] private Image _staminaBarLeft;
    [SerializeField] private Image _staminaBarRight;
    [Space]
    [SerializeField] private Image _damageImage;
    [SerializeField] private float _damageUIFadeInDuration = 1;
    [SerializeField] private float _damageUIFadeOutDuration = 5;
    [SerializeField] private float _maxAlpha = 1;

    private Coroutine _currentDamageFadeCoroutine;


    private void Start()
    {
        _damageImage.enabled = true;
        _damageImage.canvasRenderer.SetAlpha(0);
    }

    // Call this through the Unity Event.
    public void OnInitEventsComplete()
    {
        _hKPlayerUIDirector.OnUpdateWeaponAmmo.AddListener(UpdateWeaponAmmoText);
        _hKPlayerUIDirector.OnUpdateWeapon.AddListener(UpdateWeaponUI);
        _hKPlayerUIDirector.OnUpdateItemWheel.AddListener(UpdateItemWheelUI);

        _hKPlayerUIDirector.OnUpdateCurrentIItem.AddListener(OnUpdateCurrentItem);

        _hKPlayerUIDirector.OnUpdateCrossHairEnabled.AddListener(UpdateCrossHairActive);

        _hKPlayerUIDirector.OnUpdateInteractWindow.AddListener(UpdateInteractUI);

        _hKPlayerUIDirector.OnUpdateItemWheelActive.AddListener(UpdateItemWheelActive);

        _hKPlayerUIDirector.OnUpdateHealthUI.AddListener(UpdatePlayerHealthUI);
        _hKPlayerUIDirector.OnUpdateStaminaUI.AddListener(UpdatePlayerStaminaUI);
        _hKPlayerUIDirector.OnDeductPlayerHealth.AddListener(PlayerDamageUI);
    }

    private void UpdateWeaponAmmoText(WeaponBase currentWeapon)
    {
        // Update Weapon Ammo UI
        _ammoText.text = $"{currentWeapon.CurrentAmmo} / {currentWeapon.TotalAmmo}";
        _crossHairImage.sprite = currentWeapon.WeaponData.CrossHairSprite;
    }

    private void UpdateWeaponUI(WeaponBase currentWeapon)
    {
        // Update Weapon UI

        // Set Weapon Sprite.
        _weaponImage.sprite = currentWeapon.WeaponData.WeaponIconSprite;

        // Image Rect
        _weaponImage.rectTransform.anchoredPosition = currentWeapon.WeaponData.WeaponUIPositionOffset;
        _weaponImage.rectTransform.localRotation = Quaternion.Euler(currentWeapon.WeaponData.WeaponUIRotationOffset);
        _weaponImage.rectTransform.sizeDelta = new Vector2(currentWeapon.WeaponData.WeaponIconImageWidth, currentWeapon.WeaponData.WeaponIconImageHeight);

        // Update ammo.
        UpdateWeaponAmmoText(currentWeapon);
    }

    private void OnUpdateCurrentItem(InventoryItem inventoryItem, ItemTypeData systemItemData)
    {
        // Update Current (I) Item UI
        var iconData = systemItemData.GetItemTypeEntry(inventoryItem.Item.ItemType);

        _currentItemAmountText.text = inventoryItem.Amount.ToString();
        _currentItemImage.sprite = iconData.Icon;
        _currentItemImage.rectTransform.sizeDelta = iconData.IconSize * _sizeDeltaMultiplier;
    }

    private void UpdateItemWheelUI(UpdateItemWheelData updateItemWheelData)
    {
        // Update Item Wheel UI
        updateItemWheelData.ItemWheelUIManager.UpdateItemWheel(updateItemWheelData.InventoryItems, updateItemWheelData.SelectedItem, updateItemWheelData.ItemTypeData);
    }

    private void UpdateCrossHairActive(bool active)
    {
        _crossHairImage.enabled = !active;
    }

    private void UpdateInteractUI(bool active, IInteractable interactable)
    {
        _interactUIGameObject.SetActive(active);

        if (interactable != null)
        {
            _interactUIText.text = interactable.GetMessage();
        }
    }

    private void UpdateItemWheelActive(bool active)
    {
        _itemWheelUI.SetActive(active);
    }

    private void UpdatePlayerHealthUI(float playerHealth, float maxHealth)
    {
        _healthBar.fillAmount = playerHealth / maxHealth;
    }

    private void UpdatePlayerStaminaUI(float playerStamina, float maxStamina)
    {
        _staminaBarLeft.fillAmount = playerStamina / maxStamina;
        _staminaBarRight.fillAmount = playerStamina / maxStamina;
    }

    private void PlayerDamageUI(float prevHealth, float newHealth)
    {
        if (_currentDamageFadeCoroutine != null)
        {
            StopCoroutine(_currentDamageFadeCoroutine);
        }

        _currentDamageFadeCoroutine = StartCoroutine(ImageFade());
    }

    private IEnumerator ImageFade()
    {
        _damageImage.gameObject.SetActive(true);

        // Quick fade-in to maxAlpha
        float elapsedTime = 0f;
        while (elapsedTime < _damageUIFadeInDuration)
        {
            //_damageImage.canvasRenderer.SetAlpha(Mathf.Lerp(0f, _maxAlpha, elapsedTime / _damageUIFadeInDuration));
            _damageImage.canvasRenderer.SetAlpha(Mathf.Lerp(0f, _maxAlpha, elapsedTime / _damageUIFadeInDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _damageImage.canvasRenderer.SetAlpha(_maxAlpha);

        // Slow fade-out back to transparent
        elapsedTime = 0f;
        while (elapsedTime < _damageUIFadeOutDuration)
        {
            _damageImage.canvasRenderer.SetAlpha(Mathf.Lerp(_maxAlpha, 0f, elapsedTime / _damageUIFadeOutDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _damageImage.canvasRenderer.SetAlpha(0f);
        _currentDamageFadeCoroutine = null;

        _damageImage.gameObject.SetActive(false);
    }
}

public struct UpdateItemWheelData
{
    public List<InventoryItem> InventoryItems;
    public InventoryItem SelectedItem;
    public ItemWheelUIManager ItemWheelUIManager;
    public ItemTypeData ItemTypeData;
}