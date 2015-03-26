﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.SimplePersistence
{
    /// <summary>
    /// Defines a <see cref="ISetMap"/> that allows to get a specific item in the set.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the value.</typeparam>
    public interface IItemSetMap<TKey, TItem> : ISetMap<TKey, TItem>
    {
        /// <summary>
        /// Gets the item in the set referenced by the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        Task<TItem> GetItemOrDefaultAsync(TKey key, TItem item);
    }
}