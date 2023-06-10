using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreItemsMod.Scripts.Items;

public abstract class BasePresentItem : ConsumableItem
{
	public abstract List<CardInfo> CardInfos();

	public override IEnumerator ActivateSequence()
	{
		GameObject[] cardInfos = new GameObject[]
		{
			gameObject.FindChild("Card (1)"),
			gameObject.FindChild("Card (2)"),
			gameObject.FindChild("Card (3)"),
			gameObject.FindChild("Card (4)"),
		};
		
		List<CardInfo> cards = CardInfos();

		List<CardInfo> pool = new List<CardInfo>(cards.Count);
		
		SelectableCard prefab = Resources.Load<SelectableCard>("prefabs/cards/SelectableCard");
		List<CardInfo> chosenCards = new List<CardInfo>();
		for (int i = 0; i < cardInfos.Length; i++)
		{
			// get random card
			CardInfo cardInfo = null;
			if (pool.Count == 0)
			{
				pool.AddRange(cards);
			}
			
			int index = Random.RandomRangeInt(0, pool.Count);
			cardInfo = pool[index];
			pool.RemoveAt(index);
			
			chosenCards.Add(cardInfo);

			// create a visible card (Clone prefab because my prefab doesn't work... wrong shader or something)
			GameObject cardSlot = cardInfos[i];
			SelectableCard clone = Instantiate(prefab, cardSlot.transform);
			clone.transform.localPosition = new Vector3(0, 0, 0);
			clone.transform.localRotation = Quaternion.identity;
			clone.transform.localScale = new Vector3(1, 1, 1);
			clone.GetComponent<Collider>().enabled = false;
			clone.GetComponent<Animator>().enabled = false;
			clone.GetComponent<SelectableCard>().enabled = false;
			clone.GetComponent<PaperCardAnimationController>().enabled = false;
			clone.SetInfo(cardInfo);
		}
		
		base.PlayExitAnimation();
		
		yield return new WaitForSeconds(1.0f);
		
		for (int i = 0; i < chosenCards.Count; i++)
		{
			yield return base.StartCoroutine(Singleton<CardSpawner>.Instance.SpawnCardToHand(chosenCards[i], 0.5f));
		}
		
		yield return new WaitForSeconds(0.5f);
	}

}