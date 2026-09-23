using Unity.Netcode;
using UnityEngine;

namespace PartyNight.Networking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class PartyNightNetworkPlayer : NetworkBehaviour
    {
        public bool IsLocallyOwnedPlayer => IsSpawned && IsOwner;
        public bool IsServerAuthoritativePlayer =>
            IsSpawned && NetworkManager != null && NetworkManager.IsServer;
    }
}
