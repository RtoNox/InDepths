using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIReferences : MonoBehaviour
{
    [Header("Day Results UI")]
    public TextMeshProUGUI dayCompleted;
    public TextMeshProUGUI moneyEarned;
    public TextMeshProUGUI itemsSold;
    public GameObject resultsPanel;
    public Button exitResultsButton;

    [Header("Game Over UI")]
    public GameObject gameOverScreen;
    public GameObject playerHUD;
    public Button retryButton;
    public Button mainMenuButton;

    [Header("Debt UI")]
    public TextMeshProUGUI debtAmount;

    void Start()
    {
        GameManager gm = GameManager.Instance;

        gm.dayCompleted = dayCompleted;
        gm.moneyEarned = moneyEarned;
        gm.itemsSold = itemsSold;
        gm.resultsPanel = resultsPanel;
        gm.debtAmount = debtAmount;

        gm.gameOverScreen = gameOverScreen;
        gm.playerHUD = playerHUD;

        retryButton.onClick.AddListener(() => gm.RetryDay());
        mainMenuButton.onClick.AddListener(() => gm.LoadMainMenu());
        exitResultsButton.onClick.AddListener(() => gm.CloseResultsUI());
    }
}