using UnityEngine;
using UnityEngine.Events; 

// SORUMLULUK: Nesnenin canını takip eder. Drone'a eklenir.
public class Health : MonoBehaviour
{
    [Header("Can Ayarları")]
    public int maxHealth = 100;
    private int currentHealth;

    [Tooltip("Bu nesnenin Kritik Çarpışma Hızı eşiği.")]
    public float criticalDamageVelocity = 10f;

    [Header("Olaylar")]
    public UnityEvent OnTakeDamage; 
    public UnityEvent OnDeath;      

    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public int CurrentHealth => currentHealth;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        OnTakeDamage.Invoke(); 

        Debug.Log($"DRONE HASAR ALDI! Kalan Can: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath.Invoke(); 
        Debug.LogError("DRONE İMHA EDİLDİ! GÖREV BAŞARISIZ!");
        // İSTEK: Canı bitince drone yok olacak.
        Destroy(gameObject); 
    }
}