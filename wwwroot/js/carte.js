document.addEventListener("DOMContentLoaded", function () {

    // Récupérer les éléments du tableau
    var table = document.querySelector('table');
    if (!table) {
        console.warn('Aucune table trouvée, filtrage de tableau désactivé dans carte.js.');
    }

    var rows = table ? table.querySelectorAll('tbody tr') : [];
    var rowsArray = Array.from(rows);
    var filterStats = document.getElementById('filterStats');
    var paginationInfo = document.getElementById('paginationInfo');
    var paginationContainer = document.getElementById('paginationContainer');
    var pageSizeSelect = document.getElementById('pageSizeSelect');
    var totalRows = rowsArray.length;
    var currentPage = 1;
    var pageSize = pageSizeSelect ? parseInt(pageSizeSelect.value, 10) || 10 : 10;

    // Récupérer le champ de filtre
    const tableFilter = document.getElementById('table-filter');

    function getFilteredRows() {
        return rowsArray.filter(function(row) {
            return row.dataset.matchesFilter !== 'false';
        });
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
            for (var i = 1; i <= totalPages; i++) pagesToShow.push(i);
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

        // Réinitialiser "Tout sélectionner" lors du changement de page
        const selectAllCheckbox = document.getElementById('selectAllCheckbox');
        if (selectAllCheckbox) {
            selectAllCheckbox.checked = false;
        }

        renderPagination();
        updateFilterStats();
    }

    // Fonction pour mettre à jour les statistiques
    function updateFilterStats() {
        if (!filterStats) return;

        var filteredRows = getFilteredRows();

        if (!tableFilter || tableFilter.value === '') {
            filterStats.textContent = totalRows + ' fichier(s) trouvé(s)';
        } else {
            filterStats.textContent = filteredRows.length + ' fichier(s) sur ' + totalRows + ' trouvé(s)';
        }
    }

    // Fonction pour filtrer les données
    function filterData() {
        var filterValue = tableFilter.value.toLowerCase();

        rowsArray.forEach(function(row) {
            var rowText = row.textContent.toLowerCase();
            row.dataset.matchesFilter = rowText.indexOf(filterValue) === -1 ? 'false' : 'true';
        });

        currentPage = 1;
        showPage();
    }

    // Initialiser la pagination et les statistiques au chargement
    rowsArray.forEach(function(row) {
        row.dataset.matchesFilter = 'true';
    });
    showPage();

    // Ajouter un événement pour filtrer les données lors de la saisie dans le champ de filtre
    if (tableFilter) {
        tableFilter.addEventListener('input', filterData);
    } else {
        console.warn('Filtre de tableau manquant : #table-filter');
    }

    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function() {
            pageSize = parseInt(this.value, 10) || 10;
            currentPage = 1;
            showPage();
        });
    }

    // Réinitialiser le formulaire quand le modal est fermé
    var uploadForm = document.getElementById('uploadForm');
    var ajoutFichierModal = document.getElementById('ajoutFichierModal');
    if (ajoutFichierModal && uploadForm) {
        ajoutFichierModal.addEventListener('hidden.bs.modal', function () {
            uploadForm.reset();
        });
    }

    /* === GESTION DES CASES À COCHER === */
    const selectAllCheckbox = document.getElementById('selectAllCheckbox');
        const rowCheckboxes = document.querySelectorAll('.rowCheckbox');
        
        // Case "Tout sélectionner"
        if (selectAllCheckbox) {
            selectAllCheckbox.addEventListener('change', function() {
                rowCheckboxes.forEach(checkbox => {
                    checkbox.checked = selectAllCheckbox.checked;
                });
            });
        }
        
        // Cases individuelles
        rowCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function() {
                // Décocher "Tout sélectionner" si une case est décochée
                if (!this.checked && selectAllCheckbox.checked) {
                    selectAllCheckbox.checked = false;
                }
                // Cocher "Tout sélectionner" si toutes les cases sont cochées
                else if (this.checked) {
                    const allChecked = Array.from(rowCheckboxes).every(cb => cb.checked);
                    selectAllCheckbox.checked = allChecked;
                }
            });
        });
        
        // Mettre à jour "Tout sélectionner" lors du filtrage
        if (tableFilter) {
            tableFilter.addEventListener('input', function() {
                // Réinitialiser "Tout sélectionner" lors du filtrage
                selectAllCheckbox.checked = false;
            });
        }
    /* === FIN GESTION DES CASES À COCHER === */

    /* === CODE JS POUR L'AFFICHAGE AUTOMATIQUE DU NOM DE L'OPÉRATION ET LA VILLE (AJOUT) === */
    function syncOperationAndVilleFromCode(codeElement) {
        if (!codeElement) return;

        const selectedOption = codeElement.selectedOptions ? codeElement.selectedOptions[0] : codeElement.options[codeElement.selectedIndex];
        if (!selectedOption) return;

        const operationDescription = (selectedOption.dataset.operation || '').trim();
        const villeDescription = (selectedOption.dataset.ville || '').trim();

        const scope = codeElement.closest('.modal') || codeElement.closest('form') || document;
        const operationField = scope.querySelector('input[name="Operation"], input[name="operation"], input#operation, input#Operation');
        const villeField = scope.querySelector('input[name="Ville"], input[name="ville"], input#ville, input#Ville');

        if (operationField) operationField.value = operationDescription;
        if (villeField) villeField.value = villeDescription;

        console.debug('[carte.js] syncOperationAndVilleFromCode', codeElement.id || codeElement.name, codeElement.value, operationDescription, villeDescription);
    }

    function connectCodeToOperationAndVille(codeElement) {
        if (!codeElement) return;

        codeElement.addEventListener('change', function () {
            syncOperationAndVilleFromCode(codeElement);
        });

        const modal = codeElement.closest('.modal');
        if (modal) {
            modal.addEventListener('shown.bs.modal', function () {
                syncOperationAndVilleFromCode(codeElement);
            });
        }

        syncOperationAndVilleFromCode(codeElement);
    }

    const codeSelects = Array.from(document.querySelectorAll('#ajoutFichierModal select#Code, #carteModal select#Code, #vplModal select#Code, select[name="IdOpe"]'));
    if (codeSelects.length) {
        codeSelects.forEach(connectCodeToOperationAndVille);
    } else {
        console.warn('⚠️ Élément Code non trouvé (carte.js)');
    }

    // Force le remplissage quand le modal d'ajout s'ouvre
    const modalAjout = document.getElementById('ajoutFichierModal');
    if (modalAjout) {
        modalAjout.addEventListener('shown.bs.modal', function () {
            const codeSelect = modalAjout.querySelector('select#Code[name="IdOpe"]');
            if (codeSelect) {
                syncOperationAndVilleFromCode(codeSelect);
            }
        });
    }

    /* === FIN DU CODE POUR L'AFFICHAGE AUTOMATIQUE DU NOM DE L'OPÉRATION ET LA VILLE === */

    // Fonction pour obtenir les IDs des éléments sélectionnés
    function getSelectedCartesIds() {
        const selectedCheckboxes = document.querySelectorAll('.rowCheckbox:checked');
        const selectedIds = Array.from(selectedCheckboxes).map(checkbox => {
            return checkbox.getAttribute('data-carte-id');
        });
        return selectedIds;
    }

    // Exemple d'utilisation pour une action groupée
    function performGroupAction() {
        const selectedIds = getSelectedCartesIds();
        if (selectedIds.length === 0) {
            alert('Veuillez sélectionner au moins un élément');
            return;
        }
        
        console.log('IDs sélectionnés :', selectedIds);
        // Ici vous pouvez ajouter votre logique (suppression groupée, export, etc.)
    }

    carteModal.addEventListener('show.bs.modal', function (event) {
    const button = event.relatedTarget;
    document.getElementById('deleteFileFlag').value = 'false';

    // Récupération des attributs data
    const carteId = button.getAttribute('data-carte-id');
    const legendeValue = button.getAttribute('data-legende');
    const tube = button.getAttribute('data-tube');
    const datecarte = button.getAttribute('data-datecarte');
    const ville = button.getAttribute('data-ville');
    const operation = button.getAttribute('data-operation');
    const originalite = button.getAttribute('data-originalite');
    const echelle = button.getAttribute('data-echelle');
    const cote = button.getAttribute('data-cote');
    const operationId = button.getAttribute('data-idoperation');
    const idstatut = button.getAttribute('data-statut-id');  // peut être une chaîne ou un nombre
    const fichier = button.getAttribute('data-fichier');

    // Conversion de la date
    let htmlDate = '';
    if (datecarte) {
        const parts = datecarte.split('/');
        if (parts.length === 3) {
            htmlDate = `${parts[2]}-${parts[1]}-${parts[0]}`;
        }
    }

    // Fonction utilitaire
    const setFieldValue = (fieldId, value) => {
        const field = document.getElementById(fieldId);
        if (field) field.value = value;
        else console.warn(`Élément manquant : #${fieldId}`);
    };

    // Remplissage des champs simples
    setFieldValue('carteId', carteId || '');
    setFieldValue('legende', legendeValue || '');
    setFieldValue('tube', tube || '');
    setFieldValue('dateCarte', htmlDate);
    setFieldValue('ville', ville || '');
    setFieldValue('operation', operation || '');
    setFieldValue('originalite', originalite || '');
    setFieldValue('echelle', echelle || '');
    setFieldValue('cote', cote || '');
    setFieldValue('code', operationId || '');
    setFieldValue('editstatutId', idstatut ?? ''); // utilise null/undefined, pas falsy 0

    
    

    // --- Gestion du code opération (optionnel) ---
    const codeSelect = document.getElementById('code');
    if (codeSelect && operationId) {
        codeSelect.value = operationId;
        if (typeof syncOperationAndVilleFromCode === 'function') {
            syncOperationAndVilleFromCode(codeSelect);
        }
    }

    // --- Gestion du fichier ---
    const deleteFileBtn = document.getElementById('deleteCurrentFile');
    const currentFileElement = document.getElementById('currentFile');
    if (fichier && fichier.trim() !== '') {
        const filename = fichier.split('\\').pop().split('/').pop();
        currentFileElement.textContent = `Fichier actuel : ${filename}`;
        deleteFileBtn.style.display = 'block';
    } else {
        currentFileElement.textContent = 'Aucun fichier associé';
        deleteFileBtn.style.display = 'none';
    }

    // Titre de la modale
    document.getElementById('carteModalLabel').textContent = `Modifier ${legendeValue || 'la carte'}`;
});

    /* === FIN DU CODE DE GESTION DU MODAL DE MODIFICATION Carte === */

    // Gestion de la suppression du fichier actuel
    document.getElementById('deleteCurrentFile').addEventListener('click', function() {
        const carteId = document.getElementById('carteId').value;
        
        if (confirm('Êtes-vous sûr de vouloir supprimer le fichier actuel ? Cette action est irréversible.')) {
            // Créer FormData pour envoyer la requête
            const formData = new FormData();
            formData.append('id', carteId);
            
            // Récupérer le token anti-falsification
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            
            // Appeler le handler de suppression via AJAX
            fetch(`?handler=DeleteFileCarte`, {
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


    // Réinitialiser le flag de suppression si l'utilisateur sélectionne un nouveau fichier
    document.getElementById('fichier').addEventListener('change', function() {
        if (this.files.length > 0) {
            document.getElementById('deleteFileFlag').value = 'false';
            // Ne pas cacher le bouton de suppression si on veut permettre de supprimer l'ancien même avec un nouveau fichier
            // document.getElementById('deleteCurrentFile').style.display = 'none';
        }
    });

    // Gestion de l'enregistrement des modifications
    document.getElementById('savecarte').addEventListener('click', function() {
        const form = document.getElementById('carteForm');
        const formData = new FormData(form);
        const carteId = document.getElementById('carteId').value;
        
        // Ajouter l'ID ADP au FormData
        formData.append('IdCarte', carteId);
        
        // Récupérer le token anti-falsification
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        // Envoyer la requête de mise à jour
        fetch(`?handler=UpdateCarte`, {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': token
            }
        })
        .then(response => {
            if (response.ok) {
                // Fermer le modal et recharger la page
                const modal = bootstrap.Modal.getInstance(document.getElementById('carteModal'));
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

    // Gestion de la suppression complète de carte
    document.getElementById('deletecarte').addEventListener('click', function() {
        const carteId = document.getElementById('carteId').value;
        
        if (confirm('Êtes-vous sûr de vouloir supprimer complètement cet enregistrement carte ? Cette action est irréversible.')) {
            const formData = new FormData();
            formData.append('id', carteId);
            
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            
            fetch(`?handler=Deletecarte`, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            })
            .then(response => {
                if (response.ok) {
                    // Fermer le modal et recharger la page
                    const modal = bootstrap.Modal.getInstance(document.getElementById('carteModal'));
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

    // === TÉLÉCHARGEMENT DES FICHIERS SÉLECTIONNÉS EN ZIP ===
    document.getElementById('exportSelected').addEventListener('click', function(e) {
        e.preventDefault();
        
        const selectedIds = getSelectedCartesIds();
        console.log('IDs sélectionnés:', selectedIds); // Debug
        
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
            console.log('ID ajouté au formulaire:', id); // Debug
        });
        
        // Soumettre le formulaire
        document.body.appendChild(form);
        console.log('Soumission du formulaire...'); // Debug
        form.submit();
        document.body.removeChild(form);
    });

    // === SUPPRESSION DES FICHIERS SÉLECTIONNÉS ===
    document.getElementById('deleteSelected').addEventListener('click', function(e) {
        e.preventDefault();
        
        const selectedIds = getSelectedCartesIds();
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

    // === VALIDATION DES FICHIERS ADP SÉLECTIONNÉS ===
    const validateSelectedBtn = document.getElementById('validateSelected');
    if (validateSelectedBtn) {
        validateSelectedBtn.addEventListener('click', async function(e) {
            e.preventDefault();

            const selectedIds = getSelectedCartesIds(); // fonction existante qui retourne un tableau d'IDs
            console.log('IDs à valider :', selectedIds);

            if (selectedIds.length === 0) {
                alert('Veuillez sélectionner au moins un fichier à valider.');
                return;
            }

            if (!confirm(`Valider définitivement ${selectedIds.length} fichier(s) VPL ?`)) {
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

                const response = await fetch('?handler=ValidateSelectedFilesCarte', {
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

            const selectedIds = getSelectedCartesIds(); // fonction existante qui retourne un tableau d'IDs
            console.log('IDs à rejetter :', selectedIds);

            if (selectedIds.length === 0) {
                alert('Veuillez sélectionner au moins un fichier à rejetter.');
                return;
            }

            if (!confirm(`Rejeter définitivement ${selectedIds.length} fichier(s) VPL ?`)) {
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

                const response = await fetch('?handler=RejectSelectedFilesCarte', {
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

});