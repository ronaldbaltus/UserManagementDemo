using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UserManagement;
using UserManagement.Models;

namespace WebServer.Pages.Users
{
    public class DetailsModel : PageModel
    {
        private readonly UserDbContext _context;
        private readonly EventSourceRepository _eventSource;

        public DetailsModel(UserManagement.UserDbContext context, UserManagement.EventSourceRepository eventSource)
        {
            _context = context;
            _eventSource = eventSource;
        }

        public new User User { get; set; }
        public IEnumerable<RecordedEvent> Events { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User = await _context.User.FirstOrDefaultAsync(m => m.ID == id);

            if (User == null)
            {
                return NotFound();
            }

            Events = await _eventSource.GetEvents(User);

            return Page();
        }
    }
}
