using Fungus;
using UnityEngine;

public class NPCTalk : MonoBehaviour
{
    public string ChartName;
    public string targetTag = "Player";
    public bool canChat = false;
    public bool openShopAfterDialogue = true; // 对话结束后是否打开商店

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Chat();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == targetTag)
        {
            canChat = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == targetTag)
        {
            canChat = false;
        }
    }

    void Chat()
    {
        if (canChat)
        {
            Flowchart flowChart = GetComponent<Flowchart>();
            if (flowChart.HasBlock(ChartName))
            {
                flowChart.ExecuteBlock(ChartName);
            }
        }
    }

    // 由Fungus对话结束时调用
    public void OnDialogueEnded()
    {
        if (openShopAfterDialogue)
        {
            ShopManager.Instance.OpenShop();
        }
    }
}
