using System.Collections.Generic;
using System.Collections;

using UnboundLib;
using UnityEngine;
using UnboundLib.GameModes;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Utils;
using GearUpCards.Extensions;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;

namespace GearUpCards.MonoBehaviours
{
    internal class OrbLifeforceBlastModifier : RayHitEffect
    {
        private static GameObject vfxOrb = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_LifeforceBlast_Orb");
        private static GameObject vfxAOE = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_LifeforceBlast_AOE");

        private static float procTickTime = 0.2f;

        private ProjectileHit projectileHit;
        // MoveTransform moveTransform;
        private Player casterPlayer;
        private CharacterStatModifiers casterStats;

        private float healFlat, healPercent, drainFlat, drainPercent;
        private float effectRadius, effectDuration, healAmp, healHinder;

        internal GameObject orbObject;
        internal float proxyTimer = 0.0f;
        internal float procTimer = 0.0f;
        internal bool effectEnable = false;

        public void Setup()
        {
            projectileHit = transform.root.GetComponentInParent<ProjectileHit>();
            casterPlayer = projectileHit.ownPlayer;
            casterStats = casterPlayer.data.stats;
            // moveTransform = transform.root.GetComponentInChildren<MoveTransform>();

            // Orb Stats /wip
            healFlat        = 30.0f + (15.0f * casterStats.GetGearData().glyphPotency);
            healPercent     = 0.05f + (0.025f * casterStats.GetGearData().glyphPotency);
            drainFlat       = 50.0f + (25.0f * casterStats.GetGearData().glyphPotency);
            drainPercent    = 0.10f + (0.05f * casterStats.GetGearData().glyphPotency);

            effectRadius    = 5.0f + (0.5f * casterStats.GetGearData().glyphInfluence);
            effectDuration  = 5.0f + (1.0f * casterStats.GetGearData().glyphTime);

            // value is the multiplier to be used multiplicatively
            healAmp = 5.0f - (5.0f * Mathf.Pow(0.6f, casterStats.GetGearData().glyphPotency + 1));
            healHinder = Mathf.Pow(0.5f, casterStats.GetGearData().glyphPotency + 1);

            // visuals
            orbObject = Instantiate(vfxOrb, transform.root);
            orbObject.transform.localEulerAngles = new Vector3(270.0f, 180.0f, 0.0f);
            orbObject.transform.localPosition = Vector3.zero;
            orbObject.transform.localScale = Vector3.one;

            // GameObject aoeObject = Instantiate(vfxAOE, transform.root);
            // aoeObject.transform.localEulerAngles = new Vector3(270.0f, 180.0f, 0.0f);
            // aoeObject.transform.localPosition = Vector3.zero;
            // aoeObject.transform.localScale = Vector3.one * effectRadius;
        }

        public void Update()
        {
            if (effectEnable)
            {

            }
            else
            {
                MoveTransform moveTransform = GetComponentInParent<MoveTransform>();
                if (moveTransform != null)
                {
                    Setup();
                    effectEnable = true;
                }
            }
        }

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            float distance;
            foreach (Player item in PlayerManager.instance.players)
            {
                if (!item.gameObject.activeInHierarchy || item.data.healthHandler.isRespawning)
                {
                    // either dead or reviving
                    continue;
                }

                distance = (item.gameObject.transform.position - transform.root.position).magnitude;
                if (distance > effectRadius)
                {
                    // ...out of range
                    continue;
                }

                if (item.teamID == casterPlayer.teamID)
                {
                    // Heal friends
                    float healAmount = (healFlat + (item.data.maxHealth * healPercent));
                    item.data.healthHandler.Heal(healAmount);

                    LifeforceBlastStatus status = item.gameObject.GetOrAddComponent<LifeforceBlastStatus>();
                    status.ApplyEffect(healAmp, effectDuration, true);
                }
                else
                {
                    // drain enemies' lives
                    float drainAmount = (drainFlat + (item.data.maxHealth * drainPercent));
                    // item.data.health -= drainAmount * 0.5f;
                    item.data.healthHandler.Heal(-drainAmount * 0.5f);
                    item.data.healthHandler.RPCA_SendTakeDamage(new Vector2(drainAmount * 0.5f, 0.0f), this.transform.position, playerID: casterPlayer.playerID);

                    LifeforceBlastStatus status = item.gameObject.GetOrAddComponent<LifeforceBlastStatus>();
                    status.ApplyEffect(healHinder, effectDuration, false);
                }
            }

            // VFX part
            GameObject aoeObject = Instantiate(vfxAOE);
            // aoeObject.transform.localEulerAngles = new Vector3(270.0f, 180.0f, 0.0f);
            aoeObject.transform.position = hit.point;
            aoeObject.transform.localScale = Vector3.one * effectRadius;
            RemoveAfterSeconds remover = aoeObject.AddComponent<RemoveAfterSeconds>();
            remover.seconds = 2.5f;

            try
            {
                int clientTeamID = PlayerManager.instance.players.First(player => player.data.view.IsMine).teamID;
                if (projectileHit.ownPlayer.teamID == clientTeamID)
                {
                    aoeObject.transform.Find("Circle_Root/Circle_Thorns (1)").gameObject.SetActive(false);
                }
                else
                {
                    aoeObject.transform.Find("Circle_Root/Circle_Wrealth (1)").gameObject.SetActive(false);
                }
            }
            catch (System.Exception exception)
            {
                Miscs.LogWarn(exception);
            }

            return HasToReturn.canContinue;
        }
    }
}
