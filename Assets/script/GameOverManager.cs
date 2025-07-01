using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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
        // Ensure the panel is hidden initially
        gameOverPanel.SetActive(false);
        actionButton.onClick.AddListener(HandleButtonAction);
    }

    private void Update()
    {
        // Check game conditions periodically
        if (objectiveManager.IsPlayerDead() || objectiveManager.IsObjectiveCompleted())
        {
            ShowGameOverPanel(objectiveManager.IsObjectiveCompleted());
        }
    }

    private void ShowGameOverPanel(bool isVictory)
    {
        // Pause game/input and show game over panel
        Time.timeScale = 0f; // Stop game time
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

        // Unpause the game
        Time.timeScale = 1f;

        // Reset Player State
        BeatEmUpPlayerMovement player = FindObjectOfType<BeatEmUpPlayerMovement>();
        if (player != null)
        {
            Debug.Log("Player found, calling ForceReset");
            player.ForceReset();  // Call ForceReset method to reset player state
        }
        else
        {
            Debug.LogError("No player found in scene!");
        }

        // Ensure player is active again before reloading the scene
        // Reload the scene and reset the player state
        StartCoroutine(ReloadScene());
    }

    private IEnumerator ReloadScene()
    {
        // Wait a frame to make sure the player is fully reset before reloading the scene
        yield return null;

        // Ensure Time.timeScale is properly reset before scene reload
        Time.timeScale = 1f;  // Just in case it's still paused.

        // Reload the scene to start fresh
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
