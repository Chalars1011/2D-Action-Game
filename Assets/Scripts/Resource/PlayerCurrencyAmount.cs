using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCurrencyAmount : MonoBehaviour
{
    public TMP_Text currencyAmount;

    public void UpdateCurrency(int amount)
    {
        if (currencyAmount != null)
        {
            currencyAmount.text = amount.ToString();
        }
    }
}

