using System;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
	public class ChompyBulletEffect : RayHitEffect
	{
		// value per stack at 1 bullet per second
		private const float healthCullBaseFactor = 0.075f;
		private Gun shooterGun;
		private Player shooterPlayer;

		private int stackCount = 0;

		public override HasToReturn DoHitEffect(HitInfo hit)
		{
			if (!hit.transform)
			{
				return HasToReturn.canContinue;
			}
			if (stackCount <= 0)
            {
				return HasToReturn.canContinue;
            }

			// calculate user's bullet fired per second

			// do damage to victim
			CharacterData victim = hit.transform.GetComponent<CharacterData>();
			float chompDamage = healthCullBaseFactor * stackCount * victim.health;
			victim.healthHandler.DoDamage(new Vector2(chompDamage, 0.0f), Vector2.zero, new Color(1.0f, 0.0f, 0.0f, 0.5f), damagingPlayer: shooterPlayer);

			return HasToReturn.canContinue;
		}

		public void Destroy()
		{
			UnityEngine.Object.Destroy(this);
		}

		private float CalculateBulletPerSecond()
        {
			return 0.0f;
        }

		public void Setup(Player player, Gun gun)
        {
			this.shooterPlayer = player;
			this.shooterGun = gun;
        }

		public void AddStack()
        {
			this.stackCount += 1;
		}

		public void RemoveStack()
        {
			this.stackCount -= 1;
            if (this.stackCount <= 0)
            {
				this.stackCount = 0;
            }
        }
	}
}
