using anahged.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace anahged.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly GedContext _gedContext;
        public LogoutModel(GedContext context)
        {
            _gedContext = context;
        }
        public async Task<IActionResult> OnGet()
        {
            /* await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login"); */
            var sessionTokenString = Request.Cookies["SESSION_ID"];

            if (!string.IsNullOrEmpty(sessionTokenString) && Guid.TryParse(sessionTokenString, out Guid sessionToken))
            {
                var session = _gedContext.Usersessions
                    .FirstOrDefault(s => s.SessionToken == sessionToken && s.IsActive == true);

                if (session != null)
                {
                    session.IsActive = false;
                    session.DateDeconnexion = DateTime.Now;

                    _gedContext.Userconnexionlogs.Add(new Models.Userconnexionlog
                    {
                        UserId = session.UserId,
                        DateEvenement = DateTime.Now,
                        AdresseIp = session.AdresseIp,
                        TypeEvenement = "LOGOUT"
                    });

                    _gedContext.SaveChanges();
                }

                Response.Cookies.Delete("SESSION_ID");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }
    }
}
