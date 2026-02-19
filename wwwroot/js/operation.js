// operation.js - Gestion des opérations dans l'administration

document.addEventListener('DOMContentLoaded', function() {
    
    // Gestion de la création des opérations
    const createOperationButton = document.getElementById('createOpe');
    if (createOperationButton) {
        createOperationButton.addEventListener('click', async function() {
            // Récupérer les valeurs - CORRECTION ICI : createVilleOpe au lieu de createVilleId
            const codeOpe = document.getElementById('createCodeOpe').value.trim();
            const descriptionOpe = document.getElementById('createDescriptionOpe').value.trim();
            const villeId = document.getElementById('createVilleOpe').value; // ← LIGNE CORRIGÉE
            
            console.log('Données:', { codeOpe, descriptionOpe, villeId }); // Debug
            
            // Validation
            if (!codeOpe || !descriptionOpe) {
                alert('Veuillez remplir tous les champs obligatoires.');
                return;
            }
            
            // Créer FormData
            const formData = new FormData();
            formData.append('codeOperation', codeOpe);
            formData.append('descriptionOperation', descriptionOpe);
            formData.append('villeId', villeId);
            
            // Récupérer le token
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            
            try {
                const response = await fetch('?handler=CreateOperation', {
                    method: 'POST',
                    headers: {
                        'RequestVerificationToken': token
                    },
                    body: formData
                });
                
                console.log('Réponse status:', response.status); // Debug
                
                if (!response.ok) {
                    const errorText = await response.text();
                    console.error('Erreur serveur:', errorText);
                    throw new Error(`Erreur serveur: ${response.status}`);
                }
                
                const data = await response.json();
                console.log('Données reçues:', data); // Debug
                
                if (data.success) {
                    // Afficher le toast de succès
                    showSuccessToast(data.message);
                    
                    // Fermer la modale
                    const modal = bootstrap.Modal.getInstance(document.getElementById('createOpeModal'));
                    if (modal) {
                        modal.hide();
                    }
                    
                    // Réinitialiser le formulaire
                    document.getElementById('createOpeForm').reset();
                    
                    // Rafraîchir la page après 2 secondes
                    setTimeout(() => {
                        location.reload();
                    }, 2000);
                } else {
                    alert('Erreur: ' + data.message);
                }
            } catch (error) {
                console.error('Erreur:', error);
                alert('Erreur lors de la création de l\'opération: ' + error.message);
            }
        });
    }
    
    // Fonction utilitaire pour afficher les toasts
    function showSuccessToast(message) {
        const toastMessageElement = document.getElementById('successToastMessage');
        if (toastMessageElement) {
            toastMessageElement.textContent = message;
        }
        
        const toastElement = document.getElementById('successToast');
        if (toastElement) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: true,
                delay: 3000
            });
            toast.show();
        }
    }

    // Code pour la modification des opérations
    const editOperationModal = document.getElementById('editOperationModal');
    if (editOperationModal) {
        editOperationModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const operationId = button.getAttribute('data-operation-id');
            const codeOperation = button.getAttribute('data-code-operation');
            const descriptionOperation = button.getAttribute('data-description-operation');
            const villeId = button.getAttribute('data-ville-id') || '';
            
            document.getElementById('editOperationId').value = operationId;
            document.getElementById('editCodeOpe').value = codeOperation;
            document.getElementById('editDescriptionOpe').value = descriptionOperation;
            document.getElementById('editVilleOpe').value = villeId;
            
            document.getElementById('editOperationModalLabel').textContent = `Modifier l'opération ${codeOperation}`;
        });
    }

    // Gestion de la sauvegarde des modifications de l'opération
    const saveOperationButton = document.getElementById('saveOperationChanges');
    if (saveOperationButton) {
        saveOperationButton.addEventListener('click', function() {
            const operationId = document.getElementById('editOperationId').value;
            const codeOperation = document.getElementById('editCodeOpe').value;
            const descriptionOperation = document.getElementById('editDescriptionOpe').value;
            const villeId = document.getElementById('editVilleOpe').value;

            // Validation des champs obligatoires
            if (!codeOperation || !codeOperation.trim()) {
                showAlert('error', 'Le code de l\'opération est obligatoire.');
                document.getElementById('editCodeOpe').focus();
                return;
            }

            if (!descriptionOperation || !descriptionOperation.trim()) {
                showAlert('error', 'La description de l\'opération est obligatoire.');
                document.getElementById('editDescriptionOpe').focus();
                return;
            }

            // Sauvegarder le texte original
            const originalText = saveOperationButton.innerHTML;
            saveOperationButton.disabled = true;
            saveOperationButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Enregistrement...';

            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            // Préparation des données
            const formData = new URLSearchParams();
            formData.append('idOpe', operationId);
            formData.append('codeOpe', codeOperation.trim());
            formData.append('descriptionOpe', descriptionOperation.trim());
            if (villeId) {
                formData.append('villeId', villeId);
            }

            fetch('?handler=EditOperation', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: formData.toString()
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Erreur HTTP: ' + response.status);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Fermer la modale
                    const modalInstance = bootstrap.Modal.getInstance(editOperationModal);
                    if (modalInstance) {
                        modalInstance.hide();
                    }
                    
                    // Afficher le message de succès
                    showAlert('success', data.message || 'Opération modifiée avec succès');
                    
                    // Mettre à jour la ligne dans le tableau sans recharger
                    updateOperationRow(data.data, codeOperation, descriptionOperation, villeId);
                    
                    // Réinitialiser le bouton APRÈS la mise à jour
                    saveOperationButton.disabled = false;
                    saveOperationButton.innerHTML = originalText;
                    
                } else {
                    // Afficher l'erreur
                    showAlert('error', data.message || 'Erreur lors de la modification');
                    // Réinitialiser le bouton en cas d'erreur
                    saveOperationButton.disabled = false;
                    saveOperationButton.innerHTML = originalText;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showAlert('error', 'Une erreur est survenue lors de la modification');
                // Réinitialiser le bouton en cas d'erreur
                saveOperationButton.disabled = false;
                saveOperationButton.innerHTML = originalText;
            });
        });
    }

    // Fonction pour mettre à jour la ligne dans le tableau
    function updateOperationRow(operationData, newCode, newDescription, villeId) {
        if (!operationData) return;
        
        const operationId = operationData.id || operationData.idOpe;
        
        // Chercher toutes les spans cliquables
        const clickableSpans = document.querySelectorAll('.clickable-name[data-operation-id]');
        
        clickableSpans.forEach(span => {
            if (span.getAttribute('data-operation-id') === operationId.toString()) {
                // Mettre à jour le span
                span.textContent = newCode;
                span.setAttribute('data-code-operation', newCode);
                span.setAttribute('data-description-operation', newDescription);
                span.setAttribute('data-ville-id', villeId || '');
                
                // Trouver la ligne parente
                const row = span.closest('tr');
                if (row) {
                    // Mettre à jour la description (colonne suivante)
                    const descriptionCell = row.cells[3];
                    if (descriptionCell) {
                        descriptionCell.textContent = newDescription;
                    }
                    
                    // Mettre à jour la ville
                    const villeCell = row.cells[4];
                    if (villeCell) {
                        if (villeId) {
                            // Récupérer le nom de la ville depuis le select
                            const villeSelect = document.getElementById('editVilleOpe');
                            if (villeSelect) {
                                const selectedOption = villeSelect.querySelector(`option[value="${villeId}"]`);
                                if (selectedOption) {
                                    villeCell.textContent = selectedOption.textContent;
                                }
                            }
                        } else {
                            villeCell.textContent = '';
                        }
                    }
                    
                    // Ajouter un effet visuel
                    highlightUpdatedRow(row);
                }
            }
        });
    }

    // Fonction pour surligner la ligne mise à jour
    function highlightUpdatedRow(row) {
        row.classList.add('table-updated');
        
        // Retirer la classe après 2 secondes
        setTimeout(() => {
            row.classList.remove('table-updated');
        }, 2000);
    }

    // Fonction pour afficher les alertes
    function showAlert(type, message) {
        // Si toastr est disponible
        if (typeof toastr !== 'undefined') {
            toastr[type](message);
            return;
        }
        
        // Fallback simple
        if (type === 'success') {
            alert('✓ ' + message);
        } else {
            alert('✗ ' + message);
        }
    }

    // Nettoyage de la modale lorsqu'elle est fermée - CORRIGÉ pour réinitialiser le bouton aussi
    if (editOperationModal) {
        editOperationModal.addEventListener('hidden.bs.modal', function () {
            // Réinitialiser le formulaire
            document.getElementById('editOperationForm').reset();
            
            // Réinitialiser le bouton à son état normal
            if (saveOperationButton) {
                saveOperationButton.disabled = false;
                saveOperationButton.innerHTML = 'Enregistrer';
            }
            
            // Réinitialiser les messages d'erreur visuels
            const formControls = editOperationModal.querySelectorAll('.form-control');
            formControls.forEach(control => {
                control.classList.remove('is-invalid');
            });
            
            const invalidFeedbacks = editOperationModal.querySelectorAll('.invalid-feedback');
            invalidFeedbacks.forEach(feedback => {
                feedback.remove();
            });
        });
    }

    // VERSION ALTERNATIVE PLUS SIMPLE - Utiliser .finally() pour réinitialiser le bouton
    if (saveOperationButton) {
        saveOperationButton.addEventListener('click', async function() {
            const operationId = document.getElementById('editOperationId').value;
            const codeOperation = document.getElementById('editCodeOpe').value;
            const descriptionOperation = document.getElementById('editDescriptionOpe').value;
            const villeId = document.getElementById('editVilleOpe').value;

            // Validation des champs obligatoires
            if (!codeOperation || !codeOperation.trim()) {
                showAlert('error', 'Le code de l\'opération est obligatoire.');
                document.getElementById('editCodeOpe').focus();
                return;
            }

            if (!descriptionOperation || !descriptionOperation.trim()) {
                showAlert('error', 'La description de l\'opération est obligatoire.');
                document.getElementById('editDescriptionOpe').focus();
                return;
            }

            // Sauvegarder le texte original
            const originalText = saveOperationButton.innerHTML;
            saveOperationButton.disabled = true;
            saveOperationButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Enregistrement...';

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                const formData = new URLSearchParams();
                formData.append('idOpe', operationId);
                formData.append('codeOpe', codeOperation.trim());
                formData.append('descriptionOpe', descriptionOperation.trim());
                if (villeId) {
                    formData.append('villeId', villeId);
                }

                const response = await fetch('?handler=EditOperation', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    },
                    body: formData.toString()
                });

                if (!response.ok) {
                    throw new Error('Erreur HTTP: ' + response.status);
                }

                const data = await response.json();

                if (data.success) {
                    // Fermer la modale
                    const modalInstance = bootstrap.Modal.getInstance(editOperationModal);
                    if (modalInstance) {
                        modalInstance.hide();
                    }
                    
                    // Afficher le message de succès
                    //showAlert('success', data.message || 'Opération modifiée avec succès');
                    
                    // Mettre à jour la ligne dans le tableau
                    updateOperationRow(data.data, codeOperation, descriptionOperation, villeId);
                } else {
                    showAlert('error', data.message || 'Erreur lors de la modification');
                }
            } catch (error) {
                console.error('Error:', error);
                showAlert('error', 'Une erreur est survenue lors de la modification');
            } finally {
                // TOUJOURS réinitialiser le bouton, qu'il y ait succès ou erreur
                saveOperationButton.disabled = false;
                saveOperationButton.innerHTML = originalText;
            }
        });
    }

    // Gestion de la sélection/désélection de toutes les opérations
    const selectAllCheckboxOperation = document.getElementById('selectAllCheckboxOperation');
    if (selectAllCheckboxOperation) {
        selectAllCheckboxOperation.addEventListener('change', function() {
            const checkboxes = document.querySelectorAll('.operation-checkbox');
            checkboxes.forEach(checkbox => {
                checkbox.checked = selectAllCheckboxOperation.checked;
            });
        });
    }

    // Fonction pour supprimer les opérations sélectionnées
    const deleteSelectedOperationsButton = document.getElementById('deleteSelectedOperations');
    if (deleteSelectedOperationsButton) {
        deleteSelectedOperationsButton.addEventListener('click', async function() {
            const selectedCheckboxes = document.querySelectorAll('.operation-checkbox:checked');
            if (selectedCheckboxes.length === 0) {
                alert('Veuillez sélectionner au moins une opération à supprimer.');
                return;
            }

            if (!confirm(`Êtes-vous sûr de vouloir supprimer les ${selectedCheckboxes.length} opération(s) sélectionnée(s) ?`)) {
                return;
            }

            // Récupérer le token anti-falsification
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            // Récupérer les IDs des opérations sélectionnées
            const operationIds = Array.from(selectedCheckboxes).map(checkbox => {
                return parseInt(checkbox.getAttribute('data-operation-id'));
            });

            try {
                const response = await fetch('?handler=DeleteOperations', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify(operationIds)
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        alert(result.message);
                        // Recharger la page pour voir les changements
                        location.reload();
                    } else {
                        alert(result.message);
                    }
                } else {
                    alert('Erreur lors de la requête.');
                }
            } catch (error) {
                console.error('Erreur:', error);
                alert('Une erreur est survenue lors de la suppression.');
            }
        });
    }

});

// Gestion de l'importation Excel des opérations
/* document.addEventListener('DOMContentLoaded', function() {
    const excelFileInput = document.getElementById('excelFile');
    const importSubmitBtn = document.getElementById('importSubmitBtn');
    const clearFileBtn = document.getElementById('clearFileBtn');
    const previewSection = document.getElementById('previewSection');
    const previewData = document.getElementById('previewData');
    const previewStats = document.getElementById('previewStats');
    const downloadTemplateBtn = document.getElementById('downloadTemplateOperation');
    const importStatus = document.getElementById('importStatusOperation');
    const importProgress = document.getElementById('importProgressOperation');
    const importMessage = document.getElementById('importMessageOperation');
    const skipFirstRowCheckbox = document.getElementById('skipFirstRow');
    const overwriteExistingCheckbox = document.getElementById('overwriteExistingOperations');
    const importOperationForm = document.getElementById('importOperationForm');

    // Télécharger le gabarit Excel
    downloadTemplateBtn.addEventListener('click', function(e) {
        e.preventDefault();
        
        // Créer un objet Excel simple
        const data = [
            ['CodeOperation', 'DescriptionOperation'],
            ['OPR001', 'Opération 1'],
            ['OPR002', 'Opération 2'],
            ['OPR003', 'Opération 3'],
            ['OPR004', 'Opération 4'],
            ['OPR005', 'Opération 5']
        ];

        // Créer un fichier Excel
        const ws = XLSX.utils.aoa_to_sheet(data);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Opérations');
        
        // Générer et télécharger le fichier
        XLSX.writeFile(wb, 'template_operations.xlsx');
    });

    // Effacer le fichier sélectionné
    clearFileBtn.addEventListener('click', function() {
        excelFileInput.value = '';
        importSubmitBtn.disabled = true;
        previewSection.classList.add('d-none');
        previewData.innerHTML = '';
        previewStats.innerHTML = '';
    });

    // Validation du fichier Excel
    excelFileInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (!file) {
            importSubmitBtn.disabled = true;
            return;
        }

        // Vérifier l'extension
        const validExtensions = ['.xlsx', '.xls'];
        const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
        
        if (!validExtensions.includes(fileExtension)) {
            alert('Format de fichier non supporté. Veuillez sélectionner un fichier Excel (.xlsx, .xls)');
            excelFileInput.value = '';
            importSubmitBtn.disabled = true;
            return;
        }

        // Vérifier la taille (max 5MB)
        if (file.size > 5 * 1024 * 1024) {
            alert('Le fichier est trop volumineux. Taille maximale : 5MB');
            excelFileInput.value = '';
            importSubmitBtn.disabled = true;
            return;
        }

        // Activer le bouton d'importation
        importSubmitBtn.disabled = false;

        // Afficher l'aperçu du fichier
        previewExcelFile(file);
    });

    // Fonction pour afficher l'aperçu du fichier Excel
    function previewExcelFile(file) {
        const reader = new FileReader();
        
        reader.onload = function(e) {
            const data = new Uint8Array(e.target.result);
            const workbook = XLSX.read(data, { type: 'array' });
            
            // Prendre la première feuille
            const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
            const jsonData = XLSX.utils.sheet_to_json(firstSheet, { header: 1 });
            
            // Afficher l'aperçu (max 10 lignes)
            displayPreview(jsonData);
        };
        
        reader.readAsArrayBuffer(file);
    }

    // Afficher l'aperçu des données
    function displayPreview(data) {
        previewData.innerHTML = '';
        
        const skipHeader = skipFirstRowCheckbox.checked;
        const startRow = skipHeader ? 1 : 0;
        let validRows = 0;
        let totalRows = Math.min(data.length - startRow, 10); // Max 10 lignes pour l'aperçu
        
        for (let i = startRow; i < Math.min(startRow + 10, data.length); i++) {
            const row = data[i];
            if (row && row.length >= 2) {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${row[0] || ''}</td>
                    <td>${row[1] || ''}</td>
                `;
                previewData.appendChild(tr);
                validRows++;
            }
        }
        
        // Afficher les statistiques
        const totalDataRows = data.length - (skipHeader ? 1 : 0);
        previewStats.innerHTML = `
            <i class="bi bi-info-circle me-2"></i>
            ${validRows} lignes affichées sur ${totalDataRows} au total.
            ${totalDataRows > 10 ? '(Seules les 10 premières lignes sont affichées)' : ''}
        `;
        
        previewSection.classList.remove('d-none');
    }

    // Mise à jour de l'aperçu quand l'option d'en-tête change
    skipFirstRowCheckbox.addEventListener('change', function() {
        if (excelFileInput.files.length > 0) {
            previewExcelFile(excelFileInput.files[0]);
        }
    });

    // Soumission du formulaire d'importation
    importSubmitBtn.addEventListener('click', function() {
        if (!excelFileInput.files.length) {
            alert('Veuillez sélectionner un fichier Excel');
            return;
        }

        const file = excelFileInput.files[0];
        const formData = new FormData();
        formData.append('excelFile', file);
        formData.append('skipFirstRow', skipFirstRowCheckbox.checked);
        formData.append('overwriteExisting', overwriteExistingCheckbox.checked);
        
        // Ajouter le token anti-forgery
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        formData.append('__RequestVerificationToken', token);

        // Désactiver le bouton et afficher la progression
        importSubmitBtn.disabled = true;
        importSubmitBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Importation...';
        importStatus.classList.remove('d-none');
        importProgress.style.width = '10%';
        importProgress.textContent = '10%';
        importMessage.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Préparation de l\'importation...';

        // Envoyer le fichier au serveur
        fetch('?handler=ImportOperations', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`Erreur HTTP: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Mise à jour de la barre de progression
                importProgress.style.width = '100%';
                importProgress.textContent = '100%';
                importProgress.classList.remove('progress-bar-animated');
                importProgress.classList.remove('progress-bar-striped');
                importProgress.classList.add('bg-success');
                
                // Message de succès
                importMessage.innerHTML = `
                    <div class="alert alert-success">
                        <i class="bi bi-check-circle me-2"></i>
                        <strong>Importation réussie !</strong><br>
                        ${data.stats.total} opérations traitées<br>
                        ${data.stats.added} nouvelles opérations ajoutées<br>
                        ${data.stats.updated} opérations mises à jour<br>
                        ${data.stats.errors} erreurs
                    </div>
                `;
                
                // Rediriger après 3 secondes
                setTimeout(() => {
                    location.reload();
                }, 3000);
            } else {
                throw new Error(data.message || 'Erreur lors de l\'importation');
            }
        })
        .catch(error => {
            console.error('Erreur:', error);
            
            // Réinitialiser la barre de progression
            importProgress.classList.remove('progress-bar-animated');
            importProgress.classList.remove('progress-bar-striped');
            importProgress.classList.add('bg-danger');
            
            // Message d'erreur
            importMessage.innerHTML = `
                <div class="alert alert-danger">
                    <i class="bi bi-x-circle me-2"></i>
                    <strong>Erreur d'importation</strong><br>
                    ${error.message}
                </div>
            `;
            
            // Réactiver le bouton
            importSubmitBtn.disabled = false;
            importSubmitBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Importer les opérations';
        });
    });

    // Réinitialiser la modale quand elle se ferme
    document.getElementById('importOperationModal').addEventListener('hidden.bs.modal', function() {
        excelFileInput.value = '';
        importSubmitBtn.disabled = true;
        importSubmitBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Importer les opérations';
        previewSection.classList.add('d-none');
        previewData.innerHTML = '';
        previewStats.innerHTML = '';
        importStatus.classList.add('d-none');
        
        // Réinitialiser la barre de progression
        importProgress.style.width = '0%';
        importProgress.textContent = '0%';
        importProgress.classList.remove('bg-success', 'bg-danger');
        importProgress.classList.add('progress-bar-animated', 'progress-bar-striped');
        importMessage.innerHTML = '';
    });

    // Code JS pour télécharger le gabarit Excel
    downloadTemplateBtn.addEventListener('click', function(e) {
        e.preventDefault();
        console.log('Tentative de téléchargement...');
        
        // Afficher l'URL qui sera utilisée
        const url = '?handler=DownloadTemplateOperation';
        console.log('URL:', url);
        
        // Tester avec fetch pour voir la réponse
        fetch(url)
            .then(response => {
                console.log('Statut:', response.status);
                console.log('Headers:', response.headers);
                return response.blob();
            })
            .then(blob => {
                console.log('Blob créé, taille:', blob.size);
                // Forcer le téléchargement
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = 'Template_Operations.xlsx';
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
            })
            .catch(error => {
                console.error('Erreur:', error);
            });
    });

});
 */

// Gestion de l'importation Excel des opérations
document.addEventListener('DOMContentLoaded', function() {
    // Corriger les noms des IDs pour correspondre au HTML
    const excelFileInput = document.getElementById('excelFile');
    const importSubmitBtn = document.getElementById('importSubmitBtnOperation'); // ✅ Ajouté "Operation"
    const clearFileBtn = document.getElementById('clearFileBtn');
    const previewSection = document.getElementById('previewSection');
    const previewData = document.getElementById('previewData');
    const previewStats = document.getElementById('previewStats');
    const downloadTemplateBtn = document.getElementById('downloadTemplateOperation');
    const importStatus = document.getElementById('importStatusOperation'); // ✅ Ajouté "Operation"
    const importProgress = document.getElementById('importProgressOperation'); // ✅ Ajouté "Operation"
    const importMessage = document.getElementById('importMessageOperation'); // ✅ Ajouté "Operation"
    const skipFirstRowCheckbox = document.getElementById('skipFirstRow');
    const overwriteExistingCheckbox = document.getElementById('overwriteExistingOperations'); // ✅ Ajouté "s"
    const importOperationForm = document.getElementById('importOperationForm');

    // Télécharger le gabarit Excel
    downloadTemplateBtn.addEventListener('click', function(e) {
        e.preventDefault();
        
        // Utiliser l'URL correcte pour le serveur .NET
        const url = '?handler=DownloadTemplateOperation';
        
        // Créer un lien temporaire pour déclencher le téléchargement
        const link = document.createElement('a');
        link.href = url;
        link.download = 'Template_Operations.xlsx';
        link.style.display = 'none';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    });

    // Effacer le fichier sélectionné
    clearFileBtn.addEventListener('click', function() {
        excelFileInput.value = '';
        importSubmitBtn.disabled = true;
        previewSection.classList.add('d-none');
        previewData.innerHTML = '';
        previewStats.innerHTML = '';
    });

    // Validation du fichier Excel - CORRIGÉ
    excelFileInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        console.log('Fichier sélectionné:', file); // Pour déboguer
        
        if (!file) {
            importSubmitBtn.disabled = true;
            console.log('Aucun fichier sélectionné');
            return;
        }

        // Vérifier l'extension
        const validExtensions = ['.xlsx', '.xls'];
        const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
        
        if (!validExtensions.includes(fileExtension)) {
            alert('Format de fichier non supporté. Veuillez sélectionner un fichier Excel (.xlsx, .xls)');
            excelFileInput.value = '';
            importSubmitBtn.disabled = true;
            return;
        }

        // Vérifier la taille (max 5MB)
        if (file.size > 5 * 1024 * 1024) {
            alert('Le fichier est trop volumineux. Taille maximale : 5MB');
            excelFileInput.value = '';
            importSubmitBtn.disabled = true;
            return;
        }

        // Activer le bouton d'importation
        importSubmitBtn.disabled = false;
        console.log('Bouton import activé'); // Pour déboguer

        // Afficher l'aperçu du fichier
        previewExcelFile(file);
    });

    // Fonction pour afficher l'aperçu du fichier Excel
    function previewExcelFile(file) {
        const reader = new FileReader();
        
        reader.onload = function(e) {
            const data = new Uint8Array(e.target.result);
            
            // Vérifier si la bibliothèque XLSX est chargée
            if (typeof XLSX === 'undefined') {
                console.error('La bibliothèque XLSX n\'est pas chargée');
                return;
            }
            
            const workbook = XLSX.read(data, { type: 'array' });
            
            // Prendre la première feuille
            const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
            const jsonData = XLSX.utils.sheet_to_json(firstSheet, { header: 1 });
            
            // Afficher l'aperçu (max 10 lignes)
            displayPreview(jsonData);
        };
        
        reader.onerror = function(error) {
            console.error('Erreur de lecture du fichier:', error);
            alert('Erreur lors de la lecture du fichier');
        };
        
        reader.readAsArrayBuffer(file);
    }

    // Afficher l'aperçu des données
    function displayPreview(data) {
        previewData.innerHTML = '';
        
        const skipHeader = skipFirstRowCheckbox.checked;
        const startRow = skipHeader ? 1 : 0;
        let validRows = 0;
        
        // Limiter à 10 lignes pour l'aperçu
        const previewLimit = Math.min(10, data.length - startRow);
        
        for (let i = startRow; i < startRow + previewLimit; i++) {
            const row = data[i];
            if (row && row.length >= 2) {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${row[0] || ''}</td>
                    <td>${row[1] || ''}</td>
                `;
                previewData.appendChild(tr);
                validRows++;
            }
        }
        
        // Afficher les statistiques
        const totalDataRows = Math.max(0, data.length - (skipHeader ? 1 : 0));
        previewStats.innerHTML = `
            <i class="bi bi-info-circle me-2"></i>
            ${validRows} lignes affichées sur ${totalDataRows} au total.
            ${totalDataRows > 10 ? '(Seules les 10 premières lignes sont affichées)' : ''}
        `;
        
        previewSection.classList.remove('d-none');
    }

    // Mise à jour de l'aperçu quand l'option d'en-tête change
    skipFirstRowCheckbox.addEventListener('change', function() {
        if (excelFileInput.files.length > 0) {
            previewExcelFile(excelFileInput.files[0]);
        }
    });

    // Soumission du formulaire d'importation - CORRIGÉ
    importSubmitBtn.addEventListener('click', function() {
        if (!excelFileInput.files.length) {
            alert('Veuillez sélectionner un fichier Excel');
            return;
        }

        const file = excelFileInput.files[0];
        const formData = new FormData();
        formData.append('excelFile', file);
        formData.append('skipFirstRow', skipFirstRowCheckbox.checked);
        formData.append('overwriteExisting', overwriteExistingCheckbox.checked);
        
        // Ajouter le token anti-forgery
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        formData.append('__RequestVerificationToken', token);

        // Désactiver le bouton et afficher la progression
        importSubmitBtn.disabled = true;
        importSubmitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> Importation...';
        importStatus.classList.remove('d-none');
        importProgress.style.width = '10%';
        importProgress.textContent = '10%';
        importMessage.innerHTML = '<i class="bi bi-hourglass-split me-2"></i>Préparation de l\'importation...';

        // Envoyer le fichier au serveur
        fetch('?handler=ImportOperations', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`Erreur HTTP: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Mise à jour de la barre de progression
                importProgress.style.width = '100%';
                importProgress.textContent = '100%';
                importProgress.classList.remove('progress-bar-animated', 'progress-bar-striped');
                importProgress.classList.add('bg-success');
                
                // Message de succès
                importMessage.innerHTML = `
                    <div class="alert alert-success">
                        <i class="bi bi-check-circle me-2"></i>
                        <strong>Importation réussie !</strong><br>
                        Total: ${data.stats.total} opérations<br>
                        Ajoutées: ${data.stats.added}<br>
                        Mises à jour: ${data.stats.updated}<br>
                        Erreurs: ${data.stats.errors}
                    </div>
                `;
                
                // Rediriger après 3 secondes
                setTimeout(() => {
                    location.reload();
                }, 3000);
            } else {
                throw new Error(data.message || 'Erreur lors de l\'importation');
            }
        })
        .catch(error => {
            console.error('Erreur:', error);
            
            // Réinitialiser la barre de progression
            importProgress.classList.remove('progress-bar-animated', 'progress-bar-striped');
            importProgress.classList.add('bg-danger');
            
            // Message d'erreur
            importMessage.innerHTML = `
                <div class="alert alert-danger">
                    <i class="bi bi-x-circle me-2"></i>
                    <strong>Erreur d'importation</strong><br>
                    ${error.message}
                </div>
            `;
            
            // Réactiver le bouton
            importSubmitBtn.disabled = false;
            importSubmitBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Importer les opérations';
        });
    });

    // Réinitialiser la modale quand elle se ferme
    document.getElementById('importOperationModal').addEventListener('hidden.bs.modal', function() {
        excelFileInput.value = '';
        importSubmitBtn.disabled = true;
        importSubmitBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Importer les opérations';
        previewSection.classList.add('d-none');
        previewData.innerHTML = '';
        previewStats.innerHTML = '';
        importStatus.classList.add('d-none');
        
        // Réinitialiser la barre de progression
        importProgress.style.width = '0%';
        importProgress.textContent = '0%';
        importProgress.classList.remove('bg-success', 'bg-danger');
        importProgress.classList.add('progress-bar-animated', 'progress-bar-striped');
        importMessage.innerHTML = '';
    });
});



