using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace plweb.Agents
{
    public class AnnouncementsAgent
    {
        public List<Announce> GetAllAnnouncement()
        {
            var dataAgent = new MySqlDataAgent();
            return dataAgent.GetAllAnnouncement();
        }
    }
}
