<?php
<<<<<<< HEAD
require_once __DIR__ . '/includes/app_helpers.php';
app_start_session();
require_once __DIR__ . '/includes/catalog_data.php';

$_SESSION['cart'] = is_array($_SESSION['cart'] ?? null) ? $_SESSION['cart'] : [];
$cartError = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    if (!app_verify_csrf($_POST['csrf_token'] ?? null)) {
        $cartError = 'Phiên làm việc không hợp lệ. Vui lòng thử lại.';
    } else {
        $action = $_GET['action'] ?? $_POST['action'] ?? '';
        $productId = filter_var($_GET['product_id'] ?? $_POST['product_id'] ?? null, FILTER_VALIDATE_INT);

        if ($action === 'add' && $productId && $productId > 0) {
            $product = app_find_product($conn, $productId);
            $quantity = filter_var($_POST['quantity'] ?? 1, FILTER_VALIDATE_INT);
            if ($product === null) {
                $cartError = 'Sản phẩm không còn được bán.';
            } elseif ($quantity === false || $quantity < 1 || $quantity > 99) {
                $cartError = 'Số lượng phải từ 1 đến 99.';
            } else {
                $currentQuantity = (int) ($_SESSION['cart'][$productId] ?? 0);
                if ($currentQuantity + $quantity > 99) {
                    $cartError = 'Mỗi sản phẩm không thể vượt quá 99 món trong giỏ hàng.';
                } else {
                    $_SESSION['cart'][$productId] = $currentQuantity + $quantity;
                    $_SESSION['cart_message'] = 'Đã thêm sản phẩm vào giỏ hàng.';
                    app_redirect('cart.php');
                }
            }
        } elseif ($action === 'remove' && $productId && $productId > 0) {
            unset($_SESSION['cart'][$productId]);
            $_SESSION['cart_message'] = 'Đã xóa sản phẩm khỏi giỏ hàng.';
            app_redirect('cart.php');
        } elseif ($action === 'update' && is_array($_POST['quantities'] ?? null)) {
            foreach ($_POST['quantities'] as $id => $value) {
                $id = filter_var($id, FILTER_VALIDATE_INT);
                $quantity = filter_var($value, FILTER_VALIDATE_INT);
                if (!$id || $id < 1 || $quantity === false || $quantity < 0 || $quantity > 99) {
                    $cartError = 'Số lượng cập nhật không hợp lệ.';
                    break;
                }
                if ($quantity === 0) {
                    unset($_SESSION['cart'][$id]);
                } else {
                    $_SESSION['cart'][$id] = $quantity;
                }
            }
            if ($cartError === '') {
                $_SESSION['cart_message'] = 'Đã cập nhật giỏ hàng.';
                app_redirect('cart.php');
            }
        } else {
            $cartError = 'Yêu cầu không hợp lệ.';
        }
    }
}

$cartIds = array_map('intval', array_keys($_SESSION['cart']));
$cartProducts = app_fetch_products($conn, $cartIds);
$cartRows = [];
$cartTotal = 0.0;
foreach ($cartProducts as $id => $product) {
    $quantity = (int) $_SESSION['cart'][$id];
    $lineTotal = (float) $product['GiaBan'] * $quantity;
    $cartRows[] = ['product' => $product, 'quantity' => $quantity, 'line_total' => $lineTotal];
    $cartTotal += $lineTotal;
}
if (count($cartProducts) !== count(array_unique($cartIds))) {
    $_SESSION['cart'] = array_intersect_key($_SESSION['cart'], $cartProducts);
    $cartError = 'Một số món trong giỏ không còn được bán và đã được loại bỏ.';
}
$cartMessage = $_SESSION['cart_message'] ?? '';
unset($_SESSION['cart_message']);
?>
<!DOCTYPE html>
<html lang="en">
  <head>
    <title>Coffee - Free Bootstrap 4 Template by Colorlib</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    
    <link href="https://fonts.googleapis.com/css?family=Poppins:300,400,500,600,700" rel="stylesheet">
    <link href="https://fonts.googleapis.com/css?family=Josefin+Sans:400,700" rel="stylesheet">
    <link href="https://fonts.googleapis.com/css?family=Great+Vibes" rel="stylesheet">

    <link rel="stylesheet" href="css/open-iconic-bootstrap.min.css">
    <link rel="stylesheet" href="css/animate.css">
    
    <link rel="stylesheet" href="css/owl.carousel.min.css">
    <link rel="stylesheet" href="css/owl.theme.default.min.css">
    <link rel="stylesheet" href="css/magnific-popup.css">

    <link rel="stylesheet" href="css/aos.css">

    <link rel="stylesheet" href="css/ionicons.min.css">

    <link rel="stylesheet" href="css/bootstrap-datepicker.css">
    <link rel="stylesheet" href="css/jquery.timepicker.css">

    
    <link rel="stylesheet" href="css/flaticon.css">
    <link rel="stylesheet" href="css/icomoon.css">
    <link rel="stylesheet" href="css/style.css?v=20261004-3">
  </head>
  <body>
	<nav class="navbar navbar-expand-xl navbar-dark ftco_navbar bg-dark ftco-navbar-light" id="ftco-navbar">
	    <div class="container">
	      <a class="navbar-brand" href="index.php">Coffee<small>Blend</small></a>
	      <button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#ftco-nav" aria-controls="ftco-nav" aria-expanded="false" aria-label="Toggle navigation">
	        <span class="oi oi-menu"></span> Menu
	      </button>
	      <div class="collapse navbar-collapse" id="ftco-nav">
	        <ul class="navbar-nav ml-auto">
	          <li class="nav-item"><a href="index.php" class="nav-link">Trang chủ</a></li>
	          <li class="nav-item"><a href="menu.php" class="nav-link">Thực đơn</a></li>
	          <li class="nav-item"><a href="services.php" class="nav-link">Dịch vụ</a></li>
	          <li class="nav-item"><a href="blog.php" class="nav-link">Bài viết</a></li>
	          <li class="nav-item"><a href="about.php" class="nav-link">Giới thiệu</a></li>
	          <li class="nav-item dropdown">
              <a class="nav-link dropdown-toggle" href="room.php" id="dropdown04" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Cửa hàng</a>
              <div class="dropdown-menu" aria-labelledby="dropdown04">
              	<a class="dropdown-item" href="shop.php">Cửa hàng</a>
                <a class="dropdown-item" href="product-single.php">Sản phẩm</a>
                <a class="dropdown-item" href="cart.php">Giỏ hàng</a>
                <a class="dropdown-item" href="checkout.php">Thanh toán</a>
              </div>
            </li>
	          <li class="nav-item"><a href="contact.php" class="nav-link">Liên hệ</a></li>
	          <li class="nav-item cart"><a href="cart.php" class="nav-link"><span class="icon icon-shopping_cart"></span><span class="bag d-flex justify-content-center align-items-center"><small><?= app_cart_count() ?></small></span></a></li>
	        </ul>
	      </div>
		  </div>
	  </nav>
    <!-- END nav -->

    <section class="home-slider owl-carousel">
=======
$title = 'Giỏ hàng - Katinat Coffee';
$activePage = 'shop';
include "database/connect.php";
include "src/header.php";
include "src/products.php";
?>

<section class="home-slider owl-carousel">
>>>>>>> 54f9627129778467a5dc26f946680533767fb86f

      <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
      	<div class="overlay"></div>
        <div class="container">
          <div class="row slider-text justify-content-center align-items-center">

            <div class="col-md-7 col-sm-12 text-center ftco-animate">
            	<h1 class="mb-3 mt-5 bread">Giỏ hàng</h1>
	            <p class="breadcrumbs"><span class="mr-2"><a href="index.php">Trang chủ</a></span> <span>Giỏ hàng</span></p>
            </div>

          </div>
        </div>
      </div>
    </section>
		
		<section class="ftco-section ftco-cart">
			<div class="container">
				<div class="row">
    			<div class="col-md-12 ftco-animate">
<?php if ($cartError !== ''): ?><div class="alert alert-danger" role="alert"><?= app_escape($cartError) ?></div><?php endif; ?>
<?php if ($cartMessage !== ''): ?><div class="alert alert-success" role="status"><?= app_escape($cartMessage) ?></div><?php endif; ?>
    				<div class="cart-list">
<form method="post" action="cart.php">
<input type="hidden" name="csrf_token" value="<?= app_escape(app_csrf_token()) ?>">
<input type="hidden" name="action" value="update">
	    				<table class="table">
						    <thead class="thead-primary">
						      <tr class="text-center">
						        <th>&nbsp;</th>
						        <th>&nbsp;</th>
						        <th>Sản phẩm</th>
						        <th>Giá</th>
						        <th>Số lượng</th>
						        <th>Tổng cộng</th>
						      </tr>
						    </thead>
						    <tbody>
						      <?php if ($cartRows === []): ?>
						      <tr><td colspan="6" class="text-center">Giỏ hàng đang trống. <a href="menu.php">Xem thực đơn</a></td></tr>
						      <?php endif; ?>
						      <?php foreach ($cartRows as $row): $product = $row['product']; $id = (int) $product['MaSP']; ?>
						      <tr class="text-center">
						        <td class="product-remove"><button type="submit" formaction="cart.php?action=remove&amp;product_id=<?= $id ?>" aria-label="Xóa <?= app_escape($product['TenSP']) ?>"><span class="icon-close"></span></button></td>
						        <td class="image-prod"><div class="img" style="background-image:url('<?= app_escape(app_product_image($product['HinhAnh'] ?? null)) ?>');"></div></td>
						        <td class="product-name"><h3><?= app_escape($product['TenSP']) ?></h3><p><?= app_escape($product['TenLoaiSP']) ?></p></td>
						        <td class="price"><?= app_money($product['GiaBan']) ?></td>
						        <td class="quantity"><div class="input-group mb-3"><input type="number" name="quantities[<?= $id ?>]" class="quantity form-control input-number" value="<?= (int) $row['quantity'] ?>" min="0" max="99"></div></td>
						        <td class="total"><?= app_money($row['line_total']) ?></td>
						      </tr>
						      <?php endforeach; ?>
						    </tbody>
						  </table>
						  <?php if ($cartRows !== []): ?>
						  <button type="submit" class="btn btn-primary py-2 px-3">Cập nhật giỏ hàng</button>
						  <?php endif; ?>
						  </form>
					  </div>
    			</div>
    		</div>
    		<div class="row justify-content-end">
    			<div class="col col-lg-3 col-md-6 mt-5 cart-wrap ftco-animate">
    				<div class="cart-total mb-3">
<h3>Tổng giỏ hàng</h3>
    					<p class="d-flex">
    						<span>Tạm tính</span>
<span><?= app_money($cartTotal) ?></span>
    					</p>
    					<p class="d-flex">
    						<span>Giao hàng</span>
<span>Tính khi xác nhận</span>
    					</p>
    					<hr>
    					<p class="d-flex total-price">
    						<span>Tổng cộng</span>
<span><?= app_money($cartTotal) ?></span>
    					</p>
    				</div>
<p class="text-center"><a href="<?= $cartRows === [] ? 'menu.php' : 'checkout.php' ?>" class="btn btn-primary py-3 px-4"><?= $cartRows === [] ? 'Tiếp tục mua hàng' : 'Tiến hành thanh toán' ?></a></p>
    			</div>
    		</div>
			</div>
		</section>

    <?php
$relatedProducts = array_slice(array_merge(...array_map(
    static fn (array $category): array => $category['products'],
    array_values($catalogGroups)
)), 0, 4);
include __DIR__ . '/includes/related_products.php';
?>


<?php include "src/footer.php"; ?>
