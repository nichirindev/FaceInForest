using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupUI : MonoBehaviour
{
    [Header("Interaction Icon")]
    public GameObject interactionPanel;
    public Image itemIconImage;

    [Header("Counters")]
    public TMP_Text keyCountText;
    public TMP_Text noteCountText;
    public TMP_Text dollCountText;

    private int keyCount;
    private int noteCount;
    private int dollCount;

    private void Start()
    {
        HideIcon();
        UpdateAllCounters();
    }

    public void ShowIcon(Sprite icon)
    {
        if (interactionPanel == null || itemIconImage == null) return;

        interactionPanel.SetActive(true);
        if (icon != null)
            itemIconImage.sprite = icon;
    }

    public void HideIcon()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    public void AddItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.Key:
                keyCount++;
                break;
            case ItemType.Note:
                noteCount++;
                break;
            case ItemType.Doll:
                dollCount++;
                break;
        }

        UpdateAllCounters();
    }

    private void UpdateAllCounters()
    {
        if (keyCountText != null)
            keyCountText.text = keyCount.ToString();
        if (noteCountText != null)
            noteCountText.text = noteCount.ToString();
        if (dollCountText != null)
            dollCountText.text = dollCount.ToString();
    }
}
