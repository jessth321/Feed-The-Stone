using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    DEFAULT,
    WEAPON,
    TOOL,
    CONSUMABLE
}

public enum WeaponType
{
    KNIFE,
    SWORD,
    SPEAR,
    AXE,
    HAMMER,
    GREATSWORD
}
public enum ToolType
{
    AXE,
    PICKAXE,
    SHOVEL,
    FISHINGROD
}


//[System.Serializable]
public class Item : MonoBehaviour
{
    public int id;
    public ItemType itemType;
    public string itemName;
    [TextArea] public string description;
    public int maxStack;
    public int amountHeld;
    public float itemWeight;
    public Sprite spriteUI;
    public GameObject prefab;
    public GameObject particlePrefab;

    //public Item(Item item)
    //{
    //    id = item.id;
    //    itemType = item.itemType;
    //    itemName = item.itemName;
    //    description = item.description;
    //    maxStack = item.maxStack;
    //    amountHeld = item.amountHeld;
    //    itemWeight = item.itemWeight;
    //    spriteUI = item.spriteUI;
    //    prefab = item.prefab;
    //}
}

