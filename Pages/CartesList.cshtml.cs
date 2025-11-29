using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace anahged.Pages
{
    public class CartesListModel : PageModel
    {
       /*  private readonly GedServices _gedServices;
        public IList<Carte> CartesList { get; set; } = default!;

        public CartesListModel(GedServices gedservices)
        {
            _gedServices = gedservices;
        } */
        /* public void OnGet()
        {
        } */

        // Handler for search functionality
       /*  [BindProperty(SupportsGet = true)]
        public string Tube { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Ville { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Quartier { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Operation { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Legende { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Originalite { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Echelle { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DateCarte { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Cote { get; set; }

        [BindProperty(SupportsGet = true)]
        public IFormFile Fichier { get; set; } */

        /*         [BindProperty(SupportsGet = true)]
                public string Commune { get; set; } */

        /* public async Task OnGetAsync()
        {
            CartesList = await _gedServices.RechercherCarteAsync(Ville, Quartier, Operation, Legende);

            if (CartesList == null || CartesList.Count == 0)
            {
                ViewData["MessageRechercheCarte"] = "Aucun résultat trouvé 😓!";
            }

            // Cette méthode est appelée quand la page se charge
            // On s'assure que le ModelState est clean
            ModelState.Clear();
        } */

        /* public async Task<IActionResult> OnPostView(int id)
        {
            var fichierPdf = await _gedServices.GetFichierPdfAsync(id);
            return File(fichierPdf, "application/pdf", string.Empty);

        } */

        // Methode pour l'enregistrement d'une nouvelle carte
        // Rappelle de la methode d'enregistrement de nouveau fichier ADP dans GedServices
       /*  public async Task<IActionResult> OnPostUploadFileCarteAsync()
        {
            if (Fichier == null || Fichier.Length == 0)
            {
                ModelState.AddModelError("Fichier", "Veuillez sélectionner un fichier ❌.");
                return Page();
            }

            // Validation du type de fichier
            var extension = Path.GetExtension(Fichier.FileName).ToLower();
            if (extension != ".pdf")
            {
                ModelState.AddModelError("Fichier", "Seuls les fichiers PDF sont autorisés ❌.");
                return Page();
            }

            try
            {
                // Récupérer l'ID de l'utilisateur connecté
                var userId = GetCurrentUserId();
                Console.WriteLine($"ID Utilisateur connecté : {userId}");

                var message = await _gedServices.EnregistrerFichierCarteAsync(
                    Fichier, Tube, Ville, Quartier, Operation, Legende, Originalite, Echelle, DateCarte, Cote, userId
                );

                TempData["SuccessMessage"] = message;
                ViewData["MessageUpload"] = "Carte enregistrée avec succès ✅ !";

                // Réinitialiser les propriétés après succès
                ResetProperties();
                ModelState.Clear();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"Erreur lors de l'enregistrement du fichier : {ex.Message} ❌.");
                return Page();
            }
        } */
        
        /* private void ResetProperties()
        {
            Tube = string.Empty;
            Ville = string.Empty;
            Quartier = string.Empty;
            Operation = string.Empty;
            Legende = string.Empty;
            Originalite = string.Empty;
            Echelle = string.Empty;
            Cote = string.Empty;
            DateCarte = string.Empty;
            Fichier = null!;
        }
 */
        // Methode pour obtenir l'ID de l'utilisateur connecté
        /* private int GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                throw new Exception("Utilisateur non authentifié.");
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new Exception("Impossible de récupérer l'ID utilisateur. Veuillez vous reconnecter.");
            }

            return userId;
        } */

        private readonly GedServices _gedServices;

        public CartesListModel(GedServices gedServices)
        {
            _gedServices = gedServices;
        }

        public IList<Carte> CartesList { get; set; } = new List<Carte>();

        // Propriétés de recherche et d'ajout
        [BindProperty(SupportsGet = true)] public string Tube { get; set; }
        [BindProperty(SupportsGet = true)] public string Ville { get; set; }
        [BindProperty(SupportsGet = true)] public string Quartier { get; set; }
        [BindProperty(SupportsGet = true)] public string Operation { get; set; }
        [BindProperty(SupportsGet = true)] public string Legende { get; set; }
        [BindProperty(SupportsGet = true)] public string Originalite { get; set; }
        [BindProperty(SupportsGet = true)] public string Echelle { get; set; }
        [BindProperty(SupportsGet = true)] public string DateCarte { get; set; }
        [BindProperty(SupportsGet = true)] public string Cote { get; set; }
        [BindProperty(SupportsGet = true)] public IFormFile Fichier { get; set; }

        public async Task OnGetAsync()
        {
            CartesList = await _gedServices.RechercherCarteAsync(Ville, Quartier, Operation, Legende)
                          ?? new List<Carte>();

            if (!CartesList.Any())
            {
                ViewData["MessageRechercheCarte"] = "Aucun résultat trouvé 😓!";
            }

            ModelState.Clear();
        }

        public async Task<IActionResult> OnPostViewAsync(int id)
        {
            try
            {
                var fichierPdf = await _gedServices.GetFichierPdfAsync(id);
                return File(fichierPdf, "application/pdf", string.Empty); 
                /* var fichierPdf = await _gedServices.GetFichierPdfAsync(id);
                if (fichierPdf == null)
                {
                    TempData["ErrorMessage"] = "Fichier introuvable ❌.";
                    return Page();
                }
                return File(fichierPdf, "application/pdf", $"Carte_{id}.pdf"); */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du fichier PDF : {ex.Message}");
                Console.WriteLine($"Erreur dans OnPostView: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "Erreur lors de la récupération du fichier PDF ❌.");
                return NotFound($"Impossible d'ouvrir le fichier: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostUploadFileCarteAsync()
        {
            if (Fichier == null || Fichier.Length == 0)
            {
                ModelState.AddModelError("Fichier", "Veuillez sélectionner un fichier ❌.");
                return Page();
            }

            var extension = Path.GetExtension(Fichier.FileName).ToLower();
            if (extension != ".pdf")
            {
                ModelState.AddModelError("Fichier", "Seuls les fichiers PDF sont autorisés ❌.");
                return Page();
            }

            try
            {
                var userId = GetCurrentUserId();

                var message = await _gedServices.EnregistrerFichierCarteAsync(
                    Fichier, Tube, Ville, Quartier, Operation, Legende,
                    Originalite, Echelle, DateCarte, Cote, userId
                );

                TempData["SuccessMessage"] = message;
                ViewData["MessageUpload"] = "Carte enregistrée avec succès ✅ !";

                ResetProperties();
                ModelState.Clear();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Erreur lors de l'enregistrement du fichier : {ex.Message} ❌.");
                return Page();
            }
        }

        private void ResetProperties()
        {
            Tube = Ville = Quartier = Operation = Legende = Originalite = Echelle = Cote = DateCarte = string.Empty;
            Fichier = null!;
        }

        private int GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                throw new Exception("Utilisateur non authentifié.");

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new Exception("Impossible de récupérer l'ID utilisateur. Veuillez vous reconnecter.");

            return userId;
        }

        
    }
}
