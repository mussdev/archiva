using System.IO.Compression;
using System.Security.Claims;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace anahged.Pages
{
    public class AdpListModel : PageModel
    {
        private readonly GedServices _gedservices;
        public IList<Models.Adp> AdpList { get; set; } = default!;
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

        [BindProperty(SupportsGet = true)]
        public string CommuneQuartier { get; set; }

       // [BindProperty(SupportsGet = true)]
       // public string Cote { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DateDocument { get; set; }

        [BindProperty(SupportsGet = true)]
        public IFormFile Fichier { get; set; }



        public AdpListModel(GedServices gedservices, GedContext gedcontext, IWebHostEnvironment environment)
        {
            _gedservices = gedservices;
            _gedcontext = gedcontext;
            _environment = environment;
        }
        public async Task OnGetAsync()
        {
            //ViewData["MessageRecherche"] = "Recherchez ici vos documents ADP 🔍 !";
            AdpList = await _gedservices.RechercherAdpAsync(Logement, Document, Client, Boite, Code, Annee);

            if (AdpList == null || AdpList.Count == 0)
            {
                ViewData["MessageRecherche"] = "Aucun résultat trouvé 😓!";
            }
            // Cette méthode est appelée quand la page se charge
            // On s'assure que le ModelState est clean
            ModelState.Clear();
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

        // Rappelle de la methode d'enregistrement de nouveau fichier ADP dans GedServices
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

            try
            {
                // Récupérer l'ID de l'utilisateur connecté
                var userId = GetCurrentUserId();
                Console.WriteLine($"ID Utilisateur connecté : {userId}");

                var message = await _gedservices.EnregistrerFichierAdpAsync(
                    Fichier, Annee, Code, Boite, Logement, Document,
                    DateDocument, Client, Fonctions, Adresse, Contact,
                    Ville, CommuneQuartier, userId);

                TempData["SuccessMessage"] = message;
                ViewData["MessageUpload"] = "Fichier ADP enregistré avec succès ✅ !";

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
            CommuneQuartier = string.Empty;
            Fichier = null!;
        }

        // Methode pour obtenir l'ID de l'utilisateur connecté
        private int GetCurrentUserId()
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
        }
/* 
        // A supprimer
        public async Task<IActionResult> OnPostTestAsync()
        {
            Console.WriteLine("Test endpoint called");
            return Content("Test OK");
        }

        public async Task<IActionResult> OnPostDeleteFileAdp(int id)
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
        
        public async Task<IActionResult> OnPostUpdateAdp()
        {
            try
            {
                var id = int.Parse(Request.Form["IdAdp"]);
                var adp = await _gedcontext.Adps.FindAsync(id);
                if (adp == null)
                {
                    return NotFound();
                }

                // Mettre à jour les champs
                adp.Boite = Request.Form["Boite"];
                adp.Code = Request.Form["Code"];
                adp.Logement = Request.Form["Logement"];
                adp.Client = Request.Form["Client"];
                adp.Annee = Request.Form["Annee"];
                adp.Ville = Request.Form["Ville"];
                adp.Document = Request.Form["Document"];
                adp.CommuneQuartier = Request.Form["CommuneQuartier"];
                adp.Adresse = Request.Form["Adresse"];
                adp.Contact = Request.Form["Contact"];
                adp.Fonctions = Request.Form["Fonctions"];
                
                // Gérer la date
                if (DateTime.TryParse(Request.Form["DateDocument"], out DateTime dateDoc))
                {
                    adp.DateDocument = dateDoc.ToString("dd/MM/yyyy");
                }

                // Gérer le nouveau fichier uploadé
                var fichier = Request.Form.Files["Fichier"];
                if (fichier != null && fichier.Length > 0)
                {
                    // Supprimer l'ancien fichier s'il existe
                    if (!string.IsNullOrEmpty(adp.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // UTILISER LE NOM ORIGINAL DU FICHIER AVEC EXTENSION
                    var originalFileName = fichier.FileName; // Récupère le nom complet avec extension
                    var filePath = Path.Combine(_environment.WebRootPath, "ADP/NewADP/", originalFileName);
                    
                    // Créer le dossier s'il n'existe pas
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fichier.CopyToAsync(stream);
                    }
                    
                    adp.Lien = Path.Combine("ADP/NewADP/", originalFileName).Replace("\\", "/");
                }
                // Gérer la suppression du fichier (si le flag est true et pas de nouveau fichier)
                else if (Request.Form["DeleteFile"] == "true")
                {
                    if (!string.IsNullOrEmpty(adp.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    adp.Lien = null;
                }

                await _gedcontext.SaveChangesAsync();
                
                return Content("Mise à jour réussie");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur lors de la modification: " + ex.Message);
            }
        }

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
        } */

        // Methode pour modifier les informations d'un enregistrement ADP

         public async Task<IActionResult> OnPostUpdateAdp()
        {
            try
            {
                var id = int.Parse(Request.Form["IdAdp"]);
                var adp = await _gedcontext.Adps.FindAsync(id);
                if (adp == null)
                {
                    return NotFound();
                }

                // Mettre à jour les champs
                adp.Boite = Request.Form["Boite"];
                adp.Code = Request.Form["Code"];
                adp.Logement = Request.Form["Logement"];
                adp.Client = Request.Form["Client"];
                adp.Annee = Request.Form["Annee"];
                adp.Ville = Request.Form["Ville"];
                adp.Document = Request.Form["Document"];
                adp.CommuneQuartier = Request.Form["CommuneQuartier"];
                adp.Adresse = Request.Form["Adresse"];
                adp.Contact = Request.Form["Contact"];
                adp.Fonctions = Request.Form["Fonctions"];
                
                // Gérer la date
                if (DateTime.TryParse(Request.Form["DateDocument"], out DateTime dateDoc))
                {
                    adp.DateDocument = dateDoc.ToString("dd/MM/yyyy");
                }

                // Gérer le nouveau fichier uploadé
                var fichier = Request.Form.Files["Fichier"];
                if (fichier != null && fichier.Length > 0)
                {
                    // Supprimer l'ancien fichier s'il existe
                    if (!string.IsNullOrEmpty(adp.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // UTILISER LE NOM ORIGINAL DU FICHIER AVEC EXTENSION
                    var originalFileName = fichier.FileName; // Récupère le nom complet avec extension
                    var filePath = Path.Combine(_environment.WebRootPath, "ADP/NewADP/", originalFileName);
                    
                    // Créer le dossier s'il n'existe pas
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fichier.CopyToAsync(stream);
                    }
                    
                    adp.Lien = Path.Combine("ADP/NewADP/", originalFileName).Replace("\\", "/");
                }
                // Gérer la suppression du fichier (si le flag est true et pas de nouveau fichier)
                else if (Request.Form["DeleteFile"] == "true")
                {
                    if (!string.IsNullOrEmpty(adp.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, adp.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    adp.Lien = null;
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

        // Methode pour supprimer le fichier VPL associé à un enregistrement VPL
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
