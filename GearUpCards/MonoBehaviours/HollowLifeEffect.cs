using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.Networking;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;

namespace GearUpCards.MonoBehaviours
{
    internal class HollowLifeEffect : ReversibleEffect
    {
        private const float healthCapFactor = .70f;

        private const float healthCullRate = .05f;
        private const float procTime = .10f;

        private int stackCount = 0;
        float healthCapPercentage = 1.0f;

        float timer = 0.0f;

        private Player player;
        private CharacterStatModifiers stats;

        override public void OnAwake()
        {
            this.player = this.gameObject.GetComponent<Player>();
        }

        override public void OnUpdate()
        {
            timer += Time.deltaTime;

            if (timer > procTime)
            {
                // Check whether the player's health is above the certain caps, then adjust it accordingly
                // [?] EXC's [Second Wind] implement it differently and seem to either look for health removal or it tries its best to heal up to said health point
                // [!!] HDC's [Holy Light] directly compare current health with previous health each Update() calls and 'charge' on positive gains
                //      leading to it having massive charge at round starts, and will accumulate on each of ebb and flow of health now incurred by [Hollow Life]
                stackCount = base.characterStatModifiers.GetGearData().hollowLifeStack;
                RecalculateHealthCap();

                if (!player.data.dead && !player.data.healthHandler.isRespawning && stackCount > 0)
                {
                    if (player.data.HealthPercentage > .85f)
                    {
                        player.data.health = player.data.maxHealth * .85f;
                    }
                    else if (player.data.HealthPercentage > healthCapPercentage)
                    {
                        float healthCullPercentage = Mathf.Clamp(player.data.HealthPercentage - healthCapPercentage, 0.0f, healthCullRate);
                        player.data.health -= player.data.maxHealth * healthCullPercentage;

                    }
                }

                timer -= procTime;
            }
        }

        override public void OnOnDestroy()
        {

        }

        public void RecalculateHealthCap()
        {
            this.healthCapPercentage = Mathf.Pow(healthCapFactor, stackCount);
        }

    }
}
