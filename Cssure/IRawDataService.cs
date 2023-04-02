using Cssure.Models;

namespace Cssure
{
    public interface IRawDataService
    {
        void ProcessData(byte[] bytes);
    }
}
