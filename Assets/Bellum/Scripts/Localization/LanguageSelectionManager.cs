using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

public class LanguageSelectionManager : MonoBehaviour
{
    public const string STRING_TEXT_REF = "UI_Text";
    public const string LOCALE_EN = "en";
    public const string LOCALE_JP = "ja";
    public const string LOCALE_CN = "zh-Hans";
    public const string LOCALE_HK = "zh-Hant";
    public static int Selected_Locale_Index = 0;

    [SerializeField] public GameObject languageSelectionPopup;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log($"LanguageSelectionManager.Start()");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"LanguageSelectionManager.Update()");
    }

    private void Awake()
    {

    }

    private void OnEnable()
    {
        //Debug.Log($"LanguageSelectionManager.OnEnable()");
        // subscribe to event for language change
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

        // Initialize the component on enable to make sure this object
        // has the most current language configuration.
        OnLanguageChanged(LocalizationSettings.SelectedLocale);
    }

    private void OnDisable()
    {
        //Debug.Log($"LanguageSelectionManager.OnDisable()");
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
    }

    public void SelectEnglish()
    {
        
        OnSelectionChanged(getLocaleIndex(LOCALE_EN));
    }

    public void SelectJapanese()
    {
        OnSelectionChanged(getLocaleIndex(LOCALE_JP));
    }

    public void SelectSimplifiedChinese()
    {
        OnSelectionChanged(getLocaleIndex(LOCALE_CN));
    }

    public void SelectTraditionalChinese()
    {
        OnSelectionChanged(getLocaleIndex(LOCALE_HK));
    }

    public void OnLanguageChanged(Locale locale)
    {
        
    }

    private void OnSelectionChanged(int index)
    {
        Debug.Log($"LanguageSelectionManager.OnSelectionChanged() Selected index:{index}");

        Debug.Log($"LocalizationSettings.AvailableLocales:{LocalizationSettings.AvailableLocales.Locales}");
        Selected_Locale_Index = index;

        if (index >= LocalizationSettings.AvailableLocales.Locales.Count)
        {
            Debug.LogError($"LocalizationSettings.AvailableLocales.Locales.Count:{LocalizationSettings.AvailableLocales.Locales.Count} < index:{index}");
            return;
        }

        var locale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = locale;
        Debug.Log($"LanguageSelectionManager.OnSelectionChanged() SelectedLocale:{LocalizationSettings.SelectedLocale}");

        // save
        PlayerPrefs.SetInt("Language", index);
        Debug.Log($"LanguageSelectionManager.OnSelectionChanged() PlayerPrefs.SetInt(Language, index):{index}");

        // TODO: inactive check for non-selected locale and active check for selected locale

        // TODO: update language in Setting popup
    }

    private int getLocaleIndex(string localeString)
    {
        for (int i=0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            string localeName = LocalizationSettings.AvailableLocales.Locales[i].LocaleName;
            Debug.Log($"localeName:{localeName} localeString:{localeString}");
            if (localeName.Contains(localeString))
            {
                Debug.Log($"return localeIndex:{i}");
                return i;
            }
        }
        return -1;
    }
}
