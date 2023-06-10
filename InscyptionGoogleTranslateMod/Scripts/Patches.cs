using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;
using InscryptionAPI.Dialogue;
using InscryptionAPI.Items;
using UnityEngine;

namespace GoogleTranslateMod
{
	#pragma warning disable Harmony003
	[HarmonyPatch(typeof(CardManager), "Add", new System.Type[] { typeof(CardInfo) })]
	public class CardManager_Add
	{
		public static void Postfix(CardInfo newCard)
		{
			Plugin.Instance.StartCoroutine(Translate(newCard, Language.English));
		}

		public static IEnumerator Translate(CardInfo cardInfo, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();
			
			if (!string.IsNullOrEmpty(cardInfo.displayedName))
			{
				// TODO:
				// Translate displayedName to english and store in displayedName
				// Then Translate original to currentlanguage if its not english
				// - Add to LocalisationManager so the game naturally translates it
				yield return Plugin.Translate(cardInfo.displayedName, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					cardInfo.displayedName = result.englishTranslation;
				}
			}

			if (!string.IsNullOrEmpty(cardInfo.description))
			{
				yield return Plugin.Translate(cardInfo.description, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					cardInfo.description = result.englishTranslation;
				}
			}
		}
	}
	
	[HarmonyPatch(typeof(AbilityManager), "Add", new System.Type[] { typeof(string), typeof(AbilityInfo), typeof(Type), typeof(Texture)})]
	public class AbilityManager_Add
	{
		public static void Postfix(string guid, AbilityInfo info, Type behavior, Texture tex)
		{
			Plugin.Instance.StartCoroutine(Translate(info, Language.English));
		}

		public static IEnumerator Translate(AbilityInfo info, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();

			// TODO: Translate to english THEN the current language
			// Otherwise they need to switch languages again -.-'
			
			if (!string.IsNullOrEmpty(info.rulebookName))
			{
				yield return Plugin.Translate(info.rulebookName, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					info.rulebookName = result.englishTranslation;
				}
			}

			if (!string.IsNullOrEmpty(info.rulebookDescription))
			{
				yield return Plugin.Translate(info.rulebookDescription, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					info.rulebookDescription = result.englishTranslation;
				}
			}

			if (!string.IsNullOrEmpty(info.triggerText))
			{
				yield return Plugin.Translate(info.triggerText, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					info.triggerText = result.englishTranslation;
				}
			}

			if (info.abilityLearnedDialogue != null && info.abilityLearnedDialogue.lines != null)
			{
				foreach (DialogueEvent.Line line in info.abilityLearnedDialogue.lines)
				{
					yield return DialogueManager_Add.Translate(line, language, result);
				}
			}
		}
	}
	
	[HarmonyPatch(typeof(StatIconManager), "Add", new System.Type[] { typeof(string), typeof(StatIconInfo), typeof(Type)})]
	public class StatIconManager_Add
	{
		public static void Postfix(string guid, StatIconInfo info, Type behavior)
		{
			Plugin.Instance.StartCoroutine(Translate(info, Language.English));
		}

		public static IEnumerator Translate(StatIconInfo info, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();
			
			if (!string.IsNullOrEmpty(info.rulebookName))
			{
				yield return Plugin.Translate(info.rulebookName, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					info.rulebookName = result.englishTranslation;
				}
			}
			
			if (!string.IsNullOrEmpty(info.rulebookDescription))
			{
				yield return Plugin.Translate(info.rulebookDescription, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					info.rulebookDescription = result.englishTranslation;
				}
			}
		}
	}
	
	[HarmonyPatch(typeof(ConsumableItemManager), "Add", new System.Type[] { typeof(string), typeof(ConsumableItemData)})]
	public class ConsumableItemManager_Add
	{
		public static void Postfix(string pluginGUID, ConsumableItemData data)
		{
			Plugin.Instance.StartCoroutine(Translate(data, Language.English));
		}

		public static IEnumerator Translate(ConsumableItemData data, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();

			if (!string.IsNullOrEmpty(data.rulebookName))
			{
				yield return Plugin.Translate(data.rulebookName, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					data.rulebookName = result.englishTranslation;
				}
			}

			if (!string.IsNullOrEmpty(data.rulebookDescription))
			{
				yield return Plugin.Translate(data.rulebookDescription, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					data.rulebookDescription = result.englishTranslation;
				}
			}

			if (!string.IsNullOrEmpty(data.description))
			{
				yield return Plugin.Translate(data.description, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					data.description = result.englishTranslation;
				}
			}
		}
	}
	
	[HarmonyPatch(typeof(ChallengeManager), nameof(ChallengeManager.AddSpecific), new System.Type[]
	{
		typeof(string), typeof(AscensionChallengeInfo), typeof(Type), typeof(int),
		typeof(List<object>), typeof(Func<ChallengeManager.FullChallenge[], IEnumerable<AscensionChallenge>>), 
		typeof(Func<ChallengeManager.FullChallenge[], IEnumerable<AscensionChallenge>>), typeof(int),
	})]
	public class ChallengeManager_AddSpecific
	{
		public static void Postfix(string pluginGuid, AscensionChallengeInfo info, Type handlerType, int unlockLevel, 
			List<object> flags, 
			Func<ChallengeManager.FullChallenge[], IEnumerable<AscensionChallenge>> dependantChallengeGetter, 
			Func<ChallengeManager.FullChallenge[], IEnumerable<AscensionChallenge>> incompatibleChallengeGetter, 
			int numAppearancesInChallengeScreen)
		{
			Plugin.Instance.StartCoroutine(Translate(info, Language.English));
		}

		public static IEnumerator Translate(AscensionChallengeInfo data, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();

			if (!string.IsNullOrEmpty(data.title))
			{
				yield return Plugin.Translate(data.title, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					data.title = result.englishTranslation;
				}
			}

			if (!string.IsNullOrEmpty(data.description))
			{
				yield return Plugin.Translate(data.description, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					data.description = result.englishTranslation;
				}
			}
		}
	}
	
	[HarmonyPatch(typeof(StarterDeckManager), nameof(StarterDeckManager.Add), new System.Type[]
	{
		typeof(string), typeof(StarterDeckInfo), typeof(int)
	})]
	public class StarterDeckManager_Add
	{
		public static void Postfix(string pluginGuid, StarterDeckInfo info, int unlockLevel = 0)
		{
			Plugin.Instance.StartCoroutine(Translate(info, Language.English));
		}

		public static IEnumerator Translate(StarterDeckInfo data, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();

			if (!string.IsNullOrEmpty(data.title))
			{
				yield return Plugin.Translate(data.title, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					data.title = result.englishTranslation;
				}
			}
		}
	}
	
	[HarmonyPatch(typeof(DialogueManager), "Add", new System.Type[] { typeof(string), typeof(DialogueEvent)})]
	public class DialogueManager_Add
	{
		public static void Postfix(ref DialogueManager.Dialogue __result, string pluginGUID, DialogueEvent dialogueEvent)
		{
			Plugin.Instance.StartCoroutine(Translate(__result, Language.English));
		}

		public static IEnumerator Translate(DialogueManager.Dialogue data, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();
			//string parentKey = $"dialogue.{data.PluginGUID}.{data.DialogueEvent.id}.{data.DialogueEvent.groupId}";

			for (int i = 0; i < data.DialogueEvent.mainLines.lines.Count; i++)
			{
				DialogueEvent.Line line = data.DialogueEvent.mainLines.lines[i];
				if (!string.IsNullOrEmpty(line.text))
				{
					//string key = $"{parentKey}.mainline.{i}";
					yield return Plugin.Translate(line.text, result);
					if (!string.IsNullOrEmpty(result.englishTranslation))
					{
						line.text = result.englishTranslation;
					}
				}
			}
			
			for (int i = 0; i < data.DialogueEvent.repeatLines.Count; i++)
			{
				DialogueEvent.LineSet set = data.DialogueEvent.repeatLines[i];
				for (int j = 0; j < set.lines.Count; j++)
				{
					DialogueEvent.Line line = set.lines[j];
					yield return Translate(line, language, result);
				}
			}

		}

		public static IEnumerator Translate(DialogueEvent.Line data, Language language, Plugin.Result result=null)
		{
			result ??= new Plugin.Result();

			if (!string.IsNullOrEmpty(data.text))
			{
				//string key = $"{parentKey}.repeatline.{i}.{j}";
				yield return Plugin.Translate(data.text, result);
				if (!string.IsNullOrEmpty(result.englishTranslation))
				{
					data.text = result.englishTranslation;
				}
			}
		}
	}
	
	// =============================================================
	// =============================================================
	// =============================================================
	
	[HarmonyPatch(typeof(SaveManager), "SaveToFile")]
	public static class SaveManager_SaveToFile
	{
		public static void Postfix()
		{
			Plugin.TranslationsManager.Save();
		}
	}
	
	[HarmonyPatch(typeof(OptionsUI), "OnSetLanguageButtonPressed")]
	public static class OptionsUI_OnSetLanguageButtonPressed
	{
		public static void Postfix()
		{
			// Translate everything!
			Plugin.Instance.StartCoroutine(PostfixCoroutine());
		}

		private static IEnumerator PostfixCoroutine()
		{
			Counter counter = new Counter();
			Language language = Localization.CurrentLanguage;
			Plugin.Log.LogInfo($"Translation into {language}...");
			
			// Cards
			foreach (CardInfo info in CardManager.NewCards)
			{
				Plugin.Result result = new Plugin.Result();
				IEnumerator enumerator = CardManager_Add.Translate(info, language);
				Plugin.Instance.StartCoroutine(WaitForCoroutineToFinish(counter, result, enumerator));
			}

			yield return new WaitUntil(() => counter.fired == counter.completed);

			// Stat icons
			if (counter.failed == 0)
			{
				counter.Reset();
				foreach (StatIconManager.FullStatIcon info in StatIconManager.NewStatIcons)
				{
					Plugin.Result result = new Plugin.Result();
					IEnumerator enumerator = StatIconManager_Add.Translate(info.Info, language);
					Plugin.Instance.StartCoroutine(WaitForCoroutineToFinish(counter, result, enumerator));
				}

				yield return new WaitUntil(() => counter.fired == counter.completed);
			}

			// Abilities
			if (counter.failed == 0)
			{
				counter.Reset();
				foreach (AbilityManager.FullAbility info in AbilityManager.NewAbilities)
				{
					Plugin.Result result = new Plugin.Result();
					IEnumerator enumerator = AbilityManager_Add.Translate(info.Info, language);
					Plugin.Instance.StartCoroutine(WaitForCoroutineToFinish(counter, result, enumerator));
				}

				yield return new WaitUntil(() => counter.fired == counter.completed);
			}

			// Items
			if (counter.failed == 0)
			{
				counter.Reset();
				foreach (ConsumableItemData data in ConsumableItemManager.NewConsumableItemDatas)
				{
					Plugin.Result result = new Plugin.Result();
					IEnumerator enumerator = ConsumableItemManager_Add.Translate(data, language);
					Plugin.Instance.StartCoroutine(WaitForCoroutineToFinish(counter, result, enumerator));
				}

				yield return new WaitUntil(() => counter.fired == counter.completed);
			}

			// Dialogue
			if (counter.failed == 0)
			{
				counter.Reset();
				foreach (DialogueManager.Dialogue data in DialogueManager.CustomDialogue)
				{
					Plugin.Result result = new Plugin.Result();
					IEnumerator enumerator = DialogueManager_Add.Translate(data, language);
					Plugin.Instance.StartCoroutine(WaitForCoroutineToFinish(counter, result, enumerator));
				}

				yield return new WaitUntil(() => counter.fired == counter.completed);
			}

			// Challenges
			if (counter.failed == 0)
			{
				counter.Reset();
				foreach (ChallengeManager.FullChallenge data in ChallengeManager.NewChallenges)
				{
					Plugin.Result result = new Plugin.Result();
					IEnumerator enumerator = ChallengeManager_AddSpecific.Translate(data, language);
					Plugin.Instance.StartCoroutine(WaitForCoroutineToFinish(counter, result, enumerator));
				}

				yield return new WaitUntil(() => counter.fired == counter.completed);
			}

			// Starter Decks
			if (counter.failed == 0)
			{
				counter.Reset();
				foreach (StarterDeckManager.FullStarterDeck data in StarterDeckManager.NewDecks)
				{
					Plugin.Result result = new Plugin.Result();
					IEnumerator enumerator = StarterDeckManager_Add.Translate(data.Info, language);
					Plugin.Instance.StartCoroutine(WaitForCoroutineToFinish(counter, result, enumerator));
				}

				yield return new WaitUntil(() => counter.fired == counter.completed);
			}

			if (counter.failed > 0)
			{
				Plugin.Log.LogError($"Didn't finish translating everything. Consider restarting the game to finish it!");
			}
			else
			{
				Plugin.Log.LogInfo($"Successfully translated everything!");
			}
			
			Plugin.TranslationsManager.Save();
		}

		private static IEnumerator WaitForCoroutineToFinish(Counter counter, Plugin.Result result, IEnumerator enumerator)
		{
			counter.fired++;
			yield return enumerator;
			counter.completed++;
			
			if (!result.succeeded)
			{
				counter.failed++;
			}
		}
	}

	public class Counter
	{
		public int completed = 0;
		public int failed = 0;
		public int fired = 0;

		public void Reset()
		{
			completed = 0;
			failed = 0;
			fired = 0;
		}
	}
	#pragma warning restore Harmony003
}