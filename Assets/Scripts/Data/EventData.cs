using System;
using System.Collections.Generic;

namespace Data
{
    public class EventData
    {
        public string uuid;
        public long projectId;
        public List<long> personIds;
        public DateTime when;
        public long verticeId;
        public EventActionType actionType;
        public float completedActions = 1;
        
        //For debug
        public long edgeId;


        public EventData(long projectId, List<long> personIds, DateTime when, long verticeId, EventActionType actionType, long edgeId)
        {
            this.projectId = projectId;
            this.personIds = personIds;
            this.when = when;
            this.verticeId = verticeId;
            this.actionType = actionType;
            this.edgeId = edgeId;
        }

        public void IncreaseCount()
        {
            this.completedActions++;
        }
    }
}