using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text killText = null;
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;
    [SerializeField] private GameObject taskStatusParent = null;
    [SerializeField] private GameObject leaderFrame = null;
    public int kills;
    private Quaternion startRotation;

    private void Awake()
    {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
        healthBarParent.SetActive(false);
        startRotation = healthBarParent.transform.rotation;
    }
    void Update()
    {
        healthBarParent.transform.rotation = startRotation;
        taskStatusParent.transform.rotation = startRotation;
    }
    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }

    public void EnableLeaderIcon()
    {
        leaderFrame.SetActive(true);
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        if (currentHealth < maxHealth) {
            healthBarParent.SetActive(true);
        }
    }

    public void SetHealthBarColor (Color newColor)
    {
        healthBarImage.color = newColor;
    }
    public void HandleKillText()
    {
        killText.text = kills.ToString();
        kills++;
    }
}
