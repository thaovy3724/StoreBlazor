window.showAlert = (message, type = "error") => {
    Swal.fire({
        toast: true,
        position: "top-end",
        icon: type, // success | error | warning | info | question
        title: message,
        showConfirmButton: false,
        timer: 4000,
        timerProgressBar: true
    });
};

window.showDeleteAlert = async () => {
    const result = await Swal.fire({
        title: 'Bạn có chắc muốn xóa?',
        text: "Hành động này sẽ không thể hoàn tác!",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy'
    });

    return result.isConfirmed;
};

window.showLockAlert = async (title) => {
    const result = await Swal.fire({
        title: title,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy'
    });

    return result.isConfirmed;
};