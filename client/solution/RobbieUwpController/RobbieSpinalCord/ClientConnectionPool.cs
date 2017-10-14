using System.Collections.Generic;
using RobbieSpinalCord.Interfaces;

namespace RobbieSpinalCord
{
    /// <summary>
    /// Pool holding all client connections to the server.
    /// </summary>
    public class ClientConnectionPool
    {
        /// <summary>
        /// A list of clients stored by ID.
        /// </summary>
        private readonly IDictionary<string, IClient> listClients;

        /// <summary>
        /// Constructs a new client connection pool.
        /// </summary>
        public ClientConnectionPool()
        {
            listClients = new Dictionary<string, IClient>();
        }

        // todo: do we need a timeout to throw away connections that are not used anymore / any longer for a certain period of time?

        /// <summary>
        /// Gets the corresponding client for the given person ID.
        /// Creates a new connection if no client exists for the given ID.
        /// </summary>
        /// <param name="personId">The person ID to get the client for.</param>
        /// <returns>The client object from the connection pool for the given person.</returns>
        public IClient GetClient(string personId)
        {
            IClient client;
            var clientExists = listClients.ContainsKey(personId);
            if (!clientExists)
            {
                client = new SitecoreClient(personId);
                listClients.Add(personId, client);
            }
            client = listClients[personId];
            return client;
        }

        /// <summary>
        /// Changes the ID of the current session.
        /// </summary>
        /// <param name="currentId">The current ID of the client object you want to change.</param>
        /// <param name="newId">The new ID to set the client to.</param>
        /// <returns></returns>
        public IClient ChangeId(string currentId, string newId)
        {
            if (!listClients.ContainsKey(currentId)) return null;
            
            var client = listClients[currentId];
            client.PersonId = newId;
            listClients.Remove(currentId);
            listClients.Add(newId, client);

            return client;
        }

    }
}
