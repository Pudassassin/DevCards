using System;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
	public class ChompyBulletEffect : RayHitEffect
	{
		// value per stack at 1 bullet per second
		private const float healthCullBaseFactor = 0.1f;
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

			CharacterData victim = hit.transform.GetComponent<CharacterData>();
			if (victim != null)
            {
				// calculate shooter's bullet fired per second

				// do damage to victim
				float chompDamage = healthCullBaseFactor * stackCount * victim.health;
				// float chompDamage = healthCullBaseFactor * victim.health;
				victim.healthHandler.TakeDamage(new Vector2(chompDamage, 0.0f), Vector2.zero, new Color(1.0f, 0.0f, 0.0f, 0.85f), damagingPlayer: shooterPlayer);
				// victim.healthHandler.TakeDamage(new Vector2(chompDamage, 0.0f), Vector2.zero, new Color(1.0f, 0.0f, 0.0f, 0.85f));

				UnityEngine.Debug.Log($"CHOMP!, dealt [{chompDamage}] to player [{victim.player.playerID}]");
			}

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
