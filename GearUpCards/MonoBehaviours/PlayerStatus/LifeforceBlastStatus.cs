using ModdingUtils.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnboundLib.GameModes;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
    internal class LifeforceBlastStatus : ReversibleEffect
    {
        private static GameObject vfxHealBoost = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_Part_HealBoost");
        private static GameObject vfxHealHinder = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_Part_HealHinder");

        // value is the multiplier to be used multiplicatively
        private float healModifierFactor = 1.0f;
        private float effectDuration = 4.0f;
        private bool isFriendly = false;

        internal float effectTimer = 0.0f;
        internal GameObject healBoostObj, healHinderObj;
        internal ParticleSystem healBoostPart, healHinderPart;
        ParticleSystem.ShapeModule shape;

        public void ApplyEffect(float factor, float duration, bool friendly)
        {
            if (friendly == this.isFriendly)
            {
                effectTimer = 0.0f;
                if (isFriendly && factor > healModifierFactor)
                {
                    healModifierFactor = factor;
                }
                else if (!isFriendly && factor < healModifierFactor)
                {
                    healModifierFactor = factor;
                }

                if (duration > effectDuration)
                {
                    effectDuration = duration;
                }
            }
            else
            {
                effectTimer = 0.0f;
                healModifierFactor = factor;
                effectDuration = duration;
                isFriendly = friendly;
            }
        }

        public float GetHealMultiplier()
        {
            return healModifierFactor;
        }

        public override void OnUpdate()
        {
            effectTimer += TimeHandler.deltaTime;

            if (isFriendly)
            {
                if (healBoostObj == null)
                {
                    healBoostObj = Instantiate(vfxHealBoost, player.transform.root);
                    healBoostPart = healBoostObj.GetComponentInChildren<ParticleSystem>();
                }
                else
                {
                    healBoostObj.SetActive(true);
                }

                if (healHinderObj != null)
                {
                    healHinderObj.SetActive(false);
                }

                shape = healBoostPart.shape;
            }
            else
            {
                if (healHinderObj == null)
                {
                    healHinderObj = Instantiate(vfxHealHinder, player.transform.root);
                    healHinderPart = healHinderObj.GetComponentInChildren<ParticleSystem>();
                }
                else
                {
                    healHinderObj.SetActive(true);
                }

                if (healBoostObj != null)
                {
                    healBoostObj.SetActive(false);
                }

                shape = healHinderPart.shape;
            }

            shape.scale = player.transform.localScale * 1.25f;


            if (effectTimer > effectDuration)
            {
                PurgeStatus();
            }
        }

        private void PurgeStatus()
        {
            if (healBoostObj != null)
            {
                Destroy(healBoostObj);
            }
            if (healHinderObj != null)
            {
                Destroy(healHinderObj);
            }
            Destroy(this);
        }

        override public void OnOnDisable()
        {
            // This status effect should be cleared out when they are dead, reviving or not
            PurgeStatus();
        }
    }
}
