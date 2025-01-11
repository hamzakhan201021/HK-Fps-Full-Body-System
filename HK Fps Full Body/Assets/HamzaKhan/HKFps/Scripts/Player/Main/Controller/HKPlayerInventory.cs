using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace HKFps
{
    public class HKPlayerInventory : MonoBehaviour
    {

        [Space]
        [Tooltip("Inventory")]
        public List<InventoryItem> Items;
        public int DefaultSelectedIndex;

        [Space]
        public Transform ItemsParent;

        private InventoryItem _currentItem;

        [Space]
        [Header("Events")]
        [Space]
        public UnityEvent OnAddNewItemComplete;
        public UnityEvent<InventoryItem> OnSelectItem;

        void Awake()
        {
            _currentItem = Items[0];
        }

        public InventoryItem CurrentInventoryItem()
        {
            return _currentItem;
        }

        public void OnAddNewItem(ItemBase item, int amount = 1)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Item.ItemType == item.ItemType)
                {
                    Items[i].Amount += amount;

                    Destroy(item.gameObject);

                    OnAddNewItemComplete.Invoke();

                    return;
                }
            }

            // If the Default Loadout doesn't have it yet, Create a new Inventory Item.
            InventoryItem inventoryItem = new InventoryItem();

            inventoryItem.Item = item;
            inventoryItem.Amount = amount;

            Items.Add(inventoryItem);

            item.transform.SetParent(ItemsParent);
            item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            item.gameObject.SetActive(false);

            OnAddNewItemComplete.Invoke();
        }

        public void SelectItem(InventoryItem item)
        {
            _currentItem = item;

            OnSelectItem.Invoke(item);
        }
    }
}