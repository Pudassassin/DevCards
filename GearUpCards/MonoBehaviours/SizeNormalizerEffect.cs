using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using GearUpCards.Utils;

using System.Collections;

namespace GearUpCards.MonoBehaviours
{
    internal class SizeNormalizerEffect : MonoBehaviour
    {
        private const float procTime = 0.1f;

        internal Player player;
        internal CharacterStatModifiers stats;

        internal float prevSizeModifier;
        internal float prevMaxHealth;

        internal float totalScaleBefore;
        internal float totalScaleAfter;
        internal float targetScale;

        internal float timer = 0.0f;
        internal bool effectEnabled;
        internal bool effectApplied;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            Refresh();

            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, OnRoundEnd);
        }

        public void Start()
        {

        }

        // [*] [Brawler] and [Pristine Perserverence] will make player size temporary bigger than what [Size Norm.] supposed to keep due to the MAX HP increases
        // [!] severely reduce [Overpower] knockback force due to being locked down to default size (still do proper damage)

        public void Update()
        {
            timer += Time.deltaTime;

            if (timer > procTime)
            {
                if (effectEnabled)
                {
                    if (!StatsMath.ApproxEqual(this.player.data.maxHealth, prevMaxHealth))
                    {
                        effectApplied = false;
                        prevMaxHealth = this.player.data.maxHealth;
                        totalScaleBefore = Mathf.Pow(this.player.data.maxHealth / 100f * 1.2f, 0.2f) * prevSizeModifier;
                    }
                }

                if (effectEnabled && !effectApplied)
                {
                    if (totalScaleBefore >= 1.2f)
                    {
                        targetScale = 1.65f - (0.5f * Mathf.Pow(0.9f, this.totalScaleBefore));
                    }
                    else
                    {
                        targetScale = 0.1f + Mathf.Pow(1.1f, this.totalScaleBefore);
                    }

                    totalScaleAfter = prevSizeModifier * targetScale / totalScaleBefore;

                    this.player.gameObject.GetOrAddComponent<SizeNormalizerStatus>().SetSize(targetScale / totalScaleBefore);
                    effectApplied = true;

                    // UnityEngine.Debug.Log($"[SizeNorm] adjusting... [{player.playerID}] [{totalScaleBefore}] >> [{targetScale}] == [{totalScaleAfter}] >> [{stats.sizeMultiplier}]");
                }

                if (effectEnabled && effectApplied)
                {
                    // this.player.gameObject.GetOrAddComponent<SizeNormalizerStatus>().SetSize(targetScale / totalScaleBefore);
                }

                if (!effectEnabled && !effectApplied)
                {
                    this.player.gameObject.GetOrAddComponent<SizeNormalizerStatus>().SetSize(1.0f);
                    effectApplied = true;
                }
                timer -= procTime;
            }


        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            Refresh();

            yield break;
        }

        private IEnumerator OnRoundEnd(IGameModeHandler gm)
        {
            effectEnabled = false;
            effectApplied = false;

            yield break;
        }

        public void OnDisable()
        {
            bool isRespawning = player.data.healthHandler.isRespawning;
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting [{isRespawning}]");

            if (isRespawning)
            {
                // does nothing
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting!?");
            }
            else
            {
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }
        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, OnRoundEnd);
        }

        public void Refresh()
        {
            if (this.stats.GetGearData().sizeMod == GearUpConstants.ModType.sizeNormalize)
            {
                effectEnabled = true;
            }
            effectApplied = false;

            prevMaxHealth = player.data.maxHealth;
            prevSizeModifier = this.stats.sizeMultiplier;
            totalScaleBefore = Mathf.Pow(this.player.data.maxHealth / 100f * 1.2f, 0.2f) * prevSizeModifier;
        }
    }

    class SizeNormalizerStatus : ReversibleEffect
    {
        public override void OnAwake()
        {
            this.SetLivesToEffect(999);
            base.OnAwake();
        }

        public void SetSize(float size)
        {
            characterStatModifiersModifier.sizeMultiplier_mult = size;
            ApplyModifiers();
        }
    }
}
