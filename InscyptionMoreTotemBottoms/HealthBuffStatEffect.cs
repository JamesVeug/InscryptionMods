using DiskCardGame;

namespace MoreTotemBottoms;

public class HealthBuffStatEffect : ABuffStatEffect
{
	protected override void AddModToCard(PlayableCard card)
	{
		card.AddTemporaryMod(new CardModificationInfo
		{
			fromTotem = true,
			healthAdjustment = 2,
		});
	}
}