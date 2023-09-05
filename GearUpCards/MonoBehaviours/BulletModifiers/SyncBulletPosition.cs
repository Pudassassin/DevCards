using Photon.Pun;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
    internal class SyncBulletPosition : MonoBehaviour
    {
        public static float defaultSyncInterval = 0.05f;
        public static string RPCKey = GearUpCards.ModId + ":PosSync";
        public static string RPCKey2 = GearUpCards.ModId + ":VeloSync";

        public MoveTransform moveTransform;
        public float interval;
        public float lastSent;
        public PhotonView view;

        public bool enableIntervalSync = true;

        private void Awake()
        {
            this.moveTransform = GetComponentInParent<MoveTransform>();
            this.interval = defaultSyncInterval;
            this.view = GetComponentInParent<PhotonView>();
            GetComponentInParent<ChildRPC>().childRPCsVector2.Add(RPCKey, SyncPosition);
            GetComponentInParent<ChildRPC>().childRPCsVector2.Add(RPCKey2, SyncVelocity);
        }

        private void Update()
        {
            if (enableIntervalSync)
            {
                CallSyncs();
                // if (view != null && (view.IsMine) && Time.time > (this.lastSent + this.interval))
                // {
                //     GetComponentInParent<ChildRPC>().CallFunction(RPCKey, (Vector2)this.transform.root.position);
                //     this.lastSent = Time.time;
                // }
            }
        }

        public void CallSyncs()
        {
            if (view != null && (view.IsMine) && Time.time > (this.lastSent + this.interval))
            {
                GetComponentInParent<ChildRPC>().CallFunction(RPCKey, (Vector2)this.transform.root.position);
                GetComponentInParent<ChildRPC>().CallFunction(RPCKey2, (Vector2)this.moveTransform.velocity);
                this.lastSent = Time.time;
            }
        }

        public void SyncPosition(Vector2 pos)
        {
            this.transform.root.position = pos;
        }
        public void SyncVelocity(Vector2 velocity)
        {
            this.moveTransform.velocity = velocity;
        }

        private void OnDestroy()
        {
            GetComponentInParent<ChildRPC>()?.childRPCsVector2.Remove(RPCKey);
            GetComponentInParent<ChildRPC>()?.childRPCsVector2.Remove(RPCKey2);
        }
    }
}
