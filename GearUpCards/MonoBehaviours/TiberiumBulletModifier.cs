using System;
using UnityEngine;

using UnboundLib;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    public class TiberiumBulletModifier : RayHitEffect
    {
        public override HasToReturn DoHitEffect(HitInfo hit)
        {
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

                int stackCount = shooterStats.GetGearData().tiberiumBulletStack;

                // factor is per second from bullet damage
                float gunDamage = shooterGun.damage * 55.0f;
                float chronicFactor = 0.025f * stackCount;
                float burstFactor = 0.25f * stackCount;

                TiberiumToxicEffect victimToxic = hit.transform.gameObject.GetOrAddComponent<TiberiumToxicEffect>();

                // Apply toxic to victim
                victimToxic.ApplyChronicStack(gunDamage * chronicFactor, 0.2f);
                victimToxic.ApplyNewStack(gunDamage * burstFactor, 0.2f, 20, false);

            }

            return HasToReturn.canContinue;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(this);
        }

    }
}
