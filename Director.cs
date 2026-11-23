
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Director : MonoBehaviour
{
    public static Director Instance { get; private set; }

    [Header("Game UI")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text levelText;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button nextLevelButton;

    [Header("Factory")]
    public ItemFactory factory;

    [Header("Level Settings")]
    public float level1Time = 60f;
    public float level2Time = 30f;
    public float level3Time = 25f;

    public int level1PassScore = 10;
    public int level2PassScore = 8;
    public int level3PassScore = 10;

    public bool IsGameRunning { get; private set; }
    public int CurrentLevel { get; private set; }

    private int score;
    private float timeRemaining;
    private bool canAdvance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (factory == null)
        {
            factory = FindFirstObjectByType<ItemFactory>();
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(NextLevel);
            nextLevelButton.onClick.AddListener(NextLevel);
        }

        StartLevel(1);
    }

    void StartLevel(int level)
    {
        CurrentLevel = level;

        // 每關分數重新歸零
        score = 0;
        canAdvance = false;

        if (level == 1)
            timeRemaining = level1Time;
        else if (level == 2)
            timeRemaining = level2Time;
        else
            timeRemaining = level3Time;

        // 清除上一關結算文字
        if (resultText != null)
        {
            resultText.text = "";
        }

        // 隱藏下一關按鈕
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
        }

        // 隱藏結算畫面
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        IsGameRunning = true;

        UpdateUI();

        // 開始當前關卡的掉落物生成
        if (factory != null)
        {
            factory.StartLevel(level);
        }

        // 播放背景音樂
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopEffects();
            AudioManager.Instance.PlayBGM();
        }
        else
        {
            Debug.LogWarning("找不到 AudioManager，無法播放背景音樂！");
        }

        Debug.Log("開始 Level " + level);
    }

    void Update()
    {
        if (!IsGameRunning)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            UpdateUI();
            FinishLevel();
            return;
        }

        UpdateUI();
    }

    public void AddScore(int value)
    {
        if (!IsGameRunning)
            return;

        score += value;

        UpdateUI();
    }

    void FinishLevel()
    {
        // 停止遊戲
        IsGameRunning = false;

        // 停止生成水果與炸彈
        if (factory != null)
        {
            factory.StopLevel();
        }

        // 清除畫面上剩餘的掉落物
        ClearFallingItems();

        bool passed = false;
        string message = "";

        if (CurrentLevel == 1)
        {
            passed = score >= level1PassScore;
            canAdvance = passed;

            message = passed
                ? "LEVEL 1 COMPLETE!\n" +
                  "Score: " + score + "\n" +
                  "You Passed!"
                : "LEVEL 1 FAILED!\n" +
                  "Score: " + score + "\n" +
                  "Required: " + level1PassScore;
        }
        else if (CurrentLevel == 2)
        {
            passed = score >= level2PassScore;
            canAdvance = passed;

            message = passed
                ? "LEVEL 2 COMPLETE!\n" +
                  "Score: " + score + "\n" +
                  "You Passed!"
                : "LEVEL 2 FAILED!\n" +
                  "Score: " + score + "\n" +
                  "Required: " + level2PassScore;
        }
        else
        {
            passed = score >= level3PassScore;
            canAdvance = false;

            message = passed
                ? "GAME COMPLETE!\n" +
                  "Final Score: " + score + "\n" +
                  "YOU WIN!"
                : "LEVEL 3 FAILED!\n" +
                  "Final Score: " + score + "\n" +
                  "Required: " + level3PassScore;
        }

        // 播放勝利或失敗音效
        if (AudioManager.Instance != null)
        {
            if (passed)
            {
                AudioManager.Instance.PlayVictory();
            }
            else
            {
                AudioManager.Instance.PlayDefeat();
            }
        }

        // 更新結算文字
        if (resultText != null)
        {
            resultText.text = message;
        }

        // 只有過關且尚未到第三關才顯示按鈕
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(
                canAdvance && CurrentLevel < 3
            );
        }

        // 顯示結算畫面
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        // 暫停遊戲
        Time.timeScale = 0f;

        Debug.Log(message);
    }

    void ClearFallingItems()
    {
        FallingItem[] items =
            FindObjectsByType<FallingItem>(
                FindObjectsSortMode.None
            );

        foreach (FallingItem item in items)
        {
            Destroy(item.gameObject);
        }
    }

    public void NextLevel()
    {
        if (!canAdvance)
            return;

        if (IsGameRunning)
            return;

        if (CurrentLevel >= 3)
            return;

        StartLevel(CurrentLevel + 1);
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }

        if (timerText != null)
        {
            timerText.text =
                "Time: " + Mathf.CeilToInt(timeRemaining);
        }

        if (levelText != null)
        {
            levelText.text = "Level " + CurrentLevel;
        }
    }

    void OnDestroy()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(NextLevel);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
