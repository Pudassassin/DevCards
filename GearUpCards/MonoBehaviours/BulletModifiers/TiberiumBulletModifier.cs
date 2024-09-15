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
                // Gun shooterGun = projectileHit.ownWeapon.GetComponent<Gun>();
                ProjectileHit bulletHit = transform.parent.GetComponent<ProjectileHit>();

                CharacterStatModifiers shooterStats = shooterPlayer.gameObject.GetComponent<CharacterStatModifiers>();

                int stackCount = shooterStats.GetGearData().tiberiumBulletStack;

                // Calculate chronic HP Loss
                CharacterData victimChar = hit.transform.gameObject.GetComponent<CharacterData>();

                // float gunDamage         = shooterGun.damage * 55.0f;
                float gunDamage         = bulletHit.damage;
                float chronicDmg        = 0.15f * stackCount * gunDamage;
                float chronicFlat       = 1.00f * stackCount;
                float chronicHpPercent  = 0.001f * stackCount * victimChar.maxHealth;
                float burstFactor       = 0.10f * stackCount;

                TiberiumToxicEffect victimToxic = hit.transform.gameObject.GetOrAddComponent<TiberiumToxicEffect>();


                // Apply toxic to victim
                victimToxic.ApplyChronicStack(chronicDmg + chronicFlat + chronicHpPercent, 0.2f);
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
