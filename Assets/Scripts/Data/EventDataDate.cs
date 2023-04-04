using System;
using System.Collections.Generic;

namespace Data
{
    // TODO rework events to dates!
    public class EventDataDate
    {
        public List<EventData> eventList;
        public DateTime eventDate;

        // public Dictionary<long,>
        public Dictionary<long, List<EventData>> eventDictionary = new();

        public EventDataDate(List<EventData> data, DateTime time)
        {
            eventDate = time;
        }
    }
}