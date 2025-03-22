document.getElementById('inventory-form').addEventListener('submit', async function(event) {
    event.preventDefault();

    const formData = 
        (document.getElementById('location').value + ',' +
        document.getElementById('name').value + ',' + 
        document.getElementById('rarity').value + ',' + 
        document.getElementById('quantity').value + ',' +
        document.getElementById('price').value);

    const response = await fetch('/api/inventory-receiver', {
        method: 'POST',
        headers: {
            'Content-Type': 'text/data'
        },
        body: formData
    });

    if (response.ok) {
        alert('Inventory item created successfully.');
    } else {
        const errorText = await response.text();
        alert('Error: ' + errorText);
    }
});
