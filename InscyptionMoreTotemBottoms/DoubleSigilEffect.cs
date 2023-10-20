// using DiskCardGame;
// using InscryptionAPI.Card;
// using InscryptionAPI.Totems;
// using UnityEngine;
//
// namespace MoreTotemBottoms;
//
// public class DoubleSigilEffect : TotemBottomEffect
// {
// 	public static TotemEffect ID;
// 	
// 	public class DoubleSigilEffectParameters : TotemBottomData.EffectParameters
// 	{
// 		public Ability ability2;
// 	}
//
// 	public override List<TotemBottomData> GetAllOptions(int randomSeed)
// 	{
// 		List<CustomTotemBottom> customTotemBottoms = TotemManager.RunStateCustomTotems.TotemBottoms;
// 		Plugin.Log.LogInfo("[DoubleSigilEffect] customTotemBottoms: " + customTotemBottoms.Count + " " + string.Join(", ", customTotemBottoms));
// 		
// 		HashSet<Ability> existingAbilities = new HashSet<Ability>();
// 		foreach (CustomTotemBottom customTotemBottom in customTotemBottoms)
// 		{
// 			if (!existingAbilities.Contains(customTotemBottom.EffectParameters.ability))
// 			{
// 				existingAbilities.Add(customTotemBottom.EffectParameters.ability);
// 			}
// 			
// 			if (customTotemBottom.EffectID == ID && customTotemBottom.EffectParameters is DoubleSigilEffectParameters doubleSigilEffectParameters)
// 			{
// 				if (!existingAbilities.Contains(doubleSigilEffectParameters.ability2))
// 				{
// 					existingAbilities.Add(doubleSigilEffectParameters.ability2);
// 				}
// 			}
// 		}
// 		Plugin.Log.LogInfo("[DoubleSigilEffect] existingAbilities: " + existingAbilities.Count + " " + string.Join(", ", existingAbilities));
//
// 		List<AbilityInfo> fullAbilities = new List<AbilityInfo>(AbilityManager.AllAbilityInfos);
// 		fullAbilities.RemoveAll((a) => existingAbilities.Contains(a.ability) || !a.metaCategories.Contains(AbilityMetaCategory.Part1Modular));
// 		Plugin.Log.LogInfo("[DoubleSigilEffect] fullAbilities: " + fullAbilities.Count + " " + string.Join(", ", fullAbilities));
// 		
// 		fullAbilities.Sort((a, b) =>
// 		{
// 			int randomValue = SeededRandom.Range(0, 100, randomSeed++);
// 			return randomValue >= 50 ? -1 : 1;
// 		});
//
// 		List<TotemBottomData> bottoms = new List<TotemBottomData>();
// 		for (int i = 0; i < fullAbilities.Count - 1; i+=2)
// 		{
// 			Ability a = fullAbilities[i].ability;
// 			Ability b = fullAbilities[i+1].ability;
//
// 			TotemBottomData data = ScriptableObject.CreateInstance<TotemBottomData>();
// 			data.effect = ID;
//
// 			DoubleSigilEffectParameters effectParams = new DoubleSigilEffectParameters()
// 			{
// 				ability = a,
// 				ability2 = b,
// 			};
// 			data.effectParams = effectParams;
// 			
// 			
// 			Plugin.Log.LogInfo("[DoubleSigilEffect] " + effectParams.ability + " " + effectParams.ability2);
// 			bottoms.Add(data);
// 		}
// 		
// 		Plugin.Log.LogInfo("[DoubleSigilEffect] bottoms: " + bottoms.Count + " " + string.Join(", ", bottoms));
// 		return bottoms;
// 	}
//
// 	public override void ModifyCardWithTotem(TotemBottomData totemData, CardModificationInfo mod)
// 	{
// 		DoubleSigilEffectParameters parameters = (DoubleSigilEffectParameters)totemData.effectParams;
// 		mod.abilities = new List<Ability>()
// 		{
// 			parameters.ability,
// 			parameters.ability2,
// 		};
// 	}
// }