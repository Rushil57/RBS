using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace plweb.Internals
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            string AgentId = string.Empty;
            var context = connection.GetHttpContext();
            if (context != null)
            {
                if (context.Request != null && context.Request.Cookies != null)
                {
                    AgentId = context.Request.Cookies["agentId"];
                }
            }
            return AgentId;
            // return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
