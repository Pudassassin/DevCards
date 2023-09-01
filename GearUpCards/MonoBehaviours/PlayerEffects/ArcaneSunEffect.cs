using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnboundLib;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class ArcaneSunEffect : MonoBehaviour
    {
        private static GameObject vfxSunCore = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_ArcaneSun");
        private static GameObject vfxSunRay = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_Beam_SunRay");

        private const float procTickTime = .20f;
        private static float arcaneSunYPos = 1.5f;

        private static float hpDrainFactor = 0.5f;
        private static float damageFactor
        {
            get
            {
                return 1.0f - hpDrainFactor;
            }
        }

        private static float damageFlatBase = 3.0f;
        private static float damageFlatScaling = 1.0f;

        private static float damagePercentBase = 0.005f;
        private static float damagePercentScaling = 0.0025f;

        private static float damageAmpBase = 1.10f;
        private static float damageAmpScaling = 0.05f;

        private static float rampUpRateBase = 1.0f;
        private static float rampUpRateScaling = 0.2f;

        private static float preChargeBase = 7.0f;
        private static float preChargeScaling = 1.0f;

        private static float debuffRetainTimeBase = 1.0f;
        private static float debuffRetainTimeScaling = 0.5f;

        private static float debuffDecayRateBase = 1.0f;
        private static float debuffDecayRateScaling = -0.1f;

        private static float effectRadiusBase = 15.0f;
        private static float effectRadiusScaling = 1.5f;

        internal int stackCount = 0;

        private float damageFlat, damagePercent, damageAmp, rampUpRate, preCharge, debuffDecayRate, debuffRetainTime, effectRadius;

        private GameObject arcaneSunObject;
        private Dictionary<Player, GameObject> rayPlayerPairs;
        private Dictionary<Player, float> playerDistancePairs;
        private List<Player> lockedOnTargets, visibleTargets;

        internal float procTimer = 0.0f;
        // pre round setup
        internal bool effectWarmup = false;
        // core functionality
        internal bool effectEnabled = true;
        // was player respawned from true death?
        internal bool wasDeactivated = false;

        internal Player player;
        internal CharacterStatModifiers stats;

        /* DEBUG */
        internal int proc_count = 0;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            rayPlayerPairs = new Dictionary<Player, GameObject>();
            arcaneSunObject = Instantiate(vfxSunCore, transform.root);
            arcaneSunObject.transform.localScale = Vector3.one;
            arcaneSunObject.transform.localPosition = new Vector3(0.0f, player.transform.localScale.x + arcaneSunYPos, 0.0f);

            playerDistancePairs = new Dictionary<Player, float>();
            lockedOnTargets = new List<Player>();
            visibleTargets = new List<Player>();

            GameObject tempGameObject;
            RayVFXMono tempRayMono;
            foreach (Player item in PlayerManager.instance.players)
            {
                if (item.teamID == this.player.teamID)
                {
                    continue;
                }

                tempGameObject = Instantiate(vfxSunRay, arcaneSunObject.transform);
                tempGameObject.SetActive(false);
                tempRayMono = tempGameObject.AddComponent<RayVFXMono>();
                tempRayMono.TLinkFrom = arcaneSunObject.transform;
                tempRayMono.TLinkTo = item.transform.root;

                rayPlayerPairs[item] = tempGameObject;
                playerDistancePairs[item] = -1.0f;
            }

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {

        }

        public void Update()
        {
            if (ModdingUtils.Utils.PlayerStatus.PlayerAliveAndSimulated(player) && effectWarmup)
            {
                RefreshSpellStats();

                effectWarmup = false;
            }

            if (wasDeactivated && !effectWarmup)
            {
                OnRespawn();
                wasDeactivated = false;
            }

            if (effectEnabled && !effectWarmup)
            {
                procTimer += TimeHandler.deltaTime;

                if (procTimer >= procTickTime)
                {
                    // actual gameplay effects
                    bool linkFlag;
                    float distance;
                    Vector2 sightPosition = player.gameObject.transform.position;
                    ArcaneSunStatus arcaneSunStatus;

                    // check valid targets
                    foreach (Player item in rayPlayerPairs.Keys)
                    {
                        linkFlag = true;

                        if (!ModdingUtils.Utils.PlayerStatus.PlayerAliveAndSimulated(item))
                        {
                            // either dead or reviving, unlink it
                            linkFlag = false;
                        }

                        // if (!item.gameObject.activeInHierarchy || item.data.healthHandler.isRespawning)
                        // {
                        //     // either dead or reviving, unlink it
                        //     linkFlag = false;
                        // }

                        distance = (item.gameObject.transform.position - transform.root.position).magnitude;
                        if (distance > effectRadius)
                        {
                            // ...out of range
                            linkFlag = false;
                        }

                        if (PlayerManager.instance.CanSeePlayer(sightPosition, item).canSee == false)
                        {
                            // ...not in line of sight
                            linkFlag = false;
                        }

                        if (linkFlag)
                        {
                            playerDistancePairs[item] = distance;
                        }
                        else
                        {
                            playerDistancePairs[item] = -1.0f;
                            lockedOnTargets.Remove(item);
                            rayPlayerPairs[item].SetActive(false);
                        }
                    }

                    // get and arrange target priority
                    visibleTargets.Clear();
                    foreach (Player item in playerDistancePairs.Keys)
                    {
                        if (playerDistancePairs[item] < 0.0f) continue;

                        int index;
                        for (index = 0; index < visibleTargets.Count; index++)
                        {
                            if (playerDistancePairs[visibleTargets[index]] > playerDistancePairs[item])
                            {
                                break;
                            }
                        }
                        visibleTargets.Insert(index, item);
                    }

                    // lock on targets
                    int maxTargets = stackCount;
                    for (int i = 0; i < visibleTargets.Count; i++)
                    {
                        if (lockedOnTargets.Count >= maxTargets) break;
                        if (lockedOnTargets.Contains(visibleTargets[i])) continue;
                        
                        lockedOnTargets.Add(visibleTargets[i]);
                    }

                    // apply debuff and deal damage
                    float rateMul = maxTargets / (float)lockedOnTargets.Count;
                    foreach (Player item in lockedOnTargets)
                    {
                        arcaneSunStatus = item.gameObject.GetOrAddComponent<ArcaneSunStatus>();
                        arcaneSunStatus.ApplyEffect(damageAmp, rampUpRate * rateMul * procTickTime, debuffRetainTime, debuffDecayRate);
                        
                        float damage = (damageFlat + (damagePercent * item.data.maxHealth)) * (arcaneSunStatus.GetEffectCharge() + preCharge) * procTickTime;

                        item.data.health -= damage * hpDrainFactor;
                        item.data.healthHandler.RPCA_SendTakeDamage(new Vector2(damage * damageFactor, 0.0f), item.transform.position, playerID: player.playerID);

                        // beam visual
                        float rayWidth = Mathf.Clamp(1.0f + (arcaneSunStatus.GetEffectCharge() / 5.0f), 1.0f, 4.0f);

                        rayPlayerPairs[item].SetActive(true);
                        rayPlayerPairs[item].GetComponent<RayVFXMono>().SetBeamWidth(rayWidth);
                    }
                    
                    procTimer -= procTickTime;
                }
            }

            // other visuals
            arcaneSunObject.transform.localScale = Vector3.one * Mathf.Pow(1.20f, stackCount);
            arcaneSunObject.transform.localPosition = new Vector3(0.0f, player.transform.localScale.x + arcaneSunYPos, 0.0f);
        }

        public void RefreshSpellStats()
        {
            stackCount = stats.GetGearData().arcaneSunStack;
            if (stackCount <= 0)
            {
                arcaneSunObject.SetActive(false);
                effectEnabled = false;
                return;
            }

            int glyphPotency = stats.GetGearData().glyphPotency;
            int glyphMagick = stats.GetGearData().glyphMagickFragment;
            int glyphTime = stats.GetGearData().glyphTime;
            int glyphDivination = stats.GetGearData().glyphDivination;
            int glyphInfluence = stats.GetGearData().glyphInfluence;

            damageAmp       = damageAmpBase + (damageAmpScaling * glyphPotency);
            damageFlat      = damageFlatBase + (damageFlatScaling * glyphPotency);
            damagePercent   = damagePercentBase + (damagePercentScaling * glyphPotency);

            rampUpRate          = rampUpRateBase + (rampUpRateScaling * glyphMagick);
            preCharge           = preChargeBase + (preChargeScaling * glyphMagick);

            debuffRetainTime    = debuffRetainTimeBase + (debuffRetainTimeScaling * glyphTime);
            debuffDecayRate     = debuffDecayRateBase + (debuffDecayRateScaling * glyphTime);
            debuffDecayRate     = Mathf.Max(0.5f, debuffDecayRate);

            effectRadius    = effectRadiusBase + (effectRadiusScaling * (glyphDivination + glyphInfluence));

            arcaneSunObject.SetActive(true);
            effectEnabled = true;
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            wasDeactivated = false;

            effectWarmup = true;
            effectEnabled = false;

            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            if (effectWarmup)
            {
                RefreshSpellStats();
            }
            effectWarmup = false;
            // effectEnabled = true;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            foreach (GameObject item in rayPlayerPairs.Values)
            {
                item.SetActive(false);
            }

            effectEnabled = false;
            effectWarmup = false;
        
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Point End");
        
            yield break;
        }

        private void OnRespawn()
        {
            procTimer = 0.0f;
            effectEnabled = true;
        }

        public void OnDisable()
        {
            foreach (GameObject item in rayPlayerPairs.Values)
            {
                item.SetActive(false);
            }

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

                wasDeactivated = true;
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
