using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText; // Assign in the inspector
    [SerializeField] private GameObject scoreEntryPrefab; // Prefab for a score entry
    [SerializeField] private Transform scoresPanel; // Parent transform for score entries

    [SyncVar(hook = nameof(OnTimeRemainingChanged))]
    private float _timeRemaining = 300f; // 5 minutes in seconds, synced across clients

    // Dictionary to keep track of score entries for quick access
    private Dictionary<uint, TextMeshProUGUI> playerScoreEntries = new Dictionary<uint, TextMeshProUGUI>();

    private void OnEnable()
    {
        ScoresManager.OnScoreUpdated += UpdateScoresDisplay;
    }

    private void OnDisable()
    {
        ScoresManager.OnScoreUpdated -= UpdateScoresDisplay;
    }

    // Hook method called when timeRemaining changes
    void OnTimeRemainingChanged(float oldTime, float newTime)
    {
        UpdateTimerDisplay(newTime);
    }

    private void UpdateTimerDisplay(float timeInSeconds)
    {
        // Format the time as MM:SS
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    [ServerCallback]
    void Update()
    {
        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
        }
    }


    public void UpdateScoresDisplay(uint playerId, int newScore)
    {
        if (!playerScoreEntries.ContainsKey(playerId))
        {
            // Instantiate a new score entry and set it as a child of scoresPanel
            GameObject scoreEntryGO = Instantiate(scoreEntryPrefab, scoresPanel);
            TextMeshProUGUI scoreText = scoreEntryGO.GetComponent<TextMeshProUGUI>();

            // Add to dictionary for quick access
            playerScoreEntries[playerId] = scoreText;
        }

        // Update the score text
        playerScoreEntries[playerId].text = $"Player {playerId}: {newScore}";
    }
}
