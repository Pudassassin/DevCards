using GearUpCards.Utils;
using ModdingUtils.MonoBehaviours;
using System;
using UnboundLib;
using UnityEngine;

using GearUpCards.Extensions;

namespace GearUpCards.MonoBehaviours
{
    public class ObliterationModifier : RayHitEffect
    {
        private static GameObject VFXPrefab = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_OrbLiterationImpact");

        private float healthCullAreaHit = 0.9f;
        private float healthCullDirectHit = 0.85f;
        private float obliterationRadius = 4.0f;

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
            //     return HasToReturn.canContinue;
            // }

            bool hitPLayer = false;
            HollowLifeEffect status;
            float playerDistance;

            ProjectileHit projectileHit = this.gameObject.GetComponentInParent<ProjectileHit>();
            CharacterStatModifiers shooterStats = projectileHit.ownPlayer.gameObject.GetComponent<CharacterStatModifiers>();
            float glyphPotency = shooterStats.GetGearData().glyphPotency;
            float glyphInfluence = shooterStats.GetGearData().glyphInfluence;

            float effectRadius = obliterationRadius + (1.0f * glyphInfluence);

            Player victimPlayer = hit.transform.GetComponent<Player>();
            int victimID = -1;

            if (victimPlayer)
            {
                // direct hit victim take more MAX HP loss
                GameObject victim = victimPlayer.gameObject;
                float effectValue = healthCullDirectHit - (0.05f * glyphPotency);

                status = victim.GetOrAddComponent<HollowLifeEffect>();
                status.ApplyTempHealthCap(effectValue);
                victimPlayer.data.health *= effectValue;

                hitPLayer = true;
                victimID = victimPlayer.playerID;
                // return HasToReturn.canContinue;
            }

            // apply MAX HP Culling
            foreach (Player target in PlayerManager.instance.players)
            {
                if (target.playerID == victimID)
                {
                    continue;
                }

                playerDistance = (target.transform.position - gameObject.transform.position).magnitude;
                if (playerDistance <= effectRadius)
                {
                    float effectValue = healthCullAreaHit - (0.025f * glyphPotency);

                    status = target.gameObject.GetOrAddComponent<HollowLifeEffect>();
                    status.ApplyTempHealthCap(effectValue);
                    target.data.health *= effectValue;
                }
            }

            // map obliteration!
            if (!hitPLayer)
            {
                MapUtils.RPCA_DestroyMapObject(hit.transform.gameObject);
            }

            MapUtils.RPCA_DestroyMapObjectsAtArea(gameObject.transform.position, effectRadius);

            // VFX part
            GameObject VFX = Instantiate(VFXPrefab, gameObject.transform.position + new Vector3(0.0f, 0.0f, 100.0f), Quaternion.identity);
            VFX.transform.localScale = Vector3.one * effectRadius;
            VFX.name = "OrbLiterationImpactVFX_Copy";
            VFX.GetComponent<Canvas>().sortingLayerName = "MostFront";
            VFX.GetComponent<Canvas>().sortingOrder = 10000;
            VFX.AddComponent<RemoveAfterSeconds>().seconds = 1.55f;

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
