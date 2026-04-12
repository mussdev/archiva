document.addEventListener('DOMContentLoaded', function() {
    console.log('🚀 DOMContentLoaded - Initialisation du script');

    var adpTable = document.querySelector('#adpTableBody')?.closest('table');
    if (!adpTable) {
        console.warn('Aucune table ADP trouvée, pagination désactivée.');
        return;
    }

    var rowsArray = Array.from(adpTable.querySelectorAll('tbody tr'));
    var filterStats = document.getElementById('filterStats');
    var paginationInfo = document.getElementById('paginationInfo');
    var paginationContainer = document.getElementById('paginationContainer');
    var pageSizeSelect = document.getElementById('pageSizeSelect');
    var tableFilter = document.getElementById('table-filter');
    var selectAllCheckbox = document.getElementById('selectAllCheckbox');

    var currentPage = 1;
    var pageSize = pageSizeSelect ? parseInt(pageSizeSelect.value, 10) || 10 : 10;
    var totalRows = rowsArray.length;

    function getFilteredRows() {
        return rowsArray.filter(function(row) {
            return row.dataset.matchesFilter !== 'false';
        });
    }

    function getVisibleCheckboxes() {
        return rowsArray
            .filter(function(row) {
                return row.style.display !== 'none';
            })
            .map(function(row) {
                return row.querySelector('.rowCheckbox');
            })
            .filter(function(checkbox) {
                return checkbox !== null;
            });
    }

    function updateFilterStats() {
        if (!filterStats) {
            return;
        }

        var filteredRows = getFilteredRows();
        if (!tableFilter || tableFilter.value.trim() === '') {
            filterStats.textContent = totalRows + ' fichier(s) trouvé(s)';
        } else {
            filterStats.textContent = filteredRows.length + ' fichier(s) sur ' + totalRows + ' trouvé(s)';
        }
    }

    function renderPagination() {
        if (!paginationContainer || !paginationInfo) {
            return;
        }

        var filteredRows = getFilteredRows();
        var totalPages = Math.max(1, Math.ceil(filteredRows.length / pageSize));
        currentPage = Math.min(Math.max(1, currentPage), totalPages);

        paginationInfo.textContent = 'Page ' + currentPage + ' sur ' + totalPages;

        if (totalPages <= 1) {
            paginationContainer.innerHTML = '';
            return;
        }

        var pagesToShow = [];
        if (totalPages <= 7) {
            for (var i = 1; i <= totalPages; i++) {
                pagesToShow.push(i);
            }
        } else {
            pagesToShow = [1];
            if (currentPage > 3) pagesToShow.push(currentPage - 1);
            if (currentPage > 2) pagesToShow.push(currentPage);
            if (currentPage < totalPages - 1) pagesToShow.push(currentPage + 1);
            if (currentPage < totalPages - 2) pagesToShow.push(totalPages - 1);
            pagesToShow.push(totalPages);
        }

        paginationContainer.innerHTML = '';

        function appendPageButton(page, label, disabled) {
            var li = document.createElement('li');
            li.className = 'page-item' + (disabled ? ' disabled' : '') + (page === currentPage ? ' active' : '');
            var a = document.createElement('a');
            a.className = 'page-link';
            a.href = '#';
            a.textContent = label || page;
            a.addEventListener('click', function(event) {
                event.preventDefault();
                if (!disabled && page !== currentPage) {
                    currentPage = page;
                    showPage();
                }
            });
            li.appendChild(a);
            paginationContainer.appendChild(li);
        }

        appendPageButton(1, '«', currentPage === 1);

        var lastPage = 0;
        pagesToShow.forEach(function(page) {
            if (page !== lastPage) {
                if (page > lastPage + 1) {
                    var ellipsis = document.createElement('li');
                    ellipsis.className = 'page-item disabled';
                    ellipsis.innerHTML = '<span class="page-link">…</span>';
                    paginationContainer.appendChild(ellipsis);
                }
                appendPageButton(page, page, false);
                lastPage = page;
            }
        });

        appendPageButton(totalPages, '»', currentPage === totalPages);
    }

    function showPage() {
        var filteredRows = getFilteredRows();
        var totalPages = Math.max(1, Math.ceil(filteredRows.length / pageSize));
        currentPage = Math.min(Math.max(1, currentPage), totalPages);

        var startIndex = (currentPage - 1) * pageSize;
        var endIndex = currentPage * pageSize;

        rowsArray.forEach(function(row) {
            row.style.display = 'none';
        });

        filteredRows.forEach(function(row, index) {
            if (index >= startIndex && index < endIndex) {
                row.style.display = '';
            }
        });

        if (selectAllCheckbox) {
            selectAllCheckbox.checked = false;
        }

        renderPagination();
        updateFilterStats();
    }

    function filterData() {
        if (!tableFilter) return;

        var filterValue = tableFilter.value.toLowerCase();
        rowsArray.forEach(function(row) {
            var rowText = row.textContent.toLowerCase();
            row.dataset.matchesFilter = rowText.indexOf(filterValue) === -1 ? 'false' : 'true';
        });

        currentPage = 1;
        showPage();
    }

    function updateCheckboxListeners() {
        var visibleCheckboxes = getVisibleCheckboxes();
        visibleCheckboxes.forEach(function(checkbox) {
            checkbox.removeEventListener('change', handleRowCheckboxChange);
            checkbox.addEventListener('change', handleRowCheckboxChange);
        });
    }

    function handleRowCheckboxChange() {
        if (!selectAllCheckbox) return;
        var visibleCheckboxes = getVisibleCheckboxes();
        selectAllCheckbox.checked = visibleCheckboxes.length > 0 && visibleCheckboxes.every(function(cb) {
            return cb.checked;
        });
    }

    function updateSelectAllBehavior() {
        if (!selectAllCheckbox) return;

        selectAllCheckbox.addEventListener('change', function() {
            var visibleCheckboxes = getVisibleCheckboxes();
            visibleCheckboxes.forEach(function(checkbox) {
                checkbox.checked = selectAllCheckbox.checked;
            });
        });
    }

    rowsArray.forEach(function(row) {
        row.dataset.matchesFilter = 'true';
    });

    showPage();
    updateSelectAllBehavior();
    updateCheckboxListeners();

    if (tableFilter) {
        tableFilter.addEventListener('input', function() {
            filterData();
            updateCheckboxListeners();
        });
    }

    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function() {
            pageSize = parseInt(this.value, 10) || 10;
            currentPage = 1;
            showPage();
            updateCheckboxListeners();
        });
    }

    var ajoutFichierModal = document.getElementById('ajoutFichierModal');
    if (ajoutFichierModal) {
        ajoutFichierModal.addEventListener('hidden.bs.modal', function () {
            var uploadForm = document.getElementById('uploadForm');
            if (uploadForm) {
                uploadForm.reset();
            }
        });
    }

    if (tableFilter && selectAllCheckbox) {
        tableFilter.addEventListener('input', function() {
            selectAllCheckbox.checked = false;
        });
    }

    // === VALIDATION DU FORMULAIRE D'AJOUT DE FICHIER ADP ===
    var uploadForm = document.querySelector('form[asp-page-handler="UploadFileAdp"]');
    if (uploadForm) {
        uploadForm.addEventListener('submit', function(e) {
            var fileInput = document.querySelector('input[name="Fichier"]');
            if (fileInput) {
                var file = fileInput.files[0];
                
                if (!file) {
                    e.preventDefault();
                    alert('Veuillez sélectionner un fichier PDF ❌.');
                    return false;
                }
                
                // Vérifier l'extension
                var fileName = file.name;
                var extension = fileName.split('.').pop().toLowerCase();
                if (extension !== 'pdf') {
                    e.preventDefault();
                    alert('Seuls les fichiers PDF sont autorisés ❌.');
                    return false;
                }
            }
            return true;
        });
    }

    // === RÉINITIALISATION DU MODAL D'AJOUT ===
    if (ajoutFichierModal) {
        ajoutFichierModal.addEventListener('hidden.bs.modal', function () {
            var form = document.querySelector('form[asp-page-handler="UploadFileAdp"]');
            if (form) {
                form.reset();
            }
        });
    }

    // === GESTION DES CASES À COCHER DANS LE TABLEAU DE FICHIER ADP ===
    var selectAllCheckbox = document.getElementById('selectAllCheckbox');
    var rowCheckboxes = document.querySelectorAll('.rowCheckbox');
    
    // Case "Tout sélectionner"
    if (selectAllCheckbox && rowCheckboxes.length > 0) {
        selectAllCheckbox.addEventListener('change', function() {
            rowCheckboxes.forEach(checkbox => {
                checkbox.checked = selectAllCheckbox.checked;
            });
        });
    }
    
    // Cases individuelles
    if (rowCheckboxes.length > 0) {
        rowCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function() {
                // Décocher "Tout sélectionner" si une case est décochée
                if (!this.checked && selectAllCheckbox && selectAllCheckbox.checked) {
                    selectAllCheckbox.checked = false;
                }
                // Cocher "Tout sélectionner" si toutes les cases sont cochées
                else if (this.checked && selectAllCheckbox) {
                    const allChecked = Array.from(rowCheckboxes).every(cb => cb.checked);
                    selectAllCheckbox.checked = allChecked;
                }
            });
        });
    }
    
    // Mettre à jour "Tout sélectionner" lors du filtrage
    if (tableFilter && selectAllCheckbox) {
        tableFilter.addEventListener('input', function() {
            // Réinitialiser "Tout sélectionner" lors du filtrage
            selectAllCheckbox.checked = false;
        });
    }

    // === CODE JS POUR L'AFFICHAGE AUTOMATIQUE DU NOM DE L'OPÉRATION ET LA VILLE ===
    function connectCodeToOperationAndVille(codeElement) {
        if (!codeElement) return;
        const parentScope = codeElement.closest('form') || codeElement.closest('.modal-body') || document;

        function updateFields() {
            const selectedOption = codeElement.selectedOptions ? codeElement.selectedOptions[0] : codeElement.options[codeElement.selectedIndex];
            const operationDescription = selectedOption?.dataset.operation || '';
            const villeDescription = selectedOption?.dataset.ville || '';

            const operationField = parentScope.querySelector('#Operation, #operation, [name="Operation"], [name="operation"]');
            if (operationField) {
                operationField.value = operationDescription;
                console.log('Champ Operation mis à jour:', operationField.value);
            } else {
                console.error('Champ Operation non trouvé (scope)', parentScope);
            }

            const villeField = parentScope.querySelector('#Ville, #ville, [name="Ville"], [name="ville"]');
            if (villeField) {
                villeField.value = villeDescription;
                console.log('Champ Ville mis à jour:', villeField.value);
            } else {
                console.error('Champ Ville non trouvé (scope)', parentScope);
            }
        }

        codeElement.addEventListener('change', updateFields);
        if (codeElement.value) {
            updateFields();
        }
    }

    const codeSelects = Array.from(document.querySelectorAll('#Code, #code, select[name="IdOpe"]'));
    if (codeSelects.length) {
        console.log('✅ Élément(s) Code trouvé(s):', codeSelects.length);
        codeSelects.forEach(connectCodeToOperationAndVille);
    } else {
        console.warn('⚠️ Élément Code non trouvé sur cette page');
    }
});

// === FONCTIONS GLOBALES (en dehors de DOMContentLoaded) ===

// Fonction pour obtenir les IDs des éléments sélectionnés
function getSelectedAdpIds() {
    const selectedCheckboxes = document.querySelectorAll('.rowCheckbox:checked');
    const selectedIds = Array.from(selectedCheckboxes).map(checkbox => {
        return checkbox.getAttribute('data-adp-id');
    });
    return selectedIds;
}

// === GESTION DU MODAL DE MODIFICATION ADP ===
const adpModal = document.getElementById('adpModal');
if (adpModal) {
    adpModal.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;

        // Réinitialiser le flag de suppression
        document.getElementById('deleteFileFlag').value = 'false';
        
        // Extraire les données des attributs data-*
        // Récupérer les attributs (certains nouveaux)
        const adpId = button.getAttribute('data-adp-id');
        const documentValue = button.getAttribute('data-document');
        const contact = button.getAttribute('data-contact');
        const datedocument = button.getAttribute('data-datedocument'); // déjà en yyyy-MM-dd
        const annee = button.getAttribute('data-annee');
        const fonctions = button.getAttribute('data-fonctions');
        const client = button.getAttribute('data-client');
        const code = button.getAttribute('data-code');           // si besoin
        const idOpe = button.getAttribute('data-id-ope');        // NOUVEAU : ID de l'opération
        const numDossier = button.getAttribute('data-numDossier');
        const boite = button.getAttribute('data-boite');
        const logement = button.getAttribute('data-logement');
        const adresse = button.getAttribute('data-adresse');
        const communequartier = button.getAttribute('data-communequartier');
        const ville = button.getAttribute('data-ville');
        const fichier = button.getAttribute('data-fichier');
        const statutId = button.getAttribute('data-statut-id');  // NOUVEAU

        // Convertir la date du format dd/MM/yyyy vers yyyy-MM-dd pour l'input date
        let htmlDate = '';
        if (datedocument) {
            const parts = datedocument.split('/');
            if (parts.length === 3) {
                htmlDate = `${parts[2]}-${parts[1]}-${parts[0]}`; // yyyy-MM-dd
            }
        }

        // Mettre à jour les champs
        document.getElementById('adpId').value = adpId;
        document.getElementById('boite').value = boite;
        document.getElementById('Code').value = idOpe;           // ← ID corrigé (majuscule) et utilisation de idOpe
        document.getElementById('logement').value = logement;
        document.getElementById('client').value = client;
        document.getElementById('annee').value = annee;          // ← correction (annee au lieu de anneet)
        document.getElementById('ville').value = ville;
        document.getElementById('numeroDossierAdp').value = numDossier; // ← correction (suppression du 'E')
        document.getElementById('document').value = documentValue;
        // document.getElementById('communequartier').value = communequartier; // À décommenter si le champ existe
        document.getElementById('adresse').value = adresse;
        document.getElementById('contact').value = contact;
        document.getElementById('fonctions').value = fonctions;
        document.getElementById('datedocument').value = htmlDate; // déjà au bon format

        // Mise à jour du statut
        if (statutId) {
            document.getElementById('statutId').value = statutId;
        }

        // Déclencher l'événement change sur le select Code pour mettre à jour les champs Operation et Ville
        const codeSelect = document.getElementById('Code');
        if (codeSelect) {
            codeSelect.dispatchEvent(new Event('change'));
        }
    
        // Mettre à jour le nom du fichier actuel et gérer le bouton de suppression
        const deleteFileBtn = document.getElementById('deleteCurrentFile');
        const currentFileElement = document.getElementById('currentFile');
        
        if(fichier && fichier.trim() !== ''){
            let filename = fichier.split('\\').pop().split('/').pop();
            currentFileElement.textContent = `Fichier actuel : ${filename}`;
            // Stocker le nom du fichier actuel dans un data attribute
            deleteFileBtn.setAttribute('data-current-filename', filename);
            // Afficher le bouton de suppression
            deleteFileBtn.style.display = 'block';
        } else {
            currentFileElement.textContent = 'Aucun fichier associé';
            // Cacher le bouton de suppression
            deleteFileBtn.style.display = 'none';
        }
        // Mettre à jour le titre de la modale
        document.getElementById('adpModalLabel').textContent = `Modifier ${documentValue}`;
    });
}

// Gestion de la suppression du fichier actuel
const deleteCurrentFileBtn = document.getElementById('deleteCurrentFile');
if (deleteCurrentFileBtn) {
    deleteCurrentFileBtn.addEventListener('click', function() {
        const adpId = document.getElementById('adpId').value;
        
        if (confirm('Êtes-vous sûr de vouloir supprimer le fichier actuel ? Cette action est irréversible.')) {
            // Créer FormData pour envoyer la requête
            const formData = new FormData();
            formData.append('id', adpId);
            
            // Récupérer le token anti-falsification
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            
            // Appeler le handler de suppression via AJAX
            fetch(`?handler=DeleteFileAdp`, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            })
            .then(response => {
                if (response.ok) {
                    return response.text();
                } else {
                    throw new Error('Erreur lors de la suppression');
                }
            })
            .then(message => {
                // Mettre à jour l'interface
                document.getElementById('deleteFileFlag').value = 'true';
                document.getElementById('currentFile').textContent = 'Fichier supprimé avec succès';
                document.getElementById('deleteCurrentFile').style.display = 'none';
                document.getElementById('fichier').value = '';
                
                // Afficher un message de succès
                alert('Fichier supprimé avec succès !');
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Erreur lors de la suppression du fichier');
            });
        }
    });
}

// Réinitialiser le flag de suppression si l'utilisateur sélectionne un nouveau fichier
const fichierInput = document.getElementById('fichier');
if (fichierInput) {
    fichierInput.addEventListener('change', function() {
        if (this.files.length > 0) {
            document.getElementById('deleteFileFlag').value = 'false';
        }
    });
}

// Gestion de l'enregistrement des modifications
const saveAdpBtn = document.getElementById('saveadp');
if (saveAdpBtn) {
    saveAdpBtn.addEventListener('click', function() {
        const form = document.getElementById('adpForm');
        const formData = new FormData(form);
        const adpId = document.getElementById('adpId').value;
        
        // Ajouter l'ID ADP au FormData
        formData.append('IdAdp', adpId);
        
        // Récupérer le token anti-falsification
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        // Envoyer la requête de mise à jour
        fetch(`?handler=UpdateAdp`, {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': token
            }
        })
        .then(response => {
            if (response.ok) {
                // Fermer le modal et recharger la page
                const modal = bootstrap.Modal.getInstance(document.getElementById('adpModal'));
                modal.hide();
                location.reload();
            } else {
                alert('Erreur lors de la mise à jour');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Erreur lors de la mise à jour');
        });
    });
}

// Gestion de la suppression complète de ADP
const deleteAdpBtn = document.getElementById('deleteadp');
if (deleteAdpBtn) {
    deleteAdpBtn.addEventListener('click', function() {
        const adpId = document.getElementById('adpId').value;
        
        if (confirm('Êtes-vous sûr de vouloir supprimer complètement cet enregistrement VPL ? Cette action est irréversible.')) {
            const formData = new FormData();
            formData.append('id', adpId);
            
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            
            fetch(`?handler=Deleteadp`, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            })
            .then(response => {
                if (response.ok) {
                    // Fermer le modal et recharger la page
                    const modal = bootstrap.Modal.getInstance(document.getElementById('adpModal'));
                    modal.hide();
                    location.reload();
                } else {
                    alert('Erreur lors de la suppression');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Erreur lors de la suppression');
            });
        }
    });
}

// === TÉLÉCHARGEMENT DES FICHIERS SÉLECTIONNÉS EN ZIP ===
const exportSelectedBtn = document.getElementById('exportSelected');
if (exportSelectedBtn) {
    exportSelectedBtn.addEventListener('click', function(e) {
        e.preventDefault();
        
        const selectedIds = getSelectedAdpIds();
        console.log('IDs sélectionnés:', selectedIds);
        
        if (selectedIds.length === 0) {
            alert('Veuillez sélectionner au moins un fichier à télécharger');
            return;
        }
        
        // Afficher un message de traitement
        alert(`Préparation du téléchargement de ${selectedIds.length} fichier(s)...`);
        
        // Créer un formulaire pour envoyer les IDs au serveur
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '?handler=DownloadSelectedFiles';
        
        // Ajouter le token anti-falsification
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = token;
        form.appendChild(tokenInput);
        
        // Ajouter chaque ID sélectionné
        selectedIds.forEach(id => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'selectedIds';
            input.value = id;
            form.appendChild(input);
            console.log('ID ajouté au formulaire:', id);
        });
        
        // Soumettre le formulaire
        document.body.appendChild(form);
        console.log('Soumission du formulaire...');
        form.submit();
        document.body.removeChild(form);
    });
}

// === SUPPRESSION DES FICHIERS ADP SÉLECTIONNÉS ===
const deleteSelectedBtn = document.getElementById('deleteSelected');
if (deleteSelectedBtn) {
    deleteSelectedBtn.addEventListener('click', function(e) {
        e.preventDefault();
        
        const selectedIds = getSelectedAdpIds();
        if (selectedIds.length === 0) {
            alert('Veuillez sélectionner au moins un fichier à supprimer');
            return;
        }
        
        if (confirm(`Êtes-vous sûr de vouloir supprimer ${selectedIds.length} fichier(s) sélectionné(s) ? Cette action est irréversible.`)) {
            // Créer un formulaire pour envoyer les IDs au serveur
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '?handler=DeleteSelectedFiles';
            
            // Ajouter le token anti-falsification
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token;
            form.appendChild(tokenInput);
            
            // Ajouter chaque ID sélectionné
            selectedIds.forEach(id => {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = 'selectedIds';
                input.value = id;
                form.appendChild(input);
            });
            
            // Soumettre le formulaire
            document.body.appendChild(form);
            form.submit();
        }
    });
}

// === VALIDATION DES FICHIERS ADP SÉLECTIONNÉS ===
const validateSelectedBtn = document.getElementById('validateSelected');
if (validateSelectedBtn) {
    validateSelectedBtn.addEventListener('click', async function(e) {
        e.preventDefault();

        const selectedIds = getSelectedAdpIds(); // fonction existante qui retourne un tableau d'IDs
        console.log('IDs à valider :', selectedIds);

        if (selectedIds.length === 0) {
            alert('Veuillez sélectionner au moins un fichier à valider.');
            return;
        }

        if (!confirm(`Valider définitivement ${selectedIds.length} fichier(s) ADP ?`)) {
            return;
        }

        // Désactiver le bouton pendant l'opération
        const originalText = validateSelectedBtn.innerText;
        validateSelectedBtn.innerText = 'Validation en cours...';
        validateSelectedBtn.disabled = true;

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const formData = new URLSearchParams();
            selectedIds.forEach(id => formData.append('selectedIds', id));
            formData.append('__RequestVerificationToken', token);

            const response = await fetch('?handler=ValidateSelectedFilesAdp', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: formData.toString()
            });

            const result = await response.json();

            if (result.success) {
                alert(result.message);
                // Recharger la page pour voir les nouveaux statuts
                location.reload();
            } else {
                alert('Erreur : ' + result.message);
            }
        } catch (error) {
            console.error('Erreur réseau :', error);
            alert('Une erreur est survenue. Vérifiez votre connexion.');
        } finally {
            validateSelectedBtn.innerText = originalText;
            validateSelectedBtn.disabled = false;
        }
    });
}

// === REJECT DES FICHIERS ADP SÉLECTIONNÉS ===
const rejectSelectedBtn = document.getElementById('rejectSelected');
if (rejectSelectedBtn) {
    rejectSelectedBtn.addEventListener('click', async function(e) {
        e.preventDefault();

        const selectedIds = getSelectedAdpIds(); // fonction existante qui retourne un tableau d'IDs
        console.log('IDs à rejetter :', selectedIds);

        if (selectedIds.length === 0) {
            alert('Veuillez sélectionner au moins un fichier à rejetter.');
            return;
        }

        if (!confirm(`Rejeter définitivement ${selectedIds.length} fichier(s) ADP ?`)) {
            return;
        }

        // Désactiver le bouton pendant l'opération
        const originalText = rejectSelectedBtn.innerText;
        rejectSelectedBtn.innerText = 'Rejet en cours...';
        rejectSelectedBtn.disabled = true;

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const formData = new URLSearchParams();
            selectedIds.forEach(id => formData.append('selectedIds', id));
            formData.append('__RequestVerificationToken', token);

            const response = await fetch('?handler=RejectSelectedFilesAdp', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: formData.toString()
            });

            const result = await response.json();

            if (result.success) {
                alert(result.message);
                // Recharger la page pour voir les nouveaux statuts
                location.reload();
            } else {
                alert('Erreur : ' + result.message);
            }
        } catch (error) {
            console.error('Erreur réseau :', error);
            alert('Une erreur est survenue. Vérifiez votre connexion.');
        } finally {
            rejectSelectedBtn.innerText = originalText;
            rejectSelectedBtn.disabled = false;
        }
    });
}