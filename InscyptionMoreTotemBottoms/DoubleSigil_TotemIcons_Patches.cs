using DiskCardGame;
using GBC;
using HarmonyLib;
using MoreTotemBottoms;
using UnityEngine;

[HarmonyPatch]
internal class DoubleSigil_TotemIcons_Patches // Modifies how cards are rendered so up to 8 sigils can be displayed on a card
{
    private static void AddTotemSlots(CardAbilityIcons controller)
    {
        if (controller.totemIcons.Count >= 2)
        {
            return;
        }
        
        AbilityIconInteractable slot = controller.totemIcons[0];
        Vector3 transformLocalPosition = slot.transform.localPosition;
        transformLocalPosition.x -= 0.15f;
        transformLocalPosition.y = 0.2f;
        slot.OriginalLocalPosition = transformLocalPosition;
        slot.transform.localPosition = transformLocalPosition;
        slot.transform.localScale *= 0.7f;

        AbilityIconInteractable clone = GameObject.Instantiate(slot, slot.transform.parent);
        Vector3 clonePosition = clone.transform.localPosition;
        clonePosition.x += 0.25f;
        clonePosition.y = 0.2f;
        clone.OriginalLocalPosition = clonePosition;
        clone.transform.localPosition = clonePosition; 
        
        controller.totemIcons.Add(clone);
    }


    [HarmonyPrefix, HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.UpdateAbilityIcons))]
    private static void AddExtraAbilityIcons(CardAbilityIcons __instance)
    {
        Plugin.Log.LogInfo("[AddExtraAbilityIcons] UpdateAbilityIcons");
        if (SaveManager.SaveFile.IsPart1)
        {
            AddTotemSlots(__instance);
        }
    }
}