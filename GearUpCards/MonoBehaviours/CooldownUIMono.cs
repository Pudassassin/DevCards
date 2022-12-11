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

        // public float _debugScale = 4.0f;

        private const float procTime = .10f;

        internal float timer = 0.0f;
        internal bool effectWarmup = false;
        internal bool effectEnabled = true;

        internal Player player;
        internal CharacterStatModifiers stats;

        internal GameObject cooldownUI = null;

        internal UnityEngine.UI.Text scannerCDText = null;
        internal GameObject scannerIcon = null;

        internal UnityEngine.UI.Text magickCDText = null;
        internal GameObject magickIcon = null;

        internal UnityEngine.UI.Text orbLitCDText = null;
        internal GameObject orbLitIcon = null;

        internal UnityEngine.UI.Text empowerShotText = null;
        internal GameObject empowerShotIcon = null;

        internal TacticalScannerEffect scannerEffect = null;
        internal float scannerCooldown;

        internal MonoBehaviour magickEffect = null;
        internal float magickCooldown;

        internal ShieldBatteryEffect shieldBattery = null;
        internal int empowerShotCount;

        internal OrbSpellsMono orbSpellsMono = null;
        internal float orbLiterationCooldown;
        public int orbLitIndex = -1;

        // internal float tempCooldown;
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

            orbLitCDText = GameObject.Find($"{cooldownUI.name}/Canvas/OrbLiterate/Text").GetComponent<Text>();
            orbLitIcon = GameObject.Find($"{cooldownUI.name}/Canvas/OrbLiterate");

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
                        scannerCDText.text = FormatCooldown(scannerCooldown);
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
                        magickCDText.text = FormatCooldown(magickCooldown);
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
                if (orbSpellsMono != null)
                {
                    orbLitIndex = orbSpellsMono.QueryOrbSpell(OrbSpellsMono.OrbSpellType.obliteration);

                    if (orbLitIndex >= 0)
                    {
                        orbLiterationCooldown = orbSpellsMono.GetOrbSpellCooldown(orbLitIndex);
                        if (orbLiterationCooldown > 0.0f)
                        {
                            orbLitIcon.SetActive(true);
                            orbLitCDText.text = FormatCooldown(orbLiterationCooldown);
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
                }
                else
                {
                    orbLitIcon.SetActive(false);
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

            if (stats.GetGearData().orbObliterationStack > 0)
            {
                orbSpellsMono = player.gameObject.GetComponent<OrbSpellsMono>();
            }
            else
            {
                orbSpellsMono = null;
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
