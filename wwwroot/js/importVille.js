// Gestion de l'importation Excel
document.addEventListener('DOMContentLoaded', function() {
    const excelFileInput = document.getElementById('excelFile');
    const importSubmitBtn = document.getElementById('importSubmitBtn');
    const clearFileBtn = document.getElementById('clearFileBtn');
    const previewSection = document.getElementById('previewSection');
    const previewData = document.getElementById('previewData');
    const previewStats = document.getElementById('previewStats');
    const downloadTemplateBtn = document.getElementById('downloadTemplate');
    const importStatus = document.getElementById('importStatus');
    const importProgress = document.getElementById('importProgress');
    const importMessage = document.getElementById('importMessage');
    const skipFirstRowCheckbox = document.getElementById('skipFirstRow');
    const overwriteExistingCheckbox = document.getElementById('overwriteExisting');
    const importVilleForm = document.getElementById('importVilleForm');

    // Télécharger le gabarit Excel
    downloadTemplateBtn.addEventListener('click', function(e) {
        e.preventDefault();
        
        // Créer un objet Excel simple
        const data = [
            ['CodeVille', 'DescriptionVille'],
            ['VIL001', 'Paris'],
            ['VIL002', 'Lyon'],
            ['VIL003', 'Marseille'],
            ['VIL004', 'Toulouse'],
            ['VIL005', 'Nice']
        ];

        // Créer un fichier Excel
        const ws = XLSX.utils.aoa_to_sheet(data);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Villes');
        
        // Générer et télécharger le fichier
        XLSX.writeFile(wb, 'template_villes.xlsx');
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
        fetch('?handler=ImportVilles', {
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
                        ${data.stats.total} villes traitées<br>
                        ${data.stats.added} nouvelles villes ajoutées<br>
                        ${data.stats.updated} villes mises à jour<br>
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
            importSubmitBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Importer les villes';
        });
    });

    // Réinitialiser la modale quand elle se ferme
    document.getElementById('importVilleModal').addEventListener('hidden.bs.modal', function() {
        excelFileInput.value = '';
        importSubmitBtn.disabled = true;
        importSubmitBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Importer les villes';
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
        const url = '?handler=DownloadTemplate';
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
                a.download = 'Template_Villes.xlsx';
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

