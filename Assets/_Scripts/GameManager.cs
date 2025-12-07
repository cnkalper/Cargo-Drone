using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public GameObject gameOverUI;

    [Header("Player References")]
    public DroneController playerDrone; // YENÝ: Drone'u buraya tanýtacaðýz

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
        Debug.Log("Game Over! Controls disabled.");

        // 1. UI'ý aç
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // 2. DRONE KONTROLLERÝNÝ KAPAT (YENÝ KISIM)
        if (playerDrone != null)
        {
            // Uçuþu durdur
            playerDrone.enabled = false;

            // Halat kontrolünü de durdur (Varsa)
            WinchController winch = playerDrone.GetComponent<WinchController>();
            if (winch != null) winch.enabled = false;

            // Fiziksel olarak yere düþmesini istersen buna dokunma.
            // Havada asýlý kalsýn (dursun) istersen þu satýrý aç:
            // playerDrone.GetComponent<Rigidbody>().isKinematic = true; 
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}