
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    
    [Header("Panel References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI panelText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Game References")]
    [SerializeField] private ObjectiveManager objectiveManager;
    [SerializeField] private BeatEmUpPlayerMovement playerMovement;

    private void Start()
    {
        // Pastikan panel tersembunyi di awal
        gameOverPanel.SetActive(false);
        actionButton.onClick.AddListener(HandleButtonAction);
    }

    private void Update()
    {
        // Cek kondisi game secara berkala
        if (objectiveManager.IsPlayerDead() || objectiveManager.IsObjectiveCompleted())
        {
            ShowGameOverPanel(objectiveManager.IsObjectiveCompleted());
        }
    }

    private void ShowGameOverPanel(bool isVictory)
    {
        // Hentikan game/input
        Time.timeScale = 0f; // Jeda game
        gameOverPanel.SetActive(true);

        if (isVictory)
        {
            panelText.text = "You Win! Congratulations!";
            buttonText.text = "Next Level";
        }
        else
        {
            panelText.text = "Game Over! Try again!";
            buttonText.text = "Retry";
        }
    }

    private void HandleButtonAction()
    {
        Debug.Log("Game Over Manager: Handling Retry");

        // Lanjutkan waktu permainan
        Time.timeScale = 1f;

        // Cari player di scene
        BeatEmUpPlayerMovement player = FindObjectOfType<BeatEmUpPlayerMovement>();
        if (player != null)
        {
            Debug.Log("Player found, calling ForceReset");
            player.ForceReset(); // Gunakan method baru
        }
        else
        {
            Debug.LogError("No player found in scene!");
        }

        // Muat ulang scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
