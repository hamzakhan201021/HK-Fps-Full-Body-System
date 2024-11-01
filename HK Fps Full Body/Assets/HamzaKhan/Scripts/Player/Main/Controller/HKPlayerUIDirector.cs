using UnityEngine.Events;
using UnityEngine;

public class HKPlayerUIDirector : MonoBehaviour
{

    [Space]
    [Header("System References")]
    [SerializeField] private HKPlayerWeaponManager _hKPlayerWeaponManager;
    [SerializeField] private HKPlayerItemSystem _hKPlayerItemSystem;
    [SerializeField] private HKPlayerInventory _hKPlayerInventory;
    [SerializeField] private HKPlayerInteractionBase _hKPlayerInteraction;
    [SerializeField] private HKPlayerHealthSystem _hKPlayerHealthSystem;
    [Space]
    [SerializeField] private ItemWheelUIManager _hKItemWheelUIManager;
    [Space]
    [SerializeField] private ItemTypeData _systemTypeData;

    [Space]
    [Header("Events")]
    [Space]
    public UnityEvent<WeaponBase> OnUpdateWeaponAmmo;
    public UnityEvent<WeaponBase> OnUpdateWeapon;
    [Space]
    public UnityEvent<InventoryItem, ItemTypeData> OnUpdateCurrentIItem;
    [Space]
    public UnityEvent<UpdateItemWheelData> OnUpdateItemWheel;
    [Space]
    public UnityEvent<bool> OnUpdateItemWheelActive;
    public UnityEvent<bool> OnUpdateCrossHairEnabled;
    public UnityEvent<bool, IInteractable> OnUpdateInteractWindow;
    [Space]
    public UnityEvent<float, float> OnUpdateHealthUI;
    public UnityEvent<float, float> OnUpdateStaminaUI;
    public UnityEvent<float, float> OnDeductPlayerHealth;
    [Space]
    public UnityEvent OnEventInitializationComplete;

    // Start is called before the first frame update
    void Start()
    {
        InitializeUnityEvents();
        PerformDefaultEventCalls();
        /*
         * 1.On Start : Update Current Item. (CHECKED)
         * 2.When we add an item : Update Current Item. (CHECKED)
         * 3.When we decrease the number of the current item : Update Current Item. (CHECKED)
         * 4.When we select another item : Update Current Item. (CHECKED)
         * 5.No shooting when switching (any) state (CHECKED) WEAPON SYSTEM
         * 6.No reloading when switching (any) state (CHECKED) WEAPON SYSTEM
         * 7.Prevent opening window tab of the item wheel when switching (CHECKED) ITEM SYSTEM
         */
    }

    private void InitializeUnityEvents()
    {
        _hKPlayerWeaponManager.OnWeaponSet.AddListener(OnWeaponChanged);

        _hKPlayerInventory.OnAddNewItemComplete.AddListener(OnItemWheelChanged);

        _hKPlayerItemSystem.OnItemReleaseUse.AddListener(OnItemWheelChangedEvent);

        _hKPlayerInventory.OnAddNewItemComplete.AddListener(OnUpdateCurrentItem);

        _hKPlayerItemSystem.OnItemReleaseUse.AddListener(OnUpdateCurrentItemEvent);

        _hKPlayerItemSystem.OnOpenItemWheel.AddListener(OnItemWheelOpen);
        _hKPlayerItemSystem.OnCloseItemWheel.AddListener(OnItemWheelClose);
        _hKPlayerItemSystem.OnItemStartUse.AddListener(OnItemWheelCloseEvent);

        _hKPlayerWeaponManager.OnAimInput.AddListener(OnUpdateCrossHairActive);

        _hKPlayerInteraction.OnUpdateInteractUI().AddListener(OnUpdateInteractWindowActive);

        _hKItemWheelUIManager.OnWheelItemClickedEvent.AddListener(OnWheelItemSelect);

        _hKPlayerHealthSystem.OnUpdateHealth.AddListener(OnUpdateHealth);
        _hKPlayerHealthSystem.OnUpdateStamina.AddListener(OnUpdateStamina);
        _hKPlayerHealthSystem.OnDeductHealth.AddListener(OnDeductHealth);

        OnEventInitializationComplete.Invoke();
    }

    private void PerformDefaultEventCalls()
    {
        OnItemWheelChanged();
        OnUpdateCurrentItem();
        OnItemWheelClose();
    }

    void Update()
    {
        OnUpdateWeaponAmmo.Invoke(_hKPlayerWeaponManager.CurrentWeapon());
    }

    private void OnItemWheelCloseEvent(ItemBase item)
    {
        OnItemWheelClose();
    }
    
    private void OnUpdateCurrentItemEvent(ItemBase item)
    {
        OnUpdateCurrentItem();
    }

    private void OnItemWheelChangedEvent(ItemBase item)
    {
        OnItemWheelChanged();
    }

    private void OnWeaponChanged(WeaponBase currentWeapon)
    {
        OnUpdateWeapon.Invoke(currentWeapon);
    }

    private void OnUpdateCurrentItem()
    {
        OnUpdateCurrentIItem.Invoke(_hKPlayerInventory.CurrentInventoryItem(), _systemTypeData);
    }

    private void OnItemWheelChanged()
    {
        UpdateItemWheelData updateItemWheelData = new UpdateItemWheelData();

        updateItemWheelData.InventoryItems = _hKPlayerInventory.Items;
        updateItemWheelData.SelectedItem = _hKPlayerInventory.CurrentInventoryItem();
        updateItemWheelData.ItemWheelUIManager = _hKItemWheelUIManager;
        updateItemWheelData.ItemTypeData = _systemTypeData;

        OnUpdateItemWheel.Invoke(updateItemWheelData);
    }

    private void OnItemWheelOpen()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnUpdateItemWheelActive.Invoke(true);

        OnItemWheelChanged();
    }

    private void OnItemWheelClose()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnUpdateItemWheelActive.Invoke(false);
    }

    private void OnUpdateCrossHairActive(bool active)
    {
        OnUpdateCrossHairEnabled.Invoke(active);
    }

    private void OnUpdateInteractWindowActive(bool active, IInteractable interactable)
    {
        OnUpdateInteractWindow.Invoke(active, interactable);
    }

    private void OnWheelItemSelect(InventoryItem selectedItem)
    {
        _hKPlayerInventory.SelectItem(selectedItem);

        OnUpdateCurrentItem();
        OnItemWheelClose();
    }

    private void OnUpdateHealth(float health, float maxHealth)
    {
        OnUpdateHealthUI.Invoke(health, maxHealth);
    }

    private void OnUpdateStamina(float stamina, float maxStamina)
    {
        OnUpdateStaminaUI.Invoke(stamina, maxStamina);
    }

    private void OnDeductHealth(float previousHealth, float currentHealth)
    {
        OnDeductPlayerHealth.Invoke(previousHealth, currentHealth);
    }
}