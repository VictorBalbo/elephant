﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Elephant
{
    public interface IAsyncEnumerable<T> : IEnumerable<T>
    {
        Task<IAsyncEnumerator<T>> GetEnumeratorAsync(CancellationToken cancellationToken);
    }

    public interface IAsyncEnumerable : IEnumerable
    {
        Task<IAsyncEnumerator> GetEnumeratorAsync(CancellationToken cancellationToken);
    }
}