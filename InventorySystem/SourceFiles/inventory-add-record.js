document.getElementById('inventory-form').addEventListener('submit', async function(event) {
    event.preventDefault();

    const location = document.getElementById('location').value;
    const name = document.getElementById('name').value;
    const rarity = document.getElementById('rarity').value;
    const quantity = document.getElementById('quantity').value;
    const price = document.getElementById('price');

    const response = await fetch('/api/add-inventory-record', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ location, name, rarity, quantity, price })
    });

    if (response.ok) {
        alert('Inventory item created successfully.');
    } else {
        const errorText = await response.text();
        alert('Error: ' + errorText);
    }
});
