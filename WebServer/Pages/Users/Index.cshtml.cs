using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace WebServer.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly UserManagement.UserDbContext _context;

        public IndexModel(UserManagement.UserDbContext context)
        {
            _context = context;
        }

        public new IList<User> User { get;set; }

        public async Task OnGetAsync()
        {
            User = await _context.User.ToListAsync();
        }
    }
}
