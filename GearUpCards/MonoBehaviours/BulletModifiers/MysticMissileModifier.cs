using System;
using System.Collections.Generic;
using UnityEngine;

using UnboundLib;
using ModdingUtils.Utils;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    public class MysticMissileModifier : RayHitEffect
    {
        private static float seekAngleBase = 120.0f;
        private static float seekAngleScaling = 30.0f;

        private static float turnSpeedBase = 240.0f;
        private static float turnSpeedScaling = 60.0f;

        private static float accelBase = 40.0f;
        private static float accelScaling = 10.0f;
        // default speed is 50
        private static float homingSpeedBase = 40.0f;
        private static float homingSpeedScaling = 10.0f;

        private static float homingRangeBase = 30.0f;
        private static float homingRangeScaling = 10.0f;

        private static float damageLossMulBase = 0.50f;
        private static float damageLossMulScaling = 0.05f;

        private static float damageMulMin = 0.10f;


        private static float procTime = 0.05f;

        private MoveTransform bulletMove;
        private RayHitReflect rayHitReflect;
        private ProjectileHit projectileHit;
        private Explosion explosion;

        private Player shooterPlayer;
        private Gun shooterGun;
        private CharacterStatModifiers shooterStats;

        // card stack counts: extra copies boost overall stats
        private int stackCount;
        // make it bounce more before losing power
        private int glyphGeometric;
        // area of effect!
        private int glyphInfluence;
        // adjust explosion/magic blast damage ratio, make explosion more forceful!
        private int glyphPotency;
        // make it home toward enemies
        private int glyphDivination;

        private int bounceCount = 0;

        // internals
        private List<Player> enemyPlayers = new List<Player>();

        private Player homingTarget = null;
        private bool seeEnemy = false;
        private float targetEnemyDistance;

        private float seekAngle = 0.0f;
        private float turnSpeed = 0.0f;
        private float acceleration = 0.0f;
        private float homingSpeed = 0.0f;
        private float homingRange = 0.0f;

        private float prevGravity;
        private float tickTimer = 0.0f;

        // temps
        Vector3 moveVelocity, vecToTarget;

        bool effectEnable = false;

        public void Update()
        {
            if (effectEnable)
            {
                if (tickTimer >= procTime)
                {
                    if (glyphDivination > 0)
                    {
                        moveVelocity = bulletMove.velocity;
                        moveVelocity.z = 0;

                        // home to current enemy
                        if (seeEnemy)
                        {
                            if (PlayerManager.instance.CanSeePlayer(transform.root.position, homingTarget).canSee)
                            {
                                vecToTarget = homingTarget.transform.position - transform.root.position;
                                vecToTarget.z = 0;

                                if (Vector3.Angle(moveVelocity, vecToTarget) <= seekAngle / 2.0f)
                                {
                                    // rotate bullet to target
                                    Vector3 newDirection = Vector3.RotateTowards(moveVelocity.normalized, vecToTarget.normalized, turnSpeed * Mathf.Deg2Rad * procTime, 0.0f);
                                    float speedDelta = acceleration * procTime;
                                    if (Mathf.Abs(moveVelocity.magnitude - homingSpeed) <= speedDelta)
                                    {
                                        bulletMove.velocity = newDirection * homingSpeed;
                                    }
                                    else if (moveVelocity.magnitude < homingSpeed)
                                    {
                                        bulletMove.velocity = (moveVelocity.magnitude + speedDelta) * newDirection;
                                    }
                                    else
                                    {
                                        bulletMove.velocity = (moveVelocity.magnitude - speedDelta) * newDirection;
                                    }
                                }
                                else
                                {
                                    bulletMove.gravity = prevGravity;
                                    seeEnemy = false;
                                    homingTarget = null;
                                }
                            }
                            else
                            {
                                bulletMove.gravity = prevGravity;
                                seeEnemy = false;
                                homingTarget = null;
                            }
                        }

                        // scan for new enemy
                        if (!seeEnemy)
                        {
                            foreach (Player enemy in enemyPlayers)
                            {
                                // skip dead enemy player
                                if (!PlayerStatus.PlayerAliveAndSimulated(enemy) && !enemy.data.healthHandler.isRespawning)
                                {
                                    continue;
                                }
                                // player in line of sight
                                else if (PlayerManager.instance.CanSeePlayer(transform.root.position, enemy).canSee)
                                {
                                    vecToTarget = enemy.transform.position - transform.root.position;
                                    vecToTarget.z = 0;

                                    // check against seek angle and range
                                    if (Vector3.Angle(moveVelocity, vecToTarget) <= seekAngle / 2.0f && vecToTarget.magnitude <= homingRange)
                                    {
                                        // set first valid target
                                        if (!seeEnemy)
                                        {
                                            prevGravity = bulletMove.gravity;
                                            bulletMove.gravity = 0.005f;

                                            homingTarget = enemy;
                                            seeEnemy = true;
                                            targetEnemyDistance = vecToTarget.magnitude;
                                        }
                                        // set closer enemy
                                        else if (vecToTarget.magnitude < targetEnemyDistance)
                                        {
                                            homingTarget = enemy;
                                            targetEnemyDistance = vecToTarget.magnitude;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    tickTimer -= procTime;
                }

                tickTimer += TimeHandler.deltaTime;
            }
            else
            {
                MoveTransform moveTransform = GetComponentInParent<MoveTransform>();
                if (moveTransform != null)
                {
                    try
                    {
                        Setup();
                    }
                    catch (Exception exception)
                    {
                        Miscs.LogWarn(exception);

                        this.enabled = false;
                        return;
                    }
                    effectEnable = true;
                }
            }
        }

        public void Setup()
        {
            Miscs.Log("[GearUpCard] Mystic Missle: Setup()");
            projectileHit = gameObject.GetComponentInParent<ProjectileHit>();
            bulletMove = gameObject.GetComponentInParent<MoveTransform>();

            // fetch player stats
            Miscs.Log("[GearUpCard] Mystic Missle: Setup() - fetch player stats");
            shooterPlayer = projectileHit.ownPlayer;
            shooterGun = projectileHit.ownWeapon.GetComponent<Gun>();
            shooterStats = shooterPlayer.gameObject.GetComponent<CharacterStatModifiers>();

            stackCount = shooterStats.GetGearData().mysticMissileStack;
            glyphDivination = shooterStats.GetGearData().glyphDivination;
            glyphGeometric = shooterStats.GetGearData().glyphGeometric;
            glyphInfluence = shooterStats.GetGearData().glyphInfluence;
            glyphPotency = shooterStats.GetGearData().glyphPotency;

            // angle in degrees
            seekAngle = seekAngleBase + (glyphDivination + stackCount - 1) * seekAngleScaling;
            turnSpeed = turnSpeedBase + (glyphDivination + stackCount - 1) * turnSpeedScaling;
            acceleration = accelBase + (glyphDivination + stackCount - 1) * accelScaling;
            homingSpeed = homingSpeedBase + glyphDivination * homingSpeedScaling;
            homingRange = homingRangeBase + (glyphDivination + (stackCount - 1) * 2) * homingSpeedScaling;

            // declare enemies
            Miscs.Log("[GearUpCard] Mystic Missle: Setup() - declare enemies");
            foreach (Player player in PlayerManager.instance.players)
            {
                if (player.teamID != shooterPlayer.teamID)
                {
                    enemyPlayers.Add(player);
                }
            }

            // setup RayHitReflect
            Miscs.Log("[GearUpCard] Mystic Missle: Setup() - setup RayHitReflect");
            rayHitReflect = transform.root.gameObject.GetComponent<RayHitReflect>();
            if (rayHitReflect == null)
            {
                rayHitReflect = transform.root.gameObject.AddComponent<RayHitReflect>();
                rayHitReflect.reflects = 1;
                rayHitReflect.speedM = 1.0f;
                rayHitReflect.dmgM = 1.0f;
                projectileHit.effects.Insert(0, rayHitReflect);
            }
            else
            {
                rayHitReflect.reflects += 1;
            }

            projectileHit.effects.Remove(this);
            projectileHit.effects.Insert(0, this);
        }

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            if (hit.transform == null)
            {
                // return HasToReturn.canContinue;
            }

            // hitting other bullet
            if (hit.transform.gameObject.tag.Contains("Bullet"))
            {
                // give bounce buffer
                rayHitReflect.reflects++;

                // return HasToReturn.hasToReturn;
            }

            // hitting map or player

            if (hit.transform.GetComponent<Player>())
            {
                // deal direct hit magic damage

                // give bounce buffer
                rayHitReflect.reflects++;
            }

            // deal area magic damage
            

            bounceCount++;

            if (rayHitReflect.reflects <= 1)
            {
                rayHitReflect.reflects = 0;
            }
            return HasToReturn.canContinue;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(this);
        }

    }
}
