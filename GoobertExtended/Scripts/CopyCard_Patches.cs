using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch]
internal class CopyCard_Patches
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
            options.AddRange(RandomOptions.GetAllRandomOptions().Where((a) => a.AllowMultiple));
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
