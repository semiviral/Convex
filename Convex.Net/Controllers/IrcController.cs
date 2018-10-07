using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Convex.IRC.Model;
using Convex.Net.Model;
using Convex.Net.Model.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Rest.TransientFaultHandling;

namespace Convex.Net.Controllers {
    [Route("api/[controller]")]
    public class IrcController : Controller {
        #region MEMBERS

        private ClientService InterClientService { get; }
        private IrcService IrcClientReference { get; }

        #endregion

        public IrcController(ClientService clientService, IrcService service) {
            IrcClientReference = service;
            InterClientService = clientService;
        }

        //GET api/irc
        [HttpGet]
        public List<ServerMessage> Get(string guid, double unixTimestamp) {
            if (!Guid.TryParse(guid, out Guid clientGuid))
                throw new HttpRequestException("Guid must be in proper format.");

            if (!InterClientService.IsClientVerified(clientGuid))
                throw new HttpRequestException("Client must be verified.");

            DateTime localTime = new DateTime(1970, 01, 01, 00, 00, 00, 00, DateTimeKind.Utc);
            localTime = localTime.AddSeconds(unixTimestamp).ToLocalTime();

            // DateTime.MinValue == 1/1/0001 12:00:00 AM (YYYYMMDDhhmmss 1970,01,01,00,00,00,00)
            return IrcClientReference.GetMessagesByDateTimeOrDefault(localTime, DateTimeOrdinal.After).ToList();
        }
    }
}
