var interval;
function EmailConfirmation(email) {
    if (window.WebSocket) {
        alert("WebSocket is active.")
        openSocket(email, "Email");
    }
    else {
        alert("WebSocket is not active.")
        interval = setInterval(() => {
            CheckEmailConfirmationStatus(email);
        }, 5000);
    }
}