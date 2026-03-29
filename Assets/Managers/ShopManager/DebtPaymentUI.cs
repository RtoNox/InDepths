using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebtPaymentUI : MonoBehaviour
{
    public GameObject paymentPanel;
    public GameObject statusPanel;
    public TMP_InputField inputField;
    public TextMeshProUGUI statusText;

    // Open UI
    public void OpenPanel()
    {
        paymentPanel.SetActive(true);
        inputField.text = "";
    }

    // Close UI
    public void ClosePanel()
    {
        paymentPanel.SetActive(false);
    }

    // Called when Confirm button is pressed
    public void ConfirmPayment()
    {
        if (int.TryParse(inputField.text, out int amount))
        {
            GameManager.Instance.PayDebt(amount);
            ClosePanel();
        }
        else
        {
            AudioManager.Instance.PlayFailedSFX();
            Debug.Log("Invalid input!");
        }
    }

    public void ShowStatus()
    {
        statusPanel.SetActive(true);
    }

    public void CloseStatus()
    {
        statusPanel.SetActive(false);
    }
}