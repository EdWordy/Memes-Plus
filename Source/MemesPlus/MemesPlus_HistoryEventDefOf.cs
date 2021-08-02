using RimWorld;

namespace MemesPlus
{
	[DefOf]
	public static class MemesPlus_HistoryEventDefOf
	{
		public static HistoryEventDef ObservedViolence;

		public static HistoryEventDef CommittedViolence;

		static MemesPlus_HistoryEventDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MemesPlus_HistoryEventDefOf));
		}
	}
}
