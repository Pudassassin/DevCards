using System;
using UnityEngine;

using UnboundLib;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class CustomEmpowerShotModifier : RayHitEffect
    {
        private int empowerImpactCount = 1;
        private int empowerStackCount = 1;
        private float empowerBurstDelay = 0.1f;

        private Vector2 lastImpactPos;

        internal bool effectEnable = false;

        public void SetupEmpowerCharge(int impact, int stack, float burstDelay = 0.1f)
        {
            empowerImpactCount = impact;
            empowerStackCount = stack;
            empowerBurstDelay = burstDelay;
        }

        public void TempSetup()
        {
        // temp prototype
            ProjectileHit projectileHit = this.gameObject.GetComponentInParent<ProjectileHit>();
            Player shooterPlayer = projectileHit.ownPlayer;

            empowerImpactCount = shooterPlayer.data.stats.GetGearData().orbRollingBulwarkStack * 3;
            empowerStackCount = shooterPlayer.data.stats.GetGearData().orbRollingBulwarkStack;
        }

        public void Update()
        {
            if (effectEnable)
            {

            }
            else
            {
                MoveTransform moveTransform = GetComponentInParent<MoveTransform>();
                if (moveTransform != null)
                {
                    TempSetup();
                    effectEnable = true;
                }
            }
        }

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            if (empowerImpactCount <= 0)
            {
                return HasToReturn.canContinue;
            }

            lastImpactPos = hit.point;

            for (int i = 0; i < empowerStackCount; i++)
            {
                if (i == 0)
                {
                    GetComponentInParent<SpawnedAttack>().spawner.data.block.DoBlockAtPosition
                    (
                        firstBlock: true,
                        dontSetCD: true,
                        BlockTrigger.BlockTriggerType.Empower,
                        lastImpactPos - (Vector2)base.transform.forward * 0.05f,
                        onlyBlockEffects: true
                    );
                }
                else
                {
                    this.ExecuteAfterSeconds(empowerBurstDelay * (float)i, () =>
                    {
                        GetComponentInParent<SpawnedAttack>().spawner.data.block.DoBlockAtPosition
                        (
                            firstBlock: true,
                            dontSetCD: true,
                            BlockTrigger.BlockTriggerType.Empower,
                            lastImpactPos - (Vector2)base.transform.forward * 0.05f,
                            onlyBlockEffects: true
                        );
                    });
                }
            }

            empowerImpactCount--;
            return HasToReturn.canContinue;
        }
    }
}
