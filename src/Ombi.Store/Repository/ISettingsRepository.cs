using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface ISettingsRepository
    {
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        GlobalSettings Insert(GlobalSettings entity);
        Task<GlobalSettings> InsertAsync(GlobalSettings entity);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="settingsName">Name of the settings.</param>
        /// <returns></returns>
        GlobalSettings Get(string settingsName);
        Task<GlobalSettings> GetAsync(string settingsName);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task DeleteAsync(GlobalSettings entity);
        void Delete(GlobalSettings entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        Task UpdateAsync(GlobalSettings entity);
        void Update(GlobalSettings entity);


    }
}