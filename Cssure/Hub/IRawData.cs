using Microsoft.AspNetCore.SignalR;

namespace Cssure.Hub
{  
    public interface IRawData
    {
        Task RawDataUpdate(decimal count);
    }
    public class RawDataHub : Hub<IRawData>
    {
        public async Task ExpenseUpdate(decimal count)
        {
            await Clients.All.RawDataUpdate(count);
        }
    }
}
