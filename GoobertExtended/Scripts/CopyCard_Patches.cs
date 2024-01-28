using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers.Extensions;
using Pixelplacement;
using UnityEngine;

[HarmonyPatch]
internal class CopyCard_Patches
{
    public static int Seed;
    private static Vector3 selectedCardEaselPosition = Vector3.zero;
    private static Vector3 copiedCardEaselPosition = Vector3.zero;
    private static Quaternion selectedCardEaselRotation = Quaternion.identity;
    private static Quaternion copiedCardEaselRotation = Quaternion.identity;

    [HarmonyPatch(typeof(CopyCardSequencer), nameof(CopyCardSequencer.Start))]
    [HarmonyPostfix]
    public static void CopyCardSequence_Start_Patch(CopyCardSequencer __instance)
    {
	    selectedCardEaselPosition = __instance.selectedCardEasel.transform.position;
	    selectedCardEaselRotation = __instance.selectedCardEasel.transform.rotation;
	    
	    copiedCardEaselPosition = __instance.copiedCardEasel.transform.position;
	    copiedCardEaselRotation = __instance.copiedCardEasel.transform.rotation;
    }
    
    [HarmonyPatch(typeof(CopyCardSequencer), nameof(CopyCardSequencer.CopyCardSequence))]
    [HarmonyPostfix]
    public static IEnumerator CopyCardSequence_Patch(IEnumerator enumerator, CopyCardSequencer __instance)
    {
	    __instance.selectedCardEasel.transform.position = selectedCardEaselPosition;
	    __instance.selectedCardEasel.transform.rotation = selectedCardEaselRotation;
	    __instance.copiedCardEasel.transform.position = copiedCardEaselPosition;
	    __instance.copiedCardEasel.transform.rotation = copiedCardEaselRotation;

	    Singleton<ViewManager>.Instance.SwitchToView(View.Default, immediate: false, lockAfter: true);
		__instance.bottleAnim.gameObject.SetActive(value: false);
		__instance.selectedCardEasel.gameObject.SetActive(value: false);
		__instance.copiedCardEasel.gameObject.SetActive(value: false);
		__instance.copyCardToInstantiate.gameObject.SetActive(value: false);
		SelectableCard copyCard = UnityEngine.Object.Instantiate(__instance.copyCardToInstantiate.gameObject, __instance.copyCardToInstantiate.transform.parent).GetComponent<SelectableCard>();
		copyCard.SetEnabled(enabled: false);
		copyCard.gameObject.SetActive(value: true);
		CardInfo info = new CardInfo();
		copyCard.RenderInfo.hiddenAttack = (copyCard.RenderInfo.hiddenHealth = true);
		copyCard.SetInfo(info);
		__instance.selectionSlot.Disable();
		__instance.selectionSlot.SetShown(shown: false, immediate: true);
		__instance.selectionSlot.gameObject.SetActive(value: false);
		yield return new WaitForSeconds(0.2f);
		__instance.selectedCardEasel.gameObject.SetActive(value: true);
		__instance.copiedCardEasel.gameObject.SetActive(value: true);
		__instance.StartCoroutine(__instance.DropItemAnim(__instance.copiedCardEaselAnim, ""));
		yield return __instance.DropItemAnim(__instance.selectedCardEaselAnim, "wood_object_hit");
		yield return new WaitForSeconds(0.25f);
		yield return __instance.DropItemAnim(__instance.bottleAnim, "glass_object_hit");
		yield return new WaitForSeconds(1f);
		yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("CopyCardIntro1", TextDisplayer.MessageAdvanceMode.Input);
		yield return new WaitForSeconds(0.1f);
		__instance.JumpBottle(0.3f, 0.15f);
		__instance.gooAnim.SetBool("speaking", value: true);
		yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("CopyCardIntro2", TextDisplayer.MessageAdvanceMode.Input);
		__instance.JumpBottle(0.3f, 0.1f);
		__instance.gooAnim.SetBool("speaking", value: false);
		yield return __instance.pile.SpawnCards(RunState.DeckList.Count, 0.5f);
		Singleton<TableRuleBook>.Instance.SetOnBoard(onBoard: true);
		__instance.selectionSlot.gameObject.SetActive(value: true);
		__instance.selectionSlot.RevealAndEnable();
		__instance.selectionSlot.ClearDelegates();
		SelectCardFromDeckSlot selectCardFromDeckSlot = __instance.selectionSlot;
		selectCardFromDeckSlot.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(selectCardFromDeckSlot.CursorSelectStarted, (Action<MainInputInteractable>)delegate(MainInputInteractable i)
		{
			__instance.OnSlotSelected(i);
		});
		
		// Wait for player to choose a card and confirm it
		yield return __instance.confirmStone.WaitUntilConfirmation();
		
		// Painting animation
		__instance.selectionSlot.Disable();
		yield return new WaitForSeconds(0.3f);
		__instance.confirmStone.Exit();
		yield return new WaitForSeconds(0.2f);
		Singleton<ViewManager>.Instance.SwitchToView(View.Default, immediate: false, lockAfter: true);
		yield return new WaitForSeconds(0.2f);
		Tween.LocalRotation(__instance.copiedCardEasel, new Vector3(0f, -105f, 0f), 0.2f, 0f, Tween.EaseInOut);
		Tween.LocalRotation(__instance.selectedCardEasel, new Vector3(0f, 105f, 0f), 0.2f, 0.02f, Tween.EaseInOut);
		Tween.Rotate(__instance.bottleAnim.transform.parent, new Vector3(0f, -40f, 0f), Space.Self, 0.15f, 0f, Tween.EaseInOut);
		__instance.gooEyesController.SetLookTarget(__instance.selectionSlot.Card.transform);
		yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("CopyCardExamineSelection", TextDisplayer.MessageAdvanceMode.Input);
		yield return new WaitForSeconds(0.2f);
		Tween.Rotate(__instance.bottleAnim.transform.parent, new Vector3(0f, 80f, 0f), Space.Self, 0.2f, 0f, Tween.EaseInOut);
		__instance.gooEyesController.SetLookTarget(copyCard.transform);
		__instance.gooAnim.SetTrigger("painting");
		yield return new WaitForSeconds(3.5f);
		
		// check if its a bad painting
		bool badPainting = SeededRandom.Range(0, 100, Seed++) < Configs.ChanceOfCardReplacement.Value;
		SelectableCard replacementCard = null;
		if (badPainting)
		{
			SelectableCard selectableCard = ResourceBank.Get<SelectableCard>("prefabs/cards/SelectableCard");
			GameObject parent = GameObject.Find("GooWizard");
			Plugin.Log.LogInfo("GooWizard parent: " + parent);
			
			replacementCard = GameObject.Instantiate(selectableCard, parent.transform);
			replacementCard.gameObject.SetActive(false);
			
			yield return BadPainting(__instance, replacementCard);
		}
		else
		{
			// Create card clone and show
			CardInfo cardInfo = __instance.CreateCloneCard(__instance.selectionSlot.Card.Info);
			RunState.Run.playerDeck.AddCard(cardInfo);
			SaveManager.SaveToFile();
			copyCard.RenderInfo.hiddenAttack = (copyCard.RenderInfo.hiddenHealth = false);
			copyCard.SetInfo(cardInfo);
			Tween.LocalRotation(__instance.copiedCardEasel, Vector3.zero, 0.2f, 0.02f, Tween.EaseInOut);
			Tween.LocalRotation(__instance.selectedCardEasel, Vector3.zero, 0.2f, 0f, Tween.EaseInOut);
			Tween.Rotate(__instance.bottleAnim.transform.parent, new Vector3(0f, -40f, 0f), Space.Self, 0.15f, 0f,
				Tween.EaseInOut);
			__instance.gooEyesController.ClearLookTarget();
			__instance.JumpBottle(0.3f, 0.15f);
			__instance.gooAnim.SetBool("speaking", value: true);

			// wait for player to click
			yield return Singleton<TextDisplayer>.Instance.PlayDialogueEvent("CopyCardPresentResult",
				TextDisplayer.MessageAdvanceMode.Input);
		}

		__instance.JumpBottle(0.3f, 0.1f);
		__instance.gooAnim.SetBool("speaking", value: false);
		yield return new WaitForSeconds(0.1f);
		__instance.selectionSlot.SetShown(shown: false, immediate: true);
		__instance.pile.AddToPile(__instance.selectionSlot.Card.transform);
		__instance.pile.MoveCardToPile(__instance.selectionSlot.Card, flipFaceDown: true, 0.1f);
		Tween.Rotation(__instance.selectionSlot.Card.transform, new Vector3(90f, CustomRandom.RandomBetween(-3f, 3f), 0f), 0.1f, 0.35f);
		yield return new WaitForSeconds(0.2f);
		if (badPainting)
		{
			__instance.pile.AddToPile(replacementCard.transform);
			__instance.pile.MoveCardToPile(replacementCard, flipFaceDown: true, 0.1f);
			Tween.Rotation(replacementCard.transform, new Vector3(90f, CustomRandom.RandomBetween(-3f, 3f), 0f), 0.1f, 0.35f);
		}
		else
		{
			__instance.pile.AddToPile(copyCard.transform);
			__instance.pile.MoveCardToPile(copyCard, flipFaceDown: true, 0.1f);
			Tween.Rotation(copyCard.transform, new Vector3(90f, CustomRandom.RandomBetween(-3f, 3f), 0f), 0.1f, 0.35f);
		}
		

		yield return new WaitForSeconds(0.75f);
		__instance.StartCoroutine(__instance.pile.DestroyCards());
		__instance.LiftItemAnim(__instance.bottleAnim, "glass_object_up");
		yield return new WaitForSeconds(0.1f);
		__instance.LiftItemAnim(__instance.selectedCardEaselAnim, "wood_object_up");
		__instance.LiftItemAnim(__instance.copiedCardEaselAnim, "");
		yield return new WaitForSeconds(0.25f);
		__instance.confirmStone.SetStoneInactive();
		__instance.bottleAnim.gameObject.SetActive(value: false);
		__instance.selectedCardEasel.gameObject.SetActive(value: false);
		__instance.copiedCardEasel.gameObject.SetActive(value: false);
		if (Singleton<GameFlowManager>.Instance != null)
		{
			Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.Map);
		}
	}

	private static IEnumerator BadPainting(CopyCardSequencer __instance, SelectableCard card)
	{
		// Say: Oh.... you don't need to see that...
		// Say: Master deserves better!
		foreach (string s in GetRandomBadPaintingText())
		{
			yield return Singleton<TextDisplayer>.Instance.ShowUntilInput(s, -2.5f, 0.5f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Goo);
		}
		
		// Face camera and move easels out of the way
		Tween.Rotate(__instance.bottleAnim.transform.parent, new Vector3(0f, -40f, 0f), Space.Self, 0.15f, 0f, Tween.EaseInOut);
		Tween.Position(__instance.selectedCardEasel.transform, __instance.selectedCardEasel.transform.position + Vector3.right * 0.75f, 0.2f, 0f);
		Tween.Position(__instance.copiedCardEasel.transform, __instance.copiedCardEasel.transform.position + Vector3.left * 0.75f, 0.2f, 0f);
		
		yield return new WaitForSeconds(0.2f);
		
		//card.transform.localPosition = new Vector3(-1.01f, 6.21f, 0.594f);
		//card.transform.localRotation = Quaternion.Euler(0f, -15.5f, 0f);
		card.transform.localPosition = new Vector3(0.469f, 3.35f, 5.15f);
		card.transform.localRotation = Quaternion.Euler(-2.19f, 116f, 7);
		card.transform.localScale = new Vector3(3.55f, 3.55f, 3.55f);
		card.gameObject.SetActive(true);
		
		// Show random card
		CreateBadCard(card);

		__instance.gooEyesController.ClearLookTarget();
		__instance.JumpBottle(0.3f, 0.15f);
		__instance.gooAnim.SetBool("speaking", value: true);
		
		// player clicks and is given the card
		yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("I'm sorry!", -2.5f, 0.5f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Goo);
		yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("You deserve better!", -2.5f, 0.5f, Emotion.Neutral, TextDisplayer.LetterAnimation.Jitter, DialogueEvent.Speaker.Goo);
		
	}

	private static List<string[]> BadPaintingTexts = new List<string[]>()
	{
		new[] { 
			"Rrrgh!", 
			"What have I done!?" 
		}, 
		new[]
		{
			"Arrrgh!", 
			"This was too difficult!"
		},
		new[]
		{
			"Master deserves better!", 
			"Don't tell Master!"
		}, 
		new[]
		{
			"My attempt failed!", 
			"I am not ready!"
		},
		new[]
		{
			"Too tricky!", 
			"Next attempt MUST work!"
		}
	};

	private static string[] GetRandomBadPaintingText()
	{
		int index = SeededRandom.Range(0, BadPaintingTexts.Count, Seed++);
		return BadPaintingTexts[index];
	}

	private static void CreateBadCard(SelectableCard card)
	{
		// Create card clone and show
		CardInfo cardInfo = GetRandomCardInfo();
		RunState.Run.playerDeck.AddCard(cardInfo);
		SaveManager.SaveToFile();
		card.RenderInfo.hiddenAttack = (card.RenderInfo.hiddenHealth = false);
		card.SetInfo(cardInfo);
	}

	private static CardInfo GetRandomCardInfo()
	{
		List<CardInfo> cards = new List<CardInfo>();
		
		string[] strings = Configs.CardReplacements.Value.Split(',');
		foreach (string s in strings)
		{
			CardInfo cardInfo = CardLoader.GetCardByName(s.Trim());
			if (cardInfo != null)
			{
				cards.Add(cardInfo);
			}
		}

		CardInfo info = cards.GetSeededRandom(Seed++);
		return info;
	}

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
