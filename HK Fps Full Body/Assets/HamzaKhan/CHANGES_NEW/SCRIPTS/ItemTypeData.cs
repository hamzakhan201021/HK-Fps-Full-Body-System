using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTypeData", menuName = "ScriptableObjects/ItemTypeData")]
public class ItemTypeData : ScriptableObject
{
    public List<ItemTypeEntry> ItemTypeEntries;

    // Find the item data based on the type
    public ItemTypeEntry GetItemTypeEntry(ItemType itemType)
    {
        return ItemTypeEntries.Find(entry => entry.ItemType == itemType);
    }
}

[System.Serializable]
public class ItemTypeEntry
{
    public ItemType ItemType;
    public Sprite Icon;
    public Vector2 IconSize;
}