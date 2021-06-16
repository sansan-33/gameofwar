using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageSelectionManager : MonoBehaviour
{
    private const int LOCALE_EN = 0;
    private const int LOCALE_JP = 1;
    private const int LOCALE_CN = 2;
    private const int LOCALE_HK = 3;

    [SerializeField] public GameObject languageSelectionPopup;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectEnglish()
    {
        OnSelectionChanged(LOCALE_EN);
    }

    public void SelectJapanese()
    {
        OnSelectionChanged(LOCALE_JP);
    }

    public void SelectSimplifiedChinese()
    {
        OnSelectionChanged(LOCALE_CN);
    }

    public void SelectTraditionalChinese()
    {
        OnSelectionChanged(LOCALE_HK);
    }

    void OnSelectionChanged(int index)
    {
        Debug.Log($"LanguageSelectionManager.OnSelectionChanged() Selected index:{index}");

        var locale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = locale;
        Debug.Log($"LanguageSelectionManager.OnSelectionChanged() SelectedLocale:{LocalizationSettings.SelectedLocale}");

        // inactive check for non-selected locale and active check for selected locale

        // update language in Setting popup
    }
}
