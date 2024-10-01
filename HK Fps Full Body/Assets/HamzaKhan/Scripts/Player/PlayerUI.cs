using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{


    [Space]
    [Header("Weapon")]
    [SerializeField] private GameObject _crossHairUI;
    [SerializeField] private GameObject _interactUI;

    [SerializeField] private TextMeshProUGUI _ammoText;

    [SerializeField] private Image _weaponImage;
    [SerializeField] private Image _crossHairImage;

    private TextMeshProUGUI _interactUIText;

    private Vector2 _weaponIconSize = Vector2.zero;

    void Start()
    {
        _interactUIText = _interactUI.GetComponentInChildren<TextMeshProUGUI>();
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
}
