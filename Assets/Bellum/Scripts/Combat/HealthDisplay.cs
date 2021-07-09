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
    private float lerpSpeed = 2f;
    float lerpTimer = 0f;
    public int kills;
    private Quaternion startRotation;
    float currentHealth = 0f;
    float maxHealth = 0f;
    
    private void Awake()
    {
        health.ClientOnHealthUpdated += HandleHealthUpdated;
        if (GetComponent<Unit>().unitType == UnitMeta.UnitType.KING || GetComponent<Unit>().unitType == UnitMeta.UnitType.HERO
            || GetComponent<Unit>().unitType == UnitMeta.UnitType.QUEEN  || GetComponent<Unit>().unitType == UnitMeta.UnitType.DOOR)
            healthBarParent.SetActive(true);
        else
            healthBarParent.SetActive(false);
        startRotation = healthBarParent.transform.rotation;
    }
    void Update()
    {
        healthBarParent.transform.rotation = startRotation;
        updatedHealthUI();
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
    private void HandleHealthUpdated(int _currentHealth, int _maxHealth, int _lastDamageDeal)
    {
        currentHealth = _currentHealth;
        maxHealth = _maxHealth;
        lerpTimer = 0f;
        currentHealthText.text = currentHealth.ToString();
        if (currentHealth < maxHealth)
            healthBarParent.SetActive(true);
        if (currentHealth == 0)
            healthBarParent.SetActive(false);

    }
    private void updatedHealthUI()
    {
        float fillF = healthBarImage.fillAmount;
        float fillB = healthBarImageLast.fillAmount;
        float hFraction = (float)currentHealth / (float) maxHealth;
        
        if (fillB > hFraction)
        {
            healthBarImage.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / lerpSpeed;
            //Debug.Log($"fillB {fillB}, hFraction {hFraction}, percentComplete {percentComplete}");
            healthBarImageLast.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }
        if (fillF < hFraction)
        {
            healthBarImageLast.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / lerpSpeed;
            healthBarImage.fillAmount = Mathf.Lerp(fillF, healthBarImageLast.fillAmount, percentComplete);
        }
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
}
