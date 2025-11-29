using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace anahged.Pages
{
    public class AdministrationModel : PageModel
    {
        private readonly AdministrationService _administrationService;
        public IList<Models.User> UserList { get; set; } = default!;
        public IList<Models.Groupe> RoleList { get; set; } = default!;
        public AdministrationModel(AdministrationService administrationService)
        {
            _administrationService = administrationService;
        }
        public void OnGet()
        {
            UserList = _administrationService.GetAllUsers().ToList();
            RoleList = _administrationService.GetAllRoles().ToList();
        }

        // Handler pour la modification d'utilisateur
        public async Task<IActionResult> OnPostEditUserAsync(int userId, string nom, string prenom, string email, string contact, int groupeId, int actif)
        {
            var user = await _administrationService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "Utilisateur non trouvé" });
            }

            user.NomUser = nom;
            user.PrenomUser = prenom;
            user.Email = email;
            user.Contact = contact;
            user.GroupeId = groupeId;
            user.Actif = actif;

            var result = await _administrationService.UpdateUserAsync(user);

            if (result)
            {
                return new JsonResult(new { success = true, message = "Utilisateur modifié avec succès" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Erreur lors de la modification" });
            }
        }
        
        public class CreateUserRequest
        {
            public string Nom { get; set; } = string.Empty;
            public string Prenom { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Contact { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public int GroupeId { get; set; }
            public int Actif { get; set; }
        }
        // Handler pour la création d'utilisateur
        public async Task<IActionResult> OnPostCreateUserAsync([FromBody] CreateUserRequest request)
        {
            try
            {
                var newUser = new User
                {
                    NomUser = request.Nom,
                    PrenomUser = request.Prenom,
                    Contact = request.Contact,
                    Email = request.Email,
                    GroupeId = request.GroupeId,
                    Actif = request.Actif
                };

                var result = await _administrationService.CreateUserAsync(newUser, request.Password);
                
                // Retournez un objet avec les propriétés attendues par le frontend
                return new JsonResult(new
                {
                    // success = result.Success, 
                    // message = result.Message 
                    success = result.success,
                    message = result.message
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = "Erreur lors de la création: " + ex.Message 
                });
            }
        }
        
        // Handler pour la suppression d'utilisateur
        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            var result = await _administrationService.DeleteUserAsync(userId);

            if (result)
            {
                return new JsonResult(new { success = true, message = "Utilisateur supprimé avec succès" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Erreur lors de la suppression" });
            }
        }
    }
}
