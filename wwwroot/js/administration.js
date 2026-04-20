document.addEventListener('DOMContentLoaded', function() {

    // Sélectionner tous les boutons d'onglet de la zone de journalisation
    const tabs = document.querySelectorAll('button[data-log-tab]');
    // Sélectionner tous les conteneurs de tableaux de la zone de journalisation
    const containers = document.querySelectorAll('.log-table-container');

    function setActiveTab(target) {
        if (!target) {
            target = 'all';
        }

        tabs.forEach(btn => {
            const isActive = btn.getAttribute('data-log-tab') === target;
            btn.classList.toggle('active', isActive);
        });

        containers.forEach(container => {
            const isActive = container.getAttribute('data-log-tab') === target;
            container.classList.toggle('active', isActive);
            container.hidden = !isActive;
        });

        // Initialize pagination for validations tab when activated
        if (target === 'validations') {
            initializeValidationPagination();
        }
        // Initialize pagination for transactions tab when activated
        if (target === 'transactions') {
            initializeTransactionPagination();
        }
        // Initialize pagination for operations tab when activated
        if (target === 'operations') {
            initializeOperationPagination();
        }
        // Initialize pagination for cartes tab when activated
        if (target === 'villes') {
            initializeCartePagination();
        }
        // Initialize pagination for login sessions tab when activated
        if (target === 'login') {
            initializeLoginPagination();
        }
        // Initialize pagination for all logs tab when activated
        if (target === 'all') {
            initializeAllPagination();
        }
    }

    function getTabFromUrl() {
        const url = new URL(window.location.href);
        const tabFromQuery = url.searchParams.get('tab');
        if (tabFromQuery) {
            return tabFromQuery;
        }
        const hash = url.hash;
        if (hash && hash.startsWith('#tab-')) {
            return hash.replace('#tab-', '');
        }
        return null;
    }

    function updateUrlParam(key, value) {
        const url = new URL(window.location.href);
        url.searchParams.set(key, value);
        history.replaceState(null, '', url.toString());
    }

    const initialTab = getTabFromUrl() || 'all';
    setActiveTab(initialTab);

    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            const target = this.getAttribute('data-log-tab');
            setActiveTab(target);
            updateUrlParam('tab', target);
        });
    });

    async function submitValidationForm(event) {
        event.preventDefault();
        const form = event.target;
        const formData = new FormData(form);
        formData.set('tab', 'validations');
        const url = `${window.location.pathname}?${new URLSearchParams(formData).toString()}`;

        try {
            const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok) {
                form.submit();
                return;
            }

            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const newTableBody = doc.getElementById('validationTableBody');
            const currentTableBody = document.getElementById('validationTableBody');
            const newPaginationInfo = doc.getElementById('validationPaginationInfo');
            const currentPaginationInfo = document.getElementById('validationPaginationInfo');
            const newSizeForm = doc.getElementById('validationPageSizeForm');
            const currentSizeForm = document.getElementById('validationPageSizeForm');
            const newNavForm = doc.getElementById('validationPageNavForm');
            const currentNavForm = document.getElementById('validationPageNavForm');

            if (newTableBody && currentTableBody && newPaginationInfo && currentPaginationInfo && newSizeForm && currentSizeForm && newNavForm && currentNavForm) {
                currentTableBody.innerHTML = newTableBody.innerHTML;
                currentPaginationInfo.innerHTML = newPaginationInfo.innerHTML;
                currentSizeForm.innerHTML = newSizeForm.innerHTML;
                currentNavForm.innerHTML = newNavForm.innerHTML;
                attachValidationFormListeners();
                history.replaceState(null, '', url);
            } else {
                form.submit();
            }
        } catch (error) {
            form.submit();
        }
    }

    function attachValidationFormListeners() {
        const sizeForm = document.getElementById('validationPageSizeForm');
        const navForm = document.getElementById('validationPageNavForm');

        if (sizeForm) {
            sizeForm.removeEventListener('submit', submitValidationForm);
            sizeForm.addEventListener('submit', submitValidationForm);
        }

        if (navForm) {
            navForm.removeEventListener('submit', submitValidationForm);
            navForm.addEventListener('submit', submitValidationForm);
        }
    }

    attachValidationFormListeners();

    // Function to initialize validation pagination
    function initializeValidationPagination() {
        // Gestion de la pagination pour le tableau de validation
        // Éléments du tableau des validations
        const tbody = document.getElementById('validationTableBody');
        const pageSizeSelect = document.getElementById('validationPageSizeSelect');
        const paginationContainer = document.getElementById('validationPaginationContainer');
        const paginationInfo = document.getElementById('validationPaginationInfo');
        
        if (!tbody || !pageSizeSelect || !paginationContainer || !paginationInfo) {
            console.warn("Éléments de pagination introuvables pour le tableau des validations");
            return;
        }
        
        // Récupération de toutes les lignes du tableau
        let allRows = Array.from(tbody.querySelectorAll('tr'));
        
        // Variables de pagination
        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect.value, 10);
        
        // Fonction pour afficher les lignes de la page courante
        function renderTable() {
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            const rowsToShow = allRows.slice(start, end);
            
            // Masquer toutes les lignes puis afficher celles de la page
            allRows.forEach(row => row.style.display = 'none');
            rowsToShow.forEach(row => row.style.display = 'table-row');
            
            // Mettre à jour les infos de pagination
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationInfo.innerText = `Page ${currentPage} sur ${totalPages} (${allRows.length} élément${allRows.length > 1 ? 's' : ''})`;
        }
        
        // Génération des boutons de pagination
        function renderPagination() {
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationContainer.innerHTML = '';
            
            if (totalPages <= 1) return;
            
            // Bouton Précédent
            const prevLi = document.createElement('li');
            prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
            prevLi.innerHTML = `<a class="page-link" href="#" aria-label="Précédent">&laquo;</a>`;
            prevLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage > 1) {
                    currentPage--;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(prevLi);
            
            // Numéros de pages (avec gestion des ellipses si nécessaire)
            const maxVisible = 5;
            let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
            let endPage = Math.min(totalPages, startPage + maxVisible - 1);
            if (endPage - startPage + 1 < maxVisible) {
                startPage = Math.max(1, endPage - maxVisible + 1);
            }
            
            if (startPage > 1) {
                addPageButton(1);
                if (startPage > 2) addEllipsis();
            }
            
            for (let i = startPage; i <= endPage; i++) {
                addPageButton(i);
            }
            
            if (endPage < totalPages) {
                if (endPage < totalPages - 1) addEllipsis();
                addPageButton(totalPages);
            }
            
            // Bouton Suivant
            const nextLi = document.createElement('li');
            nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
            nextLi.innerHTML = `<a class="page-link" href="#" aria-label="Suivant">&raquo;</a>`;
            nextLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage < totalPages) {
                    currentPage++;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(nextLi);
            
            function addPageButton(pageNum) {
                const li = document.createElement('li');
                li.className = `page-item ${pageNum === currentPage ? 'active' : ''}`;
                li.innerHTML = `<a class="page-link" href="#">${pageNum}</a>`;
                li.addEventListener('click', (e) => {
                    e.preventDefault();
                    currentPage = pageNum;
                    renderTable();
                    renderPagination();
                });
                paginationContainer.appendChild(li);
            }
            
            function addEllipsis() {
                const li = document.createElement('li');
                li.className = 'page-item disabled';
                li.innerHTML = '<span class="page-link">...</span>';
                paginationContainer.appendChild(li);
            }
        }
        
        // Réinitialisation lors du changement du nombre d'éléments par page
        function onPageSizeChange() {
            pageSize = parseInt(pageSizeSelect.value, 10);
            currentPage = 1;
            renderTable();
            renderPagination();
        }
        
        // Écouteur d'événement sur le selecteur de taille
        pageSizeSelect.addEventListener('change', onPageSizeChange);
        
        // Initialisation
        renderTable();
        renderPagination();
    }

    // Function to initialize login pagination
    function initializeLoginPagination() {
        // Gestion de la pagination pour le tableau des sessions de connexion
        const tbody = document.getElementById('loginTableBody');
        const pageSizeSelect = document.getElementById('loginPageSizeSelect');
        const paginationContainer = document.getElementById('loginPaginationContainer');
        const paginationInfo = document.getElementById('loginPaginationInfo');

        if (!tbody || !pageSizeSelect || !paginationContainer || !paginationInfo) {
            console.warn("Éléments de pagination introuvables pour le tableau des sessions de connexion");
            return;
        }

        let allRows = Array.from(tbody.querySelectorAll('tr'));
        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect.value, 10);

        function renderTable() {
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            const rowsToShow = allRows.slice(start, end);
            allRows.forEach(row => row.style.display = 'none');
            rowsToShow.forEach(row => row.style.display = 'table-row');

            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationInfo.innerText = `Page ${currentPage} sur ${totalPages} (${allRows.length} élément${allRows.length > 1 ? 's' : ''})`;
        }

        function renderPagination() {
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationContainer.innerHTML = '';

            if (totalPages <= 1) return;

            const prevLi = document.createElement('li');
            prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
            prevLi.innerHTML = `<a class="page-link" href="#" aria-label="Précédent">&laquo;</a>`;
            prevLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage > 1) {
                    currentPage--;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(prevLi);

            const maxVisible = 5;
            let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
            let endPage = Math.min(totalPages, startPage + maxVisible - 1);
            if (endPage - startPage + 1 < maxVisible) {
                startPage = Math.max(1, endPage - maxVisible + 1);
            }

            if (startPage > 1) {
                addPageButton(1);
                if (startPage > 2) addEllipsis();
            }

            for (let i = startPage; i <= endPage; i++) {
                addPageButton(i);
            }

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) addEllipsis();
                addPageButton(totalPages);
            }

            const nextLi = document.createElement('li');
            nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
            nextLi.innerHTML = `<a class="page-link" href="#" aria-label="Suivant">&raquo;</a>`;
            nextLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage < totalPages) {
                    currentPage++;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(nextLi);

            function addPageButton(pageNum) {
                const li = document.createElement('li');
                li.className = `page-item ${pageNum === currentPage ? 'active' : ''}`;
                li.innerHTML = `<a class="page-link" href="#">${pageNum}</a>`;
                li.addEventListener('click', (e) => {
                    e.preventDefault();
                    currentPage = pageNum;
                    renderTable();
                    renderPagination();
                });
                paginationContainer.appendChild(li);
            }

            function addEllipsis() {
                const li = document.createElement('li');
                li.className = 'page-item disabled';
                li.innerHTML = '<span class="page-link">...</span>';
                paginationContainer.appendChild(li);
            }
        }

        function onPageSizeChange() {
            pageSize = parseInt(pageSizeSelect.value, 10);
            currentPage = 1;
            renderTable();
            renderPagination();
        }

        pageSizeSelect.addEventListener('change', onPageSizeChange);
        renderTable();
        renderPagination();
    }

    // Initialize if validations tab is active on load
    if (initialTab === 'validations') {
        initializeValidationPagination();
    }
    // Initialize if transactions tab is active on load
    if (initialTab === 'transactions') {
        initializeTransactionPagination();
    }
    // Initialize if operations tab is active on load
    if (initialTab === 'operations') {
        initializeOperationPagination();
    }
    // Initialize if cartes tab is active on load
    if (initialTab === 'villes') {
        initializeCartePagination();
    }
    // Initialize if login sessions tab is active on load
    if (initialTab === 'login') {
        initializeLoginPagination();
    }
    // Initialize if all logs tab is active on load
    if (initialTab === 'all') {
        initializeAllPagination();
    }

    // Function to initialize operation pagination
    function initializeOperationPagination() {
        console.log('Initializing operation pagination');
        // Gestion de la pagination pour le tableau des opérations
        // Éléments du tableau des opérations
        const tbody = document.getElementById('operationTableBody');
        const pageSizeSelect = document.getElementById('operationPageSizeSelect');
        const paginationContainer = document.getElementById('operationPaginationContainer');
        const paginationInfo = document.getElementById('operationPaginationInfo');
        
        console.log('Elements found:', {tbody, pageSizeSelect, paginationContainer, paginationInfo});
        
        if (!tbody || !pageSizeSelect || !paginationContainer || !paginationInfo) {
            console.warn("Éléments de pagination introuvables pour le tableau des opérations");
            return;
        }
        
        // Récupération de toutes les lignes du tableau
        let allRows = Array.from(tbody.querySelectorAll('tr'));
        console.log('All rows:', allRows.length);
        
        // Variables de pagination
        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect.value, 10);
        
        // Fonction pour afficher les lignes de la page courante
        function renderTable() {
            console.log('Rendering table, page:', currentPage, 'pageSize:', pageSize);
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            const rowsToShow = allRows.slice(start, end);
            
            // Masquer toutes les lignes puis afficher celles de la page
            allRows.forEach(row => row.style.display = 'none');
            rowsToShow.forEach(row => row.style.display = 'table-row');
            
            // Mettre à jour les infos de pagination
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationInfo.innerText = `Page ${currentPage} sur ${totalPages} (${allRows.length} élément${allRows.length > 1 ? 's' : ''})`;
        }
        
        // Génération des boutons de pagination
        function renderPagination() {
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationContainer.innerHTML = '';
            
            if (totalPages <= 1) return;
            
            // Bouton Précédent
            const prevLi = document.createElement('li');
            prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
            prevLi.innerHTML = `<a class="page-link" href="#" aria-label="Précédent">&laquo;</a>`;
            prevLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage > 1) {
                    currentPage--;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(prevLi);
            
            // Numéros de pages (avec gestion des ellipses si nécessaire)
            const maxVisible = 5;
            let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
            let endPage = Math.min(totalPages, startPage + maxVisible - 1);
            if (endPage - startPage + 1 < maxVisible) {
                startPage = Math.max(1, endPage - maxVisible + 1);
            }
            
            if (startPage > 1) {
                addPageButton(1);
                if (startPage > 2) addEllipsis();
            }
            
            for (let i = startPage; i <= endPage; i++) {
                addPageButton(i);
            }
            
            if (endPage < totalPages) {
                if (endPage < totalPages - 1) addEllipsis();
                addPageButton(totalPages);
            }
            
            // Bouton Suivant
            const nextLi = document.createElement('li');
            nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
            nextLi.innerHTML = `<a class="page-link" href="#" aria-label="Suivant">&raquo;</a>`;
            nextLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage < totalPages) {
                    currentPage++;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(nextLi);
            
            function addPageButton(pageNum) {
                const li = document.createElement('li');
                li.className = `page-item ${pageNum === currentPage ? 'active' : ''}`;
                li.innerHTML = `<a class="page-link" href="#">${pageNum}</a>`;
                li.addEventListener('click', (e) => {
                    e.preventDefault();
                    currentPage = pageNum;
                    renderTable();
                    renderPagination();
                });
                paginationContainer.appendChild(li);
            }
            
            function addEllipsis() {
                const li = document.createElement('li');
                li.className = 'page-item disabled';
                li.innerHTML = '<span class="page-link">...</span>';
                paginationContainer.appendChild(li);
            }
        }
        
        // Réinitialisation lors du changement du nombre d'éléments par page
        function onPageSizeChange() {
            console.log('Page size changed to:', pageSizeSelect.value);
            pageSize = parseInt(pageSizeSelect.value, 10);
            currentPage = 1;
            renderTable();
            renderPagination();
        }
        
        // Écouteur d'événement sur le selecteur de taille
        pageSizeSelect.addEventListener('change', onPageSizeChange);
        console.log('Event listener added to operationPageSizeSelect');
        
        // Initialisation
        renderTable();
        renderPagination();
    }

    // Function to initialize transaction pagination
    function initializeTransactionPagination() {
        console.log('Initializing transaction pagination');
        // Gestion de la pagination pour le tableau des transactions
        // Éléments du tableau des transactions
        const tbody = document.getElementById('transactionTableBody');
        const pageSizeSelect = document.getElementById('transactionPageSizeSelect');
        const paginationContainer = document.getElementById('transactionPaginationContainer');
        const paginationInfo = document.getElementById('transactionPaginationInfo');
        
        console.log('Elements found:', {tbody, pageSizeSelect, paginationContainer, paginationInfo});
        
        if (!tbody || !pageSizeSelect || !paginationContainer || !paginationInfo) {
            console.warn("Éléments de pagination introuvables pour le tableau des transactions");
            return;
        }
        
        // Récupération de toutes les lignes du tableau
        let allRows = Array.from(tbody.querySelectorAll('tr'));
        console.log('All rows:', allRows.length);
        
        // Variables de pagination
        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect.value, 10);
        
        // Fonction pour afficher les lignes de la page courante
        function renderTable() {
            console.log('Rendering table, page:', currentPage, 'pageSize:', pageSize);
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            const rowsToShow = allRows.slice(start, end);
            
            // Masquer toutes les lignes puis afficher celles de la page
            allRows.forEach(row => row.style.display = 'none');
            rowsToShow.forEach(row => row.style.display = 'table-row');
            
            // Mettre à jour les infos de pagination
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationInfo.innerText = `Page ${currentPage} sur ${totalPages} (${allRows.length} élément${allRows.length > 1 ? 's' : ''})`;
        }
        
        // Génération des boutons de pagination
        function renderPagination() {
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationContainer.innerHTML = '';
            
            if (totalPages <= 1) return;
            
            // Bouton Précédent
            const prevLi = document.createElement('li');
            prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
            prevLi.innerHTML = `<a class="page-link" href="#" aria-label="Précédent">&laquo;</a>`;
            prevLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage > 1) {
                    currentPage--;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(prevLi);
            
            // Numéros de pages (avec gestion des ellipses si nécessaire)
            const maxVisible = 5;
            let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
            let endPage = Math.min(totalPages, startPage + maxVisible - 1);
            if (endPage - startPage + 1 < maxVisible) {
                startPage = Math.max(1, endPage - maxVisible + 1);
            }
            
            if (startPage > 1) {
                addPageButton(1);
                if (startPage > 2) addEllipsis();
            }
            
            for (let i = startPage; i <= endPage; i++) {
                addPageButton(i);
            }
            
            if (endPage < totalPages) {
                if (endPage < totalPages - 1) addEllipsis();
                addPageButton(totalPages);
            }
            
            // Bouton Suivant
            const nextLi = document.createElement('li');
            nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
            nextLi.innerHTML = `<a class="page-link" href="#" aria-label="Suivant">&raquo;</a>`;
            nextLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage < totalPages) {
                    currentPage++;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(nextLi);
            
            function addPageButton(pageNum) {
                const li = document.createElement('li');
                li.className = `page-item ${pageNum === currentPage ? 'active' : ''}`;
                li.innerHTML = `<a class="page-link" href="#">${pageNum}</a>`;
                li.addEventListener('click', (e) => {
                    e.preventDefault();
                    currentPage = pageNum;
                    renderTable();
                    renderPagination();
                });
                paginationContainer.appendChild(li);
            }
            
            function addEllipsis() {
                const li = document.createElement('li');
                li.className = 'page-item disabled';
                li.innerHTML = '<span class="page-link">...</span>';
                paginationContainer.appendChild(li);
            }
        }
        
        // Réinitialisation lors du changement du nombre d'éléments par page
        function onPageSizeChange() {
            console.log('Page size changed to:', pageSizeSelect.value);
            pageSize = parseInt(pageSizeSelect.value, 10);
            currentPage = 1;
            renderTable();
            renderPagination();
        }
        
        // Écouteur d'événement sur le selecteur de taille
        pageSizeSelect.addEventListener('change', onPageSizeChange);
        console.log('Event listener added to pageSizeSelect');
        
        // Initialisation
        renderTable();
        renderPagination();
    }

    // Function to initialize carte pagination
    function initializeCartePagination() {
        console.log('Initializing carte pagination');
        // Gestion de la pagination pour le tableau des cartes
        // Éléments du tableau des cartes
        const tbody = document.getElementById('carteTableBody');
        const pageSizeSelect = document.getElementById('cartePageSizeSelect');
        const paginationContainer = document.getElementById('cartePaginationContainer');
        const paginationInfo = document.getElementById('cartePaginationInfo');
        
        console.log('Elements found:', {tbody, pageSizeSelect, paginationContainer, paginationInfo});
        
        if (!tbody || !pageSizeSelect || !paginationContainer || !paginationInfo) {
            console.warn("Éléments de pagination introuvables pour le tableau des cartes");
            return;
        }
        
        // Récupération de toutes les lignes du tableau
        let allRows = Array.from(tbody.querySelectorAll('tr'));
        console.log('All rows:', allRows.length);
        
        // Variables de pagination
        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect.value, 10);
        
        // Fonction pour afficher les lignes de la page courante
        function renderTable() {
            console.log('Rendering table, page:', currentPage, 'pageSize:', pageSize);
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            const rowsToShow = allRows.slice(start, end);
            
            // Masquer toutes les lignes puis afficher celles de la page
            allRows.forEach(row => row.style.display = 'none');
            rowsToShow.forEach(row => row.style.display = 'table-row');
            
            // Mettre à jour les infos de pagination
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationInfo.innerText = `Page ${currentPage} sur ${totalPages} (${allRows.length} élément${allRows.length > 1 ? 's' : ''})`;
        }
        
        // Génération des boutons de pagination
        function renderPagination() {
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationContainer.innerHTML = '';
            
            if (totalPages <= 1) return;
            
            // Bouton Précédent
            const prevLi = document.createElement('li');
            prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
            prevLi.innerHTML = `<a class="page-link" href="#" aria-label="Précédent">&laquo;</a>`;
            prevLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage > 1) {
                    currentPage--;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(prevLi);
            
            // Numéros de pages (avec gestion des ellipses si nécessaire)
            const maxVisible = 5;
            let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
            let endPage = Math.min(totalPages, startPage + maxVisible - 1);
            if (endPage - startPage + 1 < maxVisible) {
                startPage = Math.max(1, endPage - maxVisible + 1);
            }
            
            if (startPage > 1) {
                addPageButton(1);
                if (startPage > 2) addEllipsis();
            }
            
            for (let i = startPage; i <= endPage; i++) {
                addPageButton(i);
            }
            
            if (endPage < totalPages) {
                if (endPage < totalPages - 1) addEllipsis();
                addPageButton(totalPages);
            }
            
            // Bouton Suivant
            const nextLi = document.createElement('li');
            nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
            nextLi.innerHTML = `<a class="page-link" href="#" aria-label="Suivant">&raquo;</a>`;
            nextLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage < totalPages) {
                    currentPage++;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(nextLi);
            
            function addPageButton(pageNum) {
                const li = document.createElement('li');
                li.className = `page-item ${pageNum === currentPage ? 'active' : ''}`;
                li.innerHTML = `<a class="page-link" href="#">${pageNum}</a>`;
                li.addEventListener('click', (e) => {
                    e.preventDefault();
                    currentPage = pageNum;
                    renderTable();
                    renderPagination();
                });
                paginationContainer.appendChild(li);
            }
            
            function addEllipsis() {
                const li = document.createElement('li');
                li.className = 'page-item disabled';
                li.innerHTML = '<span class="page-link">...</span>';
                paginationContainer.appendChild(li);
            }
        }
        
        // Réinitialisation lors du changement du nombre d'éléments par page
        function onPageSizeChange() {
            console.log('Page size changed to:', pageSizeSelect.value);
            pageSize = parseInt(pageSizeSelect.value, 10);
            currentPage = 1;
            renderTable();
            renderPagination();
        }
        
        // Écouteur d'événement sur le selecteur de taille
        pageSizeSelect.addEventListener('change', onPageSizeChange);
        console.log('Event listener added to cartePageSizeSelect');
        
        // Initialisation
        renderTable();
        renderPagination();
    }

    // Function to initialize all logs pagination
    function initializeAllPagination() {
        console.log('Initializing all logs pagination');
        // Gestion de la pagination pour le tableau de tous les logs
        const tbody = document.getElementById('allTableBody');
        const pageSizeSelect = document.getElementById('allPageSizeSelect');
        const paginationContainer = document.getElementById('allPaginationContainer');
        const paginationInfo = document.getElementById('allPaginationInfo');

        console.log('Elements found:', {tbody, pageSizeSelect, paginationContainer, paginationInfo});

        if (!tbody || !pageSizeSelect || !paginationContainer || !paginationInfo) {
            console.warn("Éléments de pagination introuvables pour le tableau de tous les logs");
            return;
        }

        let allRows = Array.from(tbody.querySelectorAll('tr'));
        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect.value, 10);

        function renderTable() {
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            const rowsToShow = allRows.slice(start, end);
            allRows.forEach(row => row.style.display = 'none');
            rowsToShow.forEach(row => row.style.display = 'table-row');

            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationInfo.innerText = `Page ${currentPage} sur ${totalPages} (${allRows.length} élément${allRows.length > 1 ? 's' : ''})`;
        }

        function renderPagination() {
            const totalPages = Math.ceil(allRows.length / pageSize) || 1;
            paginationContainer.innerHTML = '';

            if (totalPages <= 1) return;

            const prevLi = document.createElement('li');
            prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
            prevLi.innerHTML = `<a class="page-link" href="#" aria-label="Précédent">&laquo;</a>`;
            prevLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage > 1) {
                    currentPage--;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(prevLi);

            const maxVisible = 5;
            let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
            let endPage = Math.min(totalPages, startPage + maxVisible - 1);
            if (endPage - startPage + 1 < maxVisible) {
                startPage = Math.max(1, endPage - maxVisible + 1);
            }

            if (startPage > 1) {
                addPageButton(1);
                if (startPage > 2) addEllipsis();
            }

            for (let i = startPage; i <= endPage; i++) {
                addPageButton(i);
            }

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) addEllipsis();
                addPageButton(totalPages);
            }

            const nextLi = document.createElement('li');
            nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
            nextLi.innerHTML = `<a class="page-link" href="#" aria-label="Suivant">&raquo;</a>`;
            nextLi.addEventListener('click', (e) => {
                e.preventDefault();
                if (currentPage < totalPages) {
                    currentPage++;
                    renderTable();
                    renderPagination();
                }
            });
            paginationContainer.appendChild(nextLi);

            function addPageButton(pageNum) {
                const li = document.createElement('li');
                li.className = `page-item ${pageNum === currentPage ? 'active' : ''}`;
                li.innerHTML = `<a class="page-link" href="#">${pageNum}</a>`;
                li.addEventListener('click', (e) => {
                    e.preventDefault();
                    currentPage = pageNum;
                    renderTable();
                    renderPagination();
                });
                paginationContainer.appendChild(li);
            }

            function addEllipsis() {
                const li = document.createElement('li');
                li.className = 'page-item disabled';
                li.innerHTML = '<span class="page-link">...</span>';
                paginationContainer.appendChild(li);
            }
        }

        function onPageSizeChange() {
            console.log('Page size changed to:', pageSizeSelect.value);
            pageSize = parseInt(pageSizeSelect.value, 10);
            currentPage = 1;
            renderTable();
            renderPagination();
        }

        pageSizeSelect.addEventListener('change', onPageSizeChange);
        console.log('Event listener added to allPageSizeSelect');

        renderTable();
        renderPagination();
    }
});