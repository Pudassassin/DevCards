using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using System.Collections;
using System;
using UnityEngine.UI;

namespace GearUpCards.MonoBehaviours
{
    internal class CooldownUIMono : MonoBehaviour
    {
        private static GameObject UIPrefab = GearUpCards.VFXBundle.LoadAsset<GameObject>("UI_CardCD");

        // public float _debugScale = 4.0f;

        private const float procTime = .10f;

        internal float timer = 0.0f;
        internal bool effectWarmup = false;
        internal bool effectEnabled = true;

        internal Player player;
        internal CharacterStatModifiers stats;

        internal GameObject cooldownUI = null;
        internal Text scannerCD = null;
        internal GameObject scannerIcon = null;
        internal Text magickCD = null;
        internal GameObject magickIcon = null;

        internal TacticalScannerEffect scannerEffect = null;
        internal float scannerCooldown;

        internal MonoBehaviour magickEffect = null;
        internal float magickCooldown;

        /* DEBUG */
        // internal int proc_count = 0;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
            cooldownUI = Instantiate(UIPrefab, player.transform.position, Quaternion.identity);
            cooldownUI.name = $"CooldownUI_p({player.playerID})";

            cooldownUI.transform.localPosition += new Vector3(0.0f, 0.0f, 50.0f);
            cooldownUI.GetComponentInChildren<Canvas>().sortingLayerName = "MostFront";

            scannerCD = GameObject.Find($"{cooldownUI.name}/Canvas/Scanner/Text").GetComponent<Text>();
            scannerIcon = GameObject.Find($"{cooldownUI.name}/Canvas/Scanner");

            magickCD = GameObject.Find($"{cooldownUI.name}/Canvas/Magick/Text").GetComponent<Text>();
            magickIcon = GameObject.Find($"{cooldownUI.name}/Canvas/Magick");
        }

        public void Update()
        {
            float cooldown;
            cooldownUI.transform.position = player.transform.position;
            cooldownUI.transform.localScale = Vector3.one * 3.0f;

            if (effectEnabled)
            {
                cooldownUI.SetActive(true);

                // Tactical Scanner Cooldown
                if (scannerEffect != null)
                {
                    cooldown = scannerEffect.GetCooldown();
                    if (cooldown > 0.0f)
                    {
                        scannerIcon.SetActive(true);
                        scannerCD.text = FormatCooldown(cooldown);
                    }
                    else
                    {
                        scannerIcon.SetActive(false);
                    }
                }
                else
                {
                    scannerIcon.SetActive(false);
                }

                // Unique Magick Cooldown
                if (stats.GetGearData().uniqueMagick != GearUpConstants.ModType.none &&
                    stats.GetGearData().uniqueMagick != GearUpConstants.ModType.disabled
                   )
                {
                    cooldown = stats.GetGearData().t_uniqueMagickCooldown;
                    if (cooldown > 0.0f)
                    {
                        magickIcon.SetActive(true);
                        magickCD.text = FormatCooldown(cooldown);
                    }
                    else
                    {
                        magickIcon.SetActive(false);
                    }
                }
                else
                {
                    magickIcon.SetActive(false);
                }

            }
            else
            {
                cooldownUI.SetActive(false);
            }

        }

        internal string FormatCooldown(float cooldown)
        {
            if (cooldown >= 9.9f) return $"{cooldown:f0}";
            else return $"{cooldown:f1}";
        }

        public void FetchAbilities()
        {
            // Common block ability with cooldowns
            if (stats.GetGearData().tacticalScannerStack > 0)
            {
                scannerEffect = player.gameObject.GetOrAddComponent<TacticalScannerEffect>();
            }
            else
            {
                scannerEffect = null;
            }

            // Unique Magicks
            // switch (stats.GetGearData().uniqueMagick)
            // {
            //     case GearUpConstants.ModType.disabled:
            //         magickEffect = null;
            //         break;
            // 
            //     case GearUpConstants.ModType.none:
            //         magickEffect = null;
            //         break;
            // 
            //     case GearUpConstants.ModType.magickAntiBullet:
            //         magickEffect = player.gameObject.GetOrAddComponent<AntiBulletMagickEffect>();
            //         break;
            // 
            //     default:
            //         magickEffect = null;
            //         break;
            // }
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectWarmup = true;
            effectEnabled = false;

            FetchAbilities();

            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            effectWarmup = false;
            effectEnabled = true;

            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Battle Start");

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            effectEnabled = false;
        
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Point End");
        
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
                effectEnabled = false;
                cooldownUI.SetActive(false);
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }

        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

    }
}
