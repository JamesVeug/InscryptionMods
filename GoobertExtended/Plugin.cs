using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using UnityEngine;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
	public const string PluginGuid = "jamesgames.inscryption.goobertextended";
	public const string PluginName = "Goobert Extended";
	public const string PluginVersion = "1.2.2";

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
			CardInfo temporaryCard = cardToCopy.CloneAsTemporaryCard();
			if (!Configs.DisablePaintDecal.Value)
			{
				foreach (CardModificationInfo item2 in temporaryCard.Mods.FindAll((CardModificationInfo x) =>
					         !x.nonCopyable))
				{
					if (item2.singletonId != "paint_decal")
					{
						CardModificationInfo item = (CardModificationInfo)item2.Clone();
						temporaryCard.Mods.Add(item);
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
				temporaryCard.Mods.Add(cardModificationInfo);
			}

			CardModificationInfo copyCardMod = new CardModificationInfo();
			temporaryCard.Mods.Add(copyCardMod);
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
				List<RandomOptions.WeightOption> options2 = options.FindAll((a) => a.Condition(temporaryCard));
				options2.Sort((a, b) => a.Weight - b.Weight);
				
				int totalWeight = options2.Sum((a) => a.Weight);
				int randomWeight = SeededRandom.Range(0, totalWeight, Seed++);
				int weight = 0;
				for (int j = 0; j < options2.Count; j++)
				{
					weight += options2[j].Weight;
					if (weight > randomWeight)
					{
						options2[j].OnChosen(temporaryCard, copyCardMod);
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
					RandomOptions.OnNameChange(temporaryCard, copyCardMod);
				}
			}

			__result = temporaryCard;
			TemporaryCardInfo.SaveTemporaryCard(temporaryCard);
			return false;
		}
	}
	
	// [HarmonyPatch]
	// internal class DuplicateMerge_Patches
	// {
	// 	[HarmonyPatch(typeof(DuplicateMergeSequencer), nameof(DuplicateMergeSequencer.GetValidDuplicateCards))]
	// 	[HarmonyPostfix]
	// 	internal static void Postfix(ref List<CardInfo> __result)
	// 	{
	// 		Dictionary<string, List<CardInfo>> singletonIds = new Dictionary<string, List<CardInfo>>();
	// 		foreach (CardInfo cardInfo in RunState.DeckList)
	// 		{
	// 			if (!cardInfo.IsTemporaryCardInfo())
	// 				continue;
	// 			
	// 			var mod = cardInfo.Mods.Find((a) => !string.IsNullOrEmpty(a.singletonId) && a.singletonId.StartsWith("temporary_card"));
	//
	// 			string substring = mod.singletonId.Substring("temporary_card|".Length);
	// 			Plugin.Log.LogInfo($"[DuplicateMerge_Patches] {cardInfo.name} {substring}");
	// 			if (singletonIds.TryGetValue(substring, out List<CardInfo> list))
	// 			{
	// 				list.Add(cardInfo);
	// 			}
	// 			else
	// 			{
	// 				singletonIds.Add(substring, new List<CardInfo>() {cardInfo});
	// 			}
	// 		}
	//
	// 		foreach (KeyValuePair<string,List<CardInfo>> pair in singletonIds)
	// 		{
	// 			Log.LogInfo($"[DuplicateMerge_Patches] {pair.Key} {pair.Value.Count}");
	// 			if (pair.Value.Count > 1)
	// 			{
	// 				__result.AddRange(pair.Value);
	// 			}
	// 		}
	// 	}
	// 	
	// 	[HarmonyPatch(typeof(DuplicateMergeSequencer), nameof(DuplicateMergeSequencer.MergeCards))]
	// 	[HarmonyPrefix]
	// 	internal static bool Prefix(DuplicateMergeSequencer __instance, CardInfo card1, CardInfo card2, ref CardInfo __result)
	// 	{
	// 		bool aIsTemporary = card1.IsTemporaryCardInfo(); 
	// 		bool bIsTemporary = card2.IsTemporaryCardInfo(); 
	// 		if (!aIsTemporary && !bIsTemporary)
	// 		{
	// 			return true;
	// 		}
	// 		
	// 		
	// 		CardInfo cardByName = Instantiate(card1);
	// 		cardByName.Mods = new List<CardModificationInfo>(card1.Mods);
	// 		cardByName.baseAttack += card2.baseAttack;
	// 		cardByName.baseHealth += card2.baseHealth;
	// 		cardByName.cost = Mathf.Max(0, cardByName.cost + card2.cost - cardByName.cost);
	// 		cardByName.bonesCost = Mathf.Max(0, cardByName.bonesCost + card2.bonesCost - cardByName.bonesCost);
	// 		cardByName.energyCost = Mathf.Max(0, cardByName.energyCost + card2.energyCost - cardByName.energyCost);
	// 		cardByName.gemsCost = CombineGemCosts(card1, card2);
	// 		cardByName.tribes.AddRange(card2.tribes.Where((a)=>!card1.tribes.Contains(a)));
	// 		cardByName.abilities = CombineBaseAbilities(card1, card2);
	// 		
	// 		
	// 		CardModificationInfo duplicateMod = DuplicateMergeSequencer.GetDuplicateMod(0, 0);
	// 		int num = 0;
	// 		foreach (CardModificationInfo mod in card1.Mods)
	// 		{
	// 			if (mod != null && mod.fromCardMerge)
	// 			{
	// 				num += mod.abilities.Count;
	// 			}
	// 		}
	// 		foreach (CardModificationInfo mod2 in card2.Mods)
	// 		{
	// 			if (mod2.fromCardMerge)
	// 			{
	// 				duplicateMod.fromCardMerge = true;
	// 			}
	// 			foreach (Ability ability in mod2.abilities)
	// 			{
	// 				if (!card1.HasAbility(ability) && duplicateMod.abilities.Count + num < 4)
	// 				{
	// 					duplicateMod.abilities.Add(ability);
	// 				}
	// 			}
	// 		}
	// 		
	// 		RunState.Run.playerDeck.ModifyCard(card1, duplicateMod);
	// 		RunState.Run.playerDeck.RemoveCard(card2);
	//
	// 		
	// 		TemporaryCardInfo.SaveAsTemporaryCard(cardByName, card1);
	// 		return false;
	// 	}
	//
	// 	private static List<Ability> CombineBaseAbilities(CardInfo card1, CardInfo card2)
	// 	{
	// 		List<Ability> list = new List<Ability>();
	// 		list.AddRange(card1.abilities);
	// 		list.AddRange(card2.abilities.Where((a)=>!list.Contains(a)));
	//
	// 		return list;
	// 	}
	//
	// 	private static List<GemType> CombineGemCosts(CardInfo card1, CardInfo card2)
	// 	{
	// 		List<GemType> list = new List<GemType>();
	// 		list.AddRange(card1.gemsCost);
	// 		list.AddRange(card2.gemsCost);
	//
	// 		foreach (GemType gemType in Enum.GetValues(typeof(GemType)))
	// 		{
	// 			int count = Mathf.CeilToInt(list.Count((a) => a == gemType) / 2.0f);
	// 			list.RemoveAll((a) => a == gemType);
	// 			for (int i = 0; i < count; i++)
	// 			{
	// 				list.Add(gemType);
	// 			}
	// 		}
	// 		return list;
	// 	}
	// }
}
