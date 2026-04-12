using System.IO.Compression;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace anahged.Pages
{
    public class VplListModel : PageModel
    {
        private readonly GedServices _gedServices;
        public IList<Models.Operation> OperationList { get; set; } = new List<Models.Operation>();
        public IList<SelectListItem> UserAllowedStatuts { get; set; } = new List<SelectListItem>();
        public IList<Vpl> VplList { get; set; } = new List<Vpl>();
        public readonly GedContext _gedContext;
        private readonly IWebHostEnvironment _environment;

        // Handler for search functionality
        [BindProperty(SupportsGet = true)]
        public string Logement { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Document { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Client { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Boite { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Annee { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Fonctions { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Adresse { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Contact { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Ville { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string CommuneQuartier { get; set; } = string.Empty;

       // [BindProperty(SupportsGet = true)]
       // public string Cote { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DateDocument { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public IFormFile? Fichier { get; set; }

        [BindProperty(SupportsGet = true)]
        public int IdOpe { get; set; }

        [BindProperty(SupportsGet = true)]
        public string NumeroDossierVpl { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int StatutId { get; set; }


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
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour accéder à cette page.";
                Response.Redirect("/Login");
                return;
            }

            VplList = await _gedServices.RechercherVlpAsync(Document, Annee, Client, Logement, Boite, Code);

            if (VplList == null || VplList.Count == 0)
            {
                ViewData["MessageRechercheVpl"] = "Aucun résultat trouvé 😓!";
            }

            ModelState.Clear();

            // Charger la liste des opérations pour le dropdown
            OperationList = await _gedContext.Operations.Include(o => o.IdVilleNavigation).ToListAsync();

            // Récuperer les statuts liés à l'utilisateur connecté
            UserAllowedStatuts = await _gedContext.Userstatuts
                .Where(us => us.UserId == GetCurrentUserId())
                .Join(_gedContext.Statuts,
                    userstatut => userstatut.IdStatut,
                    statut => statut.IdStatut,
                    (userstatut, statut) => new SelectListItem
                    {
                        Value = statut.IdStatut.ToString(),
                        Text = statut.DescriptionStatut
                    })
                .Distinct()
                .ToListAsync();
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

        // handler methode pour enregistrer un nouveau fichier VPL
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
               // Console.WriteLine($"ID Utilisateur connecté : {userId}");

                var message = await _gedServices.EnregistrerFichierVplAsync(
                    Fichier, Annee, Boite, Logement, Document,
                    DateDocument, Client, Fonctions, Adresse, Contact,
                     userId, IdOpe, NumeroDossierVpl, StatutId);

                TempData["SuccessMessage"] = message;
                ViewData["MessageUpload"] = "Fichier VPL enregistré avec succès ✅ !";

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
            if (User?.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("Utilisateur non authentifié.");
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new InvalidOperationException("Impossible de récupérer l'ID utilisateur. Veuillez vous reconnecter.");
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
                vpl.Lien = string.Empty;
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
            // Vérification de l'authentification (optionnelle, adaptez selon votre besoin)
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour effectuer cette action.";
                return RedirectToPage("/Login");
            }

            try
            {
                // Récupération de l'ID
                if (!int.TryParse(Request.Form["IdVpl"], out int id))
                    return BadRequest("ID invalide");

                var vpl = await _gedContext.Vpls.FindAsync(id);
                if (vpl == null)
                    return NotFound();

                // Conservation des anciennes valeurs
                var ancienVpl = new
                {
                    vpl.Boite,
                    vpl.Code,
                    vpl.IdOpe,
                    vpl.Logement,
                    vpl.Client,
                    vpl.Annee,
                    vpl.Ville,
                    vpl.Document,
                    vpl.CommuneQuartier,
                    vpl.Adresse,
                    vpl.Contact,
                    vpl.Fonctions,
                    vpl.NumDossierVpl,
                    vpl.DernierStatutVplId,
                    vpl.DateDocument,
                    vpl.Lien
                };

                bool modificationChamps = false;
                bool changementStatut = false;
                bool changementFichier = false;

                // Mise à jour des champs simples
                vpl.Boite = Request.Form["Boite"].ToString() ?? "";
                if (vpl.Boite != ancienVpl.Boite) modificationChamps = true;

                // Gestion de l'opération (IdOpe)
                int? newIdOpe = int.TryParse(Request.Form["IdOpe"], out int idOpe) ? idOpe : (int?)null;
                if (newIdOpe != ancienVpl.IdOpe)
                {
                    vpl.IdOpe = newIdOpe;
                    modificationChamps = true;
                    // Optionnel : mettre à jour le champ Code avec le code de l'opération
                    if (newIdOpe.HasValue)
                    {
                        var operation = await _gedContext.Operations.FindAsync(newIdOpe.Value);
                        vpl.Code = operation?.CodeOpe ?? "";
                    }
                    else
                    {
                        vpl.Code = "";
                    }
                }

                vpl.Logement = Request.Form["Logement"].ToString() ?? "";
                if (vpl.Logement != ancienVpl.Logement) modificationChamps = true;

                vpl.Client = Request.Form["Client"].ToString() ?? "";
                if (vpl.Client != ancienVpl.Client) modificationChamps = true;

                vpl.Annee = Request.Form["Annee"].ToString() ?? "";
                if (vpl.Annee != ancienVpl.Annee) modificationChamps = true;

                vpl.Ville = Request.Form["Ville"].ToString() ?? "";
                if (vpl.Ville != ancienVpl.Ville) modificationChamps = true;

                vpl.Document = Request.Form["Document"].ToString() ?? "";
                if (vpl.Document != ancienVpl.Document) modificationChamps = true;

                vpl.CommuneQuartier = Request.Form["CommuneQuartier"].ToString() ?? "";
                if (vpl.CommuneQuartier != ancienVpl.CommuneQuartier) modificationChamps = true;

                vpl.Adresse = Request.Form["Adresse"].ToString() ?? "";
                if (vpl.Adresse != ancienVpl.Adresse) modificationChamps = true;

                vpl.Contact = Request.Form["Contact"].ToString() ?? "";
                if (vpl.Contact != ancienVpl.Contact) modificationChamps = true;

                vpl.Fonctions = Request.Form["Fonctions"].ToString() ?? "";
                if (vpl.Fonctions != ancienVpl.Fonctions) modificationChamps = true;

                vpl.NumDossierVpl = Request.Form["NumeroDossierVpl"].ToString() ?? "";
                if (vpl.NumDossierVpl != ancienVpl.NumDossierVpl) modificationChamps = true;

                // Statut
                int? newStatutId = int.TryParse(Request.Form["statutId"], out int statutId) ? statutId : (int?)null;
                if (newStatutId != ancienVpl.DernierStatutVplId)
                {
                    vpl.DernierStatutVplId = newStatutId;
                    modificationChamps = true;
                    changementStatut = true;
                }

                // Date
                if (DateTime.TryParse(Request.Form["DateDocument"], out DateTime dateDoc))
                {
                    string newDate = dateDoc.ToString("dd/MM/yyyy");
                    if (newDate != ancienVpl.DateDocument)
                    {
                        vpl.DateDocument = newDate;
                        modificationChamps = true;
                    }
                }

                // Gestion du fichier
                var fichier = Request.Form.Files["Fichier"];
                bool deleteFile = Request.Form["DeleteFile"] == "true";

                if (fichier != null && fichier.Length > 0)
                {
                    // Supprimer l'ancien fichier s'il existe
                    if (!string.IsNullOrEmpty(vpl.Lien))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, vpl.Lien);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    // Sauvegarder le nouveau fichier
                    var originalFileName = fichier.FileName;
                    var filePath = Path.Combine(_environment.WebRootPath, "SICOGI/NewSICOGI/", originalFileName);
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fichier.CopyToAsync(stream);
                    }

                    vpl.Lien = Path.Combine("SICOGI/NewSICOGI/", originalFileName).Replace("\\", "/");
                    modificationChamps = true;
                    changementFichier = true;
                }
                else if (deleteFile && !string.IsNullOrEmpty(vpl.Lien))
                {
                    // Suppression du fichier sans remplacement
                    var oldFilePath = Path.Combine(_environment.WebRootPath, vpl.Lien);
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    vpl.Lien = null;
                    modificationChamps = true;
                    changementFichier = true;
                }

                // Si aucune modification, on peut retourner directement
                if (!modificationChamps)
                {
                    return Content("Aucune modification détectée");
                }

                // Sauvegarde des modifications
                await _gedContext.SaveChangesAsync();

                // Enregistrement de l'historique général
                var historiqueVpl = new HistoriqueVpl
                {
                    IdVpl = vpl.IdVpl,
                    UserId = GetCurrentUserId(), // À implémenter selon votre méthode d'authentification
                    DateHisto = DateTime.Now,
                    DateVu = DateOnly.FromDateTime(DateTime.Now),
                    TypeAction = "Modification",
                    Commentaire = $"VPL modifié - Boite: {vpl.Boite}, Logement: {vpl.Logement}, Client: {vpl.Client}, Statut ID: {vpl.DernierStatutVplId}, Numéro Dossier: {vpl.NumDossierVpl}, Opération ID: {vpl.IdOpe}"
                };
                _gedContext.HistoriqueVpls.Add(historiqueVpl);

                // Si changement de statut, ajouter une entrée dans Validationsfiles (ou une table dédiée)
                if (changementStatut)
                {
                    var validationfile = new Validationsfile
                    {
                        IdVpl = vpl.IdVpl,   // Supposons que Validationsfile ait une propriété IdVpl
                        UserId = GetCurrentUserId(),
                        DateValidation = DateTime.Now,
                        TypeAction = "Modification",
                        Commentaire = $"Statut modifié - Nouveau statut ID: {vpl.DernierStatutVplId}"
                    };
                    _gedContext.Validationsfiles.Add(validationfile);
                }

                // Optionnel : tracer les modifications de fichier si nécessaire
                if (changementFichier)
                {
                    // Vous pouvez ajouter une autre entrée d'historique ou enrichir le commentaire
                    // Par exemple, on pourrait ajouter une ligne dans une table dédiée "FichierHistorique"
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

        // Methode pour afficher le PDF dans une nouvelle fenêtre
        public async Task<IActionResult> OnGetPdfData(int id)
        {
            try
            {
                Console.WriteLine($"Tentative de récupération du PDF pour ID {id}");
                var fichierPdf = await _gedServices.GetFichierPdfVplAsync(id);
                
                if (fichierPdf == null || fichierPdf.Length == 0)
                    return NotFound("Le fichier est vide.");
                
                // Vérifier les 4 premiers octets (doivent être %PDF)
                if (fichierPdf.Length >= 4 && 
                    fichierPdf[0] == 0x25 && fichierPdf[1] == 0x50 && 
                    fichierPdf[2] == 0x44 && fichierPdf[3] == 0x46)
                {
                    Console.WriteLine("Signature PDF valide détectée.");
                }
                else
                {
                    Console.WriteLine("ATTENTION : Signature PDF invalide.");
                    return BadRequest("Le fichier n'est pas un PDF valide.");
                }
                
                Response.Headers.Append("Content-Disposition", "inline; filename=document.pdf");
                return File(fichierPdf, "application/pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans OnGetPdfData : {ex.Message}");
                return NotFound(ex.Message);
            }
        }
    }
}
