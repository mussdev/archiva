using System.IO.Compression;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Namespace
{
    public class TraitementFichierCarteModel : PageModel
    {
        private readonly GedServices _gedServices;
        private readonly IWebHostEnvironment _environment;
        public readonly GedContext _gedcontext;
        public IList<SelectListItem> UserAllowedStatuts { get; set; } = new List<SelectListItem>();
        public IList<anahged.Models.Operation> OperationList { get; set; } = new List<anahged.Models.Operation>();
        public IList<Carte> CarteListEnAttenteValidation { get; set; } = new List<Carte>();

        // Propriétés de recherche et d'ajout
        [BindProperty(SupportsGet = true)] public string? Tube { get; set; }
        [BindProperty(SupportsGet = true)] public string? Ville { get; set; }
        [BindProperty(SupportsGet = true)] public string? Quartier { get; set; }
        [BindProperty(SupportsGet = true)] public string? Operation { get; set; }
        [BindProperty(SupportsGet = true)] public string? Legende { get; set; }
        [BindProperty(SupportsGet = true)] public string? Originalite { get; set; }
        [BindProperty(SupportsGet = true)] public string? Echelle { get; set; }
        [BindProperty(SupportsGet = true)] public string? DateCarte { get; set; }
        [BindProperty(SupportsGet = true)] public string? Cote { get; set; }
        [BindProperty(SupportsGet = true)] public IFormFile? Fichier { get; set; }
        [BindProperty(SupportsGet = true)] public int IdOperation { get; set; }
        [BindProperty(SupportsGet = true)] public int StatutId { get; set; }

        public TraitementFichierCarteModel(GedServices gedServices, IWebHostEnvironment environment, GedContext gedcontext)
        {
            _gedServices = gedServices;
            _environment = environment;
            _gedcontext = gedcontext;
        }

        public async Task OnGetAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour accéder à cette page.";
                Response.Redirect("/Login");
                return;
            }

            CarteListEnAttenteValidation = await _gedServices.RechercherCarteAsync(Ville, Quartier, Operation, Legende)
                          ?? new List<Carte>();

            if (!CarteListEnAttenteValidation.Any())
            {
                ViewData["MessageRechercheCarte"] = "Aucun résultat trouvé 😓!";
            }

            // Charger la liste des opérations pour le dropdown
            OperationList = await _gedcontext.Operations.Include(o => o.IdVilleNavigation).ToListAsync();

            // Récuperer les statuts liés à l'utilisateur connecté
            UserAllowedStatuts = await _gedcontext.Userstatuts
                .Where(us => us.UserId == GetCurrentUserId())
                .Join(_gedcontext.Statuts,
                    userstatut => userstatut.IdStatut,
                    statut => statut.IdStatut,
                    (userstatut, statut) => new SelectListItem
                    {
                        Value = statut.IdStatut.ToString(),
                        Text = statut.DescriptionStatut
                    })
                .Distinct()
                .ToListAsync();

            // Charger la liste des fichiers Adp en statut en attente et nouveau
            CarteListEnAttenteValidation = await _gedcontext.Cartes
                .Include(o => o.IdOpeNavigation)
                .Include(v => v.IdOpeNavigation.IdVilleNavigation)
                .Include(s => s.DernierStatutCarte)
                .Where(s => s.DernierStatutCarte.DescriptionStatut == "En attente" || s.DernierStatutCarte.DescriptionStatut == "Nouveau")
                .ToListAsync();

            ModelState.Clear();
        }

          public async Task<IActionResult> OnPostViewAsync(int id)
        {
            try
            {
                var fichierPdf = await _gedServices.GetFichierPdfAsync(id);
                return File(fichierPdf, "application/pdf", string.Empty);
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

        // Méthode pour enregistrer un fichier PDF de carte
        public async Task<IActionResult> OnPostUploadFileCarteAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour effectuer cette action.";
                return RedirectToPage("/Login");
            }

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

                if (IdOperation <= 0)
                {
                    ModelState.AddModelError("IdOperation", "Veuillez sélectionner une opération valide.");
                    return Page();
                }

                var message = await _gedServices.EnregistrerFichierCarteAsync(
                    Fichier!,
                    Tube,
                    Ville,
                    Quartier,
                    IdOperation,
                    Operation,
                    Legende,
                    Originalite,
                    Echelle,
                    DateCarte,
                    Cote,
                    userId,
                    StatutId
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

        // Réinitialiser les propriétés après un enregistrement réussi
        private void ResetProperties()
        {
            Tube = Ville = Quartier = Operation = Legende = Originalite = Echelle = Cote = DateCarte = string.Empty;
            Fichier = null!;
        }

        // Méthode pour obtenir l'ID de l'utilisateur connecté
        private int GetCurrentUserId()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return 0;

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return 0;

            return userId;
        }

        // Méthode pour modifier les informations d'un enregistrement d'une carte
        public async Task<IActionResult> OnPostUpdateCarte()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour effectuer cette action.";
                return RedirectToPage("/Login");
            }

            try
            {
                // Récupération de l'ID
                if (!int.TryParse(Request.Form["IdCarte"], out int id))
                    return BadRequest("ID invalide");

                var carte = await _gedcontext.Cartes.FindAsync(id);
                if (carte == null)
                    return NotFound();

                // --- Conservation des anciennes valeurs ---
                var ancienCarte = new
                {
                    carte.Tube,
                    carte.Legende,
                    carte.DateCarte,
                    carte.Operation,
                    carte.Echelle,
                    carte.Originalite,
                    carte.Cote,
                    carte.Ville,
                    carte.Quartier,
                    carte.Lien,
                    carte.IdOpe,
                    carte.DernierStatutCarteId
                };

                bool modificationChamps = false;
                bool changementStatut = false;
                bool changementFichier = false;

                // --- Mise à jour des champs simples ---
                string newTube = Request.Form["Tube"].ToString() ?? "";
                if (newTube != ancienCarte.Tube)
                {
                    carte.Tube = newTube;
                    modificationChamps = true;
                }

                string newLegende = Request.Form["Legende"].ToString() ?? "";
                if (newLegende != ancienCarte.Legende)
                {
                    carte.Legende = newLegende;
                    modificationChamps = true;
                }

                string newOperation = Request.Form["Operation"].ToString() ?? "";
                if (newOperation != ancienCarte.Operation)
                {
                    carte.Operation = newOperation;
                    modificationChamps = true;
                }

                string newEchelle = Request.Form["Echelle"].ToString() ?? "";
                if (newEchelle != ancienCarte.Echelle)
                {
                    carte.Echelle = newEchelle;
                    modificationChamps = true;
                }

                string newOriginalite = Request.Form["Originalite"].ToString() ?? "";
                if (newOriginalite != ancienCarte.Originalite)
                {
                    carte.Originalite = newOriginalite;
                    modificationChamps = true;
                }

                string newCote = Request.Form["Cote"].ToString() ?? "";
                if (newCote != ancienCarte.Cote)
                {
                    carte.Cote = newCote;
                    modificationChamps = true;
                }

                string newVille = Request.Form["Ville"].ToString() ?? "";
                if (newVille != ancienCarte.Ville)
                {
                    carte.Ville = newVille;
                    modificationChamps = true;
                }

                string newQuartier = Request.Form["Quartier"].ToString() ?? "";
                if (newQuartier != ancienCarte.Quartier)
                {
                    carte.Quartier = newQuartier;
                    modificationChamps = true;
                }

                int? newIdOpe = int.TryParse(Request.Form["IdOperation"], out int idOpe) ? idOpe : (int?)null;
                if (newIdOpe != ancienCarte.IdOpe)
                {
                    carte.IdOpe = newIdOpe;
                    modificationChamps = true;
                }

                // --- Gestion de la date ---
                if (DateTime.TryParse(Request.Form["DateCarte"], out DateTime dateDoc))
                {
                    string newDate = dateDoc.ToString("dd/MM/yyyy");
                    if (newDate != ancienCarte.DateCarte)
                    {
                        carte.DateCarte = newDate;
                        modificationChamps = true;
                    }
                }

                // --- Gestion du statut ---
                int? newStatutId = int.TryParse(Request.Form["statutId"], out int statutId) ? statutId : (int?)null;
                if (newStatutId != ancienCarte.DernierStatutCarteId)
                {
                    carte.DernierStatutCarteId = newStatutId;
                    modificationChamps = true;
                    changementStatut = true;
                }

                // --- Gestion du fichier ---
                var fichier = Request.Form.Files["Fichier"];
                bool deleteFile = Request.Form["DeleteFile"] == "true";

                if (fichier != null && fichier.Length > 0)
                {
                    // Supprimer l'ancien fichier s'il existe
                    if (!string.IsNullOrEmpty(carte.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, carte.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    // Sauvegarder le nouveau fichier
                    var originalFileName = fichier.FileName;
                    var filePath = Path.Combine(_environment.WebRootPath, "CARTES/NewCARTES/", originalFileName);
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fichier.CopyToAsync(stream);
                    }

                    carte.Lien = Path.Combine("CARTES/NewCARTES/", originalFileName).Replace("\\", "/");
                    modificationChamps = true;
                    changementFichier = true;
                }
                else if (deleteFile && !string.IsNullOrEmpty(carte.Lien))
                {
                    // Suppression du fichier sans remplacement
                    var oldFilePath = Path.Combine(_environment.WebRootPath, carte.Lien);
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    carte.Lien = null;
                    modificationChamps = true;
                    changementFichier = true;
                }

                // Si aucune modification, retourner directement
                if (!modificationChamps)
                {
                    return Content("Aucune modification détectée");
                }

                // Sauvegarde des modifications
                await _gedcontext.SaveChangesAsync();

                // --- Enregistrement de l'historique général (HistoriqueCarte) ---
                var historiqueCarte = new HistoriqueCarte
                {
                    IdCarte = carte.IdCarte,
                    UserId = GetCurrentUserId(),
                    DateHisto = DateTime.Now,
                    DateVu = DateOnly.FromDateTime(DateTime.Now),
                    TypeAction = "Modification",
                    Commentaire = $"Carte modifiée - Tube: {carte.Tube}, Légende: {carte.Legende}, Date: {carte.DateCarte}, Opération: {carte.Operation}, Ville: {carte.Ville}, Statut ID: {carte.DernierStatutCarteId}"
                };
                _gedcontext.HistoriqueCartes.Add(historiqueCarte);

                // --- Si changement de statut, ajouter une entrée dans Validationsfile (ou table dédiée) ---
                if (changementStatut)
                {
                    var validationfile = new Validationsfile
                    {
                        IdCarte = carte.IdCarte,  // Assurez-vous que Validationsfile a une propriété IdCarte
                        UserId = GetCurrentUserId(),
                        DateValidation = DateTime.Now,
                        TypeAction = "ModificationStatut",
                        Commentaire = $"Statut modifié - Nouveau statut ID: {carte.DernierStatutCarteId}"
                    };
                    _gedcontext.Validationsfiles.Add(validationfile);
                }

                // Optionnel : tracer le changement de fichier si nécessaire
                if (changementFichier)
                {
                    // Vous pouvez ajouter une autre entrée d'historique spécifique au fichier
                    // Par exemple, ajouter une ligne dans une table "FichierHistorique" ou enrichir le commentaire
                }

                await _gedcontext.SaveChangesAsync();

                return Content("Mise à jour réussie");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur lors de la modification: " + ex.Message);
            }
        }

 

         // Methode pour supprimer le fichier ADP associé à un enregistrement ADP
        public async Task<IActionResult> OnPostDeleteFileCarte(int id)
        {
            try
            {
                if(id <= 0)
                {
                    return BadRequest("ID invalide pour la suppression du fichier.");
                }   

                var carte = await _gedcontext.Cartes.FindAsync(id);
                if (carte == null)
                {
                    return NotFound();
                }

                // Supprimer le fichier physique
                if (!string.IsNullOrEmpty(carte.Lien))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, carte.Lien);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Mettre à jour la base de données
                carte.Lien = null;
                await _gedcontext.SaveChangesAsync();

                return Content("Fichier supprimé avec succès");
            }
            catch (Exception ex)
            {
                // Log l'erreur
                return StatusCode(500, "Erreur lors de la suppression du fichier: " + ex.Message);
            }
        }

        // Methode pour supprimer le fichier VPL associé à un enregistrement VPL
        public async Task<IActionResult> OnPostDeleteAdp(int id)
        {
            try
            {
                var carte = await _gedcontext.Cartes.FindAsync(id);
                if (carte == null)
                {
                    return NotFound();
                }

                // Supprimer le fichier physique
                if (!string.IsNullOrEmpty(carte.Lien))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, carte.Lien);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Supprimer de la base de données
                _gedcontext.Cartes.Remove(carte);
                await _gedcontext.SaveChangesAsync();

                return Content("Suppression réussie");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur lors de la suppression: " + ex.Message);
            }
        }

        // Téléchargement des fichiers sélectionnés en ZIP
        public async Task<IActionResult> OnPostDownloadSelectedFiles([FromForm] List<int> selectedIds)
        {
            try
            {
                // Récupérer les fichiers sélectionnés
                var selectedFiles = await _gedcontext.Cartes
                    .Where(a => selectedIds.Contains(a.IdCarte))
                    .ToListAsync();

                Console.WriteLine($"Fichiers trouvés en BD: {selectedFiles.Count}");

                if (!selectedFiles.Any())
                {
                    TempData["ErrorMessage"] = "Aucun fichier sélectionné";
                    return RedirectToPage();
                }

                // Obtenir le chemin de base de l'application
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                Console.WriteLine($"Chemin wwwroot: {webRootPath}");

                // Liste pour stocker les fichiers valides
                var validFiles = new List<(string filePath, string fileName)>();

                foreach (var file in selectedFiles)
                {
                    Console.WriteLine($"Vérification fichier ID {file.IdCarte}: {file.Lien}");

                    if (!string.IsNullOrEmpty(file.Lien))
                    {
                        // Construire le chemin complet
                        string fullPath;

                        if (Path.IsPathRooted(file.Lien))
                        {
                            // C'est déjà un chemin absolu
                            fullPath = file.Lien;
                        }
                        else
                        {
                            // C'est un chemin relatif, le construire à partir de wwwroot
                            fullPath = Path.Combine(webRootPath, file.Lien);
                        }

                        Console.WriteLine($"Chemin complet: {fullPath}");
                        Console.WriteLine($"Le fichier existe: {System.IO.File.Exists(fullPath)}");

                        if (System.IO.File.Exists(fullPath))
                        {
                            var fileName = $"{file.Tube}_{file.IdCarte}_{Path.GetFileName(file.Lien)}";
                            validFiles.Add((fullPath, fileName));
                            Console.WriteLine($"Fichier valide: {fullPath}");
                        }
                        else
                        {
                            Console.WriteLine($"Fichier non trouvé: {fullPath}");

                            // Essayer d'autres emplacements possibles
                            var alternativePaths = new[]
                            {
                        Path.Combine(Directory.GetCurrentDirectory(), file.Lien),
                        Path.Combine(Environment.CurrentDirectory, file.Lien),
                        file.Lien
                    };

                            foreach (var altPath in alternativePaths)
                            {
                                if (System.IO.File.Exists(altPath))
                                {
                                    Console.WriteLine($"Fichier trouvé à l'emplacement alternatif: {altPath}");
                                    var fileName = $"{file.Tube}_{file.IdCarte}_{Path.GetFileName(file.Lien)}";
                                    validFiles.Add((altPath, fileName));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Chemin vide pour l'ID {file.IdCarte}");
                    }
                }

                Console.WriteLine($"Fichiers valides trouvés: {validFiles.Count}");

                if (!validFiles.Any())
                {
                    TempData["ErrorMessage"] = "Aucun fichier physique trouvé sur le serveur. Vérifiez les chemins des fichiers.";
                    return RedirectToPage();
                }

                // Créer un fichier temporaire pour le ZIP
                var tempZipPath = Path.GetTempFileName();
                Console.WriteLine($"Création du ZIP temporaire: {tempZipPath}");

                using (var fileStream = new FileStream(tempZipPath, FileMode.Create))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    int filesAdded = 0;

                    foreach (var (filePath, fileName) in validFiles)
                    {
                        try
                        {
                            // Créer l'entrée dans le ZIP
                            var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);

                            using var entryStream = entry.Open();
                            using var originalFileStream = System.IO.File.OpenRead(filePath);
                            await originalFileStream.CopyToAsync(entryStream);

                            filesAdded++;
                            Console.WriteLine($"Fichier ajouté au ZIP: {fileName}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erreur avec {filePath}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"Total fichiers ajoutés au ZIP: {filesAdded}");

                    if (filesAdded == 0)
                    {
                        fileStream.Close();
                        System.IO.File.Delete(tempZipPath);
                        TempData["ErrorMessage"] = "Aucun fichier valide n'a pu être ajouté au ZIP";
                        return RedirectToPage();
                    }
                }

                // Lire le fichier ZIP créé
                var zipBytes = await System.IO.File.ReadAllBytesAsync(tempZipPath);
                var zipFileName = $"ADP_Selection_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

                Console.WriteLine($"Taille du ZIP: {zipBytes.Length} bytes");

                // Nettoyer le fichier temporaire
                System.IO.File.Delete(tempZipPath);

                return File(zipBytes, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur générale: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Erreur lors du téléchargement : {ex.Message}";
                return RedirectToPage();
            }
        }

        // Suppression des fichiers sélectionnés
        public async Task<IActionResult> OnPostDeleteSelectedFiles([FromForm] List<int> selectedIds)
        {
            try
            {
                if (!User.IsInRole("Administrateur"))
                {
                    TempData["ErrorMessage"] = "Vous n'avez pas les droits pour supprimer des fichiers";
                    return RedirectToPage();
                }

                var filesToDelete = await _gedcontext.Cartes
                    .Where(a => selectedIds.Contains(a.IdCarte))
                    .ToListAsync();

                foreach (var file in filesToDelete)
                {
                    // Supprimer le fichier physique
                    if (!string.IsNullOrEmpty(file.Lien) && System.IO.File.Exists(file.Lien))
                    {
                        System.IO.File.Delete(file.Lien);
                    }
                    // Supprimer l'enregistrement de la base
                    _gedcontext.Cartes.Remove(file);
                }

                await _gedcontext.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{filesToDelete.Count} fichier(s) supprimé(s) avec succès";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
