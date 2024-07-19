using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunaChatChit.Context;
using TunaChatChit.Resources;

namespace TunaChatChit.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ChatContext _context;
        private string Type;

        public MessageController(ChatContext context)
        {
            _context = context;
            Type = GetType().Name.Replace("Controller", "");
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages.ToListAsync();
            return Ok(messages);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetMessage(int Id)
        {
            var message = await _context.MessageContents.FindAsync(Id);
            if (message == null)
            {
                return NotFound("No message content found.");
            }
            return Ok(message);
        }
        public class MessageDTO
        {
            public int Id { get; set; }
            public int SendId { get; set; }
            public int ReceiveId { get; set; }
            public string SentTime { get; set; }
        }

        [HttpGet("{sendId}/{receiveId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesBySenderReceiver(int sendId, int receiveId)
        {
            var messages = await _context.Messages
                .Where(m => (m.SendId == sendId && m.ReceiveId == receiveId) || (m.SendId == receiveId && m.ReceiveId == sendId))
                .ToListAsync();

            var messageDTOs = messages.Select(m => new MessageDTO
            {
                Id = m.Id,
                SendId = m.SendId,
                ReceiveId = m.ReceiveId,
                SentTime = m.SentTime.ToString("HH:mm dd-MM-yyyy")
            }).ToList();

            return Ok(messageDTOs);
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromForm] Message message)
        {
            if (message == null)
            {
                return BadRequest();
            }

            message.SentTime = DateTime.Now;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null)
            {
                return BadRequest();
            }

            message.SentTime = DateTime.Now;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            //await WebSocketMiddleware.NotifyUser(message.ReceiveId.ToString(), "refresh");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
