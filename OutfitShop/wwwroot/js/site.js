// Dùng document.ready để đảm bảo toàn bộ HTML đã được tải xong trước khi chạy script
$(document).ready(function () {
    // 1. Tính tổng tiền ngay khi tải trang lần đầu
    calculateGrandTotal();

    // 2. Lắng nghe sự kiện thay đổi số lượng (dùng 'change')
    $('.qty-input').on('change', function () {
        let input = $(this);
        let qty = parseInt(input.val());
        let price = parseFloat(input.data('price'));
        let id = input.data('id');

        // Validate số lượng tối thiểu là 1
        if (isNaN(qty) || qty < 1) {
            input.val(1);
            qty = 1;
        }

        // Cập nhật giao diện dòng sản phẩm ngay lập tức
        let newTotal = qty * price;
        input.closest('tr').find('.item-total').text(newTotal.toLocaleString('vi-VN') + ' đ');

        // Tính lại tổng giỏ hàng
        calculateGrandTotal();

        // 3. Gửi AJAX lên server để cập nhật Session
        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            data: { id: id, quantity: qty },
            success: function (response) {
                if (!response.success) {
                    toastr.error("Lỗi cập nhật: " + (response.message || "Không xác định"));
                }
            },
            error: function () {
                toastr.error("Không thể kết nối đến máy chủ!");
            }
        });
    });
});

/**
 * Hàm tính tổng tiền toàn giỏ hàng
 * Dùng .replace(/[^0-9]/g, '') để lọc chỉ lấy số, bỏ qua ký tự "đ" hoặc dấu chấm/phẩy
 */
function calculateGrandTotal() {
    let grandTotal = 0;
    $('.item-total').each(function () {
        let valText = $(this).text().replace(/[^0-9]/g, '');
        let val = parseInt(valText);
        if (!isNaN(val)) {
            grandTotal += val;
        }
    });

    $('#grand-total').text(grandTotal.toLocaleString('vi-VN'));
}