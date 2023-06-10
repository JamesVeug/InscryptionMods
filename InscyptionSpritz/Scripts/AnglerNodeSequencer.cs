using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Encounters;
using Pixelplacement;
using SpritzMod.Scripts.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SpritzMod.Scripts
{
	public class AnglerNodeData : CustomNodeData
	{
		public override void Initialize()
		{
			base.Initialize();
			// This node can only generate if the BaseDifficulty challenge is active
			//this.AddGenerationPrerequisite(() => AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.BaseDifficulty));
            
			// This node is forced to generate if more than one BaseDifficulty challenge is active
			// and a deck trial node has been placed on the map already
			this.AddForceGenerationCondition((y, nodes) => true);
		}
	}
	
    public class AnglerNodeSequencer : MonoBehaviour, ICustomNodeSequence
    {
	    
        private static List<CustomDialogueData> dialogue;
        
        private CardPile deckPile;
        private GameObject selectableCardPrefab;
        
        private SelectableCard chosenReward;
        private List<FishHookCardSelected> selectableCards = new List<FishHookCardSelected>();
        private List<CardSlot> opponentChoiceSlots = new List<CardSlot>();
        private Transform fishHook = null;
        private Animator fishHookAnimator;
        private FishHookCardSelected m_cardSelected;
        private Vector3 m_fishHookCachedPosition;
        private SelectCardFromDeckSlot sacrificeSlot;
        private ConfirmStoneButton confirmSacrificeButton;
        private bool isFirstHook;

        private Vector3 slotDefaultPosition;
        private Quaternion slotDefaultRotation;
        private Vector3 slotHookPosition;
        private Quaternion slotHookRotation;
        private readonly GameObject sceneryObject;

        public static void Initialize()
        {
	        NodeManager.Add<AnglerNodeSequencer, AnglerNodeData>(
		        new[]
		        {
			        Utils.GetTextureFromPath("Artwork/Fishing Node.png"),
			        Utils.GetTextureFromPath("Artwork/Fishing Node2.png"),
			        Utils.GetTextureFromPath("Artwork/Fishing Node3.png"),
			        Utils.GetTextureFromPath("Artwork/Fishing Node4.png")
		        },
		        NodeManager.NodePosition.SpecialEventRandom | NodeManager.NodePosition.Act1Available
	        );

            dialogue = DataUtil.LoadAllInDirectory<CustomDialogueData>(Plugin.Directory, ".*customdialogue");
            Plugin.Log.LogInfo("Loaded " + dialogue.Count + " dialogue!");
        }

        public AnglerNodeSequencer()
        {
	        Plugin.Log.LogInfo("[AnglerNodeSequencer] Constructor Starting");
	        gameObject.name = nameof(AnglerNodeSequencer);
	        
	        GameObject cardRemoverSequence = GameObject.Find("CardRemoveSequencer");

	        // Sacrifice slot
	        GameObject stoneCircleTemplate = Utils.FindObjectInChildren(cardRemoverSequence, "StoneCircle");
	        GameObject ritualStone = Utils.FindObjectInChildren(stoneCircleTemplate.gameObject, "RitualStone");
	        sacrificeSlot = Instantiate(ritualStone.GetComponentInChildren<SelectCardFromDeckSlot>(), transform, true);
	        sacrificeSlot.gameObject.SetActive(false);
	        slotDefaultPosition = sacrificeSlot.transform.position;
	        slotDefaultPosition.y = 5.03f;
	        slotDefaultRotation = Quaternion.identity;
	        slotHookPosition = slotDefaultPosition;
	        slotHookRotation = Quaternion.identity;
	        
	        // Sacrifice button
	        GameObject confirmButton = Utils.FindObjectInChildren(cardRemoverSequence, "ConfirmStoneButton");
	        confirmButton = Instantiate(confirmButton, transform, true);
	        confirmButton.transform.position += new Vector3(0, 0, 0.5f);
	        confirmButton.gameObject.SetActive(true);
	        confirmSacrificeButton = Utils.FindObjectInChildren(confirmButton, "ConfirmButton").GetComponent<ConfirmStoneButton>();

	        // Deck
	        CardSingleChoicesSequencer templateClass = GameObject.Find("CardChoiceSelector").GetComponent<CardSingleChoicesSequencer>();
	        deckPile = Instantiate(templateClass.deckPile, transform, true);
	        sacrificeSlot.pile = deckPile;
	        
	        // Multiple choice slots
            selectableCardPrefab = Instantiate(templateClass.selectableCardPrefab, transform, true);
            SelectableCard selectedCard = selectableCardPrefab.GetComponent<SelectableCard>();
            FishHookCardSelected.Create(selectedCard, selectableCardPrefab);
            selectableCardPrefab.gameObject.SetActive(false);


            // Spawn Cards
            int totalSlots = Singleton<BoardManager>.Instance.opponentSlots.Count;
            float x = (float)(totalSlots - 1) * 0.5f * -1.5f;
            this.selectableCards = SpawnCards(totalSlots, new Vector3(x, slotDefaultPosition.y, 2f), 1.5f);
            
            // Card slots
            for (var i = 0; i < totalSlots; i++)
            {
	            CardSlot slot = Singleton<BoardManager>.Instance.opponentSlots[i];
	            CardSlot clone = Instantiate(slot, transform, true);
	            Vector3 position = selectableCards[i].transform.position;
	            position.y = 5.01f;
	            clone.transform.position = position;
	            clone.SetEnabled(false);
	            opponentChoiceSlots.Add(clone);
            }
            
            // Scenery
            sceneryObject = Instantiate(ResourceBank.Get<GameObject>("Prefabs/Environment/TableEffects/HookedFishTableEffects"), transform);
            sceneryObject.SetActive(false);

            Plugin.Log.LogInfo("[AnglerNodeSequencer] Constructor Done");
        }

        public IEnumerator ShowDialogue(string dialogueName)
        {
	        CustomDialogueData dialogueData = dialogue.Find((a) => a.name == dialogueName);
	        if (dialogueData == null)
	        {
		        Plugin.Log.LogError("Could not find dialogue with name '" + dialogueName + "'");
		        yield break;
	        }

	        for (int i = 0; i < dialogueData.dialogue.Count; i++)
	        {
		        CustomDialogueDataMessage data = dialogueData.dialogue[i];
		        yield return Singleton<TextDisplayer>.Instance.ShowUntilInput(data.message, -2.5f, 0.4f, data.emotion, data.letterAnimation);
	        }
        }

        public IEnumerator ExecuteCustomSequence(CustomNodeData nodeData)
        {
            Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.CardMerging, false);
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
            LeshyAnimationController.Instance.PutOnMask(LeshyAnimationController.Mask.Angler, true);
            yield return new WaitForSeconds(0.35f);
            Singleton<TableRuleBook>.Instance.SetOnBoard(true);
            sceneryObject.SetActive(true);
            AudioController.Instance.PlaySound2D("angler_fish_enter", MixerGroup.TableObjectsSFX, 0.35f, 0f, null, null, null, null, false);
            yield return this.deckPile.SpawnCards(RunState.DeckList.Count, 0.5f);
            yield return ShowSlots();
            yield return new WaitForSeconds(0.5f);
            yield return ShowDialogue("intro");
            
            yield return SacrificeCard();
            
            yield return ChooseCards();

            Destroy(sceneryObject);
            Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
            yield return new WaitForSeconds(0.25f);
            yield return ShowDialogue("outro");
            yield return new WaitForSeconds(0.25f);
            yield return LeshyAnimationController.Instance.TakeOffMask();
            Singleton<TableRuleBook>.Instance.SetOnBoard(false);
            sacrificeSlot.gameObject.SetActive(false);
            yield return this.deckPile.DestroyCards(0.5f);
            yield return HideSlots();
            
            // Add exit to map sequence
            yield return new WaitForSeconds(0.25f);
            if (Singleton<GameFlowManager>.Instance != null)
            {
	            Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.Map, null);
            }
        }

        private IEnumerator SacrificeCard()
        {
	        Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(ViewController.ControlMode.CardMerging, false);
	        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
	        
	        yield return new WaitForSeconds(0.3f);
	        sacrificeSlot.transform.position = slotDefaultPosition;
	        sacrificeSlot.transform.rotation = slotDefaultRotation;
	        sacrificeSlot.gameObject.SetActive(true);
	        yield return new WaitForSeconds(0.3f);
	        
	        sacrificeSlot.Disable();
	        yield return ShowDialogue("before_sacrifice");
	        Singleton<ViewManager>.Instance.SwitchToView(View.CardMergeSlots, false, false);
	        
	        // Choose card to destroy
	        sacrificeSlot.RevealAndEnable();
	        sacrificeSlot.ClearDelegates();
	        sacrificeSlot.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(sacrificeSlot.CursorSelectStarted, new Action<MainInputInteractable>(OnSacrificeSlotSelected));
	        sacrificeSlot.backOutInputPressed = null;
	        sacrificeSlot.backOutInputPressed = (Action)Delegate.Combine(sacrificeSlot.backOutInputPressed, new Action(delegate()
	        {
		        if (sacrificeSlot.Enabled)
		        {
			        this.OnSacrificeSlotSelected(sacrificeSlot);
		        }
	        }));
	        yield return confirmSacrificeButton.WaitUntilConfirmation();
	        sacrificeSlot.ClearDelegates();
	        
	        // Destroy card
	        CardInfo sacrificedInfo = this.sacrificeSlot.Card.Info;
	        RunState.Run.playerDeck.RemoveCard(sacrificedInfo);
	        sacrificeSlot.Card.Anim.PlayDeathAnimation(false);
	        AudioController.Instance.PlaySound3D("sacrifice_default", MixerGroup.TableObjectsSFX, this.sacrificeSlot.transform.position, 1f, 0f, null, null, null, null, false);
	        yield return new WaitForSeconds(0.5f);
	        sacrificeSlot.DestroyCard();
	        yield return new WaitForSeconds(0.25f);
	        yield return ShowDialogue("after_sacrifice");
	        confirmSacrificeButton.SetEnabled(false);
        }

        private void OnSacrificeSlotSelected(MainInputInteractable slot)
        {
	        sacrificeSlot.SetEnabled(false);
	        sacrificeSlot.ShowState(HighlightedInteractable.State.NonInteractable, false, 0.15f);
	        confirmSacrificeButton.Exit();
	        (slot as SelectCardFromDeckSlot).SelectFromCards(GetValidSacrificeCards(), OnSacrificeSelectionEnded, false);
        }
        
        private List<CardInfo> GetValidSacrificeCards()
        {
	        return new List<CardInfo>(RunState.DeckList);
        }
        
        private void OnSacrificeSelectionEnded()
        {
	        sacrificeSlot.SetShown(true, false);
	        sacrificeSlot.ShowState(HighlightedInteractable.State.Interactable, false, 0.15f);
	        //Singleton<ViewManager>.Instance.SwitchToView(View.CardMergeSlots, false, true);
	        Singleton<ViewManager>.Instance.SwitchToView(View.BoardCentered, false, true);
	        if (this.sacrificeSlot.Card != null)
	        {
		        confirmSacrificeButton.Enter();
	        }
        }

        public IEnumerator ChooseCards()
        {
	        //Singleton<ViewManager>.Instance.SwitchToView(View.CardMergeSlots, false, false);
	        Singleton<ViewManager>.Instance.SwitchToView(View.OpponentQueueCentered, false, false);
            Tween.Position(sacrificeSlot.transform, slotHookPosition, 0.3f, 0f);
            Tween.Rotation(sacrificeSlot.transform, slotHookRotation, 0.3f, 0f);
            yield return ShowDialogue("before_choosing_card");
            yield return this.CardSelectionSequence();
            yield return ShowDialogue("after_choosing_card");

            SetCollidersEnabled(false);
            yield return new WaitForSeconds(0.25f);
            this.CleanUpCards(true);
            confirmSacrificeButton.Exit();

            if (fishHook != null)
            {
	            Destroy(fishHook.gameObject);
            }

            Singleton<TextDisplayer>.Instance.Clear();
        }
        
        private List<CardInfo> GenerateCardChoices()
        {
	        List<CardInfo> allCards = new List<CardInfo>(CardLoader.AllData);
	        allCards.RemoveAll(RemoveCardFromChoices);
	        
	        int randomSeed = SaveManager.SaveFile.GetCurrentRandomSeed();
	        List<CardInfo> chosenCards = new List<CardInfo>();
	        while (chosenCards.Count < 4 && allCards.Count > 0)
	        {
		        int index = SeededRandom.Range(0, allCards.Count, randomSeed++);
		        chosenCards.Add(allCards[index]);
		        allCards.RemoveAt(index);
	        }
	        
	        return chosenCards;
        }

        private bool RemoveCardFromChoices(CardInfo a)
        {
	        if (a.temple != CardTemple.Nature) 
		        return true;

	        if (!a.metaCategories.Contains(CardMetaCategory.ChoiceNode))
		        return true;
	        
	        if (a.traits.Contains(Trait.Pelt))
		        return true;
	        
	        return false;
        }

        private IEnumerator DestroyHookedCard()
        {
	        confirmSacrificeButton.Disable();

	        m_cardSelected.Anim.PlayDeathAnimation(true);
	        m_cardSelected.coll.enabled = false;
	        AudioController.Instance.PlaySound3D("sacrifice_default", MixerGroup.TableObjectsSFX, this.sacrificeSlot.transform.position, 1f, 0f, null, null, null, null, false);
	        yield return new WaitForSeconds(0.5f);
	        //Singleton<ViewManager>.Instance.SwitchToView(View.CardMergeSlots, false, false);
	        Singleton<ViewManager>.Instance.SwitchToView(View.OpponentQueueCentered, false, false);
	        Destroy(m_cardSelected.gameObject);
	        m_cardSelected = null;

	        fishHook.transform.position = m_fishHookCachedPosition;
	        fishHook.gameObject.SetActive(true);
	        fishHookAnimator.Rebind();
	        SetCollidersEnabled(true);
        }
        
        private IEnumerator CardSelectionSequence()
        {
	        isFirstHook = true;
			chosenReward = null;
			confirmSacrificeButton.ClearDelegates();
			confirmSacrificeButton.CursorSelectEnded += (a)=>
			{
				if (m_cardSelected != null)
				{
					StartCoroutine(DestroyHookedCard());
				}
			};
			
			// Show cards
			List<CardInfo> choices = GenerateCardChoices();
			for (int i = 0; i < choices.Count; i++)
			{
				CardInfo cardChoice = choices[i];
				FishHookCardSelected selectableCard = selectableCards[i];
				selectableCard.gameObject.SetActive(true);
				selectableCard.SetParticlesEnabled(true);
				selectableCard.SetEnabled(false);
				selectableCard.Initialize(cardChoice, OnManuallyChooseCard, OnCardHook, true, OnCardInspected);
				selectableCard.SetFaceDown(true, true);
				Vector3 position = selectableCard.transform.position;
				selectableCard.transform.position += Vector3.forward * 5f + new Vector3(-0.5f + Random.value * 1f, 0f, 0f);
				Tween.Position(selectableCard.transform, position, 0.3f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
				Tween.Rotate(selectableCard.transform, new Vector3(0f, 0f, Random.value * 1.5f), Space.Self, 0.4f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
				yield return new WaitForSeconds(0.2f);
				ParticleSystem componentInChildren = selectableCard.GetComponentInChildren<ParticleSystem>();
				if (componentInChildren != null)
				{
					ParticleSystem.EmissionModule emissionModule = componentInChildren.emission;
					emissionModule.rateOverTime = 0f;
				}
			}
			
			yield return new WaitForSeconds(0.2f);
			
			// Show Fishhook in hand
			fishHook = Singleton<FirstPersonController>.Instance.AnimController
				.SpawnFirstPersonAnimation("FirstPersonFishHook").transform;
			fishHook.localPosition = new Vector3(0f, 0, 4f) + Vector3.right * 3f;
			fishHook.localEulerAngles = new Vector3(0f, 0f, 0f);
			MoveFishhookToPosition(selectableCards[selectableCards.Count - 1].transform.position);
			fishHookAnimator = fishHook.GetComponentInChildren<Animator>();
			
			// Wait for player to hook one
			SetCollidersEnabled(true);
			yield return new WaitForSeconds(1);
			yield return new WaitUntil(() => this.chosenReward != null);
			confirmSacrificeButton.ClearDelegates();
			yield return this.AddCardToDeckAndCleanUp(this.chosenReward);
		}

		private IEnumerator AddCardToDeckAndCleanUp(SelectableCard card)
		{
			Singleton<RuleBookController>.Instance.SetShown(false, true);
			yield return this.RewardChosenSequence(card);
			ProgressionData.SetMechanicLearned(MechanicsConcept.CardChoice);
			this.AddChosenCardToDeck();
			Singleton<TextDisplayer>.Instance.Clear();
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		private void AddChosenCardToDeck()
		{
			this.deckPile.AddToPile(this.chosenReward.transform);
			SaveManager.SaveFile.CurrentDeck.AddCard(this.chosenReward.Info);
		}
		
		private IEnumerator RewardChosenSequence(SelectableCard card)
		{
			card.OnCardAddedToDeck();
			this.deckPile.MoveCardToPile(card, true, 0, 0.7f);
			yield return null;
		}

		private List<FishHookCardSelected> SpawnCards(int totalSlots, Vector3 anchorPos, float cardSpacing = 1.5f)
		{
			List<FishHookCardSelected> list = new List<FishHookCardSelected>();
			for (int i = 0; i < totalSlots; i++)
			{
				FishHookCardSelected selectableCard = this.SpawnSelectableSlot();

				selectableCard.transform.position = new Vector3(anchorPos.x + cardSpacing * (float)i, anchorPos.y, anchorPos.z);
				list.Add(selectableCard);
			}
			return list;
		}
		
		private FishHookCardSelected SpawnSelectableSlot()
		{
			GameObject selectionSlots = Object.Instantiate<GameObject>(this.selectableCardPrefab, transform, true);
			Destroy(selectionSlots.GetComponent<SelectableCard>());
			selectionSlots.SetActive(false);
			return selectionSlots.GetComponent<FishHookCardSelected>();
		}
		
		private void OnManuallyChooseCard(SelectableCard card)
		{
			if (this.chosenReward == null)
			{
				this.chosenReward = card;
			}
		}
		
		private void SetCollidersEnabled(bool collidersEnabled)
		{
			foreach (FishHookCardSelected selectableCard in this.selectableCards)
			{
				selectableCard.SetEnabled(collidersEnabled);
			}
		}
		
		private void CleanUpCards(bool doTween = true)
		{
			ResetLocalRotations();
			foreach (FishHookCardSelected selectableCard in this.selectableCards)
			{
				selectableCard.SetInteractionEnabled(false);
				if (selectableCard != this.chosenReward)
				{
					if (doTween)
					{
						Tween.Position(selectableCard.transform, selectableCard.transform.position + Vector3.forward * 20f, 0.5f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, null, true);
					}
					Object.Destroy(selectableCard.gameObject, doTween ? 0.5f : 0f);
				}
			}
			this.selectableCards.Clear();
		}
		
		private void ResetLocalRotations()
		{
			foreach (FishHookCardSelected selectableCard in this.selectableCards)
			{
				selectableCard.SetLocalRotation(0f, 40f, false);
			}
		}
		
		private void OnCardHook(SelectableCard card)
		{
			StartCoroutine(OnCardHookCoroutine(card));
		}

		private IEnumerator OnCardHookCoroutine(SelectableCard card)
		{
			m_cardSelected = (FishHookCardSelected)card;
			selectableCards.Remove(m_cardSelected);
			SetCollidersEnabled(false);

			Transform fishHookTransform = fishHook.transform;
			card.SetLocalPosition(Vector3.zero, 0f, true);
			
			AudioController.Instance.PlaySound3D("angler_use_hook", MixerGroup.TableObjectsSFX, card.transform.position, 1f, 0.1f);
			fishHookAnimator.SetTrigger("hook");
			
			yield return new WaitForSeconds(0.66f);
			//Singleton<ViewManager>.Instance.SwitchToView(View.CardMergeConfirm, false, false);
			Singleton<ViewManager>.Instance.SwitchToView(View.BoardCentered, false, false);
			m_fishHookCachedPosition = fishHookTransform.position; 
			Tween.Position(fishHookTransform, slotHookPosition + new Vector3(0, -0.5f, -1.5f), 0.2f, 0f, Tween.EaseOut);
			Tween.Position(card.transform,  slotHookPosition + new Vector3(0, 0.05f, 0), 0.2f, 0f, Tween.EaseOut);
			card.SetLocalRotation(0, 20f, true);
			yield return new WaitForSeconds(0.15f);

			m_cardSelected.FlipUp();
			
			// Hide fish hook
			fishHook.gameObject.SetActive(false);
			
			if (isFirstHook)
			{
				yield return ShowDialogue("after_first_hook");
				isFirstHook = false;
			}

			// If it's the last card then select it
			int totalCardsSelectable = selectableCards.Count;

			// Turn on card's collider so the player can clicked on it
			m_cardSelected.SetEnabled(true);

			// Let the player sacrifice just in case
			if (totalCardsSelectable > 0)
			{
				confirmSacrificeButton.confirmView = Singleton<ViewManager>.Instance.CurrentView;
				confirmSacrificeButton.SetButtonInteractable();
			}
		}
		
		private void OnCardInspected(SelectableCard card)
		{
			card.Anim.PlayRiffleSound();
			this.ResetLocalRotations();
			card.SetLocalRotation(card.FaceDown ? 3f : -3f, 20f, false);
			
			if (card != m_cardSelected)
			{
				this.MoveFishhookToPosition(card.transform.position);
			}
		}

        private void MoveFishhookToPosition(Vector3 targetPos)
        {
	        Tween.Position(fishHook, new Vector3(targetPos.x, targetPos.y + 0.75f, targetPos.z - 1.5f), 0.2f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
        }
        
        public IEnumerator ShowSlots()
        {
	        //this.HideAllSlots();
	        foreach (CardSlot slot in opponentChoiceSlots)
	        {
		        slot.SetShown(true, false);
		        slot.SetEnabled(false);
		        yield return new WaitForSeconds(0.05f);
	        }
        }
        
        public IEnumerator HideSlots()
        {
	        foreach (CardSlot slot in opponentChoiceSlots)
	        {
		        slot.SetShown(false, false);
		        yield return new WaitForSeconds(0.075f);
	        }
        }
    }
}