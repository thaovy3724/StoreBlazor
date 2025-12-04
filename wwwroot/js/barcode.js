window.html5QrCodeInstance = window.html5QrCodeInstance || null;

window.initBarcodeScanner = async function (dotNetRef) {
    const reader = document.getElementById("reader");
    if (!reader) return;

    // Nếu scanner đã chạy rồi → KHÔNG khởi tạo lại
    if (window.html5QrCodeInstance !== null) return;

    window.html5QrCodeInstance = new Html5Qrcode("reader");

    window.html5QrCodeInstance.start(
        { facingMode: "environment" },
        { fps: 10, qrbox: 250 },
        decodedText => {
            dotNetRef.invokeMethodAsync("HandleBarcodeScanned", decodedText);
        }
    );
};

window.stopBarcodeScanner = async function () {
    if (window.html5QrCodeInstance) {
        await window.html5QrCodeInstance.stop();
        await window.html5QrCodeInstance.clear();
        window.html5QrCodeInstance = null;
    }
};
window.hideModal = function (modalId) {
    const modalEl = document.getElementById(modalId);
    if (!modalEl) return;

    const modal = bootstrap.Modal.getInstance(modalEl)
        || new bootstrap.Modal(modalEl);

    modal.hide();
}

window.showModal = function (modalId) {
    const modalEl = document.getElementById(modalId);
    if (!modalEl) return;

    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}
