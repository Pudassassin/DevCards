using GearUpCards.Utils;
using ModdingUtils.MonoBehaviours;
using System;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
	public class ObliterationModifier : RayHitEffect
	{
		private float healthCullFactor = 0.80f;
		private float obliterationRadius = 1.5f;

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
				status.CullMaxHealth(healthCullFactor);

				hitPLayer = true;
				// return HasToReturn.canContinue;
			}

            // map object obliteration!
            if (!hitPLayer)
            {
				MapUtils.RPCA_DestroyMapObject(hit.transform.gameObject);
				MapUtils.RPCA_DestroyMapObjectsAtArea(gameObject.transform.position, obliterationRadius);
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

            try
            {
				ApplyModifiers();
			}
            catch (Exception exception)
            {
				Miscs.LogWarn("[GearUp] ObliterationStatus: caught an exception!");
				Miscs.LogWarn(exception);
            }
		}
	}
}
