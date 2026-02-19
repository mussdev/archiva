using System.IO.Compression;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace anahged.Pages
{
    public class CartesListModel : PageModel
    {

        private readonly GedServices _gedServices;
        private readonly IWebHostEnvironment _environment;
        public readonly GedContext _gedcontext;

        public CartesListModel(GedServices gedServices, IWebHostEnvironment environment, GedContext context)
        {
            _gedServices = gedServices;
            _environment = environment;
            _gedcontext = context;
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

        // Réinitialiser les propriétés après un enregistrement réussi
        private void ResetProperties()
        {
            Tube = Ville = Quartier = Operation = Legende = Originalite = Echelle = Cote = DateCarte = string.Empty;
            Fichier = null!;
        }

        // Méthode pour obtenir l'ID de l'utilisateur connecté
        private int GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                throw new Exception("Utilisateur non authentifié.");

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new Exception("Impossible de récupérer l'ID utilisateur. Veuillez vous reconnecter.");

            return userId;
        }


        // Methode pour modifier les informations d'un enregistrement d'une carte
        public async Task<IActionResult> OnPostUpdateCarte()
        {
            try
            {
                var id = int.Parse(Request.Form["IdCarte"]);
                var carte = await _gedcontext.Cartes.FindAsync(id);
                if (carte == null)
                {
                    return NotFound();
                }

                // Mettre à jour les champs
                carte.Tube = Request.Form["Tube"];
                carte.Legende = Request.Form["Legende"];
                carte.DateCarte = Request.Form["DateCarte"];
                carte.Operation = Request.Form["Operation"];
                carte.Echelle = Request.Form["Echelle"];
                carte.Originalite = Request.Form["Originalite"];
                carte.Cote = Request.Form["Cote"];
                carte.Ville = Request.Form["Ville"];
                carte.Quartier = Request.Form["Quartier"];          
                // Gérer la date
                if (DateTime.TryParse(Request.Form["DateCarte"], out DateTime dateDoc))
                {
                    carte.DateCarte = dateDoc.ToString("dd/MM/yyyy");
                }

                // Gérer le nouveau fichier uploadé
                var fichier = Request.Form.Files["Fichier"];
                if (fichier != null && fichier.Length > 0)
                {
                    // Supprimer l'ancien fichier s'il existe
                    if (!string.IsNullOrEmpty(carte.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, carte.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // UTILISER LE NOM ORIGINAL DU FICHIER AVEC EXTENSION
                    var originalFileName = fichier.FileName; // Récupère le nom complet avec extension
                    var filePath = Path.Combine(_environment.WebRootPath, "CARTES/NewCARTES/", originalFileName);
                    
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
                    
                    carte.Lien = Path.Combine("CARTES/NewCARTES/", originalFileName).Replace("\\", "/");
                }
                // Gérer la suppression du fichier (si le flag est true et pas de nouveau fichier)
                else if (Request.Form["DeleteFile"] == "true")
                {
                    if (!string.IsNullOrEmpty(carte.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, carte.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    carte.Lien = null;
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
