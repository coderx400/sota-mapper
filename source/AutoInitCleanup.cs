using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotAMapper
{
    public class AutoInitCleanup : IDisposable
    {
        private Action _initCode;
        private Action _cleanupCode;

        public AutoInitCleanup(Action initCode, Action cleanupCode)
        {
            _initCode = initCode;
            _cleanupCode = cleanupCode;

            _initCode?.Invoke();
        }

        public void Dispose()
        {
            _cleanupCode?.Invoke();
        }
    }
}
