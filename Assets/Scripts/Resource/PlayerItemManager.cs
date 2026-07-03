using System.Collections.Generic;
using UnityEngine;
using GameArchitecture.Narrative;

public class PlayerItemManager : MonoBehaviour, IStateTracked
{
    public static PlayerItemManager Instance { get; private set; }

    // ʹ���ֵ������ͬ�Ļ��Һ�ƿ��
    private Dictionary<string, CurrencyData> currencies = new Dictionary<string, CurrencyData>();
    private Dictionary<string, BottleData> bottles = new Dictionary<string, BottleData>();

    [SerializeField] private CurrencyData goldCurrency;
    [SerializeField] private BottleData bloodBottleData;
    // �������Ӹ���ƿ������

    private void Awake()
    {
        // ����ģʽʵ��
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // �����Һ�ƿ�����ӵ��ֵ���
        currencies.Add("Gold", goldCurrency);
        bottles.Add("BloodBottle", bloodBottleData);
        // �������Ӹ���ƿ��

        // ��������
        LoadData();

        // 注册到 SaveManager
        if (SaveManager.Instance != null)
            SaveManager.Instance.RegisterTracker(this);
    }

    private void Start()
    {
        currencies["Gold"].UpdateAmount();
        bottles["BloodBottle"].UpdateAmount();

    }

    #region ���ҹ���
    // ��ȡָ�����ҵĵ�ǰ����
    public int GetCurrencyAmount(string currencyName)
    {
        if (currencies.ContainsKey(currencyName))
        {
            return currencies[currencyName].currentAmount;
        }
        Debug.LogError($"Currency {currencyName} not found.");
        return 0;
    }

    // ����ָ������
    public void AddCurrency(string currencyName, int amount)
    {
        if (amount < 0) { Debug.LogError("Cannot add negative currency."); return; }
        if (!currencies.ContainsKey(currencyName)) { Debug.LogError($"Currency {currencyName} not found."); return; }

        currencies[currencyName].AddCurrency(amount);
        SaveData();
    }

    // ����ָ������
    public bool SpendCurrency(string currencyName, int amount)
    {
        if (amount < 0) { Debug.LogError("Cannot spend negative currency."); return false; }
        if (!currencies.ContainsKey(currencyName)) { Debug.LogError($"Currency {currencyName} not found."); return false; }

        if (currencies[currencyName].SpendCurrency(amount))
        {
            SaveData();
            return true;
        }
        return false;
    }
    #endregion

    #region ƿ�ӹ���
    // ��ȡָ��ƿ�ӵĵ�ǰ����
    public int GetBottleAmount(string bottleName)
    {
        if (bottles.ContainsKey(bottleName))
        {
            return bottles[bottleName].currentAmount;
        }
        Debug.LogError($"Bottle {bottleName} not found.");
        return 0;
    }

    public int GetBottleMax(string bottleName)
    {
        if (bottles.ContainsKey(bottleName))
            return bottles[bottleName].maxAmount;
        return 0;
    }

    // ����ָ��ƿ��
    public void AddBottle(string bottleName, int amount)
    {
        if (amount < 0) { Debug.LogError("Cannot add negative bottles."); return; }
        if (!bottles.ContainsKey(bottleName)) { Debug.LogError($"Bottle {bottleName} not found."); return; }

        bottles[bottleName].AddBottle(amount);
        SaveData();
    }

    // ʹ��ָ��ƿ��
    public bool UseBottle(string bottleName)
    {
        if (!bottles.ContainsKey(bottleName)) { Debug.LogError($"Bottle {bottleName} not found."); return false; }

        if (bottles[bottleName].UseBottle())
        {
            SaveData();
            return true;
        }
        return false;
    }
    #endregion

    // ���ݱ���
    private void SaveData()
    {
        foreach (var pair in currencies)
        {
            PlayerPrefs.SetInt(pair.Key + "Amount", pair.Value.currentAmount);
        }

        foreach (var pair in bottles)
        {
            PlayerPrefs.SetInt(pair.Key + "Amount", pair.Value.currentAmount);
        }

        PlayerPrefs.Save();
    }

    // ���ݼ���
    private void LoadData()
    {
        foreach (var pair in currencies)
        {
            if (PlayerPrefs.HasKey(pair.Key + "Amount"))
            {
                pair.Value.currentAmount = PlayerPrefs.GetInt(pair.Key + "Amount");
            }
        }

        foreach (var pair in bottles)
        {
            if (PlayerPrefs.HasKey(pair.Key + "Amount"))
            {
                pair.Value.currentAmount = PlayerPrefs.GetInt(pair.Key + "Amount");
            }
        }
    }

    // IStateTracked
    [System.Serializable] private class EcoSnap { public int gold, bottles; }
    // 供 SaveManager 命令直调（绕过公开 API，防递归）
    public void CmdAddGold(int a) { currencies["Gold"].AddCurrency(a); SaveData(); }
    public void CmdSpendGold(int a) { currencies["Gold"].SpendCurrency(a); SaveData(); }
    public void CmdAddBottle(int a) { bottles["BloodBottle"].AddBottle(a); SaveData(); }

    string IStateTracked.StateId => "Economy";
    string IStateTracked.CaptureState()
    {
        return JsonUtility.ToJson(new EcoSnap { gold = GetCurrencyAmount("Gold"), bottles = GetBottleAmount("BloodBottle") });
    }
    void IStateTracked.RestoreState(string json)
    {
        var s = JsonUtility.FromJson<EcoSnap>(json);
        int cg = GetCurrencyAmount("Gold"); if (cg > 0) SpendCurrency("Gold", cg);
        if (s.gold > 0) AddCurrency("Gold", s.gold);
        int cb = GetBottleAmount("BloodBottle");
        for (int i = 0; i < cb; i++) UseBottle("BloodBottle");
        if (s.bottles > 0) AddBottle("BloodBottle", s.bottles);
    }
}