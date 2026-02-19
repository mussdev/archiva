// Dans votre fichier JavaScript ou dans un script inline après la modale

// Code javascript pour gérer d'autres interactions si nécessaire
document.addEventListener('DOMContentLoaded', function() {
    // Initialiser les toasts
    const successToast = new bootstrap.Toast(document.getElementById('successToast'));
    // Gestion du menu d'administration
    document.querySelectorAll('.admin-menu-link').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Retirer la classe active de tous les liens et sections
            document.querySelectorAll('.admin-menu-link').forEach(l => l.classList.remove('active'));
            document.querySelectorAll('.admin-section').forEach(s => s.classList.remove('active'));
            
            // Ajouter la classe active au lien cliqué
            this.classList.add('active');
            
            // Afficher la section correspondante
            const targetId = this.getAttribute('data-target');
            document.getElementById(targetId).classList.add('active');
            
            // Mettre à jour l'URL sans recharger la page
            history.pushState(null, null, `#${targetId}`);
        });
    });
    
    // Vérifier si une ancre est présente dans l'URL au chargement de la page
    if (window.location.hash) {
        const targetId = window.location.hash.substring(1);
        const targetLink = document.querySelector(`.admin-menu-link[data-target="${targetId}"]`);
        
        if (targetLink) {
            targetLink.click();
        }
    }

    // Filtrage des utilisateurs
    const filterInput = document.getElementById('filterInput');
    const tableBody = document.getElementById('userTableBody');
    const rows = tableBody.getElementsByTagName('tr');
    const filterStats = document.getElementById('filterStats');

    // Initialiser le compteur
    filterStats.textContent = `${rows.length} utilisateur(s)`;

    filterInput.addEventListener('input', function() {
        const filterText = this.value.toLowerCase();
        let visibleCount = 0;
        
        for (let i = 0; i < rows.length; i++) {
            const cells = rows[i].getElementsByTagName('td');
            let found = false;
            
            // Vérifier chaque cellule (sauf la dernière colonne Actif)
            for (let j = 0; j < cells.length - 1; j++) {
                const cellText = cells[j].textContent.toLowerCase();
                if (cellText.includes(filterText)) {
                    found = true;
                    break;
                }
            }
            
            // Afficher ou masquer la ligne en fonction du résultat
            if (found) {
                rows[i].style.display = '';
                visibleCount++;
                
                // Mettre en évidence le texte correspondant
                if (filterText) {
                    rows[i].classList.add('highlight');
                } else {
                    rows[i].classList.remove('highlight');
                }
            } else {
                rows[i].style.display = 'none';
                rows[i].classList.remove('highlight');
            }
        }
        
        // Mettre à jour le compteur
        filterStats.textContent = `${visibleCount} utilisateur${visibleCount !== 1 ? 's' : ''}`;
    });

    // Gestion du modal de modification d'utilisateur
    const userModal = document.getElementById('userModal');
    if (userModal) {
        userModal.addEventListener('show.bs.modal', function (event) {
            // Le bouton qui a déclenché la modale
            const button = event.relatedTarget;
            
            // Extraire les données des attributs data-*
            const userId = button.getAttribute('data-user-id');
            const nom = button.getAttribute('data-nom');
            const prenom = button.getAttribute('data-prenom');
            const email = button.getAttribute('data-email');
            const contact = button.getAttribute('data-contact');
            const role = button.getAttribute('data-role');
            const actif = button.getAttribute('data-actif');
            const statuts = button.getAttribute('data-statuts'); // Nouveaux statuts
            
            // Mettre à jour le contenu de la modale
            document.getElementById('userId').value = userId;
            document.getElementById('nom').value = nom;
            document.getElementById('prenom').value = prenom;
            document.getElementById('email').value = email;
            document.getElementById('contact').value = contact;
            document.getElementById('role').value = role;
            document.getElementById('actif').checked = actif === '1';
            
            // Gestion des statuts : décocher toutes les cases d'abord
            document.querySelectorAll('.user-statut-checkbox-edit').forEach(checkbox => {
                checkbox.checked = false;
            });

            // Cocher les cases correspondant aux statuts de l'utilisateur
            if (statuts) {
                const statutArray = statuts.split(',');
                statutArray.forEach(statutId => {
                    const checkbox = document.getElementById('userStatut-edit-' + statutId);
                    if (checkbox) {
                        checkbox.checked = true;
                    }
                });
            }

            // Mettre à jour le compteur de statuts sélectionnés
            updateSelectedStatutsCountEdit();
            
            // Mettre à jour le titre de la modale
            document.getElementById('userModalLabel').textContent = `Modifier ${prenom} ${nom}`;
        });
    }

    // Gestion de la sauvegarde des modifications de l'utilisateur
    const saveUserButton = document.getElementById('saveUser');
    if (saveUserButton) {
        saveUserButton.addEventListener('click', function() {
            const userId = document.getElementById('userId').value;
            const nom = document.getElementById('nom').value;
            const prenom = document.getElementById('prenom').value;
            const email = document.getElementById('email').value;
            const contact = document.getElementById('contact').value;
            const role = document.getElementById('role').value;
            const actif = document.getElementById('actif').checked ? 1 : 0;
            const createPassword = document.getElementById('createPasswordEdit').value;
            const confirmPassword = document.getElementById('createConfirmPasswordEdit').value;

            // Récupérer les IDs des statuts sélectionnés
            const statutIds = Array.from(document.querySelectorAll('.user-statut-checkbox-edit:checked'))
                .map(checkbox => checkbox.value)
                .join(','); // Convertir en chaîne séparée par des virgules

            // Validation du mot de passe (si fourni)
            if (createPassword && createPassword !== confirmPassword) {
                alert('Les mots de passe ne correspondent pas');
                return;
            }
        
            // Appel AJAX à la méthode OnPostEditUserAsync
            fetch('?handler=EditUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: new URLSearchParams({
                    'userId': userId,
                    'nom': nom,
                    'prenom': prenom,
                    'email': email,
                    'contact': contact,
                    'groupeId': role,
                    'actif': actif,
                    'password': createPassword || '', // Envoyer le mot de passe s'il est fourni
                    'statuts': statutIds  // Envoyer les IDs des statuts sélectionnés sous forme de chaîne
                })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message);
                    // Fermer la modale et actualiser la page
                    const modal = bootstrap.Modal.getInstance(document.getElementById('userModal'));
                    modal.hide();
                    location.reload();
                } else {
                    alert(data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Une erreur est survenue');
            });
        });
    }

    // Gestion de la suppression
    const deleteUserButton = document.getElementById('deleteUser');
    if (deleteUserButton) {
        deleteUserButton.addEventListener('click', function() {
            const userId = document.getElementById('userId').value;
            const nom = document.getElementById('nom').value;
            const prenom = document.getElementById('prenom').value;
            
            if(confirm(`Êtes-vous sûr de vouloir supprimer l'utilisateur ${prenom} ${nom} ?`)) {
                // Appel AJAX à la méthode OnPostDeleteUserAsync
                fetch('?handler=DeleteUser', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: new URLSearchParams({
                        'userId': userId
                    })
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert(data.message);
                        // Fermer la modale et actualiser la page
                        const modal = bootstrap.Modal.getInstance(document.getElementById('userModal'));
                        modal.hide();
                        location.reload();
                    } else {
                        alert(data.message);
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('Une erreur est survenue');
                });
            }
        });
    }

    // Gestion de la création d'utilisateur
    const createUserButton = document.getElementById('createUser');
    if (createUserButton) {
        createUserButton.addEventListener('click', function() {
            const nom = document.getElementById('createNom').value;
            const prenom = document.getElementById('createPrenom').value;
            const contact = document.getElementById('createContact').value;
            const email = document.getElementById('createEmail').value;
            const password = document.getElementById('createPassword').value;
            const confirmPassword = document.getElementById('createConfirmPassword').value;
            const role = document.getElementById('createRole').value;
            const actif = document.getElementById('createActif').checked ? 1 : 0;

            // Validation
            if (!nom || !prenom || !contact || !email || !password || !confirmPassword || !role) {
                alert('Veuillez remplir tous les champs obligatoires.');
                return;
            }

            if (password !== confirmPassword) {
                alert('Les mots de passe ne correspondent pas.');
                return;
            }

            if (password.length < 6) {
                alert('Le mot de passe doit contenir au moins 6 caractères.');
                return;
            }

            // Récupérer les IDs des statuts sélectionnés
            const statutIds = Array.from(document.querySelectorAll('.user-statut-checkbox:checked'))
                .map(checkbox => parseInt(checkbox.value));

            // Appel AJAX à la méthode OnPostCreateUserAsync
            fetch('?handler=CreateUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({
                    nom: nom,
                    prenom: prenom,
                    email: email,
                    contact: contact,
                    password: password,
                    groupeId: parseInt(role),
                    actif: actif,
                    statutIds: statutIds  // Ajout des statuts
                })
            })
            .then(response => {
            // Vérifier si la réponse est OK (statut 200-299)
            if (!response.ok) {
                // Si ce n'est pas le cas, essayer de lire le message d'erreur
                return response.json().then(errorData => {
                    throw new Error(errorData.message || 'Erreur serveur');
                }).catch(() => {
                    // Si le parsing JSON échoue, utiliser le texte brut
                    return response.text().then(text => {
                        throw new Error(text || 'Erreur serveur');
                    });
                });
            }
            // Si la réponse est OK, parser le JSON
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Afficher le toast de succès
                const successToast = new bootstrap.Toast(document.getElementById('successToast'));
                successToast.show();
                
                // Fermer la modale
                const createUserModal = bootstrap.Modal.getInstance(document.getElementById('createUserModal'));
                createUserModal.hide();
                
                // Réinitialiser le formulaire
                document.getElementById('createUserForm').reset();
                
                // Actualiser la page après un délai pour voir le toast
                setTimeout(() => {
                    location.reload();
                }, 1500);
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Une erreur est survenue: ' + error.message);
        });
        });
    }

    // Gestion de la création des statuts
    const createStatutButton = document.getElementById('createStatut');
    if (createStatutButton) {
        createStatutButton.addEventListener('click', function() {
            const form = document.getElementById('createStatutForm');
            const formData = new FormData(form);

            // Validation : vérifier que les champs obligatoires ne sont pas vides
            const codeStatut = formData.get('codeStatut');
            const descriptionStatut = formData.get('descriptionStatut');
            // Note : le champ noteStatut n'est pas obligatoire, donc pas de vérification

            if (!codeStatut || !codeStatut.trim() || !descriptionStatut || !descriptionStatut.trim()) {
                alert('Veuillez remplir tous les champs obligatoires.');
                return;
            }

            // Appel AJAX à la méthode OnPostCreateUserAsync
            fetch('?handler=CreateStatut', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: formData
            })
            .then(response => {
            // Vérifier si la réponse est OK (statut 200-299)
            if (!response.ok) {
                // Si ce n'est pas le cas, essayer de lire le message d'erreur
                return response.json().then(errorData => {
                    throw new Error(errorData.message || 'Erreur serveur');
                }).catch(() => {
                    // Si le parsing JSON échoue, utiliser le texte brut
                    return response.text().then(text => {
                        throw new Error(text || 'Erreur serveur');
                    });
                });
            }
            // Si la réponse est OK, parser le JSON
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // 1. Mettre à jour le message du toast
                const toastMessageElement = document.getElementById('successToastMessage');
                if (toastMessageElement) {
                    toastMessageElement.textContent = data.message; // "Statut créé avec succès"
                }
                
                // 2. Afficher le toast
                const toastElement = document.getElementById('successToast');
                if (toastElement) {
                    const toast = new bootstrap.Toast(toastElement, {
                        autohide: true,
                        delay: 3000
                    });
                    toast.show();
                }
                
                // Fermer la modale
                const createStatutModal = bootstrap.Modal.getInstance(document.getElementById('createStatutModal'));
                createStatutModal.hide();

                // Réinitialiser le formulaire
                document.getElementById('createStatutForm').reset();

                // Actualiser la page après un délai pour voir le toast
                setTimeout(() => {
                    location.reload();
                }, 2000);
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Une erreur est survenue: ' + error.message);
        });
        });
    }

    // Code pour la gestion des chechkboxes dans le tableau des statuts
    const selectAllCheckbox = document.getElementById('selectAllCheckboxStatuts');
    const checkboxes = document.querySelectorAll('.statut-checkbox');

    // Sélectionner/désélectionner toutes les cases
    selectAllCheckbox.addEventListener('click', function() {
        checkboxes.forEach(checkbox => {
            checkbox.checked = selectAllCheckbox.checked;
        });
    });

    // Mettre à jour la case "Tout sélectionner"
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('click', function() {
            const allChecked = Array.from(checkboxes).every(cb => cb.checked);
            selectAllCheckbox.checked = allChecked;
        });
    });
  
    // Code pour la suppression multiple des statuts - VERSION AVEC RECHARGEMENT
    const deleteSelectedButton = document.getElementById('deleteSelected');

    if (!deleteSelectedButton) {
        console.error("Bouton 'Supprimer la sélection' non trouvé !");
        return;
    }

    deleteSelectedButton.addEventListener('click', function(e) {
        e.preventDefault();
        
        // Récupérer les IDs des statuts sélectionnés depuis data-statut-id
        const selectedStatutIds = Array.from(document.querySelectorAll('.statut-checkbox:checked'))
            .map(checkbox => parseInt(checkbox.getAttribute('data-statut-id')));
        
        console.log('IDs sélectionnés:', selectedStatutIds);

        if (selectedStatutIds.length === 0) {
            alert('Veuillez sélectionner au moins un statut à supprimer.');
            return;
        }

        if (!confirm(`Êtes-vous sûr de vouloir supprimer les ${selectedStatutIds.length} statut(s) sélectionné(s) ?`)) {
            return;
        }

        // Récupérer le token anti-falsification
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!token) {
            console.error('Token anti-falsification non trouvé !');
            alert('Erreur de sécurité. Veuillez recharger la page.');
            return;
        }

        console.log('Envoi des données:', selectedStatutIds);
        
        // Désactiver le bouton pendant l'opération
        deleteSelectedButton.disabled = true;
        deleteSelectedButton.textContent = 'Suppression en cours...';
        
        // Appel AJAX à la méthode OnPostDeleteStatutsAsync
        fetch('?handler=DeleteStatuts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token.value
            },
            body: JSON.stringify(selectedStatutIds) // Envoyer directement le tableau
        })
        .then(response => {
            console.log('Réponse reçue:', response.status);
            if (!response.ok) {
                throw new Error('Erreur HTTP: ' + response.status);
            }
            return response.json();
        })
        .then(data => {
            console.log('Données reçues:', data);
            if (data.success) {
                alert(data.message);
                
                // OPTION 2: Rechargement sans cache (force le serveur à renvoyer les données)
                location.reload(true);
                
            } else {
                alert('Erreur: ' + data.message);
                deleteSelectedButton.disabled = false;
                deleteSelectedButton.textContent = 'Supprimer la sélection';
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Une erreur est survenue: ' + error.message);
            deleteSelectedButton.disabled = false;
            deleteSelectedButton.textContent = 'Supprimer la sélection';
        });
    });


    // Code pour la modification des statuts
    const editStatutModal = document.getElementById('editStatutModal');
    if (editStatutModal) {
        editStatutModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const statutId = button.getAttribute('data-statut-id');
            const codeStatut = button.getAttribute('data-code-statut');
            const descriptionStatut = button.getAttribute('data-description-statut');
            const noteStatut = button.getAttribute('data-note-statut');
            
            document.getElementById('editStatutId').value = statutId;
            document.getElementById('editCodeStatut').value = codeStatut;
            document.getElementById('editDescriptionStatut').value = descriptionStatut;
            document.getElementById('editNoteStatut').value = noteStatut;
            
            document.getElementById('editStatutModalLabel').textContent = `Modifier le statut ${codeStatut}`;
        });
    }

    // Gestion de la sauvegarde des modifications du statut
    const saveStatutButton = document.getElementById('saveStatutChanges');
    if (saveStatutButton) {
        saveStatutButton.addEventListener('click', function() {
            const statutId = document.getElementById('editStatutId').value;
            const codeStatut = document.getElementById('editCodeStatut').value;
            const descriptionStatut = document.getElementById('editDescriptionStatut').value;
            const noteStatut = document.getElementById('editNoteStatut').value;

            if (!codeStatut || !codeStatut.trim() || !descriptionStatut || !descriptionStatut.trim()) {
                alert('Veuillez remplir tous les champs obligatoires.');
                return;
            }

            const originalText = saveStatutButton.innerHTML;
            saveStatutButton.disabled = true;
            saveStatutButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Enregistrement...';

            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch('?handler=EditStatut', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams({
                    'idStatut': statutId,
                    'codeStatut': codeStatut,
                    'descriptionStatut': descriptionStatut,
                    'noteStatut': noteStatut
                })
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
                    const modal = bootstrap.Modal.getInstance(editStatutModal);
                    modal.hide();
                    
                    // Afficher message et recharger
                    alert(data.message || 'Statut modifié avec succès');
                    location.reload(); // <-- ICI on recharge la page
                } else {
                    alert('Erreur: ' + data.message);
                    saveStatutButton.disabled = false;
                    saveStatutButton.innerHTML = originalText;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Une erreur est survenue lors de la modification');
                saveStatutButton.disabled = false;
                saveStatutButton.innerHTML = originalText;
            });
        });
    }

    // Gestion du "Tout sélectionner" dans la modale de création d'utilisateur
    const checkAllUserStatuts = document.getElementById('checkAllUserStatuts');
    if (checkAllUserStatuts) {
        checkAllUserStatuts.addEventListener('change', function() {
            const allCheckboxes = document.querySelectorAll('.user-statut-checkbox');
            allCheckboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
            updateSelectedStatutsCount();
        });
    }

    // Mettre à jour le compteur quand une checkbox change
    const statutCheckboxes = document.querySelectorAll('.user-statut-checkbox');
    statutCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', updateSelectedStatutsCount);
    });

    // Fonction pour mettre à jour le compteur de statuts sélectionnés
    function updateSelectedStatutsCount() {
        const selectedCount = document.querySelectorAll('.user-statut-checkbox:checked').length;
        const countElement = document.getElementById('selectedStatutsCount');
        if (countElement) {
            countElement.textContent = selectedCount;
        }
    }

    // Initialiser le compteur
    updateSelectedStatutsCount();

    // Gestion du "Tout sélectionner" dans la modale de modification d'utilisateur
    const checkAllUserStatutsEdit = document.getElementById('checkAllUserStatutsEdit');
    if (checkAllUserStatutsEdit) {
        checkAllUserStatutsEdit.addEventListener('change', function() {
            const allCheckboxes = document.querySelectorAll('.user-statut-checkbox-edit');
            allCheckboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
            updateSelectedStatutsCountEdit();
        });
    }

    // Mettre à jour le compteur quand une checkbox change
    const statutCheckboxesEdit = document.querySelectorAll('.user-statut-checkbox-edit');
    statutCheckboxesEdit.forEach(checkbox => {
        checkbox.addEventListener('change', updateSelectedStatutsCountEdit);
    });

    // Fonction pour mettre à jour le compteur de statuts sélectionnés dans la modal d'édition
    function updateSelectedStatutsCountEdit() {
        const selectedCount = document.querySelectorAll('.user-statut-checkbox-edit:checked').length;
        const countElement = document.getElementById('selectedStatutsCountEdit');
        if (countElement) {
            countElement.textContent = selectedCount;
        }
    }

    // Initialiser le compteur
    updateSelectedStatutsCountEdit();

    /** Fin de la gestion des statuts */

    /* Gestion des villes */
    // Gestion de la création des villes
    const createVilleButton = document.getElementById('createVille');
    if (createVilleButton) {
        createVilleButton.addEventListener('click', function() {
            const form = document.getElementById('createVilleForm');
            const formData = new FormData(form);

            // Validation : vérifier que les champs obligatoires ne sont pas vides
            const codeVille = formData.get('codeVille');
            const descriptionVille = formData.get('descriptionVille');
            // Note : le champ noteVille n'est pas obligatoire, donc pas de vérification

            if (!codeVille || !codeVille.trim() || !descriptionVille || !descriptionVille.trim()) {
                alert('Veuillez remplir tous les champs obligatoires.');
                return;
            }

            // Appel AJAX à la méthode OnPostCreateUserAsync
            fetch('?handler=CreateVille', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: formData
            })
            .then(response => {
            // Vérifier si la réponse est OK (statut 200-299)
            if (!response.ok) {
                // Si ce n'est pas le cas, essayer de lire le message d'erreur
                return response.json().then(errorData => {
                    throw new Error(errorData.message || 'Erreur serveur');
                }).catch(() => {
                    // Si le parsing JSON échoue, utiliser le texte brut
                    return response.text().then(text => {
                        throw new Error(text || 'Erreur serveur');
                    });
                });
            }
            // Si la réponse est OK, parser le JSON
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // 1. Mettre à jour le message du toast
                const toastMessageElement = document.getElementById('successToastMessage');
                if (toastMessageElement) {
                    toastMessageElement.textContent = data.message; // "Statut créé avec succès"
                }
                
                // 2. Afficher le toast
                const toastElement = document.getElementById('successToast');
                if (toastElement) {
                    const toast = new bootstrap.Toast(toastElement, {
                        autohide: true,
                        delay: 3000
                    });
                    toast.show();
                }
                
                // Fermer la modale
                const createVilleModal = bootstrap.Modal.getInstance(document.getElementById('createVilleModal'));
                createVilleModal.hide();

                // Réinitialiser le formulaire
                document.getElementById('createVilleForm').reset();

                // Actualiser la page après un délai pour voir le toast
                setTimeout(() => {
                    location.reload();
                }, 2000);
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Une erreur est survenue: ' + error.message);
        });
        });
    }

    // Code pour la modification des villes
    const editVilleModal = document.getElementById('editVilleModal');
    if (editVilleModal) {
        editVilleModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const villeId = button.getAttribute('data-ville-id');
            const codeVille = button.getAttribute('data-code-ville');
            const descriptionVille = button.getAttribute('data-description-ville');
            
            document.getElementById('editVilleId').value = villeId;
            document.getElementById('editCodeVille').value = codeVille;
            document.getElementById('editDescriptionVille').value = descriptionVille;
            
            document.getElementById('editVilleModalLabel').textContent = `Modifier la ville ${codeVille}`;
        });
    }

    // Gestion de la sauvegarde des modifications de la ville
    const saveVilleButton = document.getElementById('saveVilleChanges');
    if (saveVilleButton) {
        saveVilleButton.addEventListener('click', function() {
            const villeId = document.getElementById('editVilleId').value;
            const codeVille = document.getElementById('editCodeVille').value;
            const descriptionVille = document.getElementById('editDescriptionVille').value;

            if (!codeVille || !codeVille.trim() || !descriptionVille || !descriptionVille.trim()) {
                alert('Veuillez remplir tous les champs obligatoires.');
                return;
            }

            const originalText = saveStatutButton.innerHTML;
            saveStatutButton.disabled = true;
            saveStatutButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Enregistrement...';

            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch('?handler=EditVille', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: new URLSearchParams({
                    'idVille': villeId,
                    'codeVille': codeVille,
                    'descriptionVille': descriptionVille
                })
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
                    const modal = bootstrap.Modal.getInstance(editVilleModal);
                    modal.hide();
                    
                    // Afficher message et recharger
                    alert(data.message || 'Ville modifiée avec succès');
                    location.reload(); // <-- ICI on recharge la page
                } else {
                    alert('Erreur: ' + data.message);
                    saveVilleButton.disabled = false;
                    saveVilleButton.innerHTML = originalText;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Une erreur est survenue lors de la modification');
                saveVilleButton.disabled = false;
                saveVilleButton.innerHTML = originalText;
            });
        });
    }

    // Gestion du "Tout sélectionner" pour les suppressions des villes dans le tableau
    const selectAllCheckboxVille = document.getElementById('selectAllCheckboxVille');
    const checkboxesVille = document.querySelectorAll('.ville-checkbox');

    // Sélectionner/désélectionner toutes les cases
    selectAllCheckboxVille.addEventListener('click', function() {
        checkboxesVille.forEach(checkbox => {
            checkbox.checked = selectAllCheckboxVille.checked;
        });
    });

    // Mettre à jour la case "Tout sélectionner"
    checkboxesVille.forEach(checkbox => {
        checkbox.addEventListener('click', function() {
            const allChecked = Array.from(checkboxesVille).every(cb => cb.checked);
            selectAllCheckboxVille.checked = allChecked;
        });
    });

    // Code pour la suppression multiple des villes - VERSION AVEC RECHARGEMENT
    const deleteSelectedButtonVille = document.getElementById('deleteSelectedVille');
    if (!deleteSelectedButtonVille) {
        console.error("Bouton 'Supprimer la sélection' non trouvé !");
        return;
    }

    deleteSelectedButtonVille.addEventListener('click', function(e) {
        e.preventDefault();

        // Récupérer les IDs des villes sélectionnés depuis data-ville-id
        const selectedVilleIds = Array.from(document.querySelectorAll('.ville-checkbox:checked'))
            .map(checkbox => parseInt(checkbox.getAttribute('data-ville-id')));
        
        console.log('IDs sélectionnés:', selectedVilleIds);

        if (selectedVilleIds.length === 0) {
            alert('Veuillez sélectionner au moins une ville à supprimer.');
            return;
        }

        if (!confirm(`Êtes-vous sûr de vouloir supprimer les ${selectedVilleIds.length} ville(s) sélectionnée(s) ?`)) {
            return;
        }

        // Récupérer le token anti-falsification
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!token) {
            console.error('Token anti-falsification non trouvé !');
            alert('Erreur de sécurité. Veuillez recharger la page.');
            return;
        }

        console.log('Envoi des données:', selectedVilleIds);
        
        // Désactiver le bouton pendant l'opération
        deleteSelectedButtonVille.disabled = true;
        deleteSelectedButtonVille.textContent = 'Suppression en cours...';
        
        // Appel AJAX à la méthode OnPostDeleteStatutsAsync
        fetch('?handler=DeleteVille', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token.value
            },
            body: JSON.stringify(selectedVilleIds) // Envoyer directement le tableau
        })
        .then(response => {
            console.log('Réponse reçue:', response.status);
            if (!response.ok) {
                throw new Error('Erreur HTTP: ' + response.status);
            }
            return response.json();
        })
        .then(data => {
            console.log('Données reçues:', data);
            if (data.success) {
                alert(data.message);
                
                // OPTION 2: Rechargement sans cache (force le serveur à renvoyer les données)
                location.reload(true);
                
            } else {
                alert('Erreur: ' + data.message);
                deleteSelectedButtonVille.disabled = false;
                deleteSelectedButtonVille.textContent = 'Supprimer la sélection';
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Une erreur est survenue: ' + error.message);
            deleteSelectedButtonVille.disabled = false;
            deleteSelectedButtonVille.textContent = 'Supprimer la sélection';
        });
    }); 

    /* Fin de la gestion des villes */
                    
});
