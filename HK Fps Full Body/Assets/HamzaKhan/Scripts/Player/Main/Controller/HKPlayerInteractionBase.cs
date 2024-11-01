using UnityEngine;
using UnityEngine.Events;

public abstract class HKPlayerInteractionBase : MonoBehaviour
{
    public abstract void UpdatePlayerInteractions(bool reloading, bool interactTriggered);
    public abstract void SwapWeapon(WeaponBase newWeapon);
    public abstract void AddNewItem(ItemBase item, int amount = 1);
    public abstract void PickNewWeapon();
    public abstract void AddAmmo(int ammoToAdd);

    public abstract bool IsInInteractionRange();

    public abstract UnityEvent OnSwap();
    public abstract UnityEvent<ItemBase, int> OnAddNewItem();
    public abstract UnityEvent<WeaponBase> OnPickNewWeapon();
    public abstract UnityEvent<int> OnAddAmmo();
    public abstract UnityEvent<bool, IInteractable> OnUpdateInteractUI();

    public abstract WeaponBase CurrentWeapon();
}