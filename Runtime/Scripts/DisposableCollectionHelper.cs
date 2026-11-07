#nullable enable
using System;
using System.Collections.Generic;

namespace VAPI
{
    /// <summary>
    /// An <see cref="IDisposable"/> class used to dispose multiple <see cref="IDisposable"/> instances in a row.
    /// <br></br>
    /// Can dispose in reverse order.
    /// </summary>
    public sealed class DisposableCollectionHelper : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="disposeInReverseOrder">If this is true, the disposing will be done using a reverse for loop rather than a regular for loop</param>
        public DisposableCollectionHelper(bool disposeInReverseOrder)
        {
            _reverseForLoop = disposeInReverseOrder;
        }
        private List<IDisposable> _disposables = new List<IDisposable>();
        private bool _reverseForLoop;

        /// <summary>
        /// Adds a new Disposable instance to the internal list
        /// </summary>
        /// <param name="disposable">The disposable instance, can be null</param>
        public void AddDisposable(IDisposable? disposable)
        {
            if(disposable != null)
                _disposables.Add(disposable);
        }

        /// <summary>
        /// Disposes the <see cref="IDisposable"/> instances in this.
        /// </summary>
        public void Dispose()
        {
            if(_reverseForLoop)
            {
                DisposeReverse();
            }
            else
            {
                DisposeForward();
            }
            _disposables.Clear();
        }

        private void DisposeReverse()
        {
            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                _disposables[i].Dispose();
            }
        }

        private void DisposeForward()
        {
            for(int i = 0; i < _disposables.Count; i++)
            {
                _disposables[i].Dispose();
            }
        }
    }
}