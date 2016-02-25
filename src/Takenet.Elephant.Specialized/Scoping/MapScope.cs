﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.Elephant.Specialized.Scoping
{
    public class MapScope : IScope
    {
        private readonly ISetMap<string, string> _keysSetMap;

        public MapScope(string name, ISetMap<string, string> keysSetMap)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (keysSetMap == null) throw new ArgumentNullException(nameof(keysSetMap));
            Name = name;
            _keysSetMap = keysSetMap;
        }

        internal Func<string, Task> RemoveKeyFunc;

        public string Name { get; }

        public virtual async Task ClearAsync()
        {
            if (RemoveKeyFunc == null) return;

            var keysSet = await _keysSetMap.GetValueOrDefaultAsync(Name).ConfigureAwait(false);
            if (keysSet != null)
            {
                var keysEnumerable = await keysSet.AsEnumerableAsync().ConfigureAwait(false);
                await keysEnumerable.ForEachAsync(k => RemoveKeyFunc(k), CancellationToken.None);
            }

            await _keysSetMap.TryRemoveAsync(Name).ConfigureAwait(false);            
        }

        protected internal virtual Task AddKeyAsync(string key)
        {
            if (RemoveKeyFunc == null) throw new InvalidOperationException("The RemoveKeyFunc must be set before changing the scope");
            return _keysSetMap.AddItemAsync(Name, key);
        }

        protected internal virtual Task RemoveKeyAsync(string key)
        {
            if (RemoveKeyFunc == null) throw new InvalidOperationException("The RemoveKeyFunc must be set before changing the scope");
            return _keysSetMap.TryRemoveItemAsync(Name, key);
        }
    }
}