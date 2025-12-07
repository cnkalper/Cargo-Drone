using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // BU SATIR ÞART! (Coroutine için)

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public GameObject gameOverUI;
    public GameObject winUI;

    [Header("Player References")]
    public DroneController playerDrone;

    [Header("Win Settings")]
    [Tooltip("Kazandýktan sonra oyunun durmasý için kaç saniye beklensin?")]
    public float winDelay = 2.0f; // Varsayýlan 2 saniye bekle

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over!");

        if (gameOverUI != null) gameOverUI.SetActive(true);

        if (playerDrone != null)
        {
            playerDrone.enabled = false;
            WinchController winch = playerDrone.GetComponent<WinchController>();
            if (winch != null) winch.enabled = false;
        }
    }

    public void LevelComplete()
    {
        if (isGameOver) return;

        isGameOver = true; // Oyunu "bitti" moduna al ama henüz durdurma
        Debug.Log("YOU WIN! Waiting for delay...");

        // Gecikmeli bitiþi baþlat
        StartCoroutine(WinSequence());
    }

    // Zamanlayýcý Fonksiyon
    IEnumerator WinSequence()
    {
        // Belirlenen süre kadar bekle (Oyun bu sýrada akmaya devam eder)
        yield return new WaitForSeconds(winDelay);

        // --- SÜRE DOLDU, ÞÝMDÝ DURDUR ---

        // 1. Kazanma Ekranýný Aç
        if (winUI != null) winUI.SetActive(true);

        // 2. Oyunu Durdur
        Time.timeScale = 0f;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}