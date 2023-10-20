using System;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Saves;
using UnityEngine;

[HarmonyPatch]
public class TemporaryCardInfo
{
    public static void SaveAsTemporaryCard(CardInfo cardInfo)
    {
        int value = ModdedSaveManager.RunState.GetValueAsInt(Plugin.PluginGuid, "TotalTempCards") + 1;
        ModdedSaveManager.RunState.SetValue(Plugin.PluginGuid, "TotalTempCards", value);

        cardInfo.name += "_temp"+value;
        string json = JsonUtility.ToJson(cardInfo);
        ModdedSaveManager.RunState.SetValue(Plugin.PluginGuid, cardInfo.name, json);
    }
    
    public static CardInfo GetTemporaryCard(string cardInfoName)
    {
        string s = ModdedSaveManager.RunState.GetValue(Plugin.PluginGuid, cardInfoName);
        if (!string.IsNullOrEmpty(s))
        {
            CardInfo cardInfo = ScriptableObject.CreateInstance<CardInfo>();
            JsonUtility.FromJsonOverwrite(s, cardInfo);
            return cardInfo;
        }

        return null;
    }

    [HarmonyPatch(typeof(CardManager), "LogCardInfo", new Type[] { typeof(CardInfo), typeof(string) })]
    [HarmonyPrefix]
    public static bool GetTemporaryCard(ref CardInfo info, ref string cardInfoName)
    {
        if (info == null)
        {
            info = GetTemporaryCard(cardInfoName);
        }

        return true;
    }
}