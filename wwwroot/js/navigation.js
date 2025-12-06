// Navigation functions for client-side routing
window.navigateToClient = function (url) {
    window.location.href = url;
};

window.navigateToClientWithDelay = function (url, delayMs) {
    setTimeout(() => {
        window.location.href = url;
    }, delayMs);
};

// Toggle password visibility
window.togglePasswordVisibility = function (inputId) {
    const input = document.getElementById(inputId);
    if (input) {
        if (input.type === "password") {
            input.type = "text";
        } else {
            input.type = "password";
        }
    }
};