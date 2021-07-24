using TMPro;

public static class StaticClass
{
    public static string CrossSceneInformation { get; set; }
    public static string UserID { get; set; }
    public static string Username { get; set; }
    public static int SelectedCardSlot { get; set; }
    public static int SelectedTeamTab { get; set; }
    public static int SelectedCharacterTab { get; set; }
    public static UnitMeta.UnitKey[] teamMembers { get; set; }

    //User Profile
    public static string level { get; set; }
    public static string gold { get; set; }
    public static string diamond { get; set; }
    public static string ruby { get; set; }
    public static string opal { get; set; }
    public static string emerald { get; set; }
    public static string sapphire { get; set; }
    public static string topaz { get; set; }
    public static string experience { get; set; }
    public static string highestPoint { get; set; }

    public static string TotalPower { get; set; }
    public static UnitMeta.Race playerRace { get; set; }
    public static UnitMeta.Race enemyRace { get; set; }
    public static bool IsFlippedCamera { get; set; }
    public static string Chapter { get; set; }
    public static string Mission { get; set; }
    public static string ItemCode { get; set; }
    public static string EventRankingID { get; set; }
    public static float HighestDamage { get; set; }

    public static TMP_FontAsset defaultFontJp { get; set; }
    public static TMP_FontAsset defaultFontCn { get; set; }
    public static TMP_FontAsset defaultFontHk { get; set; }
    public static TMP_FontAsset defaultFontEn { get; set; }
}