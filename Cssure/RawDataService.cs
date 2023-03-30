using Cssure.Models;

namespace Cssure
{
    public class RawDataService : IRawDataService
    {
        public void ProcessData(byte[] bytes)
        {
            Console.WriteLine(bytes.Length);
        }
    }
}
