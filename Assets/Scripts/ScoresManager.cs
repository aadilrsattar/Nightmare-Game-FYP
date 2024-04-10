using Mirror;
using UnityEngine;

public class ScoresManager : NetworkBehaviour
{
    public class SyncDictionaryPlayerScores : SyncDictionary<uint, int> { }
    readonly public SyncDictionaryPlayerScores playerScores = new SyncDictionaryPlayerScores();

    // Define an event for score updates.
    public delegate void ScoreUpdateAction(uint playerId, int newScore);
    public static event ScoreUpdateAction OnScoreUpdated;

    public override void OnStartServer()
    {
        base.OnStartServer();
        playerScores.Callback += OnPlayerScoreChanged;
        NetworkServer.RegisterHandler<RequestScoresMessage>(OnRequestScores);

    }

    [Server]
    public void AddScore(NetworkConnectionToClient conn, int scoreToAdd)
    {
        var playerId = conn.identity.netId;

        if (playerScores.ContainsKey(playerId))
        {
            playerScores[playerId] += scoreToAdd;
        }
        else
        {
            playerScores.Add(playerId, scoreToAdd);
        }

        // Note: The actual update notification to clients is now handled by the callback
    }

    // This callback is triggered whenever the SyncDictionary changes.
    private void OnPlayerScoreChanged(SyncDictionary<uint, int>.Operation op, uint playerId, int score)
    {
        // Use an event to notify about the score change.
        OnScoreUpdated?.Invoke(playerId, score);
    }

    [Server]
    private void OnDestroy()
    {
        // Clean up to avoid memory leaks
        if (playerScores != null)
        {
            playerScores.Callback -= OnPlayerScoreChanged;
        }
    }

    // ClientRpc to update clients about score changes. This is an alternative method if you want direct updates.
    [ClientRpc]
    private void RpcUpdateClientScores(uint playerId, int newScore)
    {
        OnScoreUpdated?.Invoke(playerId, newScore);
    }

    [Server]
    public void InitializePlayerScore(NetworkConnectionToClient conn, int initialScore)
    {
        var playerId = conn.identity.netId;

        // Check if the player already has a score entry; if not, initialize with the given initialScore
        if (!playerScores.ContainsKey(playerId))
        {
            playerScores.Add(playerId, initialScore);

            // Optionally, notify clients of the new score entry
            RpcUpdateClientScores(playerId, initialScore);
        }
    }
    private void OnRequestScores(NetworkConnection conn, RequestScoresMessage msg)
    {
        foreach (var score in playerScores)
        {
            // Send each score to the requesting client
            TargetUpdateScore(conn, score.Key, score.Value);
        }
    }
    [TargetRpc]
    public void TargetUpdateScore(NetworkConnection target, uint playerId, int score)
    {
        // Find the GameUIManager instance in the scene.
        GameUIManager gameUIManager = FindObjectOfType<GameUIManager>();

        if (gameUIManager != null)
        {
            // Call the method to update or add the score entry.
            gameUIManager.UpdateScoresDisplay(playerId, score);
        }
        else
        {
            Debug.LogError("GameUIManager not found in the scene.");
        }
    }

    [Server]
    public void SendCurrentScoresTo(NetworkConnection conn)
    {
        // Assuming playerScores is the SyncDictionary holding the scores
        foreach (var kvp in playerScores)
        {
            TargetUpdateScore(conn, kvp.Key, kvp.Value);
        }
    }

    public struct RequestScoresMessage : NetworkMessage { }
}
