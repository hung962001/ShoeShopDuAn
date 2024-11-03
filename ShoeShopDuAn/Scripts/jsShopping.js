$(document).ready(function () {
    ShowCount();

    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;
        var tQuantity = $('#quantity_value').text();
        if (tQuantity !== '') {
            quantity = parseFloat(tQuantity); // Đổi sang parseFloat để hỗ trợ số thập phân
        }

        if (quantity > 0) { // Kiểm tra số lượng phải lớn hơn 0
            $.ajax({
                url: '/shoppingcart/addtocart',
                type: 'POST',
                data: { id: id, quantity: quantity },
                success: function (rs) {
                    if (rs.Success) {
                        $('#checkout_items').html(rs.Count);
                        alert(rs.msg);
                    }
                },
                error: function () {
                    alert("Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.");
                }
            });
        } else {
            alert("Số lượng không hợp lệ!");
        }
    });

    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var quantityInput = $('#Quantity_' + id).val();
        // Thay thế dấu phẩy bằng dấu chấm
        var quantity = parseFloat(quantityInput.replace(',', '.'));

        console.log(quantity); // Kiểm tra giá trị của quantity

        if (!isNaN(quantity) && quantity > 0) { // Kiểm tra nếu quantity là một số và lớn hơn 0
            Update(id, quantity); // Gửi giá trị đến server
        } else {
            alert("Số lượng không hợp lệ!"); // Hiển thị thông báo nếu số lượng không hợp lệ
        }
    });

    $('body').on('click', '.btnDeleteAll', function (e) {
        e.preventDefault();
        var conf = confirm('Bạn có chắc muốn xóa hết sản phẩm trong giỏ hàng?');
        if (conf) {
            DeleteAll();
        }
    });

    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?');
        if (conf) {
            $.ajax({
                url: '/shoppingcart/Delete',
                type: 'POST',
                data: { id: id },
                success: function (rs) {
                    if (rs.Success) {
                        $('#checkout_items').html(rs.Count);
                        $('#trow_' + id).remove();
                        LoadCart();
                    }
                },
                error: function () {
                    alert("Không thể xóa sản phẩm. Vui lòng thử lại.");
                }
            });
        }
    });
});

function ShowCount() {
    $.ajax({
        url: '/shoppingcart/ShowCount',
        type: 'GET',
        success: function (rs) {
            $('#checkout_items').html(rs.Count);
        },
        error: function () {
            alert("Không thể tải số lượng sản phẩm trong giỏ hàng.");
        }
    });
}

function DeleteAll() {
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        },
        error: function () {
            alert("Không thể xóa tất cả sản phẩm. Vui lòng thử lại.");
        }
    });
}

function Update(id, quantity) {
    quantity = parseFloat(quantity);
    if (isNaN(quantity) || quantity <= 0) {
        alert("Số lượng không hợp lệ!"); // Hiển thị thông báo nếu số lượng không hợp lệ
        return;
    }
    $.ajax({
        url: '/shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            } else {
                alert(rs.msg);
            }
        },
        error: function () {
            alert("Không thể cập nhật sản phẩm. Vui lòng thử lại.");
        }
    });
}


function LoadCart() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart',
        type: 'GET',
        success: function (rs) {
            $('#load_data').html(rs);
        },
        error: function () {
            alert("Không thể tải giỏ hàng. Vui lòng thử lại.");
        }
    });
}
