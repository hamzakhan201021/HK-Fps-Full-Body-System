using UnityEngine;

namespace HKFps
{
    [System.Serializable]
    public class InventoryItem
    {
        [Tooltip("Amount of items")]
        public int Amount = 2;
        [Tooltip("Item")]
        public ItemBase Item;
    }
}