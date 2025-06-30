using Microsoft.AspNetCore.SignalR;
using PlatformerAPI.Data;
using PlatformerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace PlatformerAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        public async Task SendMessage(string username, string message)
        {
            var msg = new Message
            {
                Username = username,
                Text = message,
                Timestamp = DateTime.UtcNow
            };

            await _db.Messages.AddAsync(msg);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", msg.Username, msg.Text, msg.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public async Task GetHistory()
        {
            var messages = await _db.Messages.OrderBy(m => m.Timestamp).ToListAsync();
            foreach (var msg in messages)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", msg.Username, msg.Text, msg.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await GetHistory();
        }
    }
}
