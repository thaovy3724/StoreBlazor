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
﻿// Sử dụng thư viện JsBarcode
window.generateBarcode = function (barcodeValue) {
    try {
        JsBarcode("#barcode-svg", barcodeValue, {
            format: "CODE128",
            width: 2,
            height: 100,
            displayValue: false,
            margin: 10
        });
    } catch (err) {
        console.error("Error generating barcode:", err);
    }
};

window.printBarcode = function () {
    const barcodeContent = document.getElementById('barcode-display').innerHTML;
    const printWindow = window.open('', '_blank', 'width=600,height=400');

    printWindow.document.write(`
        <html>
        <head>
            <title>Print Barcode</title>
            <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    margin: 0;
                    font-family: Arial, sans-serif;
                }
                #barcode-container {
                    text-align: center;
                }
            </style>
        </head>
        <body>
            <div id="barcode-container">
                ${barcodeContent}
            </div>
        </body>
        </html>
    `);

    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 250);
};

window.downloadBarcode = function (barcodeValue, productName) {
    const svg = document.getElementById('barcode-svg');
    if (!svg) return;

    const svgData = new XMLSerializer().serializeToString(svg);
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    const img = new Image();

    img.onload = function () {
        canvas.width = img.width;
        canvas.height = img.height;
        ctx.drawImage(img, 0, 0);

        canvas.toBlob(function (blob) {
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `barcode_${productName}_${barcodeValue}.png`;
            link.click();
            URL.revokeObjectURL(url);
        });
    };

    img.src = 'data:image/svg+xml;base64,' + btoa(unescape(encodeURIComponent(svgData)));
};
