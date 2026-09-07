using TMPro;
using UnityEngine;

public class FlashlightUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    public TextMeshProUGUI TMPtext;
    [SerializeField] private Canvas rootCanvas;

    [Header("State")]
    public bool isOpen = false;

    public void DisableRootCanvas() => rootCanvas.gameObject.SetActive(false);
    public void EnableRootCanvas() => rootCanvas.gameObject.SetActive(true);

    public void Open()
    {
        isOpen = true;
        panel.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        panel.SetActive(false);
    }

    public void SetText(string text)
    {
        TMPtext.text = text;
    }
}