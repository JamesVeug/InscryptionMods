using DiskCardGame;

namespace MoreTotemBottoms;

public class PowerBuffStatEffect : ABuffStatEffect
{
	protected override void AddModToCard(PlayableCard card)
	{
		card.AddTemporaryMod(new CardModificationInfo
		{
			fromTotem = true,
			attackAdjustment = 1,
		});
	}
}