using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Utils;
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

        internal Text scannerCDText = null;
        internal GameObject scannerIcon = null;

        internal Text magickCDText = null;
        internal GameObject magickIcon = null;

        internal Text orbLibCDText = null;
        internal GameObject orbLibIcon = null;

        internal Text empowerShotText = null;
        internal GameObject empowerShotIcon = null;

        internal TacticalScannerEffect scannerEffect = null;
        internal float scannerCooldown;

        internal MonoBehaviour magickEffect = null;
        internal float magickCooldown;

        internal ShieldBatteryEffect shieldBattery = null;
        internal float empowerShotCount;

        internal OrbSpellsMono orbSpells = null;
        internal float orbLiterationCooldown;

        internal float tempCooldown;
        private bool wasDeactivated = false;

        /* DEBUG */
        // internal int proc_count = 0;
        bool skipOrbs = false;
        bool skipBattery = false;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            // GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
            cooldownUI = Instantiate(UIPrefab, player.transform.position, Quaternion.identity);
            cooldownUI.name = $"CooldownUI_p({player.playerID})";

            cooldownUI.transform.localPosition += new Vector3(0.0f, 0.0f, 50.0f);
            cooldownUI.GetComponentInChildren<Canvas>().sortingLayerName = "MostFront";

            scannerCDText = GameObject.Find($"{cooldownUI.name}/Canvas/Scanner/Text").GetComponent<Text>();
            scannerIcon = GameObject.Find($"{cooldownUI.name}/Canvas/Scanner");

            magickCDText = GameObject.Find($"{cooldownUI.name}/Canvas/Magick/Text").GetComponent<Text>();
            magickIcon = GameObject.Find($"{cooldownUI.name}/Canvas/Magick");

            orbLibCDText = GameObject.Find($"{cooldownUI.name}/Canvas/OrbLiterate/Text").GetComponent<Text>();
            orbLibIcon = GameObject.Find($"{cooldownUI.name}/Canvas/OrbLiterate");

            empowerShotText = GameObject.Find($"{cooldownUI.name}/Canvas/EmpowerShots/Text").GetComponent<Text>();
            empowerShotIcon = GameObject.Find($"{cooldownUI.name}/Canvas/EmpowerShots");
        }

        public void Update()
        {
            if (wasDeactivated)
            {
                effectEnabled = true;
                wasDeactivated = false;
            }

            cooldownUI.transform.position = player.transform.position;
            cooldownUI.transform.localScale = Vector3.one * 3.0f;

            if (effectEnabled)
            {
                cooldownUI.SetActive(true);

                // Tactical Scanner Cooldown
                if (scannerEffect != null)
                {
                    tempCooldown = scannerEffect.GetCooldown();
                    if (tempCooldown > 0.0f)
                    {
                        scannerIcon.SetActive(true);
                        scannerCDText.text = FormatCooldown(tempCooldown);
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
                    tempCooldown = stats.GetGearData().t_uniqueMagickCooldown;
                    if (tempCooldown > 0.0f)
                    {
                        magickIcon.SetActive(true);
                        magickCDText.text = FormatCooldown(tempCooldown);
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

                // (temp) OrbLiterate Cooldown
                if (!skipOrbs)
                {
                    // Miscs.Log("[GearUp] CooldownUIMono try fetching orbs");
                    try
                    {
                        if (orbSpells != null)
                        {
                            int spellIndex = orbSpells.QueryOrbSpell(OrbSpellsMono.OrbSpellStats.OrbSpellType.obliteration);
                            if (spellIndex < 0)
                            {
                                orbLibIcon.SetActive(false);
                            }
                            else
                            {
                                tempCooldown = orbSpells.GetOrbSpellCooldown(spellIndex);
                                if (tempCooldown > 0.0f)
                                {
                                    orbLibIcon.SetActive(true);
                                    orbLibCDText.text = FormatCooldown(tempCooldown);
                                }
                                else
                                {
                                    orbLibIcon.SetActive(false);
                                }
                            }
                        }
                        else
                        {
                            orbLibIcon.SetActive(false);
                        }
                    }
                    catch (Exception exception)
                    {
                        Miscs.LogWarn(exception.ToString());

                        orbLibIcon.SetActive(false);
                        skipOrbs = true;
                    }
                }

                // (temp) Empower Shots Counter
                if (!skipBattery)
                {
                    // Miscs.Log("[GearUp] CooldownUIMono try fetching ShieldBattery");
                    try
                    {
                        if (shieldBattery != null)
                        {
                            int shotCount = shieldBattery.GetChargeCount();
                            if (shotCount > 0)
                            {
                                empowerShotIcon.SetActive(true);
                                empowerShotText.text = shotCount.ToString();
                            }
                            else
                            {
                                empowerShotIcon.SetActive(false);
                            }
                        }
                        else
                        {
                            empowerShotIcon.SetActive(false);
                        }
                    }
                    catch (Exception exception)
                    {
                        Miscs.LogWarn(exception.ToString());

                        empowerShotIcon.SetActive(false);
                        skipBattery = true;
                    }
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

            if (stats.GetGearData().shieldBatteryStack > 0)
            {
                shieldBattery = player.gameObject.GetOrAddComponent<ShieldBatteryEffect>();
            }
            else
            {
                shieldBattery = null;
            }

            if (stats.GetGearData().orbObliterationStack > 0)
            {
                shieldBattery = player.gameObject.GetOrAddComponent<ShieldBatteryEffect>();
            }
            else
            {
                shieldBattery = null;
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
            //effectWarmup = true;
            effectEnabled = true;

            FetchAbilities();

            yield break;
        }

        // private IEnumerator OnBattleStart(IGameModeHandler gm)
        // {
        //     effectWarmup = false;
        //     effectEnabled = true;
        // 
        //     // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Battle Start");
        // 
        //     yield break;
        // }

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
                wasDeactivated = true;

                effectEnabled = false;
                cooldownUI.SetActive(false);
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }

        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            // GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            Destroy(cooldownUI);
        }

    }
}
