document.addEventListener('DOMContentLoaded', function() {

    // Sélectionner tous les boutons d'onglet
    const tabs = document.querySelectorAll('[data-tab]');
    // Sélectionner tous les conteneurs de tableaux
    const containers = document.querySelectorAll('.log-table-container');

    // Ajouter un écouteur à chaque bouton
    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            const target = this.getAttribute('data-tab');
            
            // Désactiver tous les boutons (enlever la classe active)
            tabs.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            
            // Masquer tous les conteneurs
            containers.forEach(container => container.classList.remove('active'));
            
            // Afficher le conteneur correspondant à l'onglet cliqué
            const activeContainer = document.getElementById(`tab-${target}`);
            if (activeContainer) {
                activeContainer.classList.add('active');
            }
        });
    });

});