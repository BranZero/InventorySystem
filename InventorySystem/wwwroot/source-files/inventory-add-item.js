document.getElementById('add-item-form').addEventListener('submit', async function(event) {
    event.preventDefault();
    const name = document.getElementById('name').value;
    const type = document.getElementById('type').value;
    const description = document.getElementById('description').value;

    fetch('/api/add-item', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ name, type, description })
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