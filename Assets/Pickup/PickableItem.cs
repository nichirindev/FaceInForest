using UnityEngine;

public enum ItemType
{
    Key,
    Note,
    Doll
}

public class PickableItem : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemType itemType;

    [Header("UI Icon (shown when looking at item)")]
    public Sprite itemIcon;

    [Header("Optional")]
    public string itemName = "Item";

    private bool isPickedUp;

    public void PickUp()
    {
        if (isPickedUp) return;
        isPickedUp = true;
        gameObject.SetActive(false);
    }
}
