using System;
using System.IO;

namespace Omnius.Xeus.Service.Primitives
{
    public class TestsBase : IDisposable
    {
        public TestsBase()
        {
            if (Directory.Exists(UnitTestEnvironment.TempDirectoryPath))
            {
                Directory.Delete(UnitTestEnvironment.TempDirectoryPath, true);
            }

            Directory.CreateDirectory(UnitTestEnvironment.TempDirectoryPath);
        }

        public void Dispose()
        {
            Directory.Delete(UnitTestEnvironment.TempDirectoryPath, true);
        }
    }
}
