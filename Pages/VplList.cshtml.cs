using System.IO.Compression;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace anahged.Pages
{
    public class VplListModel : PageModel
    {
        private readonly GedServices _gedServices;
        //public IList<Vpl> VplList { get; set; } = default!;
        public IList<Vpl> VplList { get; set; } = new List<Vpl>();
        public readonly GedContext _gedContext;
        private readonly IWebHostEnvironment _environment;

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


        public VplListModel(GedServices gedservices, GedContext gedContext, IWebHostEnvironment environment)
        {
            _gedServices = gedservices;
            _gedContext = gedContext;
            _environment = environment;
        }
       /*  public void OnGet()
        {
        } */

        public async Task OnGetAsync()
        {
            VplList = await _gedServices.RechercherVlpAsync(Document, Annee, Client, Logement, Boite, Code);

            if (VplList == null || VplList.Count == 0)
            {
                ViewData["MessageRechercheVpl"] = "Aucun résultat trouvé 😓!";
            }

            ModelState.Clear();
        }

        public async Task<IActionResult> OnPostView(int id)
        {
            try
            {
                // Diagnostic temporaire
                //await _gedServices.TestFichierVpl(id);

                var fichierPdf = await _gedServices.GetFichierPdfVplAsync(id);
                return File(fichierPdf, "application/pdf", string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans OnPostView: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return NotFound($"Impossible d'ouvrir le fichier: {ex.Message}");
            }
        }

        // Rappelle de la methode d'enregistrement de nouveau fichier ADP dans GedServices
        public async Task<IActionResult> OnPostUploadFileVplAsync()
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

                var message = await _gedServices.EnregistrerFichierVplAsync(
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

        // Methode pour supprimer le fichier VPL associé à un enregistrement
        public async Task<IActionResult> OnPostDeleteFileVpl(int id)
        {
            try
            {
                var vpl = await _gedContext.Vpls.FindAsync(id);
                if (vpl == null)
                {
                    return NotFound();
                }

                // Supprimer le fichier physique
                if (!string.IsNullOrEmpty(vpl.Lien))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, vpl.Lien);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Mettre à jour la base de données
                vpl.Lien = null;
                await _gedContext.SaveChangesAsync();

                return Content("Fichier supprimé avec succès");
            }
            catch (Exception ex)
            {
                // Log l'erreur
                return StatusCode(500, "Erreur lors de la suppression du fichier: " + ex.Message);
            }
        }
        
        // Methode pour modifier les informations d'un enregistrement VPL
        public async Task<IActionResult> OnPostUpdateVpl()
        {
            try
            {
                var id = int.Parse(Request.Form["IdVpl"]);
                var vpl = await _gedContext.Vpls.FindAsync(id);
                if (vpl == null)
                {
                    return NotFound();
                }

                // Mettre à jour les champs
                vpl.Boite = Request.Form["Boite"];
                vpl.Code = Request.Form["Code"];
                vpl.Logement = Request.Form["Logement"];
                vpl.Client = Request.Form["Client"];
                vpl.Annee = Request.Form["Annee"];
                vpl.Ville = Request.Form["Ville"];
                vpl.Document = Request.Form["Document"];
                vpl.CommuneQuartier = Request.Form["CommuneQuartier"];
                vpl.Adresse = Request.Form["Adresse"];
                vpl.Contact = Request.Form["Contact"];
                vpl.Fonctions = Request.Form["Fonctions"];

                // Gérer la date
                if (DateTime.TryParse(Request.Form["DateDocument"], out DateTime dateDoc))
                {
                    vpl.DateDocument = dateDoc.ToString("dd/MM/yyyy");
                }

                // Gérer le nouveau fichier uploadé
                var fichier = Request.Form.Files["Fichier"];
                if (fichier != null && fichier.Length > 0)
                {
                    // Supprimer l'ancien fichier s'il existe
                    if (!string.IsNullOrEmpty(vpl.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, vpl.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // CORRECTION : Utiliser le nom complet du fichier avec extension
                    var fileName = fichier.FileName; // Nom complet avec extension
                    var filePath = Path.Combine(_environment.WebRootPath, "SICOGI/NewSICOGI/", fileName);

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

                    vpl.Lien = Path.Combine("SICOGI/NewSICOGI/", fileName).Replace("\\", "/");
                }
                // Gérer la suppression du fichier (si le flag est true et pas de nouveau fichier)
                else if (Request.Form["DeleteFile"] == "true")
                {
                    if (!string.IsNullOrEmpty(vpl.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, vpl.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    vpl.Lien = null;
                }

                await _gedContext.SaveChangesAsync();

                return Content("Mise à jour réussie");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erreur lors de la modification: " + ex.Message);
            }
        }

        // Methode pour supprimer le fichier VPL associé à un enregistrement VPL
        public async Task<IActionResult> OnPostDeleteVpl(int id)
        {
            try
            {
                var vpl = await _gedContext.Vpls.FindAsync(id);
                if (vpl == null)
                {
                    return NotFound();
                }

                // Supprimer le fichier physique
                if (!string.IsNullOrEmpty(vpl.Lien))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, vpl.Lien);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Supprimer de la base de données
                _gedContext.Vpls.Remove(vpl);
                await _gedContext.SaveChangesAsync();

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
                var selectedFiles = await _gedContext.Vpls
                    .Where(a => selectedIds.Contains(a.IdVpl))
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
                    Console.WriteLine($"Vérification fichier ID {file.IdVpl}: {file.Lien}");

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
                        Console.WriteLine($"Chemin vide pour l'ID {file.IdVpl}");
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

                var filesToDelete = await _gedContext.Vpls
                    .Where(a => selectedIds.Contains(a.IdVpl))
                    .ToListAsync();

                foreach (var file in filesToDelete)
                {
                    // Supprimer le fichier physique
                    if (!string.IsNullOrEmpty(file.Lien) && System.IO.File.Exists(file.Lien))
                    {
                        System.IO.File.Delete(file.Lien);
                    }
                    // Supprimer l'enregistrement de la base
                    _gedContext.Vpls.Remove(file);
                }

                await _gedContext.SaveChangesAsync();
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
