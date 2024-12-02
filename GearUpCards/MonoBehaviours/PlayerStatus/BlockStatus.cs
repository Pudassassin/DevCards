using GearUpCards.Utils;
using ModdingUtils.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.UI;

namespace GearUpCards.MonoBehaviours
{
    internal class BlockStatus : ReversibleEffect
    {
        public static float DefaultBlockIFrame = 0.3f;
        public static Color DefaultShieldOnCD = new Color(0.1752f, 0.1912f, 0.2075f, 1f);
        public static Color DefaultShieldOffCD = new Color(0.8f, 0.8f, 0.8f, 1f);

        // value is the multiplier to be used multiplicatively
        private float blockIFrameFactor = 1.0f;
        private float effectDuration = 4.0f;
        private bool isFriendly = false;

        internal bool wasDisabled = false;

        internal float effectTimer = 0.0f;

        // visuals
        private GameObject shieldStoneObj = null;
        private SpriteRenderer shieldStoneSprite = null;
        private SetColorByBlockCD colorSetter = null;

        private GameObject shieldCDObj = null;
        private Image shieldCDImage = null;

        public void ApplyEffect(float factor, float duration, bool friendly)
        {
            if (friendly == this.isFriendly)
            {
                effectTimer = 0.0f;
                if (isFriendly && factor > blockIFrameFactor)
                {
                    blockIFrameFactor = factor;
                }
                else if (!isFriendly && factor < blockIFrameFactor)
                {
                    blockIFrameFactor = factor;
                }

                if (duration > effectDuration)
                {
                    effectDuration = duration;
                }
            }
            else
            {
                effectTimer = 0.0f;
                blockIFrameFactor = factor;
                effectDuration = duration;
                isFriendly = friendly;
            }
        }

        public float GetBlockIFrameMultiplier()
        {
            return blockIFrameFactor;
        }

        public override void OnAwake()
        {
            // visual and VFX
            shieldStoneObj = Miscs.GetChildByHierachy(gameObject, "Limbs\\ArmStuff\\ShieldStone");
            colorSetter = shieldStoneObj.GetComponent<SetColorByBlockCD>();
            shieldStoneSprite = shieldStoneObj.GetComponent<SpriteRenderer>();

            shieldCDObj = Miscs.GetChildByHierachy(shieldStoneObj, "Canvas\\Image");
            shieldCDImage = shieldCDObj.GetComponent<Image>();

            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointEnd);
        }

        public override void OnUpdate()
        {
            if (wasDisabled)
            {
                PurgeStatus();
            }

            effectTimer += TimeHandler.deltaTime;

            if (isFriendly)
            {
                
            }
            else
            {
                // visual and VFX
                if (blockIFrameFactor <= 0.0f)
                {
                    if (!block.IsOnCD())
                    {
                        shieldStoneSprite.color = Color.red;
                    }

                    shieldCDImage.color = Color.red;
                }
            }

            if (effectTimer > effectDuration)
            {
                PurgeStatus();
            }
        }

        private void PurgeStatus()
        {
            // clean up code

            // visual and VFX
            if (!block.IsOnCD())
            {
                shieldStoneSprite.color = DefaultShieldOffCD;
            }

            shieldCDImage.color = DefaultShieldOffCD;

            // unhook events
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
