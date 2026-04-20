using System.Security.Claims;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace anahged.Pages
{
    public class LoginModel : PageModel
    {
         public void OnGet()
         {
         }

        private readonly ConnexionService _connexionService;
        private readonly GedContext _gedContext;

        public LoginModel(ConnexionService connexionService, GedContext gedContext)
        {
            _connexionService = connexionService;
            _gedContext = gedContext;
        }
        /* public void OnGet()
        {
        } */

        [BindProperty]
        public string NomUser { get; set; }

        [BindProperty]
        public string PrenomUser { get; set; }

        [BindProperty]
        public string Email { get; set; } = null!;

        [BindProperty]
        public string Pwd { get; set; } = null!;

        /*  public void OnGet()
           {
           } */

        public async Task<IActionResult> OnPost()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("", "Nom d'utilisateur est obligatoire");
                return Page();
            }

            bool result = _connexionService.Authentifier(Email, Pwd);

            // 🔴 CAS : CONNEXION ÉCHOUÉE
            if (!result)
            {
                var userFail = _gedContext.Users.FirstOrDefault(u => u.Email == Email);
                if (userFail != null)
                {
                    _gedContext.Userconnexionlogs.Add(new Userconnexionlog
                    {
                        UserId = userFail.UserId,
                        DateEvenement = DateTime.Now,
                        AdresseIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        TypeEvenement = "FAIL"
                    });

                    _gedContext.SaveChanges();
                }

                ModelState.AddModelError("", "Nom d'utilisateur ou mot de passe incorrect");
                return Page();
            }

            // ✅ CAS : CONNEXION RÉUSSIE
            var utilisateur = _gedContext.Users.First(u => u.Email == Email);
            var initiales = _connexionService.GetUserInitiales(utilisateur);

            var role = _gedContext.Groupes
                .FirstOrDefault(g => g.GroupeId == utilisateur.GroupeId)?.NomGroupe ?? "";

            // 🔐 Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Email),
                new Claim("initiales", initiales),
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", utilisateur.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // ✅ CRÉATION SESSION
            var sessionToken = Guid.NewGuid().ToString("N");

            _gedContext.Usersessions.Add(new Usersession
            {
                UserId = utilisateur.UserId,
                SessionToken = Guid.Parse(sessionToken),
                DateConnexion = DateTime.Now,
                AdresseIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                IsActive = true
            });

            // ✅ LOG LOGIN
            _gedContext.Userconnexionlogs.Add(new Userconnexionlog
            {
                UserId = utilisateur.UserId,
                DateEvenement = DateTime.Now,
                AdresseIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                TypeEvenement = "LOGIN"
            });

            _gedContext.SaveChanges();

            // 🍪 Cookie Session
            Response.Cookies.Append("SESSION_ID", sessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Redirect("/TableauDeBord");   
        }
    }
}