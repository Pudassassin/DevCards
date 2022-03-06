using System;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
	public class CrystalShardModifier : RayHitEffect
	{
		// value per stack at 1 bullet per second
		// [!] This card can be dreadful to someone who managed to get *2* or more [Pristine Perserverence]
		private const float healthCullBaseFactor = 0.1f;
		private Gun shooterGun;
		private Player shooterPlayer;

		public void Awake()
        {
			shooterGun = transform.parent.GetComponent<ProjectileHit>().ownWeapon.GetComponent<Gun>();
			shooterPlayer = transform.parent.GetComponent<ProjectileHit>().ownPlayer;
			// shooterGun = shooterPlayer.data.weaponHandler.gun;

			// extra crystal stats such as:
			// crystal type
			//	   - basic
			//	   - cryo > slow and extra gravity
			//	   - pyro > more burst damage upfront and larger explosion
			//     - shock > stun/silent, minimal damage
			//     - mimic > potentially pass on original bullet modifiers on its [Energized Shard], [Unstable Shard]
			//     - tiberium > reduced healing + poison short and long term, will propagate with [Unstable Shard] and grow if left alone
			//
			// [Energized Shard] basically if those shard can, by expanding its lifespan, shoot nearby enemy
			// [Unstable Shard] whether the shards will create weaker frag bullets on shard explosion
			// [Sticky Shard] whether their crystal will stick on victims for a little longer
			// [Refined Crystal] stacks for generic shard stats up
			// [Harden Crystal] for a more resilliant bullet against enemy's and whether it will collide with other shards of the same gun

			// [Crystal Bulwark] or [Craggy Rebuke] whether the shooter player is immune/resist to their own crystals

			// shooterGun.Attack() //scripted attack
			// shooterGun.transform //forward for shoot angle, had to change rotation somehow, and change back,
			//	  or just make a dummy object with dummy gun

			// attach "Crystal Shard" Prefab to this GameObject
			// attach dummy Gun component and link to crystal shard, copy most of the basic stats and depends on current crystal type
		}

		public override HasToReturn DoHitEffect(HitInfo hit)
		{
			// UnityEngine.Debug.Log($"CHOMP!, hit [{hit.transform.gameObject.name}]");
			if (hit.transform == null)
			{
				// into the void, leave nothing behind

				return HasToReturn.canContinue;
			}

			if (hit.transform.gameObject.tag.Contains("Bullet"))
            {
				// resolve shard explosion immediately

				return HasToReturn.canContinue;
            }

			if (hit.transform.gameObject.tag.Contains("Player"))
            {
				// resolve shard explosion immediately (if not too 'sticky')

				// apply on hit effect on victim
				CharacterData victim = hit.transform.gameObject.GetComponent<CharacterData>();

				return HasToReturn.canContinue;
			}

			// presumably hit map element


			return HasToReturn.canContinue;
		}

		public void Destroy()
		{
			UnityEngine.Object.Destroy(this);
		}


    }
}
