using DiskCardGame;
using UnityEngine;

namespace SpritzMod.Scripts
{
    public class FishHookCardSelected : SelectableCard
    {
        public override void OnCursorSelectStart()
        {
            base.Anim.PlayRiffleSound();
            if (Flipped)
            {
                if (this.cardFlipped != null)
                {
                    this.cardFlipped(this);
                    return;
                }
            }
            else if (this.cardSelected != null)
            {
                this.cardSelected(this);
            }
        }
        
        public void FlipUp()
        {
            if (this.flippedBackTexture != null)
            {
                base.StartCoroutine(base.FlipCardbackTexture(this.flippedBackTexture));
                this.Flipped = false;
                this.flippedBackTexture = null;
            }
            else
            {
                this.Flipped = false;
                base.SetFaceDown(false, false);
            }
        }

        public static FishHookCardSelected Create(SelectableCard template, GameObject parent)
        {
            FishHookCardSelected fishHookCardSelected = parent.AddComponent<FishHookCardSelected>();
            fishHookCardSelected.pickupCursorType = template.pickupCursorType;
            fishHookCardSelected.animationParent = template.animationParent;
            fishHookCardSelected.localRotationOffset = template.localRotationOffset;
            fishHookCardSelected.dustParticles = template.dustParticles;
            fishHookCardSelected.canFlip = template.canFlip;
            fishHookCardSelected.cardSelected = template.cardSelected;
            fishHookCardSelected.cardFlipped = template.cardFlipped;
            fishHookCardSelected.cardInspected = template.cardInspected;
            fishHookCardSelected.intendedLocalPos = template.intendedLocalPos;
            fishHookCardSelected.localPositionSpeed = template.localPositionSpeed;
            fishHookCardSelected.intendedLocalRot = template.intendedLocalRot;
            fishHookCardSelected.currentLocalRot = template.currentLocalRot;
            fishHookCardSelected.localRotSpeed = template.localRotSpeed;
            fishHookCardSelected.flippedBackTexture = template.flippedBackTexture;
            return fishHookCardSelected;
        }
    }
}