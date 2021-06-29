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
    [SerializeField] private Image healthBarImageLast = null;
    [SerializeField] private GameObject leaderFrame = null;
    [SerializeField] private TMP_Text currentHealthText = null;
    [SerializeField] private TMP_Text levelText = null;
    [SerializeField] private GameObject kingIcon = null;
    [SerializeField] private GameObject heroIcon = null;
    [SerializeField] private GameObject defaultIcon = null;
    [SerializeField] public Sprite healthBarEnemyImage = null;
    private float lastDisplayTime;
    private float displayDelay = 3f;
    public int kills;
    private Quaternion startRotation;
    
    private void Awake()
    {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
        if (GetComponent<Unit>().unitType == UnitMeta.UnitType.KING || GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO || GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
            healthBarParent.SetActive(true);
        else
            healthBarParent.SetActive(false);
        startRotation = healthBarParent.transform.rotation;
    }
    void Update()
    {
        healthBarParent.transform.rotation = startRotation;
    }
    private void OnDestroy()
    {
        health.ClientOnHealthUpdated -= HandleHealthUpdated;
    }
    public void flipHealthBar()
    {
        //Debug.Log($"{name} {tag} flipHealthBar {startRotation} {startRotation * Quaternion.Euler(0, 180, 0)} ");
        startRotation *= Quaternion.Euler(0, 180, 0); // this adds a 90 degrees Y rotation
    }
    public void EnableLeaderIcon()
    {
        leaderFrame.SetActive(true);
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth, int lastDamageDeal)
    {
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        if(lastDisplayTime + displayDelay < Time.time)
            StartCoroutine(LerpHealthBar(healthBarImageLast, currentHealth, lastDamageDeal, maxHealth, 5f));
        currentHealthText.text = currentHealth.ToString();
        if (currentHealth < maxHealth) {
            healthBarParent.SetActive(true);
        }
        //Debug.Log($"{name} HandleHealthUpdated currentHealth {currentHealth}, lastDamageDeal {lastDamageDeal} ");
    }
    IEnumerator LerpHealthBar(Image healthBar, int currentHealth, int lastDamageDeal, int maxHealth, float lerpDuration)
    {
        if (maxHealth == 0) { yield break; }
        int startHealth = currentHealth + lastDamageDeal;
        int endHealth = currentHealth;
        float timeElapsed = 0f;
        float valueToLerp = 0f;
        while (timeElapsed < lerpDuration)
        {
            valueToLerp = Mathf.Lerp(startHealth, endHealth, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            healthBar.fillAmount = valueToLerp / maxHealth;
        }
        lastDisplayTime = Time.time;
        yield return null;
    }
    public void SetHealthBarColor (Color newColor)
    {
        healthBarImage.sprite = newColor == Color.blue ? healthBarImage.sprite : healthBarEnemyImage;
    }
    public void HandleKillText(int kill)
    {
        //Debug.Log($"HandleKillText {kills} {name}");
        kills += kill;
        killText.text = kills.ToString();
    }
    public void SetUnitLevel(int level, UnitMeta.UnitType unitType)
    {
        health.SetUnitLevel(level);
        SetUnitTypeIcon(unitType);
        levelText.text = health.GetUnitLevel().ToString();
    }
    public void SetUnitTypeIcon(UnitMeta.UnitType unitType)
    {
        switch (unitType)
        {
            case UnitMeta.UnitType.KING:
                kingIcon.SetActive(true);
                break;
            case UnitMeta.UnitType.HERO:
                heroIcon.SetActive(true);
                break;
            default:
                defaultIcon.SetActive(true);
                break;
        }
    }
    public void showHealthDisplay(bool flag)
    {
        healthBarParent.SetActive(flag);
        Debug.Log($"Hide {name} Health Display {flag}");
    }
}
