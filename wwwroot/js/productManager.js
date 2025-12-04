window.product = {
    initUploadButton: function () {
        const uploadBtn = document.getElementById("upload-btn");
        const fileInput = document.getElementById("product-image");

        if (uploadBtn && fileInput) {
            uploadBtn.onclick = () => fileInput.click();
        }
    }
};
