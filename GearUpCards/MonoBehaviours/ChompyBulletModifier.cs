using System;
using UnityEngine;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    public class ChompyBulletModifier : RayHitEffect
    {
        // value per stack at around default bullet per second >>> each bullet at [3.0] BPS deal this much value
        // [!] This card can be dreadful to someone who managed to get *2* or more [Pristine Perserverence]
        private const float healthCullBaseFactor = 0.80f;
        private float healthCullPercentage;

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

            if (hit.transform.GetComponent<Player>())
            {
                // get the shooter gun stats
                ProjectileHit projectileHit = this.gameObject.GetComponentInParent<ProjectileHit>();

                Player shooterPlayer = projectileHit.ownPlayer;
                Gun shooterGun = projectileHit.ownWeapon.GetComponent<Gun>();
                CharacterStatModifiers shooterStats = shooterPlayer.gameObject.GetComponent<CharacterStatModifiers>();

                // calculate stats, stacking this card will be multiplicative with diminishing return
                healthCullPercentage = Mathf.Max(healthCullBaseFactor, shooterStats.GetGearData().chompyBulletStack);
                float hpToDamageScale = 1.0f - healthCullPercentage;

                CharacterData victim = hit.transform.gameObject.GetComponent<CharacterData>();

                // calculate shooter's bullet fired per second
                float bps = StatsMath.GetGunBPS(shooterGun);

                // do damage to victim
                float chompDamage = hpToDamageScale / Mathf.Clamp(bps/2.0f - 0.5f, 1.0f, 50.0f) * victim.health;
                victim.healthHandler.RPCA_SendTakeDamage(new Vector2(chompDamage, 0.0f), this.transform.position, playerID: shooterGun.player.playerID);
                // victim.healthHandler.TakeDamage(new Vector2(chompDamage, 0.0f), Vector2.zero, new Color(1.0f, 0.0f, 0.0f, 0.85f));

            }

            return HasToReturn.canContinue;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(this);
        }

    }
}
