using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SiegeSanity.Patches
{
    /// <summary>
    /// Защита от ванильного вылета при старте осады в тике карты.
    ///
    /// Стек из крашдампа (dotnet-dump, символизированный managed-стек):
    ///   System.NullReferenceException
    ///     MobileParty.OnPartyJoinedSiegeInternal()
    ///     ← set_BesiegerCamp ← SiegeEvent..ctor ← SiegeEventManager.StartSiegeEvent
    ///     ← EncounterManager.StartSettlementEncounter ← Campaign.Tick()
    /// Ни одного кадра постороннего мода в стеке — это ванильный edge-case
    /// в осадном учёте, вылезающий при большом числе ИИ-осад (замечен на связке
    /// с Diplomacy и SiegeFix).
    ///
    /// ЧЕСТНО О КОМПРОМИССЕ: это finalizer, который ГЛОТАЕТ исключение, а не
    /// чинит причину. Партия, на которой оно возникло, просто не присоединится
    /// к этой осаде. Мы считаем, что тихо пропущенное присоединение лучше
    /// вылета всей игры, но это именно размен, а не полноценное лечение.
    /// Глотаем ТОЛЬКО NullReferenceException и ТОЛЬКО из этого метода —
    /// всё остальное пробрасывается дальше, чтобы не прятать чужие поломки.
    /// </summary>
    [HarmonyPatch]
    public static class SiegeStartCrashGuard
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var mpType = AccessTools.TypeByName("TaleWorlds.CampaignSystem.Party.MobileParty");
            if (mpType == null)
            {
                Log.Write("[CrashGuard] тип MobileParty не найден — патч пропущен");
                yield break;
            }
            var m = AccessTools.Method(mpType, "OnPartyJoinedSiegeInternal");
            if (m == null)
            {
                Log.Write("[CrashGuard] OnPartyJoinedSiegeInternal не найден — патч пропущен");
                yield break;
            }
            yield return m;
        }

        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception == null) return null;
            if (!Settings.Current.GuardSiegeStartCrash) return __exception;

            if (__exception is NullReferenceException)
            {
                Log.Write("[CrashGuard] проглочен NullReferenceException в " +
                          "OnPartyJoinedSiegeInternal — игра не упала, партия не вошла в осаду");
                return null;
            }
            return __exception;
        }
    }
}
