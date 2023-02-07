using System.Collections.Generic;

using UnboundLib;
using UnityEngine;
using UnboundLib.GameModes;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Utils;
using GearUpCards.Extensions;
using System.Collections;
using UnityEngine.UI;

namespace GearUpCards.MonoBehaviours
{
    internal class CooldownUIMono : MonoBehaviour
    {
        private static GameObject UIPrefab = GearUpCards.VFXBundle.LoadAsset<GameObject>("UI_CardCD");
        private static float iconGapWidth = 0.5f;

        // public float _debugScale = 4.0f;

        private const float procTime = .10f;

        internal float timer = 0.0f;
        internal bool effectWarmup = false;
        internal bool effectEnabled = true;

        internal Player player;
        internal CharacterStatModifiers stats;

        internal GameObject cooldownUI = null;

        internal UnityEngine.UI.Text scannerText = null;
        internal GameObject scannerIcon = null;

        internal UnityEngine.UI.Text magickText = null;
        internal GameObject magickIcon = null;

        // Orb Spells
        internal List<GameObject> orbIcons;

        internal UnityEngine.UI.Text orbLitText = null;
        internal GameObject orbLitIcon = null;

        internal UnityEngine.UI.Text rollBorbText = null;
        internal GameObject rollBorbIcon = null;

        internal UnityEngine.UI.Text LFDuorbText = null;
        internal GameObject LFDuorbIcon = null;

        internal UnityEngine.UI.Text LFBlastText = null;
        internal GameObject LFBlastIcon = null;

        internal OrbSpellsMono orbSpellsMono = null;

        internal float tempOrbCooldown;
        internal Vector3 tempPos;

        // internal float orbLiterationCooldown;
        public int orbLitIndex = -1;

        // internal float rollingBorbwarkCooldown;
        public int rollingBorbwarkIndex = -1;

        // internal float lifeforceDuorbityCooldown;
        public int lifeforceDuorbityIndex = -1;

        // internal float lifeforceBlastCooldown;
        public int lifeforceBlastIndex = -1;
        // ==========

        internal UnityEngine.UI.Text empowerShotText = null;
        internal GameObject empowerShotIcon = null;

        internal TacticalScannerEffect scannerEffect = null;
        internal float scannerCooldown;

        internal MonoBehaviour magickEffect = null;
        internal float magickCooldown;

        internal ShieldBatteryEffect shieldBattery = null;
        internal int empowerShotCount;

        // internal float tempCooldown;
        private bool wasDeactivated = false;

        /* DEBUG */
        // internal int proc_count = 0;
        // bool skipOrbs = false;
        // bool skipBattery = false;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            orbIcons = new List<GameObject>();

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

            scannerText = GameObject.Find($"{cooldownUI.name}/Canvas/Scanner/Text").GetComponent<Text>();
            scannerIcon = GameObject.Find($"{cooldownUI.name}/Canvas/Scanner");

            magickText = GameObject.Find($"{cooldownUI.name}/Canvas/Magick/Text").GetComponent<Text>();
            magickIcon = GameObject.Find($"{cooldownUI.name}/Canvas/Magick");

            orbLitText = GameObject.Find($"{cooldownUI.name}/Canvas/OrbLiterate/Text").GetComponent<Text>();
            orbLitIcon = GameObject.Find($"{cooldownUI.name}/Canvas/OrbLiterate");

            rollBorbText = GameObject.Find($"{cooldownUI.name}/Canvas/RollingBorbwark/Text").GetComponent<Text>();
            rollBorbIcon = GameObject.Find($"{cooldownUI.name}/Canvas/RollingBorbwark");

            LFDuorbText = GameObject.Find($"{cooldownUI.name}/Canvas/LifeforceDuorbity/Text").GetComponent<Text>();
            LFDuorbIcon = GameObject.Find($"{cooldownUI.name}/Canvas/LifeforceDuorbity");

            LFBlastText = GameObject.Find($"{cooldownUI.name}/Canvas/LifeforceBlast/Text").GetComponent<Text>();
            LFBlastIcon = GameObject.Find($"{cooldownUI.name}/Canvas/LifeforceBlast");

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
                    scannerCooldown = scannerEffect.GetCooldown();

                    if (scannerCooldown > 0.0f)
                    {
                        scannerIcon.SetActive(true);
                        scannerText.text = FormatCooldown(scannerCooldown);
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
                    magickCooldown = stats.GetGearData().t_uniqueMagickCooldown;
                    if (magickCooldown > 0.0f)
                    {
                        magickIcon.SetActive(true);
                        magickText.text = FormatCooldown(magickCooldown);
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

                // Orb Spells cooldown
                orbIcons.Clear();
                if (orbSpellsMono != null)
                {
                    // Orb-Literate! Cooldown
                    orbLitIndex = orbSpellsMono.QueryOrbSpell(OrbSpellsMono.OrbSpellType.obliteration);

                    if (orbLitIndex >= 0)
                    {
                        tempOrbCooldown = orbSpellsMono.GetOrbSpellCooldown(orbLitIndex);
                        if (tempOrbCooldown > 0.0f)
                        {
                            orbIcons.Add(orbLitIcon);
                            orbLitIcon.SetActive(true);
                            orbLitText.text = FormatCooldown(tempOrbCooldown);
                        }
                        else
                        {
                            orbLitIcon.SetActive(false);
                        }
                    }
                    else
                    {
                        orbLitIcon.SetActive(false);
                    }

                    // Rolling Borbwark Cooldown
                    rollingBorbwarkIndex = orbSpellsMono.QueryOrbSpell(OrbSpellsMono.OrbSpellType.rollingBulwark);

                    if (rollingBorbwarkIndex >= 0)
                    {
                        tempOrbCooldown = orbSpellsMono.GetOrbSpellCooldown(rollingBorbwarkIndex);
                        if (tempOrbCooldown > 0.0f)
                        {
                            orbIcons.Add(rollBorbIcon);
                            rollBorbIcon.SetActive(true);
                            rollBorbText.text = FormatCooldown(tempOrbCooldown);
                        }
                        else
                        {
                            rollBorbIcon.SetActive(false);
                        }
                    }
                    else
                    {
                        rollBorbIcon.SetActive(false);
                    }

                    // Lifeforce Duorbity Cooldown
                    lifeforceDuorbityIndex = orbSpellsMono.QueryOrbSpell(OrbSpellsMono.OrbSpellType.lifeforceDuality);

                    if (lifeforceDuorbityIndex >= 0)
                    {
                        tempOrbCooldown = orbSpellsMono.GetOrbSpellCooldown(lifeforceDuorbityIndex);
                        if (tempOrbCooldown > 0.0f)
                        {
                            orbIcons.Add(LFDuorbIcon);
                            LFDuorbIcon.SetActive(true);
                            LFDuorbText.text = FormatCooldown(tempOrbCooldown);
                        }
                        else
                        {
                            LFDuorbIcon.SetActive(false);
                        }
                    }
                    else
                    {
                        LFDuorbIcon.SetActive(false);
                    }

                    // Lifeforce Blast! Cooldown
                    lifeforceBlastIndex = orbSpellsMono.QueryOrbSpell(OrbSpellsMono.OrbSpellType.lifeforceBlast);

                    if (lifeforceBlastIndex >= 0)
                    {
                        tempOrbCooldown = orbSpellsMono.GetOrbSpellCooldown(lifeforceBlastIndex);
                        if (tempOrbCooldown > 0.0f)
                        {
                            orbIcons.Add(LFBlastIcon);
                            LFBlastIcon.SetActive(true);
                            LFBlastText.text = FormatCooldown(tempOrbCooldown);
                        }
                        else
                        {
                            LFBlastIcon.SetActive(false);
                        }
                    }
                    else
                    {
                        LFBlastIcon.SetActive(false);
                    }

                    // reposition icons
                    for (int i = 0; i < orbIcons.Count; i++)
                    {
                        tempPos = orbIcons[i].transform.localPosition;
                        tempPos.x = ((float)i * iconGapWidth) - ((float)(orbIcons.Count - 1) * iconGapWidth / 2.0f);
                        orbIcons[i].transform.localPosition = tempPos;
                    }
                }
                else
                {
                    orbLitIcon.SetActive(false);
                    rollBorbIcon.SetActive(false);
                    LFBlastIcon.SetActive(false);
                    LFDuorbIcon.SetActive(false);
                }

                // if (!skipOrbs)
                // {
                //     Miscs.Log("[GearUp] CooldownUIMono try fetching orbs");
                //     
                // 
                //     try
                //     {
                //         
                //     }
                //     catch (Exception exception)
                //     {
                //         Miscs.LogWarn(exception.ToString());
                //     
                //         orbLibIcon.SetActive(false);
                //         skipOrbs = true;
                //     }
                // }

                // (temp) Empower Shots Counter
                if (shieldBattery != null)
                {
                    empowerShotCount = shieldBattery.GetChargeCount();
                    if (empowerShotCount > 0)
                    {
                        empowerShotIcon.SetActive(true);
                        empowerShotText.text = empowerShotCount.ToString();
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

                // if (!skipBattery)
                // {
                //     // Miscs.Log("[GearUp] CooldownUIMono try fetching ShieldBattery");
                //     try
                //     {
                //
                //     }
                //     catch (Exception exception)
                //     {
                //         Miscs.LogWarn(exception.ToString());
                // 
                //         empowerShotIcon.SetActive(false);
                //         skipBattery = true;
                //     }
                // }

            }
            else
            {
                cooldownUI.SetActive(false);
            }

        }

        internal string FormatCooldown(float cooldown)
        {
            if (cooldown >= 4.9f) return $"{cooldown:f0}";
            else return $"{cooldown:f1}";
        }

        public void FetchAbilities()
        {
            // Common block ability with cooldowns
            if (stats.GetGearData().tacticalScannerStack > 0)
            {
                scannerEffect = player.gameObject.GetComponent<TacticalScannerEffect>();
            }
            else
            {
                scannerEffect = null;
            }

            if (stats.GetGearData().shieldBatteryStack > 0)
            {
                shieldBattery = player.gameObject.GetComponent<ShieldBatteryEffect>();
            }
            else
            {
                shieldBattery = null;
            }

            // if (stats.GetGearData().orbObliterationStack > 0 || stats.GetGearData().orbRollingBulwarkStack > 0)
            // {
            //     orbSpellsMono = player.gameObject.GetComponent<OrbSpellsMono>();
            // }
            // else
            // {
            //     orbSpellsMono = null;
            // }

            orbSpellsMono = player.gameObject.GetComponent<OrbSpellsMono>();

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
