document.getElementById('add-warehouse-form').addEventListener('submit', async function(event) {
    event.preventDefault();
    const name = document.getElementById('name').value;

    fetch('/api/add-warehouse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ name })
    })
    .then(response => {
        const text = response.text()
        console.log(text);
        document.getElementById('add-item-form').reset();
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Failed to add item.');
    });
});