using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
[RequireComponent(typeof(TextMeshProUGUI))]
public class UpdateTextValue : MonoBehaviour
{
    public float amplifier = 1.0f;
    [SerializeField] private Scrollbar m_bar;
    private TextMeshProUGUI tmp;
    // Start is called before the first frame update
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    public void UpdateText()
    {
        if (!m_bar) return;
        tmp.SetText((m_bar.value * amplifier).ToString("0.00"));

    }
}
