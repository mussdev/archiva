using System.IO.Compression;
using System.Security.Claims;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace anahged.Pages
{
    [Authorize]
    public class TraitementFicherAdpModel : PageModel
    {
        private readonly GedServices _gedservices;
       // public IList<Models.Adp> AdpList { get; set; } = new List<Models.Adp>();
        public IList<Models.Adp> AdpListEnAttenteValidation { get; set;} = new List<Models.Adp>();
        public IList<Models.Operation> OperationList { get; set; } = new List<Models.Operation>();
        public IList<SelectListItem> UserAllowedStatuts { get; set; } = new List<SelectListItem>();

        private readonly IWebHostEnvironment _environment;
        public readonly GedContext _gedcontext;

        // Handler for search functionality
        [BindProperty(SupportsGet = true)]
        public string Logement { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Document { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Client { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Boite { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Annee { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Fonctions { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Adresse { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Contact { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Ville { get; set; }

/*         [BindProperty(SupportsGet = true)]
        public string CommuneQuartier { get; set; } */

       // [BindProperty(SupportsGet = true)]
       // public string Cote { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DateDocument { get; set; }

        [BindProperty(SupportsGet = true)]
        public IFormFile Fichier { get; set; }

        [BindProperty(SupportsGet = true)]
        public int IdOpe { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int statutId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? NumeroDossierAdp { get; set; }

        [BindProperty(SupportsGet = true)]
        public int DernierStatutAdpId { get; set; }
        
        public TraitementFicherAdpModel(GedServices gedservices, GedContext gedcontext, IWebHostEnvironment environment)
        {
            _gedservices = gedservices;
            _gedcontext = gedcontext;
            _environment = environment;
        }
        public async Task OnGetAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour accéder à cette page.";
                Response.Redirect("/Login");
                return;
            }

            //ViewData["MessageRecherche"] = "Recherchez ici vos documents ADP 🔍 !";
           // var resulAdp = await _gedservices.RechercherAdpAsync(Logement, Document, Client, Boite, Code, Annee);
            
           // AdpList = resulAdp?? new List<Models.Adp>();

            if (AdpListEnAttenteValidation.Count == 0)
            {
                ViewData["MessageRecherche"] = "Aucun résultat trouvé 😓!";
            }
            // Cette méthode est appelée quand la page se charge
            // On s'assure que le ModelState est clean
            ModelState.Clear();

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
            AdpListEnAttenteValidation = await _gedcontext.Adps
                .Include(o => o.IdOpeNavigation)
                .Include(v => v.IdOpeNavigation.IdVilleNavigation)
                .Include(s => s.DernierStatutAdp)
                .Where(s => s.DernierStatutAdp.DescriptionStatut == "En attente" || s.DernierStatutAdp.DescriptionStatut == "Nouveau")
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostView(int id)
        {
          
            try
            {
                var fichierPdf = await _gedservices.GetFichierPdfAdpAsync(id);
                return File(fichierPdf, "application/pdf", string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return NotFound($"Impossible d'ouvrir le fichier: {ex.Message}");
            }
        }

        // handler pour l'enregistrement de nouveaux fichiers ADP
        public async Task<IActionResult> OnPostUploadFileAdp()
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

            // Vérification de la validité de l'id de l'opération sélectionnée et statut
            if(IdOpe <= 0)
            {
                ModelState.AddModelError("IdOpe", "Veuillez sélectionner une opération valide ❌.");
                return Page();
            }            
            if (statutId <= 0)
            {
                ModelState.AddModelError("statutId", "Veuillez sélectionner un statut valide ❌.");
                return Page();
            }
            
            try
            {
                // Récupérer l'ID de l'utilisateur connecté
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    TempData["ErrorMessage"] = "Utilisateur non authentifié. Veuillez vous connecter.";
                    return RedirectToPage("/Login");
                }

                // Console.WriteLine($"ID Utilisateur connecté : {userId}");

                var message = await _gedservices.EnregistrerFichierAdpAsync(
                    Fichier, Annee, Boite, Logement, Document,
                    DateDocument, Client, Fonctions, Adresse, Contact,
                    IdOpe, userId, statutId, NumeroDossierAdp
                );

                TempData["SuccessMessage"] = message;
                ViewData["MessageUpload"] = "Fichier ADP enregistré avec succès ✅ !";
                Console.WriteLine($"Message du service : {message}");
                // Réinitialiser les propriétés après succès
                ResetProperties();
                ModelState.Clear();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ///Console.WriteLine("=== EXCEPTION ===");
                Console.WriteLine($"Message: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                // Pour DbUpdateException, afficher les entités en conflit
                if (ex is DbUpdateException dbEx)
                {
                    foreach (var entry in dbEx.Entries)
                    {
                        Console.WriteLine($"Entité: {entry.Entity.GetType().Name}, État: {entry.State}");
                    }
                }
                ModelState.AddModelError(string.Empty, $"Erreur : {ex.Message}");
                return Page();
            }
        }
        
        private void ResetProperties()
        {
            Annee = string.Empty;
            Code = string.Empty;
            Boite = string.Empty;
            Logement = string.Empty;
            Document = string.Empty;
            DateDocument = string.Empty;
            Client = string.Empty;
            Fonctions = string.Empty;
            Adresse = string.Empty;
            Contact = string.Empty;
            Ville = string.Empty;
         //   CommuneQuartier = string.Empty;
            Fichier = null!;
        }

        // Methode pour obtenir l'ID de l'utilisateur connecté
        private int GetCurrentUserId()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return 0;
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return 0;
            }

            return userId;
        }

        // Methode de modification des enregistrements ADP
        public async Task<IActionResult> OnPostUpdateAdp()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour effectuer cette action.";
                return RedirectToPage("/Login");
            }

            try
            {
                var id = int.Parse(Request.Form["IdAdp"]);
                var adp = await _gedcontext.Adps.FindAsync(id);
                if (adp == null)
                    return NotFound();

                // Conserver une copie des anciennes valeurs pour comparaison
                var ancienAdp = new
                {
                    adp.Boite,
                    adp.Code,
                    adp.Logement,
                    adp.Client,
                    adp.Annee,
                    adp.Ville,
                    adp.Document,
                    adp.CommuneQuartier,
                    adp.Adresse,
                    adp.Contact,
                    adp.Fonctions,
                    adp.NumDossierAdp,
                    adp.IdOpe,
                    adp.DernierStatutAdpId,
                    adp.DateDocument,
                    adp.Lien
                };

                // Variables pour savoir si des changements ont eu lieu
                bool modificationChamps = false;
                bool changementStatut = false;
                bool changementFichier = false;

                // Mise à jour des champs
                adp.Boite = Request.Form["Boite"];
                if (adp.Boite != ancienAdp.Boite) modificationChamps = true;

                adp.Code = Request.Form["Code"];
                if (adp.Code != ancienAdp.Code) modificationChamps = true;

                adp.Logement = Request.Form["Logement"];
                if (adp.Logement != ancienAdp.Logement) modificationChamps = true;

                adp.Client = Request.Form["Client"];
                if (adp.Client != ancienAdp.Client) modificationChamps = true;

                adp.Annee = Request.Form["Annee"];
                if (adp.Annee != ancienAdp.Annee) modificationChamps = true;

                adp.Ville = Request.Form["Ville"];
                if (adp.Ville != ancienAdp.Ville) modificationChamps = true;

                adp.Document = Request.Form["Document"];
                if (adp.Document != ancienAdp.Document) modificationChamps = true;

                adp.CommuneQuartier = Request.Form["CommuneQuartier"];
                if (adp.CommuneQuartier != ancienAdp.CommuneQuartier) modificationChamps = true;

                adp.Adresse = Request.Form["Adresse"];
                if (adp.Adresse != ancienAdp.Adresse) modificationChamps = true;

                adp.Contact = Request.Form["Contact"];
                if (adp.Contact != ancienAdp.Contact) modificationChamps = true;

                adp.Fonctions = Request.Form["Fonctions"];
                if (adp.Fonctions != ancienAdp.Fonctions) modificationChamps = true;

                adp.NumDossierAdp = Request.Form["NumeroDossierAdp"];
                if (adp.NumDossierAdp != ancienAdp.NumDossierAdp) modificationChamps = true;

                // IdOpe
                int? newIdOpe = int.TryParse(Request.Form["IdOpe"], out int idOpe) ? idOpe : (int?)null;
                if (newIdOpe != ancienAdp.IdOpe)
                {
                    adp.IdOpe = newIdOpe;
                    modificationChamps = true;
                }

                // Statut
                int? newStatutId = int.TryParse(Request.Form["statutId"], out int statutId) ? statutId : (int?)null;
                if (newStatutId != ancienAdp.DernierStatutAdpId)
                {
                    adp.DernierStatutAdpId = newStatutId;
                    modificationChamps = true;
                    changementStatut = true;
                }

                // Date
                if (DateTime.TryParse(Request.Form["DateDocument"], out DateTime dateDoc))
                {
                    string newDate = dateDoc.ToString("dd/MM/yyyy");
                    if (newDate != ancienAdp.DateDocument)
                    {
                        adp.DateDocument = newDate;
                        modificationChamps = true;
                    }
                }

                // Gestion du fichier
                var fichier = Request.Form.Files["Fichier"];
                bool deleteFile = Request.Form["DeleteFile"] == "true";

                if (fichier != null && fichier.Length > 0)
                {
                    // Nouveau fichier uploadé → suppression de l'ancien
                    if (!string.IsNullOrEmpty(adp.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    var originalFileName = fichier.FileName;
                    var filePath = Path.Combine(_environment.WebRootPath, "ADP/NewADP/", originalFileName);
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fichier.CopyToAsync(stream);
                    }

                    adp.Lien = Path.Combine("ADP/NewADP/", originalFileName).Replace("\\", "/");
                    modificationChamps = true;
                    changementFichier = true;
                }
                else if (deleteFile && !string.IsNullOrEmpty(adp.Lien))
                {
                    // Suppression demandée sans nouveau fichier
                    var oldFilePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    adp.Lien = null;
                    modificationChamps = true;
                    changementFichier = true;
                }

                // Sauvegarder les modifications si quelque chose a changé
                if (modificationChamps)
                {
                    await _gedcontext.SaveChangesAsync();

                    // Historique général (pour toute modification)
                    var historiqueAdp = new HistoriqueAdp
                    {
                        IdAdp = adp.IdAdp,
                        UserId = GetCurrentUserId(),
                        DateHisto = DateTime.Now,
                        DateVu = DateOnly.FromDateTime(DateTime.Now),
                        TypeAction = "Modification",
                        Commentaire = $"ADP modifié - Boite: {adp.Boite}, Logement: {adp.Logement}, Client: {adp.Client}, Statut ID: {adp.DernierStatutAdpId}, Numéro Dossier: {adp.NumDossierAdp}, Opération ID: {adp.IdOpe}"
                    };
                    _gedcontext.HistoriqueAdps.Add(historiqueAdp);

                    // Validation seulement si le statut a changé
                    if (changementStatut)
                    {
                        var validationfile = new Validationsfile
                        {
                            IdAdp = adp.IdAdp,
                            UserId = GetCurrentUserId(),
                            DateValidation = DateTime.Now,
                            TypeAction = "Modification",
                            Commentaire = $"Statut modifié - Nouveau statut ID: {adp.DernierStatutAdpId}"
                        };
                        _gedcontext.Validationsfiles.Add(validationfile);
                    }

                    // Si vous voulez aussi tracer les modifications de fichier, vous pouvez ajouter un autre type d'historique
                    if (changementFichier)
                    {
                        // Par exemple, ajouter un commentaire supplémentaire dans l'historique général
                        // ou créer une autre table dédiée. Ici, on peut juste enrichir le commentaire.
                        // Pour simplifier, on peut ajouter une entrée spécifique si nécessaire.
                    }

                    await _gedcontext.SaveChangesAsync();

                    return Content("Mise à jour réussie");
                }
                else
                {
                    // Rien n'a changé : on ne fait rien, mais on peut retourner un message
                    return Content("Aucune modification détectée");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur lors de la modification: " + ex.Message);
            }
        }

        // handler methode pour supprimer le fichier ADP associé à un enregistrement ADP
        public async Task<IActionResult> OnPostDeleteFileAdp(int id)
        {
            try
            {
                if(id <= 0)
                {
                    return BadRequest("ID invalide pour la suppression du fichier.");
                }   

                var adp = await _gedcontext.Adps.FindAsync(id);
                if (adp == null)
                {
                    return NotFound();
                }

                // Supprimer le fichier physique
                if (!string.IsNullOrEmpty(adp.Lien))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Mettre à jour la base de données
                adp.Lien = null;
                await _gedcontext.SaveChangesAsync();

                return Content("Fichier supprimé avec succès");
            }
            catch (Exception ex)
            {
                // Log l'erreur
                return StatusCode(500, "Erreur lors de la suppression du fichier: " + ex.Message);
            }
        }

        // handler methode pour supprimer le fichier VPL associé à un enregistrement VPL
        public async Task<IActionResult> OnPostDeleteAdp(int id)
        {
            try
            {
                var adp = await _gedcontext.Adps.FindAsync(id);
                if (adp == null)
                {
                    return NotFound();
                }

                // Supprimer le fichier physique
                if (!string.IsNullOrEmpty(adp.Lien))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Supprimer de la base de données
                _gedcontext.Adps.Remove(adp);
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
                var selectedFiles = await _gedcontext.Adps
                    .Where(a => selectedIds.Contains(a.IdAdp))
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
                    Console.WriteLine($"Vérification fichier ID {file.IdAdp}: {file.Lien}");

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
                            var fileName = $"{file.Boite}_{file.Code}_{Path.GetFileName(file.Lien)}";
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
                                    var fileName = $"{file.Boite}_{file.Code}_{Path.GetFileName(file.Lien)}";
                                    validFiles.Add((altPath, fileName));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Chemin vide pour l'ID {file.IdAdp}");
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

                var filesToDelete = await _gedcontext.Adps
                    .Where(a => selectedIds.Contains(a.IdAdp))
                    .ToListAsync();

                foreach (var file in filesToDelete)
                {
                    // Supprimer le fichier physique
                    if (!string.IsNullOrEmpty(file.Lien) && System.IO.File.Exists(file.Lien))
                    {
                        System.IO.File.Delete(file.Lien);
                    }
                    // Supprimer l'enregistrement de la base
                    _gedcontext.Adps.Remove(file);
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
