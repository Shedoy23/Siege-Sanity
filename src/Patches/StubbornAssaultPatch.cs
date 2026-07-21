using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace SiegeSanity.Patches
{
    /// <summary>
    /// «Упрямый штурм»: атакующие не откатываются от стены при первых потерях.
    ///
    /// Как считает ваниль (TacticBreachWalls):
    ///   отступать, если (оставшаяся сила / стартовая) &lt; порог,
    /// а порог зависит от ПРОГРЕССА штурма: стартует с 1.0 и снижается за
    /// достижения — за каждую забравшуюся наверх формацию, за каждое открытое
    /// направление. Пока никто не залез и путь закрыт, порог держится около 0.6,
    /// то есть хватает потери ~40% силы, чтобы вся тактика ушла в отступление.
    /// Получается замкнутый круг: «не можем залезть» → «нет прогресса» →
    /// «высокий порог» → быстрый откат → штурм буксует и повторяется.
    ///
    /// Замысел вменяемый (не кидать людей на неприступную стену), но на практике
    /// даёт бесконечные качели. Патч заменяет прогресс-зависимый порог на
    /// фиксированный низкий: штурм идёт до конца, а отступление остаётся только
    /// предохранителем от полного истребления.
    ///
    /// ЭТО НЕ БАГФИКС, А ИЗМЕНЕНИЕ ПОВЕДЕНИЯ — по умолчанию выключено.
    ///
    /// Используется только публичный API (TacticComponent.Team,
    /// TeamQuerySystem.RemainingPowerRatio), без приватных полей — патч
    /// переживает обновления игры. Если метод не найден, патч тихо пропускается.
    /// </summary>
    [HarmonyPatch]
    public static class StubbornAssaultPatch
    {
        public static MethodBase TargetMethod()
        {
            MethodBase m = null;
            try { m = AccessTools.Method(typeof(TacticBreachWalls), "ShouldRetreat"); }
            catch { }
            if (m == null)
                Log.Write("[StubbornAssault] TacticBreachWalls.ShouldRetreat не найден — патч пропущен");
            return m;
        }

        [HarmonyPrefix]
        public static bool Prefix(TacticBreachWalls __instance, ref bool __result)
        {
            try
            {
                var s = Settings.Current;
                if (!s.StubbornAssault) return true;         // выключено → ваниль

                var team = __instance?.Team;
                if (team?.QuerySystem == null) return true;   // не смогли → ваниль

                __result = team.QuerySystem.RemainingPowerRatio < s.StubbornRetreatRatio;
                return false;   // ваниль пропускаем: порог наш, прогресс не важен
            }
            catch (Exception ex)
            {
                Log.Write($"[StubbornAssault] prefix warn: {ex.Message}");
                return true;    // любая неожиданность → ванильное поведение
            }
        }
    }
}
