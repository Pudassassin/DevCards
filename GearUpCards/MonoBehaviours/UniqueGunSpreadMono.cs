using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;

using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;
using HarmonyLib;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class UniqueGunSpreadMono : MonoBehaviour
    {
        private const float procTime = 0.1f;
        public static int flakProjectileAdd = 1;

        internal Player player;
        internal Gun gun;
        internal CharacterStatModifiers stats;

        internal float prevSpread;
        internal float prevSpreadMul;
        internal float prevEvenSpread;
        internal float prevGravity;

        internal float timer = 0.0f;

        internal bool effectApplied = false;
        internal bool wasDeactivated = false;

        internal Action<BlockTrigger.BlockTriggerType> blockAction;
        internal Action attackAction;
        internal Action<GameObject> shootAction;

        private GameObject oldGunObject, newGunObject, dummyGunObject;
        private GameObject gameObjectToAdd;
        private Gun playerOldGun, newSpreadGun;
        public Gun dummySpreadGun = null;
        internal bool isGunReplaced = false;
        internal GearUpConstants.ModType lastGunSpreadMod = GearUpConstants.ModType.none;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.gun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            oldGunObject = new GameObject("UniqueGunSpreadHolder_oldGun");
            oldGunObject.transform.parent = player.transform;
            oldGunObject.transform.localPosition = Vector3.zero;

            newGunObject = new GameObject("UniqueGunSpreadHolder_newGun");
            newGunObject.transform.parent = player.transform;
            newGunObject.transform.localPosition = Vector3.zero;

            dummyGunObject = new GameObject("UniqueGunSpreadHolder_dummyGun");
            dummyGunObject.transform.parent = player.transform;
            dummyGunObject.transform.localPosition = Vector3.zero;

            SetupDummyGun();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookRoundStart, OnRoundStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
            // this.playerOldGun = new Gun();

            this.attackAction = new Action(this.GetDoAttackAction(this.player, this.gun));
            // this.gun.AddAttackAction(this.attackAction);

            this.shootAction = new Action<GameObject>(this.GetDoShootAction(this.player, this.gun));
            // this.gun.ShootPojectileAction = (Action<GameObject>)Delegate.Combine(this.gun.ShootPojectileAction, this.shootAction);
        }

        public void Update()
        {
            // Respawn case
            if (wasDeactivated)
            {
                ApplyModifier();
            
                wasDeactivated = false;
                effectApplied = true;
            }

        }

        private IEnumerator OnRoundStart(IGameModeHandler gm)
        {
            prevSpread = gun.spread;
            prevEvenSpread = gun.evenSpread;
            prevSpreadMul = gun.multiplySpread;
            prevGravity = gun.gravity;

            SetupDummyGun();

            // if (stats.GetGearData().gunSpreadMod == GearUpConstants.ModType.gunSpreadFlak)
            // {
            //     Miscs.CopyGunStats(gun, playerOldGun);
            // }

            yield break;
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            ApplyModifier();

            wasDeactivated = false;
            effectApplied = true;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            RemoveModifier();

            effectApplied = false;
            wasDeactivated = false;

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
                RemoveModifier();

                wasDeactivated = true;
                effectApplied = false;
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }

        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
            GameModeManager.RemoveHook(GameModeHooks.HookRoundStart, OnRoundStart);

            this.gun.ShootPojectileAction = (Action<GameObject>)Delegate.Remove(this.gun.ShootPojectileAction, this.shootAction);
        }

        public Action GetDoAttackAction(Player player, Gun gun)
        {
            // trigger on firing one burst of bullets...?
            return delegate ()
            {
                try
                {

                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogError($"[UniqueGunSpreadMono] AttackAction failed! [{player.playerID}]");
                    UnityEngine.Debug.LogWarning(exception);
                }
            };
        }

        public Action<GameObject> GetDoShootAction(Player player, Gun gun)
        {
            // triggers on firing EACH bullet
            return delegate (GameObject bulletFired)
            {
                this.ExecuteAfterFrames(1, () =>
                {
                    try
                    {
                        switch (stats.GetGearData().gunSpreadMod)
                        {
                            case GearUpConstants.ModType.gunSpreadFlak:
                                bulletFired.GetOrAddComponent<FlakShellModifier>();
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        UnityEngine.Debug.LogError($"[UniqueGunSpreadMono] ShootAction failed! [{player.playerID}]");
                        UnityEngine.Debug.LogWarning(exception);
                    }
                });
            };
        }

        private void SetupDummyGun()
        {
            // full backup of player's original gun
            playerOldGun = oldGunObject.GetOrAddComponent<Gun>();
            Miscs.CopyGunStats(gun, playerOldGun);

            // prepare player's replacement gun
            newSpreadGun = newGunObject.GetOrAddComponent<Gun>();
            dummySpreadGun = dummyGunObject.GetOrAddComponent<Gun>();
        }

        private void ResetEffect()
        {
            if (ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(CardUtils.GearCategory.typeUniqueGunSpread))
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Remove(CardUtils.GearCategory.typeUniqueGunSpread);
            }

            if (isGunReplaced)
            {
                Miscs.CopyGunStats(playerOldGun, this.gun);
                isGunReplaced = false;
            }

            gun.ShootPojectileAction = (Action<GameObject>)Delegate.RemoveAll(gun.ShootPojectileAction, shootAction);
        }

        private void RemoveModifier()
        {
            Miscs.Log("[GearUP] UniqueGunSpread: called remove");

            if (effectApplied)
            {
                if (isGunReplaced)
                {
                    Miscs.CopyGunStats(playerOldGun, gun);
                    isGunReplaced = false;
                }

                gun.spread = prevSpread;
                gun.evenSpread = prevEvenSpread;
                gun.multiplySpread = prevSpreadMul;
                gun.gravity = prevGravity;

                gun.ShootPojectileAction = (Action<GameObject>)Delegate.RemoveAll(gun.ShootPojectileAction, shootAction);

                Miscs.Log("[GearUP] UniqueGunSpread: removed");
            }
        }

        private void ApplyModifier()
        {
            Miscs.Log("[GearUP] UniqueGunSpread: called apply");

            if (!effectApplied)
            {
                // Miscs.CopyGunStats(gun, playerOldGun);


                switch (stats.GetGearData().gunSpreadMod)
                {
                    case GearUpConstants.ModType.gunSpreadArc:
                        ApplySpreadArc();
                        break;
                    case GearUpConstants.ModType.gunSpreadLine:
                        break;
                    case GearUpConstants.ModType.gunSpreadParallel:
                        ApplySpreadParallel();
                        break;
                    case GearUpConstants.ModType.gunSpreadFlak:
                        ApplySpreadFlak();
                        break;
                    case GearUpConstants.ModType.none:
                        ResetEffect();
                        break;
                    default:
                        break;
                }

                Miscs.Log("[GearUP] UniqueGunSpread: applied");
            }
        }

        private void ApplySpreadArc()
        {
            if (prevSpread <= 0.0f)
            {
                gun.spread = 0.0f;
                gun.evenSpread = prevSpread;
                gun.multiplySpread = prevSpreadMul;
            }
            else
            {
                gun.spread = 0.01f;

                if (prevEvenSpread < 1.0f)
                {
                    gun.evenSpread = 1.0f;
                }
                else
                {
                    gun.evenSpread = prevEvenSpread;
                }

                gun.multiplySpread = prevSpreadMul * prevSpread * 135; // uas 150
            }
        }

        private void ApplySpreadParallel()
        {
            if (prevSpread <= 0.0f)
            {
                gun.spread = 0.0f;
                gun.evenSpread = prevSpread;
                gun.multiplySpread = prevSpreadMul;
            }
            else
            {
                gun.spread = 0.01f;

                if (prevEvenSpread < 1.0f)
                {
                    gun.evenSpread = 1.0f;
                }
                else
                {
                    gun.evenSpread = prevEvenSpread;
                }

                // gun.multiplySpread = prevSpreadMul * prevSpread * (50 + gun.numberOfProjectiles * 5);
                gun.multiplySpread = prevSpreadMul * prevSpread * (50 + gun.numberOfProjectiles * 3);
            }
        }

        private void ApplySpreadFlak()
        {
            // flak cannon stats
            Miscs.Log("ApplySpreadFlak()");
            Miscs.CopyGunStats(playerOldGun, newSpreadGun);

            newSpreadGun.bursts = 1 + Mathf.RoundToInt((float)playerOldGun.bursts / 2.0f);
            newSpreadGun.timeBetweenBullets = 0.10f + Mathf.Clamp(playerOldGun.timeBetweenBullets * 1.50f, 0.0f, 0.65f);

            newSpreadGun.numberOfProjectiles = 1 + Mathf.RoundToInt((float)playerOldGun.numberOfProjectiles / 10.0f);

            newSpreadGun.damage = playerOldGun.damage * 2.5f;
            newSpreadGun.damageAfterDistanceMultiplier = 1.0f;
            newSpreadGun.dmgMOnBounce = 1.0f;
            newSpreadGun.percentageDamage = 0.0f;

            newSpreadGun.projectileSpeed = Mathf.Clamp(playerOldGun.projectileSpeed * 0.25f, 1.5f, 10.0f);
            newSpreadGun.projectielSimulatonSpeed = Mathf.Clamp(playerOldGun.projectielSimulatonSpeed, 0.25f, 10.0f);
            newSpreadGun.drag = 0.0f;
            newSpreadGun.dragMinSpeed = 1.0f;

            newSpreadGun.reflects = 24;

            newSpreadGun.attackSpeed = 0.20f + playerOldGun.attackSpeed;
            newSpreadGun.attackSpeedMultiplier = 0.5f + (playerOldGun.attackSpeedMultiplier - 0.5f) * 1.25f;

            newSpreadGun.knockback = playerOldGun.knockback * 1.5f;
            newSpreadGun.recoil = playerOldGun.recoil * 0.05f;

            // fragmentation stats (to be fired 4 times)
            Miscs.Log("ApplySpreadFlak() : dummySpreadGun");
            Miscs.CopyGunStatsNoActions(playerOldGun, dummySpreadGun);

            // dummySpreadGun.holdable = null;
            // dummySpreadGun.player = null;

            dummySpreadGun.bursts = 1 + Mathf.RoundToInt((float)playerOldGun.bursts / 4.0f);
            dummySpreadGun.timeBetweenBullets = 0.15f;

            dummySpreadGun.numberOfProjectiles = flakProjectileAdd + Mathf.RoundToInt((float)playerOldGun.numberOfProjectiles / 4.0f);

            dummySpreadGun.damage = playerOldGun.damage * 0.5f;

            dummySpreadGun.projectileSpeed = Mathf.Clamp(playerOldGun.projectileSpeed, 0.5f, 25.0f);
            dummySpreadGun.projectielSimulatonSpeed = Mathf.Clamp(playerOldGun.projectielSimulatonSpeed, 0.20f, 10.0f);

            dummySpreadGun.evenSpread = 0.0f;
            dummySpreadGun.spread = 1.0f;
            dummySpreadGun.multiplySpread = 1.0f;

            // List<ObjectsToSpawn> objectsToSpawns = dummySpreadGun.objectsToSpawn.ToList();
            // GameObject gameObject = new GameObject("FlakNoRecursion", new Type[]
            // {
            //     typeof(NoFlakRecursion)
            // });
            // objectsToSpawns.Add(new ObjectsToSpawn
            // {
            //     AddToProjectile = gameObject
            // });
            // dummySpreadGun.objectsToSpawn = objectsToSpawns.ToArray();

            // Miscs.Log("ApplySpreadFlak() : clear up dummySpreadGun Flak");
            // for (int i = 0; i < dummySpreadGun.objectsToSpawn.Length; i++)
            // {
            //     if (dummySpreadGun.objectsToSpawn[i].AddToProjectile != null)
            //     {
            //         if (dummySpreadGun.objectsToSpawn[i].AddToProjectile.name.Equals("FlakCannonModifier"))
            //         {
            //             Miscs.Log("ApplySpreadFlak() : found it");
            //             Destroy(dummySpreadGun.objectsToSpawn[i].AddToProjectile.GetComponent<FlakShellModifier>());
            //         }
            //     }
            // }

            // Action doNothing = () => { };
            // Traverse.Create(dummySpreadGun).Field("attackAction").SetValue((Action) doNothing);
            // dummySpreadGun.ShootPojectileAction = new Action<GameObject>((_) => { });

            Miscs.Log("ApplySpreadFlak() : replacing gun");
            Miscs.CopyGunStats(newSpreadGun, gun);
            isGunReplaced = true;

            Miscs.Log("ApplySpreadFlak() : combine delegate");
            this.gun.ShootPojectileAction = (Action<GameObject>)Delegate.Combine(this.gun.ShootPojectileAction, this.shootAction);
        }
    }

    class BulletNoClipModifier : MonoBehaviour
    {
        private const float tickTime = 0.1f;

        private float delayTime = 1.5f;
        private bool effectEnabled = false;
        private bool persistentOverride = false;

        internal bool previousState = true;
        internal float timer = 0.0f;

        public void Awake()
        {
            if (transform.parent != null)
            {
                previousState = transform.parent.Find("Collider").gameObject.activeSelf;
                transform.parent.Find("Collider").gameObject.SetActive(false);
                effectEnabled = true;
            }
        }

        public void Update()
        {
            if (effectEnabled)
            {
                timer += TimeHandler.deltaTime;

                if (!persistentOverride)
                {
                    if (timer >= delayTime)
                    {
                        RemoveModifier();
                    }
                }
                else
                {
                    // Persistent override the no bullet-bullet collision
                    if (timer >= tickTime)
                    {
                        if (transform.parent.Find("Collider").gameObject.activeSelf == false)
                        {
                            transform.parent.Find("Collider").gameObject.SetActive(false);
                        }

                        timer -= tickTime;
                    }
                }
            }
        }

        public void SetDuration(float duration)
        {
            delayTime = duration;
        }

        public void SetPersistentOverride(bool input)
        {
            persistentOverride = input;
        }

        public void RemoveModifier()
        {
            transform.parent.Find("Collider").gameObject.SetActive(previousState);
            Destroy(this);
        }

    }

    class ParallelBulletModifier : RayHitEffect
    {
        private float delayTime = 0.1f;
        private float initSpeed = 55.0f;

        internal float timer = 0.0f;

        private float prevGravity, prevSpeed, prevSimSpeed, prevDrag;
        private Vector3 targetDirection;

        private MoveTransform bulletMove;
        private ProjectileHit bulletHit;
        private Gun shooterGun;

        private bool effectEnabled = false;


        public void Start()
        {
            if (transform.parent != null)
            {
                try
                {
                    // Miscs.Log("[GearUp] ParallelBulletModifier: Update 0");
                    bulletMove = gameObject.GetComponentInParent<MoveTransform>();

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Update 1");
                    bulletHit = gameObject.GetComponentInParent<ProjectileHit>();

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Update 2");
                    shooterGun = bulletHit.ownWeapon.GetComponent<Gun>();

                    prevGravity = bulletMove.gravity;
                    bulletMove.gravity = 0.0f;

                    prevSpeed = bulletMove.velocity.magnitude;
                    bulletMove.velocity = bulletMove.velocity.normalized * initSpeed;

                    prevDrag = bulletMove.drag;
                    bulletMove.drag = 0.0f;

                    prevSimSpeed = Traverse.Create(bulletMove).Field("simulationSpeed").GetValue<float>();
                    Traverse.Create(bulletMove).Field("simulationSpeed").SetValue((float)1.0f);

                    targetDirection = shooterGun.shootPosition.forward;

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Start");
                }
                catch (Exception exception)
                {
                    Miscs.LogWarn("[GearUP] ParallelBulletModifier: caught an exception (I swear this only happen once when it is first added to gun.objectToSpawn)");
                    Miscs.LogWarn(exception);
                    effectEnabled = false;
                }

                effectEnabled = true;
            }
        }

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            // in case bullet hit anything before going parallel, immediately restore the stats and destroy this
            timer += delayTime;

            return HasToReturn.canContinue;
        }

        public void Update()
        {
            if (effectEnabled)
            {
                timer += TimeHandler.deltaTime;

                if (timer >= delayTime)
                {
                    // plan B: delay alignment
                    bulletMove.gravity = prevGravity;
                    bulletMove.velocity = targetDirection * prevSpeed;
                    bulletMove.drag = prevDrag;
                    Traverse.Create(bulletMove).Field("simulationSpeed").SetValue((float) prevSimSpeed);

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Updated and cleared");
                    Destroy(this);
                }
            }
        }
    }

    class FlakShellModifier : RayHitEffect
    {
        public static float defaultDelayTime = 0.75f;
        private float timer = 0.0f;
         
        private Player shooterPlayer;
        private UniqueGunSpreadMono gunSpreadMono;
        private Gun dummyGun;

        public bool effectEnable = false;
        public bool checkFlakShell = false;

        private Vector3 playerPrevPos;
        private float delayTime = defaultDelayTime;

        public void Start()
        {
            delayTime = defaultDelayTime;
        }

        public void Setup()
        {
            delayTime = defaultDelayTime;
            shooterPlayer = gameObject.GetComponentInParent<ProjectileHit>().ownPlayer;
            gunSpreadMono = shooterPlayer.gameObject.GetComponent<UniqueGunSpreadMono>();
            dummyGun = gunSpreadMono.dummySpreadGun;
            // shrapnelDummyGun = gameObject.AddComponent<Gun>();
            // Miscs.CopyGunStats(shooterPlayer.gameObject.GetComponent<UniqueGunSpreadMono>().dummySpreadGun, shrapnelDummyGun);

            // effectEnable = true;
        }

        public void Update()
        {
            if (effectEnable)
            {
                // if (!checkFlakShell)
                // {
                //     bool isFlakShell = true;
                //     NoFlakRecursion[] stopRecursionFlag = gameObject.GetComponentsInChildren<NoFlakRecursion>();
                //     foreach (NoFlakRecursion item in stopRecursionFlag)
                //     {
                //         if (item.gameObject.name == "FlakNoRecursion")
                //         {
                //             isFlakShell = false;
                //             break;
                //         }
                //     }
                // 
                //     if (isFlakShell)
                //     {
                //         checkFlakShell = true;
                //     }
                //     else
                //     {
                //         Destroy(this);
                //     }
                // }

                timer += TimeHandler.deltaTime;
                if (timer >= delayTime)
                {
                    // explode after timer
                    FlakExplode();
                }
            }
            else
            {
                MoveTransform moveTransform = GetComponentInParent<MoveTransform>();
                if (moveTransform != null)
                {
                    Setup();
                    effectEnable = true;
                }
            }
        }

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            if (hit.transform != null)
            {
                Miscs.Log("[GearUp] FlakShellModifier hit: " + hit.transform.gameObject.name);
            }

            if (hit.transform.GetComponent<Player>())
            {
                // explode immediately
                Miscs.Log("> Direct Hit!");
                FlakExplode();
            }
            if (hit.transform == null)
            {
                return HasToReturn.canContinue;
            }
            if (hit.transform.gameObject.tag.Contains("Bullet"))
            {
                return HasToReturn.canContinue;
            }

            return HasToReturn.canContinue;
        }

        public void FlakExplode()
        {
            if (dummyGun != null)
            {
                // playerPrevPos = shooterPlayer.transform.position;
                // shooterPlayer.transform.position = transform.position;
                dummyGun.gameObject.transform.position = transform.position;

                Traverse.Create(dummyGun).Field("forceShootDir").SetValue((Vector3) new Vector3(1.0f, 0.0f, 0.0f));
                dummyGun.Attack(0.0f, true, useAmmo: false);

                Traverse.Create(dummyGun).Field("forceShootDir").SetValue((Vector3)new Vector3(-1.0f, 0.0f, 0.0f));
                dummyGun.Attack(0.0f, true, useAmmo: false);

                Traverse.Create(dummyGun).Field("forceShootDir").SetValue((Vector3)new Vector3(0.0f, 1.0f, 0.0f));
                dummyGun.Attack(0.0f, true, useAmmo: false);

                Traverse.Create(dummyGun).Field("forceShootDir").SetValue((Vector3)new Vector3(0.0f, -1.0f, 0.0f));
                dummyGun.Attack(0.0f, true, useAmmo: false);

                // shooterPlayer.transform.position = playerPrevPos;
            }
            else
            {
                Miscs.LogWarn("[GearUp] FlakShellModifier: Dummy Gun is NULL!");
            }

            this.ExecuteAfterFrames(1, () =>
            {
                // Miscs.Log("Boom!");
                Destroy(gameObject);
            });
        }
    }

    class BulletSpeedLimiter : MonoBehaviour
    {
        public const float defaultMinVelocity = 2.5f;
        public const float defaultMaxVelocity = 1000.0f;
        public const float defaultMinSimSpeed = 0.05f;
        public const float defaultMaxSimSpeed = 10.0f;

        private float minVelocity, maxVelocity, minSimSpeed, maxSimSpeed;
        private MoveTransform moveTransform = null;

        internal Vector3 directionNorm;
        internal float speed, simSpeed;

        public void Start()
        {
            Setup();
        }

        public void Update()
        {
            if (moveTransform == null)
            {
                moveTransform = GetComponentInParent<MoveTransform>();
            }
            else
            {
                speed = moveTransform.velocity.magnitude;
                directionNorm = moveTransform.velocity;
                directionNorm.z = 0.0f;
                directionNorm = directionNorm.normalized;

                speed = Mathf.Clamp(speed, minVelocity, maxVelocity);
                moveTransform.velocity = speed * directionNorm;

                // moveTransform.dragMinSpeed = minVelocity;

                simSpeed = Traverse.Create(moveTransform).Field("simulationSpeed").GetValue<float>();
                simSpeed = Mathf.Clamp(simSpeed, minSimSpeed, maxSimSpeed);
                Traverse.Create(moveTransform).Field("simulationSpeed").SetValue((float)simSpeed);
            }
        }

        public void Setup(float inMinVelo = defaultMinVelocity, float inMaxVelo = defaultMaxVelocity, float inMinSimSpeed = defaultMinSimSpeed, float inMaxSimSpeed = defaultMaxSimSpeed)
        {
            minVelocity = inMinVelo;
            maxVelocity = inMaxVelo;
            minSimSpeed = inMinSimSpeed;
            maxSimSpeed = inMaxSimSpeed;
        }
    }

    // class NoFlakRecursion : MonoBehaviour
    // {
    //     public void Start()
    //     {
    //         ClearMono();
    //     }
    // 
    //     public void Awake()
    //     {
    //         ClearMono();
    //     }
    // 
    //     public void OnEnable()
    //     {
    //         ClearMono();
    //     }
    // 
    //     public void ClearMono()
    //     {
    //         FlakShellModifier[] mono = gameObject.GetComponentsInChildren<FlakShellModifier>();
    //         for (int i = mono.Length - 1; i >= 0; i--)
    //         {
    //             Destroy(mono[i]);
    //         }
    // 
    //         mono = gameObject.GetComponentsInParent<FlakShellModifier>();
    //         for (int i = mono.Length - 1; i >= 0; i--)
    //         {
    //             Destroy(mono[i]);
    //         }
    //     }
    // }
}
