using System;
using System.Collections.Immutable;
using Convex.Net.Model;
using Convex.Net.Model.Services;
using Microsoft.AspNetCore.Mvc;

namespace Convex.Net.Controllers {
    [Route("api/[controller]")]
    public class ClientController {
        #region MEMBERS
        
        private ClientService InterClientService { get; }

    #endregion

        public ClientController(ClientService clientService) {
            InterClientService = clientService;
        }

        [HttpGet]
        public bool Get() {
            return true;
        }

        [HttpPost]
        public void Post(byte[] externalKey) {
            // VerifiedClients.Add(Guid.Parse(guid));
        }
    }
}
