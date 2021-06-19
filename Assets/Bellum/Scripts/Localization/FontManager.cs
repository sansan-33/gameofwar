using TMPro;
using UnityEngine;

/// <summary>
/// License: You are free to modify this file for your personal or commercial 
/// use. You may not sell or re-sell these scripts, or their derivatives, in 
/// any form other than their implementation as a system into your Unity project 
/// game/application.
/// </summary>

public class FontManager : MonoBehaviour
{
    public TMP_FontAsset defaultFontJp;    // centralized font asset for JP lang
    public TMP_FontAsset defaultFontCn;    // for Simplified Chinese
    public TMP_FontAsset defaultFontHk;    // for Traditional Chinese
    public TMP_FontAsset defaultFontEn;    // for all other latin/germanic langs

    // simple singleton declaration
    private static FontManager _instance;
    public static FontManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<FontManager>();
            return _instance;
        }
    }

    /* Change language handled by LanguageSelectionManager.
     * 
    // language change event definition
    public delegate void LanguageMgrHandler();
    public static event LanguageMgrHandler LanguageChanged;

    // call this method to properly fire the lang changed event
    private static void LanguageChangeHasOccurred()
    {
        if (LanguageChanged != null) LanguageChanged();
    }

    // specific to the M2H Localization Package -- pass standard
    // language codes.
    public void SetLanguage(string lang)
    {
        Language.SwitchLanguage(lang);

        // inform systems and components, the language has been changed.
        LanguageChangeHasOccurred();
    }*/
}
