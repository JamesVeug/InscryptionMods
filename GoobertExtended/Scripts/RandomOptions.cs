using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

public static class RandomOptions
{
	public class WeightOption
	{
		public int Weight;
		public Action<CardInfo, CardModificationInfo> OnChosen;
		public Func<CardInfo, bool> Condition;
		public bool AllowMultiple = true;
	}

	
	public static List<WeightOption> GetAllRandomOptions()
	{
		List<WeightOption> options = new List<WeightOption>
		{
			// Change sigil
			new WeightOption()
			{
				Weight = Configs.ChanceOfSigilChange.Value,
				Condition = (c) => c.Abilities.Count > 0,
				OnChosen = OnChangeSigils
			},
			// Add Sigil
			new WeightOption()
			{
				Weight = Configs.ChanceOfSigilAdd.Value,
				Condition = (c) => true,
				OnChosen = OnAddSigils
			},
			// Change Attack
			new WeightOption()
			{
				Weight = Configs.ChanceOfAttackChange.Value,
				Condition = (c) => c.Attack > 0,
				OnChosen = OnChangeAttack
			},
			// Change Health
			new WeightOption()
			{
				Weight = Configs.ChanceOfHealthChange.Value,
				Condition = (c) => c.Health > 1,
				OnChosen = OnChangeHealth
			},
			// Change Tribe
			new WeightOption()
			{
				Weight = Configs.ChanceOfTribesChange.Value,
				Condition = (c) => true,
				OnChosen = OnChangeTribe
			},
			// Change Cost
			new WeightOption()
			{
				Weight = Configs.ChanceOfCostChange.Value,
				Condition = (c) => c.BloodCost > 0 || c.BonesCost > 0 || c.EnergyCost > 0 || c.GemsCost.Count > 0,
				OnChosen = OnChangeCost
			},
		};

		return options;
	}
	
	static void OnChangeSigils(CardInfo cardByName, CardModificationInfo copyMod)
	{
		Plugin.Log.LogInfo("Change Sigil");
		// Change mods if we can. Otherwise change the card info

		
		List<CardModificationInfo> modsWithAbilities = cardByName.Mods.FindAll((CardModificationInfo x) => x.abilities.Count > 0);
		List<Ability> vanillaAbilities = cardByName.abilities;
		List<int> vanillaAbilitiesIndex = Enumerable.Range(0, cardByName.abilities.Count).ToList();

		modsWithAbilities.Sort((a, b) => SeededRandom.Bool(Plugin.Patches.Seed++) ? 1 : -1);
		vanillaAbilitiesIndex.Sort((a, b) => SeededRandom.Bool(Plugin.Patches.Seed++) ? 1 : -1);
			
		List<Ability> learnedAbilities = AbilitiesUtil.GetLearnedAbilities(false);
		learnedAbilities.Sort((a, b) => AbilitiesUtil.GetInfo(a).powerLevel - AbilitiesUtil.GetInfo(b).powerLevel);

		int shuffles = !Configs.CanShuffleAllSigils.Value
			? 1
			: SeededRandom.Range(1, modsWithAbilities.Count + vanillaAbilities.Count, Plugin.Patches.Seed++);

		//Plugin.Log.LogInfo($"Changing {shuffles} Sigils ({modsWithAbilities.Count} mods and {vanillaAbilitiesIndex.Count} vanilla");
		for (int i = 0; i < shuffles; i++)
		{
			int modWeight = modsWithAbilities.Count;
			int vanillaWeight = vanillaAbilitiesIndex.Count;
			bool chooseMod = false;
			if (modWeight > 0 && vanillaWeight > 0)
			{
				chooseMod = SeededRandom.Range(0, modWeight + vanillaWeight, Plugin.Patches.Seed++) < modWeight;
			}
			else if (modWeight > 0)
			{
				chooseMod = true;
			}

			int index2 = 0;
			Ability replacement = Ability.Apparition;
			if (Configs.SuperMode.Value && shuffles > 1)
			{
				int range = learnedAbilities.Count / shuffles;
				int minRange = i * range;
				int maxRange = (i + 1) * range;
				index2 = SeededRandom.Range(minRange, maxRange, Plugin.Patches.Seed++);
			}
			else
			{
				index2 = SeededRandom.Range(0, learnedAbilities.Count, Plugin.Patches.Seed++);
			}
			replacement = learnedAbilities[index2]; 
			learnedAbilities.RemoveAt(index2);
			
			if (chooseMod)
			{
				// TODO: Change all abilities on a mod....
				int index = SeededRandom.Range(0, modsWithAbilities[0].abilities.Count, Plugin.Patches.Seed++);
				//Plugin.Log.LogInfo($"Changing mod {modsWithAbilities[0].abilities[index]} to {replacement}");
				modsWithAbilities[0].abilities[index] = replacement;
				modsWithAbilities.RemoveAt(0);
			}
			else
			{
				int index = vanillaAbilitiesIndex[0];
				//Plugin.Log.LogInfo($"Changing vanilla {vanillaAbilities[index]} to {replacement}");
				vanillaAbilities[index] = replacement;
				vanillaAbilitiesIndex.RemoveAt(0);
			}
		}
	}
	
	static void OnAddSigils(CardInfo cardByName, CardModificationInfo copyMod)
	{
		Plugin.Log.LogInfo("Add Sigil");
		int amount = Configs.SuperMode.Value ? SeededRandom.Range(1, 2, Plugin.Patches.Seed++) : 1;
		List<Ability> list = AbilitiesUtil.GetLearnedAbilities();
		list.RemoveAll((a) =>
		{
			AbilityInfo info = AbilitiesUtil.GetInfo(a);
			if (info == null || !info.canStack && cardByName.HasAbility(a))
			{
				return true;
			}

			return false;
		});
		for (int i = 0; i < amount; i++)
		{
			Ability seededRandom = list.GetSeededRandom(Plugin.Patches.Seed++);
			list.Remove(seededRandom);
			cardByName.abilities.Add(seededRandom);
		}
	}
	
	static void OnRemoveSigil(CardInfo cardByName, CardModificationInfo copyMod)
	{
		Plugin.Log.LogInfo("Remove Sigil");
			
		int amount = Configs.SuperMode.Value ? SeededRandom.Range(1, 2, Plugin.Patches.Seed++) : 1;
		for (int i = 0; i < amount; i++)
		{
			List<CardModificationInfo> list = cardByName.Mods.FindAll((CardModificationInfo x) => x.abilities.Count > 0);

			CardModificationInfo info = list.GetSeededRandom(Plugin.Patches.Seed++);
			info.abilities.Remove(info.abilities.GetSeededRandom(Plugin.Patches.Seed++));
		}
	}
	
	static void OnChangeAttack(CardInfo cardByName, CardModificationInfo copyMod)
	{
		int amount = Configs.SuperMode.Value ? 2 : 1;
		bool increase = SeededRandom.Bool(Plugin.Patches.Seed++);
		Plugin.Log.LogInfo(increase ? "Increase Attack" : "Decrease Attack");
		if (increase)
		{
			copyMod.attackAdjustment = amount;
		}
		else
		{
			copyMod.attackAdjustment = -Mathf.Min(cardByName.Attack, amount);
		}
	}
	
	static void OnChangeHealth(CardInfo cardByName, CardModificationInfo copyMod)
	{
		int amount = Configs.SuperMode.Value ? 4 : 2;
		bool increase = SeededRandom.Bool(Plugin.Patches.Seed++);
		Plugin.Log.LogInfo(increase ? "Increase Health" : "Decrease Health");
		if (increase)
		{
			copyMod.healthAdjustment = amount;
		}
		else
		{
			copyMod.healthAdjustment = -Mathf.Min(cardByName.Health - 1, amount);
		}
	}
	
	static void OnChangeTribe(CardInfo cardByName, CardModificationInfo copyMod)
	{
		bool increase = cardByName.tribes.Count <= 0 || SeededRandom.Bool(Plugin.Patches.Seed++);
		List<Tribe> allTribes = new List<Tribe>();
		allTribes.AddRange(Enum.GetValues(typeof(Tribe)).Cast<Tribe>());
		allTribes.Remove(Tribe.None);
		allTribes.Remove(Tribe.NUM_TRIBES);
		allTribes.AddRange(TribeManager.NewTribes.Select((a)=>a.tribe));
		allTribes.RemoveAll((a) => cardByName.tribes.Contains(a));
		if (allTribes.Count == 0)
			return;

		Tribe newTribe = allTribes.GetSeededRandom(Plugin.Patches.Seed++);
		if (increase)
		{
			Plugin.Log.LogInfo("Add Tribe" + newTribe);
			cardByName.tribes.Add(newTribe);
		}
		else
		{
			int i = SeededRandom.Range(0, cardByName.tribes.Count, Plugin.Patches.Seed++);
			Plugin.Log.LogInfo($"Change Tribe {cardByName.tribes[i]} to {newTribe}");
			cardByName.tribes[i] = newTribe;
		}
	}

	private static char[] vowels = new char[]
	{
		'a','e','i','o','u'
	};
	
	private static Dictionary<char, string[]> replacements = new Dictionary<char, string[]>()
	{
		{ 'a', new string[] { "o" } },
		{ 'b', new string[] { "d" } },
		{ 'c', new string[] { "k" } },
		{ 'd', new string[] { "b" } },
		{ 'e', new string[] { } },
		{ 'f', new string[] { "t" } },
		{ 'g', new string[] { "p" } },
		{ 'h', new string[] { "m" } },
		{ 'i', new string[] { "l", "j", "1" } },
		{ 'j', new string[] { "i", "l", "1" } },
		{ 'k', new string[] { "c" } },
		{ 'l', new string[] { "i", "1" } },
		{ 'm', new string[] { "n" } },
		{ 'n', new string[] { "m" } },
		{ 'o', new string[] { "0", "a", "oo" } },
		{ 'p', new string[] { "q" } },
		{ 'q', new string[] { "p" } },
		{ 'r', new string[] { } },
		{ 's', new string[] { "z", "sh", "st" } },
		{ 't', new string[] { "th", "st", "f" } },
		{ 'u', new string[] { } },
		{ 'v', new string[] { "w" } },
		{ 'w', new string[] { "v" } },
		{ 'x', new string[] { } },
		{ 'y', new string[] { } },
		{ 'z', new string[] { "s", "zz" } },
		{ ' ', new string[] { "" } },
		{ '0', new string[] { "a" } },
		{ '1', new string[] { "i", "j", "l" } },
	};
	
	public static void OnNameChange(CardInfo cardByName, CardModificationInfo copyMod)
	{
		// Change displayName by 1-2 characters
		// Cat => Cot // Kat
		// Possum => Posum
		// Stoat => Shtoat
		// Wolf => Volf
		// Bear => Beer
		Plugin.Log.LogInfo("Change Name");
		int amountToChange = 1;//SeededRandom.Range(1, 2, Plugin.Patches.Seed++);
		for (int i = 0; i < amountToChange && cardByName.displayedName.Length > 2; i++)
		{
			int indexToChange = SeededRandom.Range(1, cardByName.displayedName.Length - 1, Plugin.Patches.Seed++);
			char character = cardByName.displayedName[indexToChange];
			bool isUpperCase = Char.IsUpper(character);
			char c = isUpperCase ? Char.ToLowerInvariant(character) : character;
			if (c == cardByName.displayedName[indexToChange - 1] && SeededRandom.Bool(Plugin.Patches.Seed++))
			{
				// SS => S
				cardByName.displayedName = cardByName.displayedName.Remove(indexToChange, 1);
			}
			else if (replacements.TryGetValue(c, out string[] others) && others.Length > 0)
			{
				// S => St
				string left = cardByName.displayedName.Substring(0, indexToChange);
				string right = cardByName.displayedName.Substring(indexToChange + 1,
					cardByName.displayedName.Length - indexToChange - 1);
					
				int replacementIndex = SeededRandom.Range(0, others.Length, Plugin.Patches.Seed++);
				string replacement = others[replacementIndex];
				if (isUpperCase)
				{
					replacement = replacement.ToUpper();
				}
				
				cardByName.displayedName = left + replacement + right;
			}
			else if (SeededRandom.Bool(Plugin.Patches.Seed++))
			{
				// Cat => Ct
				cardByName.displayedName = cardByName.displayedName.Remove(indexToChange, 1);
			}
		}
	}
	
	static void OnChangeCost(CardInfo cardByName, CardModificationInfo copyMod)
	{
		Plugin.Log.LogInfo("Change Cost");
		int multiplier = Configs.SuperMode.Value ? 2 : 1;
	
		bool increase = SeededRandom.Bool(Plugin.Patches.Seed++);
		if (cardByName.BloodCost > 0)
		{
			Plugin.Log.LogInfo(increase ? "Increase Blood Cost" : "Decrease Blood Cost");
			int amount = 1 * multiplier;
			int minDecrease = Mathf.Max(1, Mathf.Min(amount, cardByName.BloodCost - amount));
			copyMod.bloodCostAdjustment = increase ? minDecrease : -minDecrease;
		}
		else if (cardByName.BonesCost > 0)
		{
			Plugin.Log.LogInfo(increase ? "Increase Bones Cost" : "Decrease Bones Cost");
			int amount = 2 * multiplier;
			int minDecrease = Mathf.Max(1, Mathf.Min(amount, cardByName.BonesCost - amount));
			copyMod.bonesCostAdjustment = increase ? amount : -minDecrease;
		}
		else if (cardByName.EnergyCost > 0)
		{
			Plugin.Log.LogInfo(increase ? "Increase Energy Cost" : "Decrease Energy Cost");
			int amount = 2 * multiplier;
			int minDecrease = Mathf.Max(1, Mathf.Min(amount, cardByName.EnergyCost - amount));
			copyMod.energyCostAdjustment = increase ? minDecrease : -minDecrease;
		}
		else if (cardByName.GemsCost != null && cardByName.GemsCost.Count > 0)
		{
			if (increase)
			{
				Plugin.Log.LogInfo("Increase Gem Cost");
				// Find all gem types we COULD apply
				List<GemType> gemTypes = new List<GemType>();
				foreach (GemType o in Enum.GetValues(typeof(GemType)))
				{
					if (!cardByName.GemsCost.Contains(o))
					{
						gemTypes.Add(o);
					}
				}

				int amount = 1 * multiplier;
				copyMod.addGemCost = new List<GemType>();
				for (int i = 0; i < amount && gemTypes.Count > 0; i++)
				{
					int index = SeededRandom.Range(0, gemTypes.Count, Plugin.Patches.Seed++);
					copyMod.addGemCost.Add(gemTypes[index]);
					gemTypes.RemoveAt(index);
				}
			}
			else
			{
				Plugin.Log.LogInfo("Decrease Gem Cost");
				List<GemType> allGems = cardByName.GemsCost;
				
				int amount = 1 * multiplier;
				for (int i = 0; i < amount && allGems.Count > 0; i++)
				{
					int index = SeededRandom.Range(0, allGems.Count, Plugin.Patches.Seed++);
					GemType type = allGems[index];
					
					// Remove from mods if they are in mods otherwise remove from the card info
					CardModificationInfo mod = cardByName.Mods.Where((CardModificationInfo x) => x.addGemCost != null && x.addGemCost.Contains(type)).FirstOrDefault();
					if (mod != null)
					{
						mod.addGemCost.Remove(type);
					}
					else if (cardByName.gemsCost.Contains(type))
					{
						cardByName.gemsCost.Remove(type);
					}
					else
					{
						Plugin.Log.LogError($"Could not find gem type to remove! {cardByName.name} => {type}");
					}
					
					allGems = cardByName.GemsCost;
				}
			}
		}
	}
}