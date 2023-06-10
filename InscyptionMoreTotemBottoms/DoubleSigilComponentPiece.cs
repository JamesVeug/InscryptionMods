using DiskCardGame;
using HarmonyLib;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

namespace MoreTotemBottoms;

public class DoubleSigilComponentPiece : CompositeTotemPiece
{
	public Renderer emissiveRenderer2 = null;
	
	public override void SetData(ItemData data)
	{
		Plugin.Log.LogInfo($"[DoubleSigilComponentPiece] SetData " + data);
        
		// Assign gameobject the icon will appear on
		AssignEmittingIcon();
		if (data is TotemBottomData bottomData)
		{
			DoubleSigilEffect.DoubleSigilEffectParameters effectParameters = bottomData.effectParams as DoubleSigilEffect.DoubleSigilEffectParameters;
			if (effectParameters != null)
			{
				SetIcon(effectParameters.ability, ref emissiveRenderer);
				SetIcon(effectParameters.ability2, ref emissiveRenderer2);
			}
			else
			{
				Plugin.Log.LogError($"[DoubleSigilComponentPiece] effectParameters is not DoubleSigil");
			}
		}
        
		// Set icon
		base.SetData(data);
	}

	private void SetIcon(Ability ability, ref Renderer renderer)
	{
		renderer.material.mainTexture = AbilityManager.AllAbilities.Find(a =>
		{
			return a.Id == ability;
		}).Texture;
		renderer.GetComponent<AbilityIconInteractable>().AssignAbility(ability, null, null);
	}

	// SetIcon

	public void AssignEmittingIcon()
	{
		GetRenderer("IconRenderer1", ref emissiveRenderer);
		GetRenderer("IconRenderer2", ref emissiveRenderer2);
	}

	private void GetRenderer(string IconGameObjectName, ref Renderer renderer)
	{
		if (renderer != null)
		{
			return;
		}

		GameObject icon = this.gameObject.FindChild(IconGameObjectName);
		if (icon != null)
		{
			renderer = icon.GetComponent<Renderer>();
			if (renderer == null)
			{
				Plugin.Log.LogError(
					$"Could not find Renderer on GameObject with name {IconGameObjectName} to assign totem icon!");
			}
		}
		else
		{
			Plugin.Log.LogError(
				$"Could not find GameObject with name {IconGameObjectName} to assign totem icon!");
		}
	}
}

#region patches

[HarmonyPatch]
public static class DoubleSigilComponentPiece_Patches
{
	[HarmonyPostfix, HarmonyPatch(typeof(CompositeTotemPiece), nameof(CompositeTotemPiece.Start))]
	public static void Start(CompositeTotemPiece __instance)
	{
		if (__instance is DoubleSigilComponentPiece doubleSigilComponentPiece)
		{
			doubleSigilComponentPiece.emissiveRenderer2.material.SetColor("_EmissionColor", Color.black);
		}
	}
	
	[HarmonyPostfix, HarmonyPatch(typeof(CompositeTotemPiece), nameof(CompositeTotemPiece.PulseEmission))]
	public static void PulseEmission(CompositeTotemPiece __instance)
	{
		if (__instance is DoubleSigilComponentPiece doubleSigilComponentPiece)
		{
			Tween.ShaderColor(doubleSigilComponentPiece.emissiveRenderer2.material, "_EmissionColor", __instance.EmissionColor, 0.35f, 0f);
			Tween.ShaderColor(doubleSigilComponentPiece.emissiveRenderer2.material, "_EmissionColor", Color.black, 0.35f, 0.5f);
		}
	}
	
	[HarmonyPrefix, HarmonyPatch(typeof(CompositeTotemPiece), nameof(CompositeTotemPiece.SetEmitting))]
	public static bool SetEmitting(CompositeTotemPiece __instance, bool emitting, bool immediate = false)
	{
		if (__instance is DoubleSigilComponentPiece doubleSigilComponentPiece)
		{
			if (__instance.emitting != emitting)
			{
				Tween.ShaderColor(doubleSigilComponentPiece.emissiveRenderer2.material, "_EmissionColor",
					emitting ? __instance.EmissionColor : Color.black, immediate ? 0f : 0.35f, 0f);
			}
		}

		return true;
	}
	
	[HarmonyPrefix, HarmonyPatch(typeof(CompositeTotemPiece), nameof(CompositeTotemPiece.SetIconInteractable))]
	public static bool SetIconInteractable(CompositeTotemPiece __instance, bool interactable)
	{
		if (__instance is DoubleSigilComponentPiece doubleSigilComponentPiece)
		{
			doubleSigilComponentPiece.emissiveRenderer2.GetComponent<AbilityIconInteractable>().SetEnabled(interactable);
		}
		return true;
	}
} 

#endregion