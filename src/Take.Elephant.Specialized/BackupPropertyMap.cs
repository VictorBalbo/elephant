using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Elephant.Specialized
{
    /// <summary>
    /// Defines a fall back mechanism with a primary and backup maps. 
    /// For write actions, the operation must succeed in both;
    /// For queries, if the action fails in the first, it falls back to the second.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class BackupPropertyMap<TKey, TValue> : BackupStrategy<IPropertyMap<TKey, TValue>>, IPropertyMap<TKey, TValue>
    {
        public BackupPropertyMap(IPropertyMap<TKey, TValue> primary, IPropertyMap<TKey, TValue> backup, TimeSpan synchronizationTimeout)
            : this(primary, backup, new IntersectionMapSynchronizer<TKey, TValue>(synchronizationTimeout))
        {

        }

        public BackupPropertyMap(IPropertyMap<TKey, TValue> primary, IPropertyMap<TKey, TValue> backup, ISynchronizer<IMap<TKey, TValue>> synchronizer) 
            : base(primary, backup, synchronizer)
        {
            
        }

        public virtual Task<bool> TryAddAsync(TKey key, TValue value, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            return ExecuteWriteFunc(m => m.TryAddAsync(key, value, overwrite, cancellationToken));
        }

        public virtual Task<TValue> GetValueOrDefaultAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return ExecuteQueryFunc(m => m.GetValueOrDefaultAsync(key, cancellationToken));
        }

        public virtual Task<bool> TryRemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return ExecuteWriteFunc(m => m.TryRemoveAsync(key, cancellationToken));
        }

        public virtual Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            return ExecuteQueryFunc(m => m.ContainsKeyAsync(key, cancellationToken));
        }

        public virtual Task SetPropertyValueAsync<TProperty>(TKey key, string propertyName, TProperty propertyValue)
        {
            return ExecuteWriteFunc(m => m.SetPropertyValueAsync(key, propertyName, propertyValue));
        }

        public virtual Task<TProperty> GetPropertyValueOrDefaultAsync<TProperty>(TKey key, string propertyName)
        {
            return ExecuteQueryFunc(m => m.GetPropertyValueOrDefaultAsync<TProperty>(key, propertyName));
        }

        public virtual Task MergeAsync(TKey key, TValue value)
        {
            return ExecuteWriteFunc(m => m.MergeAsync(key, value));
        }
    }
}