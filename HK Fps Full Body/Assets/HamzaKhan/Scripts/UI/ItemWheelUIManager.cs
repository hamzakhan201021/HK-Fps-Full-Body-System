using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using TMPro;

namespace HKFps
{
    public class ItemWheelUIManager : MonoBehaviour
    {
        [Header("Wheel Items")]
        [SerializeField] private List<WheelItem> _wheelItems;  // List of items on the wheel.

        [Header("Icon Settings")]
        [SerializeField] private float _sizeDeltaMultiplier = 1;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _itemTitleText;
        [Space]
        [Header("Events")]
        [Space]
        public UnityEvent<InventoryItem> OnWheelItemClickedEvent;

        private string _selectedItemName;  // Track the currently selected item's name.

        /// <summary>
        /// Sets all Wheel Items: Icon, Size, Button Interactable, Amount.
        /// </summary>
        private void SetupItemWheel(List<InventoryItem> items, ItemTypeData itemTypeData)
        {
            foreach (var wheelItem in _wheelItems)
            {
                var itemData = itemTypeData.GetItemTypeEntry(wheelItem.ItemType);

                wheelItem.Image.sprite = itemData.Icon;
                wheelItem.Image.rectTransform.sizeDelta = itemData.IconSize * _sizeDeltaMultiplier;

                wheelItem.AmountText.text = "0";
            }

            foreach (var inventoryItem in items)
            {
                WheelItem matchingWheelItem = _wheelItems.Find(wheelItem => wheelItem.ItemType == inventoryItem.Item.ItemType);

                if (matchingWheelItem != null)
                {
                    matchingWheelItem.Image.enabled = true;
                    matchingWheelItem.AmountText.text = inventoryItem.Amount.ToString();

                    matchingWheelItem.Button.onClick.RemoveAllListeners();
                    InventoryItem localItem = inventoryItem;
                    matchingWheelItem.Button.onClick.AddListener(() => OnWheelItemClicked(localItem));

                    // Ensure `DisplayName` is used for UI instead of enum name.
                    matchingWheelItem.OnHoverEnter.AddListener((_) => UpdateItemNameDisplay(localItem.Item.ItemName));
                    matchingWheelItem.OnHoverExit.AddListener((_) => UpdateItemNameDisplay(_selectedItemName));
                }
            }
        }

        private void UpdateItemNameDisplay(string itemName)
        {
            _itemTitleText.text = itemName;
        }

        /// <summary>
        /// This method is called when an item button is clicked.
        /// </summary>
        private void OnWheelItemClicked(InventoryItem item)
        {
            UpdateSelectedItemName(item.Item.ItemName);

            OnWheelItemClickedEvent.Invoke(item);
        }

        /// <summary>
        /// Updates the item wheel and sets the initial selected item text.
        /// </summary>
        public void UpdateItemWheel(List<InventoryItem> inventoryItems, InventoryItem selectedItem, ItemTypeData itemTypeData)
        {
            SetupItemWheel(inventoryItems, itemTypeData);

            // Store the selected item's name.
            UpdateSelectedItemName(selectedItem.Item.ItemName);

            for (int i = 0; i < _wheelItems.Count; i++)
            {
                _wheelItems[i].Button.interactable = false;

                foreach (var inventoryItem in inventoryItems)
                {
                    if (_wheelItems[i].ItemType == inventoryItem.Item.ItemType)
                    {
                        _wheelItems[i].Button.interactable = true;

                        if (_wheelItems[i].ItemType == selectedItem.Item.ItemType)
                        {
                            _wheelItems[i].Button.Select();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Displays the item wheel UI.
        /// </summary>
        public void ShowItemWheel(bool show = true)
        {
            gameObject.SetActive(show);
        }

        /// <summary>
        /// Updates the selected item name and displays it in the center.
        /// </summary>
        private void UpdateSelectedItemName(string itemName)
        {
            _selectedItemName = itemName;
            _itemTitleText.text = itemName;
        }
    }
}