using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using TaleWorlds.Localization;

namespace SiegeSanity
{
    /// <summary>
    /// Каждый патч отключается отдельно. Это не украшательство: осадных модов
    /// много, и если наш патч с чем-то не поладит у конкретного игрока, он должен
    /// иметь возможность выключить ОДИН патч, а не сносить мод целиком.
    ///
    /// Упрямый штурм ВКЛЮЧЁН по умолчанию — это главная фича мода, и она заявлена
    /// первой строкой описания в Workshop. Изменение поведения ИИ, включённое по
    /// умолчанию и не объявленное, — это то, за что моды справедливо ругают.
    ///
    /// Патчи применяются всегда, а вот их тело спрашивает настройку в рантайме —
    /// поэтому переключение работает без перезапуска игры.
    /// </summary>
    public sealed class Settings : AttributeGlobalSettings<Settings>
    {
        // Id БЕЗ ТОЧЕК — он становится именем файла настроек.
        public override string Id => "SiegeSanity_MCM";

        public override string DisplayName =>
            new TextObject("{=SiegeSanity_DisplayName}Siege Sanity").ToString();

        public override string FolderName => "SiegeSanity";

        // ОБЯЗАТЕЛЬНО: без FormatType MCM не знает, чем писать файл на диск,
        // и настройки молча не сохраняются между сессиями.
        public override string FormatType => "json2";

        /// <summary>Удобный доступ: если MCM почему-то не поднялся, патчи
        /// работают в своём дефолтном режиме, а не падают.</summary>
        public static Settings Current => Instance ?? new Settings();

        [SettingPropertyGroup("{=SiegeSanity_GroupFixes}Bug fixes", GroupOrder = 0)]
        [SettingPropertyBool("{=SiegeSanity_RetreatFix}Fix army wipe on siege retreat", RequireRestart = false,
            HintText = "{=SiegeSanity_RetreatFixHint}Vanilla treats a failed assault as a total capture: the whole besieging army is taken prisoner or killed and its lords respawn with a single troop. With this on, survivors retreat instead.")]
        public bool FixSiegeRetreatWipe { get; set; } = true;

        [SettingPropertyGroup("{=SiegeSanity_GroupFixes}Bug fixes", GroupOrder = 0)]
        [SettingPropertyBool("{=SiegeSanity_CrashGuard}Guard against siege-start crash", RequireRestart = false,
            HintText = "{=SiegeSanity_CrashGuardHint}Swallows a vanilla NullReferenceException thrown while a party joins a siege on the campaign map. Without it the game hard-crashes. Note: the affected party simply does not join that siege.")]
        public bool GuardSiegeStartCrash { get; set; } = true;

        [SettingPropertyGroup("{=SiegeSanity_GroupBehaviour}Assault behaviour", GroupOrder = 1)]
        [SettingPropertyBool("{=SiegeSanity_Stubborn}Stubborn assault", RequireRestart = false,
            HintText = "{=SiegeSanity_StubbornHint}Attackers keep pushing instead of pulling back the moment they cannot get up the walls. This is the main feature of the mod and is ON by default. It changes AI behaviour rather than fixing a bug — turn it off here for vanilla assault behaviour.")]
        public bool StubbornAssault { get; set; } = true;

        [SettingPropertyGroup("{=SiegeSanity_GroupBehaviour}Assault behaviour", GroupOrder = 1)]
        [SettingPropertyFloatingInteger("{=SiegeSanity_StubbornRatio}Retreat threshold", 0.05f, 0.6f, "0.00", RequireRestart = false,
            HintText = "{=SiegeSanity_StubbornRatioHint}Share of remaining strength below which a stubborn assault finally breaks off. 0.25 means they pull back only after losing about 75%. Lower = more stubborn. Only used when Stubborn assault is on.")]
        public float StubbornRetreatRatio { get; set; } = 0.25f;
    }
}
