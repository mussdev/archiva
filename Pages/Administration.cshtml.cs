using System.Text.Json;
using anahged.Data;
using anahged.Models;
using anahged.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace anahged.Pages
{
    public class AdministrationModel : PageModel
    {
        private readonly AdministrationService _administrationService;
        private readonly GedContext _gedContext;
        private readonly ILogger<AdministrationModel> _logger;
        public IList<Models.User> UserList { get; set; } = default!;
        public IList<Models.Groupe> RoleList { get; set; } = default!;
        public IList<Models.Statut> StatutList { get; set; } = default!;
        public IList<Models.Operation> OperationList { get; set; } = default!;
        public IList<Models.Ville> VilleList { get; set; } = default!;
        public IList<Models.Validationsfile> ValidationList { get; set; } = default!;
        public IList<Models.HistoriqueAdp> HistoriqueAdpList { get; set; } = default!;
        public IList<Models.HistoriqueVpl> HistoriqueVplList { get; set; } = default!;
        public IList<Models.HistoriqueCarte> HistoriqueCarteList { get; set; } = default!;
        public IList<Models.Usersession> UserSessionList { get; set; } = default!;
        public IList<Models.Userconnexionlog> UserConnexionLogList { get; set; } = default!;
      /*   public int ValidationPageIndex { get; set; } = 1;
        public int ValidationPageSize { get; set; } = 10;
        public int ValidationTotalPages { get; set; }
        public int ValidationTotalCount { get; set; } */

        [BindProperty]
        public IFormFile? ExcelFile { get; set; }
        [BindProperty]
        public bool SkipFirstRow { get; set; } = true;
        [BindProperty]
        public bool SkipFirstColumn { get; set; } = true;
        [BindProperty]
        public bool OverwriteExisting { get; set; } = false;   

        public AdministrationModel(AdministrationService administrationService, GedContext gedContext, ILogger<AdministrationModel> logger)
        {
            _administrationService = administrationService;
            _gedContext = gedContext;
            _logger = logger;
        }
        public void OnGet()
        {
            // Vérifier si l'utilisateur est connecté
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour accéder à cette page.";
                Response.Redirect("/Login");
                return;
            }

            UserList = _administrationService.GetAllUsers().ToList();
            RoleList = _administrationService.GetAllRoles().ToList();
            // Récupérer la liste des statuts
            StatutList = _administrationService.GetAllStatuts().ToList();
            OperationList = _administrationService.GetOperationList().ToList();
            VilleList = _administrationService.GetAllVilles().ToList();

            // Récupérer la liste des validations
            ValidationList = _administrationService.GetAllValidations().ToList();
            // Récupérer la liste des historiques de traitement des fichiers Adp
            HistoriqueAdpList = _administrationService.GetAllHistoriqueAdp().ToList();
            // Récupérer la liste des historiques de traitement des fichiers Vpl
            HistoriqueVplList = _administrationService.GetAllHistoriqueVpl().ToList();
            // Récupérer la liste des historiques de traitement des fichiers Carte
            HistoriqueCarteList = _administrationService.GetAllHistoriqueCarte().ToList();
            // Récupérer la liste des sessions utilisateurs
            UserSessionList = _administrationService.GetAllUserSessions().ToList();
            // Récupérer la liste des logs de connexion des utilisateurs
            UserConnexionLogList = _administrationService.GetAllUserConnexionLogs().ToList();
        }

        // Handler pour la modification d'utilisateur
        public async Task<IActionResult> OnPostEditUserAsync(int userId, string nom, string prenom, string email, string contact, int groupeId, int actif, string password, string statuts)
        {
            try
            {
                //Console.WriteLine($"Début de modification pour l'utilisateur {userId}");
                //Console.WriteLine($"Statuts reçus: {statuts}");

                // Convertir la chaîne de statuts en liste d'entiers
                var statutIds = new List<int>();
                if (!string.IsNullOrEmpty(statuts))
                {
                    statutIds = statuts.Split(',')
                        .Select(s => int.TryParse(s, out var id) ? id : 0)
                        .Where(id => id > 0)
                        .ToList();
                }

                // Créer l'objet User avec les données
                var user = new User
                {
                    UserId = userId,
                    NomUser = nom,
                    PrenomUser = prenom,
                    Email = email,
                    Contact = contact,
                    GroupeId = groupeId,
                    Actif = actif
                };

                // Appeler le service de mise à jour
                var result = await _administrationService.UpdateUserAsync(user, statutIds, password);

                if (result)
                {
                    return new JsonResult(new { success = true, message = "Utilisateur modifié avec succès" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Erreur lors de la modification" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return new JsonResult(new { success = false, message = $"Erreur: {ex.Message}" });
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
            public List<int> StatutIds { get; set; } = new List<int>();
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

                var result = await _administrationService.CreateUserAsync(newUser, request.Password, request.StatutIds);
                
                return new JsonResult(new
                {
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

        // Handler pour la création de statut
        public async Task<IActionResult> OnPostCreateStatutAsync(string codeStatut, string descriptionStatut, string noteStatut)
        {
            var result = await _administrationService.CreateStatutAsync(codeStatut, descriptionStatut, noteStatut);

            if (result)
            {
                return new JsonResult(new { success = true, message = "Statut créé avec succès" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Erreur lors de la création du statut" });
            }
        }

        // Handler pour la suppression de plusieurs statuts
        public async Task<IActionResult> OnPostDeleteStatutsAsync([FromBody] List<int> statutIds)
        {
            if (statutIds == null || !statutIds.Any())
            {
                return new JsonResult(new { success = false, message = "Aucun statut sélectionné" });
            }

            try
            {
                var result = await _administrationService.DeleteStatutsAsync(statutIds);

                if (result)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = $"{statutIds.Count} statut(s) supprimé(s) avec succès",
                        deletedCount = statutIds.Count
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Erreur lors de la suppression des statuts" 
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = $"Erreur serveur: {ex.Message}" 
                });
            }
        }

        // Handler pour la modification de statut - Version optimisée
        public async Task<IActionResult> OnPostEditStatutAsync(int idStatut, string codeStatut, string descriptionStatut, string noteStatut)
        {
            try
            {
                var result = await _administrationService.UpdateStatutAsync(idStatut, codeStatut, descriptionStatut, noteStatut);

                if (result)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = "Statut modifié avec succès",
                        id = idStatut,
                        code = codeStatut,
                        description = descriptionStatut,
                        note = noteStatut
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Erreur lors de la modification du statut" 
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = $"Exception: {ex.Message}" 
                });
            }
        }

                
        /* Fin de la gestion des statuts */

        /* Gestion des opérations */   
        // Handler pour la création d'opération
        public async Task<IActionResult> OnPostCreateOperationAsync(string codeOperation, string descriptionOperation, int villeId)
        {
            var result = await _administrationService.CreateOperationAsync(codeOperation, descriptionOperation, villeId);

            if (result)
            {
                return new JsonResult(new { success = true, message = "Opération créée avec succès" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Erreur lors de la création de l'opération" });
            }
        }

         // Handler pour la modification de opération - Version optimisée  
        public async Task<IActionResult> OnPostEditOperationAsync(int idOpe, string codeOpe, string descriptionOpe, int? villeId)
        {
            try
            {
                // Validation des données
                if (string.IsNullOrWhiteSpace(codeOpe))
                    return new JsonResult(new { 
                        success = false, 
                        message = "Le code est obligatoire" 
                    });

                if (string.IsNullOrWhiteSpace(descriptionOpe))
                    return new JsonResult(new { 
                        success = false, 
                        message = "La description est obligatoire" 
                    });

                var result = await _administrationService.UpdateOperationAsync(idOpe, codeOpe, descriptionOpe, villeId);

                if (result)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = "Opération modifiée avec succès",
                        data = new {
                            id = idOpe,
                            codeOpe = codeOpe,
                            descriptionOpe = descriptionOpe,
                            villeId = villeId
                        }
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Erreur lors de la modification de l'opération" 
                    });
                }
            }
            catch (InvalidOperationException ex)
            {
                // Gestion spécifique pour le code dupliqué
                return new JsonResult(new { 
                    success = false, 
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = $"Exception: {ex.Message}" 
                });
            }
        }    

        // Handler pour la suppression de plusieurs opérations non liées
        public async Task<IActionResult> OnPostDeleteOperationsAsync([FromBody] List<int> operationIds)
        {
            if (operationIds == null || !operationIds.Any())
            {
                return new JsonResult(new { success = false, message = "Aucune opération sélectionnée" });
            }

            try
            {
                var result = await _administrationService.DeleteOperationsAsync(operationIds);
                if (result)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = $"{operationIds.Count} opération(s) supprimée(s) avec succès",
                        deletedCount = operationIds.Count
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Erreur lors de la suppression des opérations" 
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = $"Erreur serveur: {ex.Message}" 
                });
            }
        }

        // Methode d'importation des opérations à partir d'un fichier Excel
        public async Task<JsonResult> OnPostImportOperationsAsync()
        {
            try
            {
                if (ExcelFile == null || ExcelFile.Length == 0)
                {
                    return new JsonResult(new { success = false, message = "Aucun fichier sélectionné" });
                }

                // Vérifier l'extension
                var extension = Path.GetExtension(ExcelFile.FileName).ToLower();
                if (extension != ".xlsx" && extension != ".xls")
                {
                    return new JsonResult(new { success = false, message = "Format de fichier non supporté" });
                }

                // Lire le fichier Excel
                using var stream = new MemoryStream();
                await ExcelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();
                
                int startRow = SkipFirstRow ? 2 : 1; // Ignorer l'en-tête si demandé
                int addedCount = 0;
                int updatedCount = 0;
                int errorCount = 0;
                int totalCount = 0;

                for (int i = startRow; i <= rows.Count(); i++)
                {
                    try
                    {
                        var codeCell = worksheet.Cell(i, 1);
                        var descCell = worksheet.Cell(i, 2);

                        if (codeCell.IsEmpty() || descCell.IsEmpty())
                            continue;

                        string codeOperation = codeCell.GetValue<string>().Trim();
                        string descriptionOperation = descCell.GetValue<string>().Trim();

                        if (string.IsNullOrEmpty(codeOperation) || string.IsNullOrEmpty(descriptionOperation))
                        {
                            errorCount++;
                            continue;
                        }

                        totalCount++;

                        // Vérifier si l'opération existe déjà
                        var existingOperation = await _gedContext.Operations.FirstOrDefaultAsync(o => o.CodeOpe == codeOperation);
                        if (existingOperation != null)
                        {
                            if (OverwriteExisting)
                            {
                                existingOperation.DescriptionOpe = descriptionOperation;
                                _gedContext.Operations.Update(existingOperation);
                                updatedCount++;
                            }
                            else
                            {
                                // Ignorer ou compter comme erreur
                                errorCount++;
                                continue;
                            }
                        }
                        else
                        {
                            // Créer une nouvelle opération
                            var nouvelleOperation = new Operation
                            {
                                CodeOpe = codeOperation,
                                DescriptionOpe = descriptionOperation
                            };
                            _gedContext.Operations.Add(nouvelleOperation);
                            addedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        // Log l'erreur si nécessaire
                        _logger.LogError($"Erreur lors de l'importation ligne {i}: {ex.Message}");
                    }
                }

                // Sauvegarder les modifications
                if (addedCount > 0 || updatedCount > 0)
                {
                    await _gedContext.SaveChangesAsync();
                }

                return new JsonResult(new
                {
                    success = true,
                    message = "Importation terminée avec succès",
                    stats = new
                    {
                        total = totalCount,
                        added = addedCount,
                        updated = updatedCount,
                        errors = errorCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de l'importation Excel: {ex.Message}");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Erreur lors de l'importation: {ex.Message}"
                });
            }
        }

        // hanlder pour exporter un fichier modele Excel des opérations
        public IActionResult OnGetDownloadTemplateOperation()
        {
            try
            {
                // Générer le gabarit Excel
                var templateBytes = _administrationService.GenerateOperationTemplate();
                
                // Retourner le fichier
                return File(templateBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "Template_Operations.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du gabarit");
                // Vous pouvez rediriger vers une page d'erreur ou retourner un message
                TempData["ErrorMessage"] = "Impossible de générer le gabarit.";
                return RedirectToPage();
            }
        }
        

        /** Fin de la gestion des opérations */

        /** Traitement des villes */
         // Handler pour la création de ville
        public async Task<IActionResult> OnPostCreateVilleAsync(string codeVille, string descriptionVille)
        {
            var result = await _administrationService.CreateVilleAsync(codeVille, descriptionVille);

            if (result)
            {
                return new JsonResult(new { success = true, message = "Ville créée avec succès" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Erreur lors de la création de la ville" });
            }
        }

        // Handler pour la suppression de plusieurs villes
        public async Task<IActionResult> OnPostDeleteVilleAsync([FromBody] List<int> villeIds)
        {
            if (villeIds == null || !villeIds.Any())
            {
                return new JsonResult(new { success = false, message = "Aucune ville sélectionnée" });
            }

            try
            {
                var result = await _administrationService.DeleteVilleAsync(villeIds);
                if (result)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = $"{villeIds.Count} ville(s) supprimée(s) avec succès",
                        deletedCount = villeIds.Count
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Erreur lors de la suppression des villes" 
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = $"Erreur serveur: {ex.Message}" 
                });
            }
        }

        // Handler pour la modification de ville - Version optimisée
        public async Task<IActionResult> OnPostEditVilleAsync(int idVille, string codeVille, string descriptionVille)
        {
            try
            {
                var result = await _administrationService.UpdateVillesAsync(idVille, codeVille, descriptionVille);

                if (result)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = "Ville modifiée avec succès",
                        id = idVille,
                        code = codeVille,
                        description = descriptionVille
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Erreur lors de la modification de la ville" 
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { 
                    success = false, 
                    message = $"Exception: {ex.Message}" 
                });
            }
        }

        // Methode d'importation des villes à partir d'un fichier Excel
        public async Task<JsonResult> OnPostImportVillesAsync()
        {
            try
            {
                if (ExcelFile == null || ExcelFile.Length == 0)
                {
                    return new JsonResult(new { success = false, message = "Aucun fichier sélectionné" });
                }

                // Vérifier l'extension
                var extension = Path.GetExtension(ExcelFile.FileName).ToLower();
                if (extension != ".xlsx" && extension != ".xls")
                {
                    return new JsonResult(new { success = false, message = "Format de fichier non supporté" });
                }

                // Lire le fichier Excel
                using var stream = new MemoryStream();
                await ExcelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();
                
                int startRow = SkipFirstRow ? 2 : 1; // Ignorer l'en-tête si demandé
                int addedCount = 0;
                int updatedCount = 0;
                int errorCount = 0;
                int totalCount = 0;

                for (int i = startRow; i <= rows.Count(); i++)
                {
                    try
                    {
                        var codeCell = worksheet.Cell(i, 1);
                        var descCell = worksheet.Cell(i, 2);

                        if (codeCell.IsEmpty() || descCell.IsEmpty())
                            continue;

                        string codeVille = codeCell.GetValue<string>().Trim();
                        string descriptionVille = descCell.GetValue<string>().Trim();

                        if (string.IsNullOrEmpty(codeVille) || string.IsNullOrEmpty(descriptionVille))
                        {
                            errorCount++;
                            continue;
                        }

                        totalCount++;

                        // Vérifier si la ville existe déjà
                        var existingVille = await _gedContext.Villes.FirstOrDefaultAsync(v => v.CodeVille == codeVille);

                        if (existingVille != null)
                        {
                            if (OverwriteExisting)
                            {
                                existingVille.DescriptionVille = descriptionVille;
                                _gedContext.Villes.Update(existingVille);
                                updatedCount++;
                            }
                            else
                            {
                                // Ignorer ou compter comme erreur
                                errorCount++;
                                continue;
                            }
                        }
                        else
                        {
                            // Créer une nouvelle ville
                            var nouvelleVille = new Ville
                            {
                                CodeVille = codeVille,
                                DescriptionVille = descriptionVille
                            };
                            _gedContext.Villes.Add(nouvelleVille);
                            addedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        // Log l'erreur si nécessaire
                        _logger.LogError($"Erreur lors de l'importation ligne {i}: {ex.Message}");
                    }
                }

                // Sauvegarder les modifications
                if (addedCount > 0 || updatedCount > 0)
                {
                    await _gedContext.SaveChangesAsync();
                }

                return new JsonResult(new
                {
                    success = true,
                    message = "Importation terminée avec succès",
                    stats = new
                    {
                        total = totalCount,
                        added = addedCount,
                        updated = updatedCount,
                        errors = errorCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de l'importation Excel: {ex.Message}");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Erreur lors de l'importation: {ex.Message}"
                });
            }
        }

        // hanlder pour exporter un fichier modele Excel des villes
        public IActionResult OnGetDownloadTemplate()
        {
            try
            {
                // Générer le gabarit Excel
                var templateBytes = _administrationService.GenerateVilleTemplate();
                
                // Retourner le fichier
                return File(templateBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "Template_Villes.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du gabarit");
                // Vous pouvez rediriger vers une page d'erreur ou retourner un message
                TempData["ErrorMessage"] = "Impossible de générer le gabarit.";
                return RedirectToPage();
            }
        }
        /** Fin de la gestion des villes */
       

    }
   
}
