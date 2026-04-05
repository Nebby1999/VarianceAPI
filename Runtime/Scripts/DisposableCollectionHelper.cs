#nullable enable
using System;
using System.Collections.Generic;

namespace VAPI
{
    public sealed class DisposableCollectionHelper : IDisposable
    {
        public DisposableCollectionHelper(bool disposeInReverseOrder)
        {
            _reverseForLoop = disposeInReverseOrder;
        }
        private List<IDisposable> _disposables = new List<IDisposable>();
        private bool _reverseForLoop;

        public void AddDisposable(IDisposable? disposable)
        {
            if(disposable != null)
                _disposables.Add(disposable);
        }

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