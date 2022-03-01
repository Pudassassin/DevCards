using System;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
	public class ChompyBulletEffect : RayHitEffect
	{
		// value per stack at 1 bullet per second
		// [!] This card can be dreadful to someone who managed to get *2* or more [Pristine Perserverence]
		private const float healthCullBaseFactor = 0.1f;
		private Gun shooterGun;
		private Player shooterPlayer;

		public override HasToReturn DoHitEffect(HitInfo hit)
		{
			// UnityEngine.Debug.Log($"CHOMP!, hit [{hit.transform.gameObject.name}]");
			if (hit.transform == null)
			{
				return HasToReturn.canContinue;
			}
			if (hit.transform.gameObject.tag.Contains("Bullet"))
            {
				return HasToReturn.canContinue;
            }

			// UnityEngine.Debug.Log($"CHOMP! #2");

			if (hit.transform.gameObject.tag.Contains("Player"))
            {
				// UnityEngine.Debug.Log($"CHOMP! #3");

				// get/update the shooter gun stats
				// UnityEngine.Debug.Log($"CHOMP! #3.1");
				ProjectileHit projectileHit = this.gameObject.GetComponentInParent<ProjectileHit>();

				// UnityEngine.Debug.Log($"CHOMP! #3.2");
				shooterPlayer = projectileHit.ownPlayer;

				// UnityEngine.Debug.Log($"CHOMP! #3.3");
				shooterGun = projectileHit.ownWeapon.GetComponent<Gun>();

				CharacterData victim = hit.transform.gameObject.GetComponent<CharacterData>();

				// calculate shooter's bullet fired per second
				float bps = this.CalculateBulletPerSecond();

				// do damage to victim
				float chompDamage = healthCullBaseFactor / bps * victim.health;
				victim.healthHandler.RPCA_SendTakeDamage(new Vector2(chompDamage, 0.0f), this.transform.position, playerID: shooterGun.player.playerID);
				// victim.healthHandler.TakeDamage(new Vector2(chompDamage, 0.0f), Vector2.zero, new Color(1.0f, 0.0f, 0.0f, 0.85f));

				// UnityEngine.Debug.Log($"CHOMP!, dealt [{chompDamage}] to player [{victim.player.playerID}]");
			}

			return HasToReturn.canContinue;
		}

		public void Destroy()
		{
			UnityEngine.Object.Destroy(this);
		}

		private float CalculateBulletPerSecond()
        {
			// UnityEngine.Debug.Log($"CHOMP! BPS CHECK");

			float attackTime = this.shooterGun.attackSpeed * shooterGun.attackSpeedMultiplier;
			int projectileCount = this.shooterGun.numberOfProjectiles;

			float burstTime = this.shooterGun.timeBetweenBullets;
			int burstCount = this.shooterGun.bursts;

			float bps = 1.0f;
			if (burstCount > 1)
            {
				attackTime += (burstCount - 1) * burstTime;
				bps = burstCount * projectileCount / attackTime;
            }
			else
            {
			    bps = projectileCount / attackTime;
			}

			// UnityEngine.Debug.Log($"CHOMP! BPS = [{bps}]");
			return Mathf.Clamp(bps, .75f, 50.0f);
        }

		public void Setup(Player player, Gun gun)
        {
			this.shooterPlayer = player;
			this.shooterGun = gun;
        }


    }
}
