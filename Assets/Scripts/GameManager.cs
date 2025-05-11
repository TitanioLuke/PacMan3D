// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public GameObject gameOverCanvas;
    public GameObject victoryCanvas;

    [Header("Config")]
    public GameObject fruitPrefab;
    public int initialLives  = 6;
    public int scoreForFruit = 100;
    public int scoreForWin   = 2200;

    private int score;
    private int lives;
    private bool fruitSpawned;
    private int pelletsRemaining;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (victoryCanvas  != null) victoryCanvas.SetActive(false);

        StartGame();
    }

    public void StartGame()
    {
        score        = 0;
        lives        = initialLives;
        fruitSpawned = false;
        UpdateUI();
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateUI();

        if (score >= scoreForFruit && !fruitSpawned && fruitPrefab != null)
        {
            Instantiate(fruitPrefab, new Vector3(12, 0.5f, -12), Quaternion.identity);
            fruitSpawned = true;
        }

        if (score >= scoreForWin)
            LoadWinScene();
    }

    public void LoseLife()
    {
        lives--;
        UpdateUI();

        if (lives <= 1)
        {
            if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
            DisablePlayerAndGhosts();
        }
    }

    public void ActivateSpeedBoost(float duration, float boostedSpeed)
    {
        var pac = GameObject.FindGameObjectWithTag("Player");
        if (pac != null)
        {
            var pm = pac.GetComponent<PacManMovement>();
            if (pm != null)
                pm.ActivateSpeedBoost(duration, boostedSpeed);
        }
    }

    // CORREÇÃO: removido uso de '?.enabled = false'
    private void DisablePlayerAndGhosts()
    {
        // Desliga Pac-Man
        var pac = GameObject.FindGameObjectWithTag("Player");
        if (pac != null)
        {
            var pm = pac.GetComponent<PacManMovement>();
            if (pm != null)
                pm.enabled = false;
        }

        // Desliga fantasmas
        foreach (var ghost in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            var gm = ghost.GetComponent<GhostMovement>();
            if (gm != null)
                gm.enabled = false;
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (livesText != null) livesText.text = $"Lives: {lives / 2}";
    }

    public void InitPelletCount(int totalPellets)
    {
        pelletsRemaining = totalPellets;
    }

    public void PelletEaten()
    {
        pelletsRemaining--;
        if (pelletsRemaining <= 0)
            LoadWinScene();
    }

    private void LoadWinScene()
    {
        SceneManager.LoadScene("WinScreen");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
