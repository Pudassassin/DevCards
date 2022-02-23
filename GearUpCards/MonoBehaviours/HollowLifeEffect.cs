using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.Networking;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

namespace GearUpCards.MonoBehaviours
{
    internal class HollowLifeEffect : MonoBehaviour
    {
        private const float healthCapFactor = .75f;

        private int stackCount = 0;
        float healthCapPercentage = 1.0f;

        private Player player;

        void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
        }

        void Update()
        {
            // Check whether the player's health is above the certain caps, then apply 'negative' healing to keep it below the line 
            if (!player.data.dead)
            {
                if (player.data.HealthPercentage > healthCapPercentage)
                {
                    float healthCut = player.data.maxHealth * (player.data.HealthPercentage - healthCapPercentage);
                    player.data.healthHandler.Heal(-healthCut);
                }
            }
        }

        public void OnDestroy()
        {

        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(this);
        }

        public void AddStack()
        {
            this.stackCount += 1;
            this.healthCapPercentage = Mathf.Pow(healthCapFactor, stackCount);
        }

        public void RemoveStack()
        {
            this.stackCount -= 1;
            this.healthCapPercentage = Mathf.Pow(healthCapFactor, stackCount);

            if (this.stackCount <= 0)
            {
                this.Destroy();
            }
        }

    }
}
