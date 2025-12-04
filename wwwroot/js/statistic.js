let revenueChart;
let topChart;

// Cập nhật biểu đồ đường (Line Chart)
window.updateLineChart = function (labels, values, title) {
    const ctx = document.getElementById('revenueChart');
    if (!ctx) return; // Bảo vệ: Thoát nếu không tìm thấy canvas

    if (revenueChart) {
        revenueChart.destroy();
    }

    revenueChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu (₫)',
                data: values,
                borderColor: 'rgba(54, 162, 235, 1)',
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                borderWidth: 2,
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'top' },
                title: {
                    display: true,
                    text: title,
                    font: { size: 16, weight: 'bold' }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toLocaleString('vi-VN') + ' ₫';
                        }
                    }
                },
                x: {
                    ticks: {
                        font: { size: 13 },
                        autoSkip: true,
                        maxTicksLimit: 10
                    }
                }
            }
        }
    });
};

// Cập nhật biểu đồ tròn (Pie/Doughnut Chart)
window.updatePieChart = function (labels, values, title) {
    const ctx = document.getElementById('myChartTop');
    const noDataMessage = document.getElementById('noDataMessageTop');
    if (!ctx) return; // Bảo vệ: Thoát nếu không tìm thấy canvas

    if (topChart) {
        topChart.destroy();
    }

    if (!labels || labels.length === 0 || !values || values.length === 0) {
        // 2a. Ẩn Canvas, Hiển thị thông báo
        ctx.style.display = 'none';
        if (noDataMessage) {
            noDataMessage.style.display = 'flex';
        }
        topChart = null; // Đặt lại biến biểu đồ
        return; // Dừng việc vẽ biểu đồ
    }

    // 3. Nếu có dữ liệu: Hiển thị Canvas, Ẩn thông báo
    ctx.style.display = 'block'; // Hiển thị lại canvas
    if (noDataMessage) {
        noDataMessage.style.display = 'none';
    }


    topChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: [
                    'rgba(0,191,255,0.9)',
                    'rgba(64,224,208,0.9)',
                    'rgba(255,215,0,0.9)',
                    'rgba(255,165,0,0.9)',
                    'rgba(148,0,211,0.9)',
                    'rgba(255,99,132,0.9)',
                    'rgba(75,192,192,0.9)',
                    'rgba(255,159,64,0.9)',
                    'rgba(153,102,255,0.9)',
                    'rgba(54,162,235,0.9)',
                ],
                borderColor: '#fff',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '65%',
            animation: {
                animateScale: true,
                animateRotate: true
            },
            plugins: {
                legend: {
                    position: 'top',
                    labels: { font: { size: 13 } }
                },
                title: {
                    display: true,
                    text: title,
                    font: { size: 16, weight: 'bold' }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            let label = context.label || '';
                            if (label) {
                                label += ': ';
                            }
                            label += context.parsed.toLocaleString('vi-VN');
                            return label;
                        }
                    }
                }
            }
        }
    });
};

// Initialize tooltips (giữ nguyên logic ban đầu của bạn)
document.addEventListener("DOMContentLoaded", function () {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});