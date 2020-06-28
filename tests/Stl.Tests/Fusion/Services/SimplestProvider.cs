using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace Stl.Tests.Fusion.Services
{
    public interface ISimplestProvider
    {
        // These two properties are here solely for testing purposes
        int GetValueCallCount { get; }
        int GetCharCountCallCount { get; }

        void SetValue(string value);
        Task<string> GetValueAsync();
        Task<int> GetCharCountAsync();
    }

    public class SimplestProvider : ISimplestProvider, IComputedService
    {
        private volatile string _value = "";
        private readonly bool _isCaching;

        public int GetValueCallCount { get; private set; }
        public int GetCharCountCallCount { get; private set; }

        public SimplestProvider() 
            => _isCaching = GetType().Name.EndsWith("Proxy");

        public void SetValue(string value)
        {
            Interlocked.Exchange(ref _value, value);
            Invalidate();
        }

        [ComputedServiceMethod]
        public virtual Task<string> GetValueAsync()
        {
            GetValueCallCount++;
            return Task.FromResult(_value);
        }

        [ComputedServiceMethod]
        public virtual async Task<int> GetCharCountAsync()
        {
            GetCharCountCallCount++;
            var value = await GetValueAsync().ConfigureAwait(false);
            return value.Length;
        }

        protected virtual void Invalidate()
        {
            if (!_isCaching)
                return;
            Computed.Invalidate(GetValueAsync);
            // No need to invalidate GetCharCountAsync,
            // since it will be invalidated automatically.
        }
    }
}