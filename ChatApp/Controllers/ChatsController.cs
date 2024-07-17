using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Models;
using ChatApp.Hubs;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatsController(ApplicationDbContext context, IHubContext<ChatHub> chatHubContext, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _chatHubContext = chatHubContext;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Chat>>> GetChats()
        {
            return await _context.Chats.Include(c => c.Messages).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Chat>> GetChat(int id)
        {
            var chat = await _context.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chat == null)
            {
                return NotFound(new { message = "Chat not found" });
            }

            var chatUsers = await _context.ChatUsers
                .Where(cu => cu.ChatId == id)
                .Select(cu => cu.UserId)
                .ToListAsync();

            chat.Users = await _context.ChatUsers
                .Where(u => chatUsers.Contains(u.UserId))
                .ToListAsync();

            return chat;
        }

        [HttpPost]
        public async Task<ActionResult<Chat>> PostChat(Chat chat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChat), new { id = chat.Id }, chat);
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinChat(int id, [FromQuery] string userId)
        {
            var chat = await _context.Chats.Include(c => c.Users).FirstOrDefaultAsync(c => c.Id == id);
            if (chat == null)
            {
                return NotFound(new { message = "Chat not found" });
            }

            var existingUser = chat.Users.FirstOrDefault(u => u.UserId == userId);
            if (existingUser != null)
            {
                return Conflict(new { message = "User is already in this chat" });
            }

            var newChatUser = new ChatUser { UserId = userId, ChatId = id };
            _context.ChatUsers.Add(newChatUser);
            await _context.SaveChangesAsync();

            chat.Users.Add(newChatUser);
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id, [FromQuery] string ownerId)
        {
            var chat = await _context.Chats.Include(c => c.Users).FirstOrDefaultAsync(c => c.Id == id);
            if (chat == null)
            {
                return NotFound(new { message = "Chat not found" });
            }

            if (chat.OwnerId != ownerId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to delete this chat" });
            }

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            var users = chat.Users.ToList();
            foreach (var user in users)
            {
                await _chatHubContext.Clients.User(user.UserId).SendAsync("ChatDeleted", id);
            }

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Chat>>> SearchChats([FromQuery] string name, [FromQuery] int? id)
        {
            IQueryable<Chat> query = _context.Chats.Include(c => c.Messages);

            if (id.HasValue)
            {
                query = query.Where(c => c.Id == id.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            var chats = await query.ToListAsync();

            if (chats == null || chats.Count == 0)
            {
                return NotFound(new { message = "No chats found" });
            }

            return chats;
        }

        [HttpPost("{id}/send")]
        public async Task<IActionResult> SendMessage(int id, [FromQuery] string userId, [FromQuery] string message)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat == null)
            {
                return NotFound(new { message = "Chat not found" });
            }

            var chatMessage = new Message
            {
                ChatId = id,
                User = userId,
                Text = message,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(id.ToString()).SendAsync("ReceiveMessage", userId, message);

            return NoContent();
        }
    }
}
