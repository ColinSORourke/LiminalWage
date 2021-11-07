using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoraleUI : MonoBehaviour
{
    private Text moraleText;

    public void Construct()
    {
        moraleText = GetComponent<Text>();
    }

    public void UpdateDisplay(int value)
    {
        moraleText.text = "Morale: " + value.ToString();
    }
}
