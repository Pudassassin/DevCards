using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using UnityEngine;
using SoundImplementation;

using UnboundLib;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using HarmonyLib;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class PortalMagickEffect : MonoBehaviour
    {
        private static GameObject vfxPortalBlue    = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_PortalMagick_Blue");
        private static GameObject vfxPortalOrange  = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_PortalMagick_Orange");
        private static GameObject vfxPortalCrossHairs = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_PortalMagick_Crosshairs");
        private static int idCounter = 0;

        private static int portalCheckSubDiv = 12;
        private static float portalCheckTolerance = 0.333f;

        // private static GameObject portalCrosshair  = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_PortalMagick_Orange");

        private static string portalMagickRuneCircleHier = "Circle_Root/Circle_Rune";
        
        private static float spellCooldownBase = 12.0f;
        private static float spellCooldownScaling = -1.5f;
        
        private static float spellRangeBase = 25.0f;
        private static float spellRangeScaling = 5.0f;
        
        private static float spellDurationBase = 8.0f;
        private static float spellDurationScaling = 0.5f;

        private static float portalSizeBase = 2.5f;
        private static float portalSizeScaling = 0.5f;

        private static float portalDamageAmpBase = 1.25f;
        private static float portalDamageAmpScaling = 0.25f;

        private static float portalBounceAddBase = 2.1f;
        private static float portalBounceAddScaling = 2.0f;

        // public float spellCastDelay = 0.1f;
        // public float spellCastDelayTimer = 0.0f;
        // public bool spellIsCast = false;
        // public BlockTrigger.BlockTriggerType spellTrigger;

        private static float procTickTime = 0.01f;
        private static float warmupTime = 0.5f;

        internal Action<BlockTrigger.BlockTriggerType> spellAction;

        // ===== Spell modifiers =====
        // Cooldown reduction
        internal int glyphMagick = 0;
        // Portal Size
        internal int glyphInfluence = 0;
        // Portal bullet: damage boost
        internal int glyphPotency = 0;
        // Portal duration
        internal int glyphTime = 0;
        // Portal bullet: bounce add
        internal int glyphGeometric = 0;
        // Portal cast range
        internal int glyphDivination = 0;

        internal float spellCooldown, spellRange, spellDuration, portalSize, portalBounceAdd, portalDamageAmp;

        internal bool spellReady = false;

        public float CooldownTimer = 0.0f;

        private GameObject portalCrosshairs;
        private Vector3 aimDirection, portalBluePos, portalOrangePos;
        private float distanceEnd, distanceStart;

        internal float procTimer = 0.0f;
        internal bool spellWarmUp = false;
        // internal int proc_count = 0;

        internal Player player;
        internal GeneralInput generalInput;
        internal Block block;
        internal CharacterStatModifiers stats;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.generalInput = this.gameObject.GetComponent<GeneralInput>();
            this.block = this.gameObject.GetComponent<Block>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            int clientPlayerID = PlayerManager.instance.players.First(player => player.data.view.IsMine).playerID;
            if (player.playerID == clientPlayerID)
            {
                portalCrosshairs = Instantiate(vfxPortalCrossHairs);
                portalCrosshairs.transform.localScale = Vector3.one;
                portalCrosshairs.SetActive(false);

                SpriteRenderer[] spriteRenderers = portalCrosshairs.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer item in spriteRenderers)
                {
                    item.sortingLayerName = "MostFront";
                    item.sortingOrder = 10000;
                }
            }

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
            // attach the new block effect and passing along reference to owner player's stats
            this.spellAction = new Action<BlockTrigger.BlockTriggerType>(this.GetDoBlockAction(this.player, this.block).Invoke);
            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Combine(this.block.BlockAction, this.spellAction);
        }

        public void Update()
        {
            procTimer += TimeHandler.deltaTime;
            if (CooldownTimer > 0.0f)
            { 
                CooldownTimer -= TimeHandler.deltaTime;
            }

            if (spellReady && !spellWarmUp)
            {
                // determine direction and portal pos
                if (generalInput.inputType == GeneralInput.InputType.Controller)
                {
                    aimDirection = generalInput.lastAimDirection;
                    aimDirection.z = 0.0f;
                    aimDirection = aimDirection.normalized;
                    distanceEnd = spellRange;
                }
                else
                {
                    aimDirection = MainCam.instance.cam.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;
                    aimDirection.z = 0.0f;
                    distanceEnd = aimDirection.magnitude;
                    aimDirection = aimDirection.normalized;
                    distanceEnd = Mathf.Clamp(distanceEnd, distanceStart + portalSize * 2.0f, spellRange);
                }
                distanceStart = (player.transform.localScale.x + portalSize + 1.0f);

                portalBluePos = player.transform.position + (aimDirection * distanceStart);
                portalOrangePos = player.transform.position + (aimDirection * distanceEnd);

                // aim visuals
                if (portalCrosshairs != null)
                {
                    portalCrosshairs.SetActive(true);
                    float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                    portalCrosshairs.transform.position = player.transform.position;
                    portalCrosshairs.transform.eulerAngles = new Vector3(0.0f, 0.0f, aimAngle);

                    portalCrosshairs.transform.GetChild(0).localPosition = new Vector3(distanceStart, 0.0f, 0.0f);
                    portalCrosshairs.transform.GetChild(0).localScale = Vector3.one * portalSize;
                    portalCrosshairs.transform.GetChild(0).localEulerAngles = new Vector3(0.0f, 0.0f, aimAngle * -1.0f);

                    portalCrosshairs.transform.GetChild(1).localPosition = new Vector3(distanceEnd, 0.0f, 0.0f);
                    portalCrosshairs.transform.GetChild(1).localScale = Vector3.one * portalSize;
                    portalCrosshairs.transform.GetChild(1).localEulerAngles = new Vector3(0.0f, 0.0f, aimAngle * -1.0f);
                }
            }
            else
            {
                if (portalCrosshairs != null)
                {
                    portalCrosshairs.SetActive(false);
                }
            }

            if (procTimer >= procTickTime)
            {
                if (this.stats.GetGearData().uniqueMagick == GearUpConstants.ModType.magickPortal)
                {
                    RecalculateEffectStats();
                    CheckReady();
                }
                else
                {
                    spellReady = false;
                }

                procTimer -= procTickTime;
            }

            this.stats.GetGearData().t_uniqueMagickCooldown = GetCooldown();
        }

        internal void CheckReady()
        {
            if (spellReady)
            {
                return;
            }
            // else if (Time.time >= timeLastBlocked + spellCooldown)
            else if (CooldownTimer <= 0.0f)
            {
                spellReady = true;
                CooldownTimer = 0.0f;
            }
        }

        public float GetCooldown()
        {
            // float cooldown = timeLastBlocked + spellCooldown - Time.time;
            if (spellReady) return -1.0f;
            else return CooldownTimer;
        }

        // a work around the delegate limits, cheeky!
        public Action<BlockTrigger.BlockTriggerType> GetDoBlockAction(Player player, Block block)
        {
            return delegate (BlockTrigger.BlockTriggerType trigger)
            {
                bool conditionMet = (trigger == BlockTrigger.BlockTriggerType.Default && spellReady && !spellWarmUp);
                bool areaCleared = CheckPortalClearance(portalBluePos) && CheckPortalClearance(portalOrangePos);

                if (conditionMet && areaCleared)
                {
                    // setup portal controller + pair
                    GameObject portalControlObj = new GameObject($"PortalController ({idCounter})");
                    portalControlObj.transform.position = Vector3.zero;
                    portalControlObj.transform.eulerAngles = Vector3.zero;
                    portalControlObj.transform.localScale = Vector3.one;

                    PortalMagickController controllerMono = portalControlObj.AddComponent<PortalMagickController>();
                    controllerMono.portalID = idCounter;
                    controllerMono.portalDuration = spellDuration;
                    controllerMono.portalSize = portalSize;
                    controllerMono.portalDamageAmp = portalDamageAmp;
                    controllerMono.portalBounceAdd = portalBounceAdd;

                    controllerMono.portalBlue = Instantiate(vfxPortalBlue, portalControlObj.transform);
                    controllerMono.portalBlue.transform.position = portalBluePos;
                    controllerMono.portalBlue.transform.eulerAngles = Vector3.zero;
                    controllerMono.portalBlue.transform.localScale = Vector3.one * portalSize;

                    GameObject runeCircle = controllerMono.portalBlue.transform.Find(portalMagickRuneCircleHier).gameObject;
                    runeCircle.GetOrAddComponent<CircleMeshMono>();

                    controllerMono.portalOrange = Instantiate(vfxPortalOrange, portalControlObj.transform);
                    controllerMono.portalOrange.transform.position = portalOrangePos;
                    controllerMono.portalOrange.transform.eulerAngles = Vector3.zero;
                    controllerMono.portalOrange.transform.localScale = Vector3.one * portalSize;

                    runeCircle = controllerMono.portalOrange.transform.Find(portalMagickRuneCircleHier).gameObject;
                    runeCircle.GetOrAddComponent<CircleMeshMono>();

                    controllerMono.SetUp();

                    idCounter++;
                    CooldownTimer = spellCooldown;
                    spellReady = false;
                }
            };
        }

        internal void RecalculateEffectStats()
        {
            glyphMagick = this.stats.GetGearData().glyphMagickFragment;
            glyphDivination = this.stats.GetGearData().glyphDivination;
            glyphInfluence = this.stats.GetGearData().glyphInfluence;
            glyphTime = this.stats.GetGearData().glyphTime;

            glyphPotency = this.stats.GetGearData().glyphPotency;
            glyphGeometric = this.stats.GetGearData().glyphGeometric;

            spellDuration = spellDurationBase + (spellDurationScaling * glyphTime);

            spellCooldown = spellCooldownBase + (spellCooldownScaling * glyphMagick);
            spellCooldown = Mathf.Clamp(spellCooldown, 6.0f, 30.0f);

            spellRange = spellRangeBase + (spellRangeScaling * glyphDivination);

            portalSize      = portalSizeBase + (portalSizeScaling * glyphInfluence);
            portalDamageAmp = portalDamageAmpBase + (portalDamageAmpScaling * glyphPotency);
            portalBounceAdd = portalBounceAddBase + (portalBounceAddScaling * glyphGeometric);
        }

        internal bool CheckPortalClearance(Vector3 position)
        {
            bool refresherCheck = MapUtils.RPCA_UpdateMapObjectsList();
            if (!refresherCheck)
            {
                Miscs.LogWarn("CheckPointInMapObject(): RPCA_UpdateMapObjectsList failed to execute!");
                return true;
            }

            int hitCount = 0, checkCount = 1;
            float subDivAngle = 360.0f / (float)portalCheckSubDiv;
            List< MapUtils.MapObject.MapObjectType > ignoreList = new List< MapUtils.MapObject.MapObjectType>()
            {
                MapUtils.MapObject.MapObjectType.background,
                MapUtils.MapObject.MapObjectType.mapExtendedWWMOZones
            };

            // Miscs.Log($"pos: {position}");
            if (MapUtils.CheckPointInMapObject(position, ignoreList))
            {
                hitCount++;
            }

            Vector3 checkPos = new Vector3(portalSize * 0.333f, 0.0f, 0.0f);
            Vector3 tempPos;
            for (int i = 0; i < portalCheckSubDiv; i++)
            {
                checkCount++;
                tempPos = Miscs.RotateVector(checkPos, subDivAngle * (float)i);
                //Miscs.Log($"angle: {subDivAngle * (float)i} | pos: {position + tempPos}");

                if (MapUtils.CheckPointInMapObject(position + tempPos, ignoreList))
                {
                    hitCount++;
                }
            }

            checkPos = new Vector3(portalSize * 0.667f, 0.0f, 0.0f);
            for (int i = 0; i < portalCheckSubDiv; i++)
            {
                checkCount++;
                tempPos = Miscs.RotateVector(checkPos, subDivAngle * ((float)i + 0.5f));
                //Miscs.Log($"angle: {subDivAngle * ((float)i + 0.5f)} | pos: {position + tempPos}");

                if (MapUtils.CheckPointInMapObject(position + tempPos, ignoreList))
                {
                    hitCount++;
                }
            }

            float hitRatio = (float)hitCount / (float)checkCount;
            Miscs.Log($"[GearUp] CheckPortalClearance: {hitRatio} / {portalCheckTolerance}");

            if (hitRatio > portalCheckTolerance)
            {
                return false;
            }

            return true;
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            spellWarmUp = true;
            // timeLastBlocked = Time.time - spellCooldown + warmupTime;
            CooldownTimer = warmupTime;
            spellReady = false;
            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            spellWarmUp = false;
            // spellReady = true;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            spellReady = false;
            spellWarmUp = true;

            // if (portalCrosshairs != null)
            // {
            //     portalCrosshairs.SetActive(false);
            // }

            yield break;
        }

        public void OnDisable()
        {
            if (portalCrosshairs != null)
            {
                portalCrosshairs.SetActive(false);
            }
        }

        public void OnDestroy()
        {
            // This effect should persist between rounds, and at 0 stack it should do nothing mechanically
            // UnityEngine.Debug.Log($"Destroying Scanner  [{this.player.playerID}]");

            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(this.block.BlockAction, this.spellAction);
            // UnityEngine.Debug.Log($"Scanner destroyed  [{this.player.playerID}]");
        }
    }
}
