using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;

namespace MemesPlus
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			var harmony = new Harmony("memesplus.smashphil");

			harmony.Patch(original: AccessTools.Method(typeof(Pawn), nameof(Pawn.PostApplyDamage)),
				prefix: new HarmonyMethod(typeof(HarmonyPatches),
				nameof(ObservedViolence)));
		}

		private static void ObservedViolence(Pawn __instance, DamageInfo dinfo) //float totalDamageDealt
		{
			if (dinfo.Def.ExternalViolenceFor(__instance) && dinfo.Instigator != null)
			{
				if (dinfo.Instigator is Pawn instigator)
				{
					GiveCommittedViolenceHistoryEvent(__instance, instigator);
				}
				Map map = __instance.Map ?? dinfo.Instigator.Map;
				if (map is null)
				{
					return;
				}
				RegionTraverser.BreadthFirstTraverse(__instance.Position, map, (Region from, Region to) => __instance.Position.InHorDistOf(to.extentsClose.ClosestCellTo(__instance.Position), 5f), delegate (Region reg)
				{
					foreach (Thing thing in reg.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn))
					{
						if (thing is Pawn pawn && PossibleToObserve(__instance, pawn))
						{
							TryCreateObservedHistoryEvent(__instance, pawn);
						}
					}
					return true;
				});
			}
		}

		private static bool PossibleToObserve(Pawn instigator, Pawn pawn)
		{
			return instigator.Position.InHorDistOf(pawn.Position, 5f) && GenSight.LineOfSight(instigator.Position, pawn.Position, pawn.Map, true, null, 0, 0);
		}

		private static HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
		{
			if (!observer.RaceProps.Humanlike)
			{
				return null;
			}
			return MemesPlus_HistoryEventDefOf.ObservedViolence;
		}

		private static void TryCreateObservedHistoryEvent(Pawn observer, Pawn instigator)
		{
			HistoryEventDef historyEventDef = GiveObservedHistoryEvent(observer);
			if (historyEventDef != null)
			{
				HistoryEvent historyEvent = new HistoryEvent(historyEventDef, observer.Named(HistoryEventArgsNames.Doer), instigator.Named(HistoryEventArgsNames.Subject));
				Find.HistoryEventsManager.RecordEvent(historyEvent);
			}
		}

		private static void GiveCommittedViolenceHistoryEvent(Pawn observer, Pawn instigator)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(MemesPlus_HistoryEventDefOf.CommittedViolence, instigator.Named(HistoryEventArgsNames.Subject), observer.Named(HistoryEventArgsNames.Doer)));
		}
	}
}
