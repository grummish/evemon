using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVEMon.Common.Collections;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Serialization.API;
using EVEMon.Common.Serialization.Settings;
using EVEMon.Common.Threading;

namespace EVEMon.Common
{
    public class GlobalAPIKeyCollection : ReadonlyKeyedCollection<long, APIKey>
    {

        #region Indexer

        /// <summary>
        /// Gets the API key with the provided id, null when not found
        /// </summary>
        /// <param name="id">The id to look for</param>
        /// <returns>The searched API key when found; null otherwise.</returns>
        public APIKey this[long id]
        {
            get { return Items.Values.FirstOrDefault(apiKey => apiKey.ID == id); }
        }

        #endregion


        #region Addition / Removal Methods

        /// <summary>
        /// Removes the provided API key from this collection.
        /// </summary>
        /// <param name="apiKey">The API key to remove</param>
        /// <exception cref="InvalidOperationException">The API key does not exist in the list.</exception>
        public void Remove(APIKey apiKey)
        {
            // Clears the API key on the owned identities
            foreach (CharacterIdentity identity in apiKey.CharacterIdentities.Where(x => x.APIKey == apiKey))
            {
                identity.APIKey = null;
            }

            // Remove the API key
            if (!Items.Remove(apiKey.ID))
                throw new InvalidOperationException("This API key does not exist in the list.");

            EveMonClient.OnAPIKeyCollectionChanged();
        }

        /// <summary>
        /// Adds an API key to this collection.
        /// </summary>
        /// <param name="apiKey"></param>
        internal void Add(APIKey apiKey)
        {
            Items.Add(apiKey.ID, apiKey);
            EveMonClient.OnAPIKeyCollectionChanged();
        }

        #endregion


        #region Import / Export Methods

        /// <summary>
        /// Imports the serialized API key.
        /// </summary>
        /// <param name="serial"></param>
        internal void Import(IEnumerable<SerializableAPIKey> serial)
        {
            Items.Clear();
            foreach (SerializableAPIKey apikey in serial)
            {
                try
                {
                    Items.Add(apikey.ID, new APIKey(apikey));
                }
                catch (ArgumentException ex)
                {
                    EveMonClient.Trace("GlobalAPIKeyCollection.Import - " +
                                       "An API key with id {0} already existed; additional instance ignored.", apikey.ID);
                    ExceptionHandler.LogException(ex, true);
                }
            }

            EveMonClient.OnAPIKeyCollectionChanged();
        }

        /// <summary>
        /// Exports the data to a serialization object.
        /// </summary>
        /// <returns></returns>
        internal List<SerializableAPIKey> Export()
        {
            return Items.Values.Select(apikey => apikey.Export()).ToList();
        }

        #endregion
    }
}