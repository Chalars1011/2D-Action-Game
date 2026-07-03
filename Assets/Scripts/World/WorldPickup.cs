using System.Collections;
using UnityEngine;

public class WorldPickup : MonoBehaviour
{
    public enum PickupType { Gold, Bottle, RemoteAttack, Invincible, Summoning }

    [Header("拾取物类型")]
    public PickupType pickupType = PickupType.Gold;

    [Header("数量")]
    public int amount = 1;

    [Header("飞向玩家")]
    public float flyDuration = 0.4f;
    public float flyYOffset = 1.5f;
    public float minScale = 0.2f;

    [Header("抖动")]
    public float shakeAmount = 0.15f;
    public float shakeDuration = 0.15f;

    [HideInInspector] public bool playerInRange;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public void Collect()
    {
        if (!playerInRange || collected) return;
        collected = true;

        if (CanCollect())
        {
            AddToInventory();
            StartCoroutine(FlyToPlayer());
        }
        else
        {
            StartCoroutine(ShakeOnly());
        }
    }

    private IEnumerator ShakeOnly()
    {
        Vector3 startPos = transform.position;
        float timer = 0f;
        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;
            float x = Random.Range(-shakeAmount, shakeAmount);
            float y = Random.Range(-shakeAmount, shakeAmount);
            transform.position = startPos + new Vector3(x, y, 0);
            yield return null;
        }
        collected = false;
    }

    private bool CanCollect()
    {
        switch (pickupType)
        {
            case PickupType.Gold:
                return true;
            case PickupType.Bottle:
                return PlayerItemManager.Instance != null
                    && PlayerItemManager.Instance.GetBottleAmount("BloodBottle") < PlayerItemManager.Instance.GetBottleMax("BloodBottle");
            case PickupType.RemoteAttack:
                return IsItemNotFull("RemoteAttack");
            case PickupType.Invincible:
                return IsItemNotFull("Invincible");
            case PickupType.Summoning:
                return IsItemNotFull("Summoning");
        }
        return true;
    }

    private bool IsItemNotFull(string itemName)
    {
        ShopItemData item = ShopManager.Instance.GetItemByName(itemName);
        return item != null && item.currentAmount < item.maxAmount;
    }

    private void AddToInventory()
    {
        switch (pickupType)
        {
            case PickupType.Gold:
                PlayerItemManager.Instance.AddCurrency("Gold", amount);
                break;
            case PickupType.Bottle:
                PlayerItemManager.Instance.AddBottle("BloodBottle", amount);
                break;
            case PickupType.RemoteAttack:
                ShopManager.Instance.GetItemByName("RemoteAttack")?.AddItem(1);
                ShopManager.Instance.UpdateShopUI();
                break;
            case PickupType.Invincible:
                ShopManager.Instance.GetItemByName("Invincible")?.AddItem(1);
                ShopManager.Instance.UpdateShopUI();
                break;
            case PickupType.Summoning:
                ShopManager.Instance.GetItemByName("Summoning")?.AddItem(1);
                ShopManager.Instance.UpdateShopUI();
                break;
        }
    }

    private IEnumerator FlyToPlayer()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 第一阶段：抖动
        Vector3 startPos = transform.position;
        float shakeTimer = 0f;
        while (shakeTimer < shakeDuration)
        {
            shakeTimer += Time.deltaTime;
            float x = Random.Range(-shakeAmount, shakeAmount);
            float y = Random.Range(-shakeAmount, shakeAmount);
            transform.position = startPos + new Vector3(x, y, 0);
            yield return null;
        }

        // 第二阶段：飞向玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Destroy(gameObject); yield break; }

        Vector3 from = transform.position;
        Vector3 to = player.transform.position + new Vector3(0, flyYOffset, 0);
        Vector3 originalScale = transform.localScale;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        float elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            // 位移：缓入曲线，先快后慢
            transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t));
            transform.localScale = Vector3.Lerp(originalScale, originalScale * minScale, t);

            // 最后0.3秒渐隐
            float fadeT = Mathf.InverseLerp(0.7f, 1f, t);
            if (sr != null)
            {
                Color c = originalColor;
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
