using RobbieVision.Models;
using System.Collections.Generic;

namespace RobbieVision.IO
{
    /// <summary>
    /// Event specific implementation of the JsonStorage class.
    /// </summary>
    public class EventStorage
    {
        /// <summary>
        /// The maximum amount of events to keep in storage, the oldest event will be dropped when this threshold is reached.
        /// </summary>
        private const int MaxRecords = 100;

        /// <summary>
        /// Adds a SenseEvent object to the local JSON event storage.
        /// </summary>
        /// <param name="senseEvent">The SenseEvent object to store.</param>
        /// <remarks>Mind that only a certain amount of events will be persisted, configured via the private MaxRecords constant in this class.</remarks>
        public static void Add(SenseEvent senseEvent)
        {
            var storage = new JsonStorage($@"DataStorage\{senseEvent.SessionId}.json");
            var events = storage.Deserialize<List<SenseEvent>>() ?? new List<SenseEvent>();

            events.Add(senseEvent);
            if (events.Count > MaxRecords)
            {
                events.RemoveAt(0);
            }

            storage.Serialize(events);
        }
    }
}