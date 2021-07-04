using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// License: You are free to modify this file for your personal or commercial 
/// use. You may not sell or re-sell these scripts, or their derivatives, in 
/// any form other than their implementation as a system into your Unity project 
/// game/application.
/// 
/// Localization responder, configured to operate with the M2H Localization Package asset.
/// Assumes a singleton centralized manager class (LanguageManager{}) that maintains 
/// fields for the font assets for different language sets being parsed (in this case: KO, RU,
/// and EN -- all other). These centralized font assets may be overridden in the 
/// public fields here.
///
/// It is expected the LanguageManager defines an event used to indicate a language
/// change has occurred.
/// 
/// VERSION: 1.0.1
///     fixes: typo on delegate unsubscribe (-=) vice (+=)
/// </summary>

public class LocalizationResponder : MonoBehaviour
{
    //[SerializeField] private string lzLabel;          // localization label for display text.
    [SerializeField] private Material fontMaterial;   // if non-default material is desired
    [SerializeField] private TMP_FontAsset fontJp;    // override font for Japanese
    [SerializeField] private TMP_FontAsset fontCn;    // override font for Simplified Chinese
    [SerializeField] private TMP_FontAsset fontHk;    // override font for Traditional Chinese
    [SerializeField] private TMP_FontAsset fontEn;    // override font asset for latin/germanic

    private TMP_Text tmpText;       // text mesh pro text object.

    private void Awake()
    {
        // cache the TMP component on this object
        tmpText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        //Debug.Log($"LocalizationResponder.OnEnable()");

        // subscribe to event for language change
        LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

        // Initialize the component on enable to make sure this object
        // has the most current language configuration.
        OnLanguageChanged(LocalizationSettings.SelectedLocale);

        // TODO: if not login, use system locale.If system locale not available(not en, jp, zh), use en
        //else = logined, use saved language
        int langageId = PlayerPrefs.GetInt("Language");
        //Debug.Log($"LocalizationResponder.OnEnable() PlayerPrefs.GetInt(Language):{langageId}");
        if (langageId >= LocalizationSettings.AvailableLocales.Locales.Count)
        {
            Debug.LogError($"LocalizationSettings.AvailableLocales.Locales.Count:{LocalizationSettings.AvailableLocales.Locales.Count} < index:{langageId}. Use the first locale");
            langageId = 0;
        }
        var newlocale = LocalizationSettings.AvailableLocales.Locales[langageId];
        LocalizationSettings.SelectedLocale = newlocale;

        // Instantiate FontManger to get Default Font
        TMP_Asset tempFont = FontManager.Instance.defaultFontEn;

    }

    private void OnDisable()
    {
        //Debug.Log($"LocalizationResponder.OnDisable()");
        LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
        
    }

    public void OnLanguageChanged(Locale locale)
    {
        //Debug.Log($"LocalizationResponder.OnLanguageChanged Locale:{locale}");

        //Debug.Log($"fontJp:{fontJp} fontCn:{fontCn} fontHk:{fontJp} fontEn:{fontEn}");
        //Debug.Log($"defaultFontJp:{FontManager.Instance.defaultFontJp} defaultFontCn:{FontManager.Instance.defaultFontCn} defaultFontHk:{FontManager.Instance.defaultFontHk} defaultFontEn:{FontManager.Instance.defaultFontEn}");

        // determine which language is being used:
        string localeName = locale.LocaleName;
        if (localeName.Contains(LanguageSelectionManager.LOCALE_JP))
        {
            if (fontJp == null)
                // apply the centralized font asset setting
                tmpText.font = FontManager.Instance.defaultFontJp;
            else
                // apply the local font asset setting
                tmpText.font = fontJp;
        }
        else if (localeName.Contains(LanguageSelectionManager.LOCALE_CN))
        {
            if (fontCn == null)
                // apply the centralized font asset setting
                tmpText.font = FontManager.Instance.defaultFontCn;
            else
                // apply the local font asset setting
                tmpText.font = fontCn;
        }
        else if (localeName.Contains(LanguageSelectionManager.LOCALE_HK))
        {
            if (fontHk == null)
                // apply the centralized font asset setting
                tmpText.font = FontManager.Instance.defaultFontHk;
            else
                // apply the local font asset setting
                tmpText.font = fontHk;

        } else
        {
            if (fontEn == null)
                tmpText.font = FontManager.Instance.defaultFontEn;
            else
                tmpText.font = fontEn;
        }

        // If using a specific font material, map the material to the
        // appropriate font texture atlas, then set the font asset's material.
        if (fontMaterial != null)
        {
            fontMaterial.SetTexture("_MainTex", tmpText.font.material.mainTexture);
            tmpText.fontMaterial = fontMaterial;
        }

        /* set label key in Localize String Event
        // Localization label: only applies if set.
        if (lzLabel != "")
            tmpText.text = Language.Get(lzLabel);
        */
    }
}
