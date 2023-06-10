using DiskCardGame;
using HarmonyLib;

namespace MorePeltsMod.Scripts.PeltsOnlyPatches;

internal class Patches
{
	[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.CreateNode), new Type[]{typeof(int), typeof(int), typeof(List<NodeData>), typeof(List<NodeData>), typeof(int)})]
	private static class MapGenerator_CreateNode
	{
		public static void Postfix(ref NodeData __result)
		{
			if (AscensionSaveData.Data.ChallengeIsActive(Plugin.PeltsOnlyChallengeID.Challenge.challengeType))
			{
				Type type = __result.GetType();
				if (type == typeof(CardChoicesNodeData) || type == typeof(BoulderChoiceNodeData) ||
				    type == typeof(DeckTrialNodeData))
				{
					bool trader = SeededRandom.Range(0, 100, RunState.RandomSeed + __result.id) > 50;
					NodeData nodeData = trader ? new TradePeltsNodeData() : new BuyPeltsNodeData();
					nodeData.id = __result.id;
					nodeData.gridX = __result.gridX;
					nodeData.gridY = __result.gridY;
					__result = nodeData;
				}
			}
		}
	}
}