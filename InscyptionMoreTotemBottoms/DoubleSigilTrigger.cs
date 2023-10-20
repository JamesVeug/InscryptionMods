// using System.Collections;
// using DiskCardGame;
// using HarmonyLib;
// using UnityEngine;
//
// namespace MoreTotemBottoms;
//
// public class DoubleSigilTrigger : TotemTriggerReceiver
// {
// 	private List<string> cardIdsThatHaveShownAnimation = new List<string>();
//
// 	private DoubleSigilEffect.DoubleSigilEffectParameters EffectParameters => (DoubleSigilEffect.DoubleSigilEffectParameters)Data.bottom.effectParams;
// 	
// 	public override void OnBattleEnd()
// 	{
// 		cardIdsThatHaveShownAnimation.Clear();
// 	}
//
// 	public override bool RespondsToOtherCardDrawn(PlayableCard card)
// 	{
// 		Plugin.Log.LogInfo($"[RespondsToOtherCardDrawn] {card.Info.displayedName}");
// 		Plugin.Log.LogInfo($"[RespondsToOtherCardDrawn] {Data}");
// 		Plugin.Log.LogInfo($"[RespondsToOtherCardDrawn] {Data.top}");
// 		Plugin.Log.LogInfo($"[RespondsToOtherCardDrawn] {Data.top.prerequisites}");
// 		Plugin.Log.LogInfo($"[RespondsToOtherCardDrawn] {Data.top.prerequisites.tribe}");
// 		if (!(base.Totem is OpponentTotem) && card.Info.IsOfTribe(base.Data.top.prerequisites.tribe))
// 		{
// 			return !card.HasAbility(EffectParameters.ability) || !card.HasAbility(EffectParameters.ability2);
// 		}
// 		return false;
// 	}
//
// 	public override IEnumerator OnOtherCardDrawn(PlayableCard card)
// 	{
// 		if (EffectParameters.ability == Ability.None && EffectParameters.ability2 == Ability.None)
// 		{
// 			yield break;
// 		}
// 		
// 		if (DoShowCardAnimation(card))
// 		{
// 			cardIdsThatHaveShownAnimation.Add(card.Info.name);
// 			if ((Singleton<ViewManager>.Instance.Controller.LockState != ViewLockState.Locked || Singleton<ViewManager>.Instance.CurrentView == View.Hand) && Singleton<ViewManager>.Instance.Controller.CurrentControlModeAllowsView(View.Default))
// 			{
// 				Singleton<ViewManager>.Instance.SwitchToView(View.Default);
// 			}
// 			ViewLockState viewLockState = Singleton<ViewManager>.Instance.Controller.LockState;
// 			Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
// 			yield return base.Totem.ShowActivation();
// 			card.SetInteractionEnabled(interactionEnabled: false);
// 			yield return card.FlipInHand(delegate
// 			{
// 				AddModToCard(card);
// 				card.SetInteractionEnabled(interactionEnabled: false);
// 			});
// 			yield return new WaitForSeconds(0.25f);
// 			card.SetInteractionEnabled(interactionEnabled: true);
// 			Singleton<ViewManager>.Instance.Controller.LockState = viewLockState;
// 		}
// 		else
// 		{
// 			StartCoroutine(base.Totem.ShowActivation());
// 			AddModToCard(card);
// 		}
// 	}
//
// 	public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
// 	{
// 		if (otherCard.Slot.IsPlayerSlot && RespondsToOtherCardDrawn(otherCard) 
// 		                                && (!otherCard.HasAbility(EffectParameters.ability)
// 		                                || !otherCard.HasAbility(EffectParameters.ability2)))
// 		{
// 			return !otherCard.Info.Mods.Exists((CardModificationInfo x) => x.fromEvolve);
// 		}
// 		return false;
// 	}
//
// 	public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
// 	{
// 		StartCoroutine(base.Totem.ShowActivation());
// 		AddModToCard(otherCard);
// 		yield break;
// 	}
//
// 	private void AddModToCard(PlayableCard card)
// 	{
// 		if (!card.HasAbility(EffectParameters.ability))
// 		{
// 			CardModificationInfo cardModificationInfo = new CardModificationInfo();
// 			cardModificationInfo.fromTotem = true;
// 			cardModificationInfo.abilities.Add(EffectParameters.ability);
// 			card.AddTemporaryMod(cardModificationInfo);
// 		}
// 		
// 		if (!card.HasAbility(EffectParameters.ability2))
// 		{
// 			CardModificationInfo cardModificationInfo = new CardModificationInfo();
// 			cardModificationInfo.fromTotem = true;
// 			cardModificationInfo.abilities.Add(EffectParameters.ability2);
// 			card.AddTemporaryMod(cardModificationInfo);
// 		}
// 	}
//
// 	private bool DoShowCardAnimation(PlayableCard card)
// 	{
// 		return !cardIdsThatHaveShownAnimation.Contains(card.Info.name);
// 	}
// }
