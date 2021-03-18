using System;
using System.Collections.Generic;

namespace Timkoto.UsersApi.Models
{
    public class LineUpRequest
    {
        public LineUpTeam LineUpTeam { get; set; }

        public List<PlayerLineUp> LineUp { get; set; }
    }

   public class LineUpTeam
    {
        public virtual long PlayerTeamId { get; set; }

        public virtual long OperatorId { get; set; }

        public virtual long AgentId { get; set; }

        public virtual long UserId { get; set; }
        
        public virtual long ContestId { get; set; }

        public virtual string TeamName { get; set; }
        
    }

    public class PlayerLineUp
    {
        public string Position { get; set; }

        public List<ContestPlayer> Players { get; set; }
    }

}
