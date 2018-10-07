using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Convex.Net.Model.Services {
    public class ClientService {
        #region MEMBERS

        private Handshake Handshake { get; }
        private List<Guid> VerifiedClients { get; }

        #endregion

        public ClientService() {
            Handshake = new Handshake();
            VerifiedClients = new List<Guid> {new Guid("00000000-0000-0000-0000-000000000000") };
        }

        #region METHODS

        public bool IsClientVerified(Guid clientGuid) {
            return VerifiedClients.Contains(clientGuid);
        }

        #endregion
    }
}
