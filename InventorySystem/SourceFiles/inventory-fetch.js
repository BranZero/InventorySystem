document.addEventListener('DOMContentLoaded', () => {
    const locationSelect = document.getElementById('location-select');
    const tableBody = document.getElementById('inventory-table').querySelector('tbody');

    async function fetchInventory(location) {
        try {
            const response = await fetch(`/api/inventory?location=${location}`);
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const inventory = await response.json();
            return inventory;
        } catch (error) {
            console.error('Fetch error:', error);
            return [];
        }
    }

    async function updateInventory(location) {
        tableBody.innerHTML = '';
        const inventory = await fetchInventory(location);
        inventory.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${item.name}</td>
                <td>${item.quantity}</td>
                <td>${item.price}</td>
            `;
            tableBody.appendChild(row);
        });
    }

    locationSelect.addEventListener('change', (event) => {
        updateInventory(event.target.value);
    });

    // Initialize with the first location's inventory
    updateInventory(locationSelect.value);
});
