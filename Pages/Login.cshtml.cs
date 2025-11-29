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
            if (Email == null)
            {
                ModelState.AddModelError("", "Nom d'utilisateur est obligatoire");
                return Page();
            }

            bool result = _connexionService.Authentifier(Email, Pwd); // _connexionService.Authentifier(Email, Pwd);
            if (result)
            {
                var utilisateur = _gedContext.Users.FirstOrDefault(u => u.Email == Email);
                var initiales = _connexionService.GetUserInitiales(utilisateur!);

                // Récuperer le role de l'utilisateur
                var role = _gedContext.Groupes.FirstOrDefault(g => g.GroupeId == utilisateur!.GroupeId)?.NomGroupe;

                // Stocker les initiales dans une session ou un cookie si nécessaire 'Claim'
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Email),
                    new Claim("initiales", initiales), // Ajouter les initiales en tant que revendication personnalisée
                    new Claim(ClaimTypes.Role, role ), // Ajouter le rôle de l'utilisateur
                    new Claim("UserId", utilisateur!.UserId.ToString())
                };
                
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return Redirect("/TableauDeBord");
            }
            else
            {
                ModelState.AddModelError("", "Nom d'utilisateur ou mot de passe incorrect");
                return Page(); // Return a value in the else block
            }
        }
    }
}
