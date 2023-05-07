using Cssure.DTO;
using Cssure.Models;

namespace Cssure.Services
{
    public interface IRawDataService
    {
        void ProcessData(EKGSampleDTO bytes);
    }
}
