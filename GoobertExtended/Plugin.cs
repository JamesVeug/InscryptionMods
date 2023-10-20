using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
	public const string PluginGuid = "jamesgames.inscryption.goobertextended";
	public const string PluginName = "Goobert Extended";
	public const string PluginVersion = "1.2.0";

	public static Plugin Instance;

	public static ManualLogSource Log;

	private void Awake()
	{
		Logger.LogInfo($"Loading {PluginName}...");
		Instance = this;
		Log = Logger;

		new Harmony(PluginGuid).PatchAll();
		Logger.LogInfo($"Loaded {PluginName}!");
	}

	[HarmonyPatch]
	internal class Patches
	{
		public static int Seed;

		[HarmonyPatch(typeof(CopyCardSequencer), nameof(CopyCardSequencer.CreateCloneCard))]
		[HarmonyPrefix]
		private static bool CopyCardSequencer_CreateCloneCard(CopyCardSequencer __instance, CardInfo cardToCopy,
			ref CardInfo __result)
		{
			CardInfo cardByName = Instantiate(cardToCopy);
			cardByName.Mods = new List<CardModificationInfo>(cardToCopy.Mods);
			
			if (!Configs.DisablePaintDecal.Value)
			{
				foreach (CardModificationInfo item2 in cardToCopy.Mods.FindAll((CardModificationInfo x) =>
					         !x.nonCopyable))
				{
					if (item2.singletonId != "paint_decal")
					{
						CardModificationInfo item = (CardModificationInfo)item2.Clone();
						cardByName.Mods.Add(item);
					}
				}

				CardModificationInfo cardModificationInfo = new CardModificationInfo();
				cardModificationInfo.singletonId = "paint_decal";
				CopyCardSequencer.paintDecalIndex++;
				if (CopyCardSequencer.paintDecalIndex > 2)
				{
					CopyCardSequencer.paintDecalIndex = 0;
				}

				cardModificationInfo.DecalIds.Add("decal_paint_" + (CopyCardSequencer.paintDecalIndex + 1));
				cardByName.Mods.Add(cardModificationInfo);
			}

			CardModificationInfo copyCardMod = new CardModificationInfo();
			cardByName.Mods.Add(copyCardMod);
			Seed = SaveManager.SaveFile.GetCurrentRandomSeed();

			List<RandomOptions.WeightOption> options = RandomOptions.GetAllRandomOptions();
			
			int totalRandomizations;
			if (Configs.SuperMode.Value)
			{
				options.AddRange(RandomOptions.GetAllRandomOptions().Where((a)=>a.AllowMultiple));
				totalRandomizations = SeededRandom.Range(2, Mathf.Max(2, Configs.MaxRandomizations.Value), Seed++);
			}
			else
			{
				totalRandomizations = SeededRandom.Range(1, Mathf.Max(1, Configs.MaxRandomizations.Value), Seed++);
			}
			
			// Make modifications
			for (int i = 0; i < totalRandomizations && options.Count > 0; i++)
			{
				List<RandomOptions.WeightOption> options2 = options.FindAll((a) => a.Condition(cardByName));
				options2.Sort((a, b) => a.Weight - b.Weight);
				
				int totalWeight = options2.Sum((a) => a.Weight);
				int randomWeight = SeededRandom.Range(0, totalWeight, Seed++);
				int weight = 0;
				for (int j = 0; j < options2.Count; j++)
				{
					weight += options2[j].Weight;
					if (weight > randomWeight)
					{
						options2[j].OnChosen(cardByName, copyCardMod);
						options.Remove(options2[j]);
						break;
					}
				}
			}
			
			// Flavoring
			int nameChangeChance = Mathf.Clamp(Configs.ChanceOfNameChange.Value, 0, 100);
			if (nameChangeChance > 0)
			{
				if (SeededRandom.Range(0, 100, Seed++) < nameChangeChance)
				{
					RandomOptions.OnNameChange(cardByName, copyCardMod);
				}
			}

			__result = cardByName;
			TemporaryCardInfo.SaveAsTemporaryCard(cardByName);
			return false;
		}
	}
}
