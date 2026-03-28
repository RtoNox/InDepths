using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebtPaymentUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField inputField;
    public TextMeshProUGUI statusText;

    // Open UI
    public void OpenPanel()
    {
        panel.SetActive(true);
        inputField.text = "";
    }

    // Close UI
    public void ClosePanel()
    {
        panel.SetActive(false);
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
            Debug.Log("Invalid input!");
        }
    }
}