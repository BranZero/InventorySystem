
document.getElementById('loginForm').addEventListener('submit', async function(event) {
    event.preventDefault();
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    try {
        const response = await fetch('/api/login?username=${username}&password=${password}', {
            method: 'POST',
        });
        const data = await response.text(); 

        document.getElementById('responseMessage').innerText = data.message;
        console.log(data);

        const messageElement = document.getElementById('responseMessage');
        if (data === "Good") {
            messageElement.textContent = 'Login successful!';
            messageElement.style.color = 'green';
        } else {
            messageElement.textContent = 'Login failed. Please try again.';
            messageElement.style.color = 'red';
        }
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('responseMessage').innerText = 'An error occurred. Please try again.';
        document.getElementById('responseMessage').style.color = 'red';
    }
});
