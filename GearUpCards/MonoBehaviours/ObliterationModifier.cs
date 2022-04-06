using GearUpCards.Utils;
using ModdingUtils.MonoBehaviours;
using System;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
	public class ObliterationModifier : RayHitEffect
	{
		private const float healthCullBaseFactor = 0.90f;

		// private Gun shooterGun;
		// private Player shooterPlayer;

		public override HasToReturn DoHitEffect(HitInfo hit)
		{
			if (hit.transform == null)
			{
				return HasToReturn.canContinue;
			}
			// if (hit.transform.gameObject.tag.Contains("Bullet"))
			// {
			// 	return HasToReturn.canContinue;
			// }

			bool hitPLayer = false;
			if (hit.transform.gameObject.tag.Contains("Player"))
            {
				// do damage to victim ()
				GameObject victim = hit.transform.gameObject;

				ObliterationStatus status = victim.AddComponent<ObliterationStatus>();
				status.CullMaxHealth(healthCullBaseFactor);

				hitPLayer = true;
				// return HasToReturn.canContinue;
			}

            // map object obliteration!
            if (!hitPLayer)
            {
				MapUtils.RPCA_DestroyMapObject(hit.transform.gameObject);
				// AoE map destruction
            }

			return HasToReturn.canContinue;
		}


    }

	public class ObliterationStatus : ReversibleEffect
    {
		private float healthScale = 1.0f;

		public override void OnAwake()
		{
			this.SetLivesToEffect(999);
		}

		public void CullMaxHealth(float percentage)
		{
			healthScale *= percentage;

			characterDataModifier.health_mult = percentage;
			characterDataModifier.maxHealth_mult = healthScale;
			ApplyModifiers();
		}
	}
}
