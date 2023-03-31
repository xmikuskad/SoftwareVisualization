using System;

namespace Data
{
    public class EventData
    {
        public long projectId;
        public long personId;
        public DateTime when;
        public long verticeId;
        public EventActionType actionType;
        
        //For debug
        public long edgeId;


        public EventData(long projectId, long personId, DateTime when, long verticeId, EventActionType actionType, long edgeId)
        {
            this.projectId = projectId;
            this.personId = personId;
            this.when = when;
            this.verticeId = verticeId;
            this.actionType = actionType;
            this.edgeId = edgeId;
        }
    }
}