using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PickupUI pickupUI;

    [Header("Settings")]
    public float pickupRange = 3f;
    public LayerMask pickableLayer = ~0;

    private PickableItem currentTarget;

    private void Update()
    {
        DetectPickable();
        HandlePickupInput();
    }

    private void DetectPickable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickableLayer))
        {
            PickableItem item = hit.collider.GetComponent<PickableItem>();

            if (item != null && item.gameObject.activeInHierarchy)
            {
                if (currentTarget != item)
                {
                    currentTarget = item;
                    pickupUI.ShowIcon(item.itemIcon);
                }
                return;
            }
        }

        if (currentTarget != null)
        {
            currentTarget = null;
            pickupUI.HideIcon();
        }
    }

    private void HandlePickupInput()
    {
        if (currentTarget == null) return;

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            PickupItem(currentTarget);
        }
    }

    public void PickupItem(PickableItem item)
    {
        if (item == null) return;

        ItemType type = item.itemType;
        item.PickUp();
        pickupUI.AddItem(type);
        pickupUI.HideIcon();
        currentTarget = null;
    }
}
