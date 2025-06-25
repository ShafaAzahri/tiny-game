using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private BeatEmUpPlayerMovement playerMovement;
    [SerializeField] private MovementMusuh bossEnemy; // Reference ke script boss musuh

    // Enum untuk status objektif
    public enum ObjectiveState
    {
        InProgress,
        PlayerDead,
        BossDefeated
    }

    private ObjectiveState currentObjectiveState = ObjectiveState.InProgress;

    private void Update()
    {
        UpdateObjectiveState();
    }

    private void UpdateObjectiveState()
    {
        // Kondisi 1: Player Mati
        if (!playerMovement.IsAlive())
        {
            currentObjectiveState = ObjectiveState.PlayerDead;
        }
        // Kondisi 2: Boss Mati
        else if (bossEnemy != null && bossEnemy.IsDead()) // Pastikan method IsDead() ada
        {
            currentObjectiveState = ObjectiveState.BossDefeated;
        }
        else
        {
            currentObjectiveState = ObjectiveState.InProgress;
        }
    }

    // Method untuk Game Over Manager mengetahui kondisi
    public bool IsObjectiveCompleted()
    {
        return currentObjectiveState == ObjectiveState.BossDefeated;
    }

    // Method untuk Game Over Manager mengetahui apakah player mati
    public bool IsPlayerDead()
    {
        return currentObjectiveState == ObjectiveState.PlayerDead;
    }

    // Getter untuk state saat ini (opsional, untuk debugging)
    public ObjectiveState GetCurrentObjectiveState()
    {
        return currentObjectiveState;
    }
}