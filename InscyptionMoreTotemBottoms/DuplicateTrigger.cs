using System.Collections;
using DiskCardGame;
using UnityEngine;

namespace MoreTotemBottoms;

public class DuplicateTrigger : TotemTriggerReceiver
{
	private List<string> cardIdsThatHaveShownAnimation = new List<string>();
	
	public override void OnBattleEnd()
	{
		cardIdsThatHaveShownAnimation.Clear();
	}

	public override bool RespondsToOtherCardDrawn(PlayableCard card)
	{
		return !(Totem is OpponentTotem) && card.Info.IsOfTribe(Data.top.prerequisites.tribe) && !card.HasModFromTotem();
	}

	public override IEnumerator OnOtherCardDrawn(PlayableCard card)
	{
		if (DoShowCardAnimation(card))
		{
			cardIdsThatHaveShownAnimation.Add(card.Info.name);
			if ((Singleton<ViewManager>.Instance.Controller.LockState != ViewLockState.Locked || Singleton<ViewManager>.Instance.CurrentView == View.Hand) && Singleton<ViewManager>.Instance.Controller.CurrentControlModeAllowsView(View.Default))
			{
				Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
			}
			ViewLockState viewLockState = Singleton<ViewManager>.Instance.Controller.LockState;
			Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
			yield return Totem.ShowActivation();
			card.SetInteractionEnabled(false);
			yield return card.FlipInHand(delegate
			{
				DuplicateCard(card);
				card.SetInteractionEnabled(false);
			});
			yield return new WaitForSeconds(0.25f);
			card.SetInteractionEnabled(true);
			Singleton<ViewManager>.Instance.Controller.LockState = viewLockState;
		}
		else
		{
			StartCoroutine(Totem.ShowActivation());
			DuplicateCard(card);
		}
	}

	public override bool RespondsToOtherCardAssignedToSlot(PlayableCard otherCard)
	{
		if (otherCard.Slot.IsPlayerSlot && RespondsToOtherCardDrawn(otherCard) && !otherCard.HasModFromTotem())
		{
			return !otherCard.Info.Mods.Exists((CardModificationInfo x) => x.fromEvolve);
		}
		return false;
	}

	public override IEnumerator OnOtherCardAssignedToSlot(PlayableCard otherCard)
	{
		StartCoroutine(Totem.ShowActivation());
		DuplicateCard(otherCard);
		yield break;
	}

	private void DuplicateCard(PlayableCard card)
	{
		CardModificationInfo cardModificationInfo = new CardModificationInfo
		{
			fromTotem = true,
		};
		card.AddTemporaryMod(cardModificationInfo);
		StartCoroutine(Singleton<CardSpawner>.Instance.SpawnCardToHand(card.Info, card.TemporaryMods, 0.25f, null));
	}

	private bool DoShowCardAnimation(PlayableCard card)
	{
		return !cardIdsThatHaveShownAnimation.Contains(card.Info.name);
	}

}