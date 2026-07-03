using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI hotkeyText; // 可选：显示使用按键
    public Button purchaseButton;

    [HideInInspector] public ShopItemData currentItem;

    public void SetItem(ShopItemData item)
    {
        currentItem = item;

        if (item != null)
        {
            icon.sprite = item.icon;
            priceText.text = item.price.ToString();

            // 只有设置了hotkeyText才显示按键
            if (hotkeyText != null)
            {
                hotkeyText.text = item.hotkey.ToString();
            }

            UpdateAmount();
        }
    }

    public void UpdateAmount()
    {
        if (currentItem != null)
        {
            amountText.text = $"x{currentItem.currentAmount}/{currentItem.maxAmount}";

            if (currentItem.currentAmount >= currentItem.maxAmount)
            {
                purchaseButton.interactable = false;
            }
            else
            {
                purchaseButton.interactable = true;
            }
        }
    }

    public void OnPurchaseButtonClick()
    {
        if (currentItem != null)
        {
            bool success = ShopManager.Instance.PurchaseItem(currentItem);
            if (!success)
            {
                Debug.Log("Purchase failed! Not enough currency.");
            }
        }
    }
}
