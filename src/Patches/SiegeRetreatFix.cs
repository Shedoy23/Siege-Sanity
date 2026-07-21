using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;

namespace SiegeSanity.Patches
{
    /// <summary>
    /// Ванильный баг: провалившийся штурм трактуется как ПОЛНЫЙ захват — вся
    /// осаждающая армия попадает в плен или гибнет, а её лорды становятся
    /// беглецами и возрождаются с одним бойцом. Фактически армия уничтожается
    /// за одно неудачное сражение.
    ///
    /// Что делает патч:
    ///   Prefix  — если событие осадное, победитель есть, отступающей стороны
    ///             движок не проставил, а у проигравших ЕСТЬ здоровые выжившие,
    ///             временно выставляем RetreatingSide = проигравшая сторона.
    ///             Движок дальше обрабатывает это как отступление, а не как
    ///             истребление, и войска выживают ранеными.
    ///   Postfix — возвращаем RetreatingSide в None, чтобы остальные системы
    ///             видели корректный результат боя.
    ///
    /// Если выживших нет вообще — не вмешиваемся: настоящий разгром должен
    /// оставаться разгромом.
    ///
    /// RetreatingSide имеет приватный сеттер, поэтому пишем через рефлексию.
    /// PropertyInfo кэшируется один раз; если движок переименует свойство,
    /// патч просто ничего не делает (null-проверка) вместо падения.
    ///
    /// Идея взята из Bannerlord Legacy Tweaks (реализация своя).
    /// </summary>
    [HarmonyPatch(typeof(MapEvent), "CalculateAndCommitMapEventResults")]
    internal static class SiegeRetreatFix
    {
        private static readonly PropertyInfo RetreatingSideProp =
            typeof(MapEvent).GetProperty("RetreatingSide",
                BindingFlags.Public | BindingFlags.Instance);

        // Какие MapEvent мы мутировали — восстанавливаем в Postfix только их.
        private static readonly HashSet<MapEvent> _mutated = new HashSet<MapEvent>();

        private static bool IsSiegeRelated(MapEvent e)
            => e.IsSiegeAssault || e.IsSallyOut || e.IsSiegeOutside;

        [HarmonyPrefix]
        public static void Prefix(MapEvent __instance)
        {
            try
            {
                if (__instance == null) return;
                if (RetreatingSideProp == null) return;          // другая версия движка
                if (!Settings.Current.FixSiegeRetreatWipe) return;

                if (!IsSiegeRelated(__instance)) return;
                if (!__instance.HasWinner) return;
                if (__instance.RetreatingSide != BattleSideEnum.None) return;

                var defeated = __instance.GetMapEventSide(__instance.DefeatedSide);
                if (defeated == null) return;

                int survivors = defeated.GetTotalHealthyTroopCountOfSide();
                if (survivors <= 0) return;   // настоящий разгром — не мешаем

                RetreatingSideProp.SetValue(__instance, __instance.DefeatedSide);
                _mutated.Add(__instance);
            }
            catch (Exception ex)
            {
                Log.Write($"[SiegeRetreatFix] prefix error: {ex.GetType().Name}: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(MapEvent __instance)
        {
            try
            {
                if (__instance == null) return;
                if (RetreatingSideProp == null) return;
                if (!_mutated.Remove(__instance)) return;

                RetreatingSideProp.SetValue(__instance, BattleSideEnum.None);
            }
            catch (Exception ex)
            {
                Log.Write($"[SiegeRetreatFix] postfix error: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
