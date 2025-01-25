using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameController Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private int totalGameplaySeconds = 60;
    [SerializeField] private int remainingSeconds;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private int deathDepthThreshold;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        remainingSeconds = totalGameplaySeconds;
        StartCoroutine(TimerCoroutine());
    }
    private void Update()
    {
        if(playerObject.transform.position.y<-deathDepthThreshold)
        {
            GameOver();
        }
    }
    public void UpdateScore(float score)
    {
        scoreText.text = score.ToString();
    }
    public void UpdateTime(int secondsRemaining)
    {
        timerText.text = $"{secondsRemaining / 60}:{secondsRemaining % 60}";
    }

    private IEnumerator TimerCoroutine()
    {
        while (remainingSeconds > 0)
        {
            UpdateTime(remainingSeconds);
            remainingSeconds--;
            yield return new WaitForSeconds(1);
        }
        GameOver();

    }
    private void GameOver()
    {
        StopCoroutine(TimerCoroutine());
        endGamePanel.SetActive(true);
        Time.timeScale = 0;
    }
}
