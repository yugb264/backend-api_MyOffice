using Microsoft.AspNetCore.SignalR;

namespace backend_api.Hubs
{
    public class DocumentHub : Hub
    {
        public async Task JoinDocumentGroup(string documentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, documentId);
        }

        public async Task LeaveDocumentGroup(string documentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, documentId);
        }
    }
}