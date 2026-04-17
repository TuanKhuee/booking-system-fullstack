using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BookingService.Hubs
{
    public class BookingHub : Hub
    {
        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());  
        }

        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }
    }
}
