using ModdingUtils.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnboundLib.GameModes;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
    internal class ArcaneSunStatus : ReversibleEffect
    {
        private static GameObject vfxSunBurn = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_Part_SunBurn");

        // value is the multiplier to be used multiplicatively
        private float damageMultiplierFactor = 1.0f;
        private float particleEmissionScale = 2.0f;

        // private bool isDecaying = false;
        private float effectCharge = 0.0f;
        private float effectRetainTime = 1.0f;
        private float effectDecayRate = 1.0f;

        internal bool wasDisabled = false;

        internal float decayTimer = 0.0f;
        internal GameObject sunBurnObj;
        internal ParticleSystem sunBurnPart;
        ParticleSystem.ShapeModule shape;
        ParticleSystem.EmissionModule emission;

        public void ApplyEffect(float factor, float chargeAdd, float retainTime, float decayRate)
        {
            if (retainTime > effectRetainTime)
            {
                effectRetainTime = retainTime;
            }
            if (decayRate < effectDecayRate)
            {
                effectDecayRate = decayRate;
            }
            if (factor > damageMultiplierFactor)
            {
                damageMultiplierFactor = factor;
            }

            effectCharge += chargeAdd;
            // isDecaying = false;
            decayTimer = 0.0f;
        }

        public float GetDamageMultiplier()
        {
            return Mathf.Pow(damageMultiplierFactor, effectCharge);
        }

        public float GetEffectCharge()
        {
            return effectCharge;
        }

        public override void OnAwake()
        {
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointEnd);
        }

        public override void OnUpdate()
        {
            if (wasDisabled)
            {
                PurgeStatus();
            }

            decayTimer += TimeHandler.deltaTime;
            if (decayTimer >= effectRetainTime)
            {
                effectCharge -= effectDecayRate * TimeHandler.deltaTime;
            }

            if (sunBurnObj == null)
            {
                sunBurnObj = Instantiate(vfxSunBurn, player.transform.root);
                sunBurnPart = sunBurnObj.GetComponentInChildren<ParticleSystem>();
                shape = sunBurnPart.shape;
                emission = sunBurnPart.emission;
            }

            shape.scale = player.transform.localScale * 1.25f;
            emission.rateOverTime = effectCharge * particleEmissionScale;

            if (effectCharge < 0.0f)
            {
                PurgeStatus();
            }
        }

        private void PurgeStatus()
        {
            if (sunBurnObj != null)
            {
                Destroy(sunBurnObj);
            }

            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointEnd);

            Destroy(this);
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            // This status effect should be cleared out at point end, if they survived
            PurgeStatus();

            yield break;
        }

        override public void OnOnDisable()
        {
            // This status effect should be cleared out when they are dead, reviving or not
            wasDisabled = true;
            PurgeStatus();
        }
    }
}
