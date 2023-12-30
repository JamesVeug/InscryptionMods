using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch]
internal class DuplicateMerge_Patches
{
    [HarmonyPatch(typeof(DuplicateMergeSequencer), nameof(DuplicateMergeSequencer.GetValidDuplicateCards))]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this

        // Dictionary<string, List<CardInfo>> dictionary = new Dictionary<string, List<CardInfo>>();
        // ...
        // HashSet<Ability> hashSet = new HashSet<Ability>();

        // === Into this

        // Dictionary<string, List<CardInfo>> dictionary = new Dictionary<string, List<CardInfo>>();
        // ...
        // GetValidCards(dictionary);
        // HashSet<Ability> hashSet = new HashSet<Ability>();

        // ===

        MethodInfo GetValidCards =
            AccessTools.Method(typeof(DuplicateMerge_Patches), nameof(DuplicateMerge_Patches.GetValidCards));

        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        int endOfDictionaryList = codes.FindIndex((a) => a.opcode == OpCodes.Stloc_2);
        codes.Insert(endOfDictionaryList - 2, new CodeInstruction(OpCodes.Ldloc_1));
        codes.Insert(endOfDictionaryList - 1, new CodeInstruction(OpCodes.Call, GetValidCards));
        return codes;
    }

    public static void GetValidCards(Dictionary<string, List<CardInfo>> dictionary)
    {
        foreach (CardInfo card in RunState.DeckList)
        {
            if (card.name.Contains("DEATHCARD"))
                continue;

            string templateID = card.GetTemporaryCardTemplateName();
            if (RunState.DeckList.Count((a) =>
                    a.name == templateID || TemporaryCardInfo.GetTemporaryCardTemplateName(a) == templateID) > 1)
            {
                if (!dictionary.ContainsKey(templateID))
                {
                    dictionary.Add(templateID, new List<CardInfo>());
                }

                List<CardInfo> list = dictionary[templateID];
                if (!list.Contains(card))
                    list.Add(card);
            }
        }
    }

    [HarmonyPatch(typeof(DuplicateMergeSequencer), nameof(DuplicateMergeSequencer.MergeCards))]
    [HarmonyPrefix]
    internal static bool Prefix(DuplicateMergeSequencer __instance, CardInfo card1, CardInfo card2,
        ref CardInfo __result)
    {
        bool aIsTemporary = card1.IsTemporaryCardInfo();
        bool bIsTemporary = card2.IsTemporaryCardInfo();
        if (!aIsTemporary && !bIsTemporary)
        {
            return true;
        }


        CardInfo cardByName = card1.CloneAsTemporaryCard();
        cardByName.baseAttack += card2.baseAttack;
        cardByName.baseHealth += card2.baseHealth;
        cardByName.cost = Mathf.Max(0, cardByName.cost + card2.cost - cardByName.cost);
        cardByName.bonesCost = Mathf.Max(0, cardByName.bonesCost + card2.bonesCost - cardByName.bonesCost);
        cardByName.energyCost = Mathf.Max(0, cardByName.energyCost + card2.energyCost - cardByName.energyCost);
        cardByName.gemsCost = CombineLists(card1.gemsCost, card2.gemsCost);
        cardByName.tribes = CombineLists(card1.tribes, card2.tribes);
        cardByName.traits = CombineLists(card1.traits, card2.traits);
        cardByName.abilities = CombineLists(card1.abilities, card2.abilities);


        CardModificationInfo duplicateMod = DuplicateMergeSequencer.GetDuplicateMod(0, 0);
        int num = 0;
        foreach (CardModificationInfo mod in card1.Mods)
        {
            if (mod != null && mod.fromCardMerge)
            {
                num += mod.abilities.Count;
            }
        }

        foreach (CardModificationInfo mod2 in card2.Mods)
        {
            if (mod2.fromCardMerge)
            {
                duplicateMod.fromCardMerge = true;
            }

            foreach (Ability ability in mod2.abilities)
            {
                if (!card1.HasAbility(ability) && duplicateMod.abilities.Count + num < 4)
                {
                    duplicateMod.abilities.Add(ability);
                }
            }
        }

        cardByName.Mods.Add(duplicateMod);

        RunState.Run.playerDeck.AddCard(cardByName);
        RunState.Run.playerDeck.RemoveCard(card1);
        RunState.Run.playerDeck.RemoveCard(card2);


        TemporaryCardInfo.SaveTemporaryCard(cardByName);

        __result = cardByName;
        return false;
    }

    private static List<T> CombineLists<T>(List<T> card1, List<T> card2)
    {
        List<T> list = new List<T>();

        Dictionary<T, int> card1Abilities = new Dictionary<T, int>();
        foreach (T ability in card1)
        {
            if (!card1Abilities.ContainsKey(ability))
            {
                card1Abilities.Add(ability, 0);
            }

            card1Abilities[ability]++;
        }

        Dictionary<T, int> card2Abilities = new Dictionary<T, int>();
        foreach (T ability in card2)
        {
            if (!card2Abilities.ContainsKey(ability))
            {
                card2Abilities.Add(ability, 0);
            }

            card2Abilities[ability]++;
        }

        foreach (KeyValuePair<T, int> card2Count in card2Abilities)
        {
            if (card1Abilities.TryGetValue(card2Count.Key, out int card1Count))
            {
                card1Abilities[card2Count.Key] = Mathf.Max(card1Count, card2Count.Value);
            }
            else
            {
                card1Abilities[card2Count.Key] = card2Count.Value;
            }
        }

        foreach (KeyValuePair<T, int> pair in card1Abilities)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                list.Add(pair.Key);
            }
        }

        return list;
    }
}
