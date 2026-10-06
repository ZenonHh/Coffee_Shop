<?php
<<<<<<< HEAD
require_once __DIR__ . '/includes/app_helpers.php';
app_start_session();
require_once __DIR__ . '/includes/catalog_data.php';

$requestedProductId = filter_input(INPUT_GET, 'id', FILTER_VALIDATE_INT);
$selectedProduct = $requestedProductId ? app_find_product($conn, $requestedProductId) : null;
if ($selectedProduct === null) {
    http_response_code(404);
}
$selectedProductImage = $selectedProduct === null
    ? ''
    : app_product_image($selectedProduct['HinhAnh'] ?? null);
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
$title = 'Sản phẩm - Katinat Coffee';
$activePage = 'shop';
$extraScriptsFile = 'src/quantity-script.php';
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
            	<h1 class="mb-3 mt-5 bread">Product Detail</h1>
	            <p class="breadcrumbs"><span class="mr-2"><a href="index.php">Trang chủ</a></span> <span>Product Detail</span></p>
            </div>

          </div>
        </div>
      </div>
    </section>

    <section class="ftco-section">
    	<div class="container">
    		<div class="row">
    			<div class="col-lg-6 mb-5 ftco-animate">
					<?php if ($selectedProduct !== null): ?>
					<a href="<?= app_escape($selectedProductImage) ?>" class="image-popup"><img src="<?= app_escape($selectedProductImage) ?>" class="img-fluid" alt="<?= app_escape($selectedProduct['TenSP']) ?>"></a>
    			</div>
    			<div class="col-lg-6 product-details pl-md-5 ftco-animate">
					<h3><?= app_escape($selectedProduct['TenSP']) ?></h3>
					<p class="price"><span><?= app_money($selectedProduct['GiaBan']) ?></span></p>
					<p>Sản phẩm thuộc bộ sưu tập đồ uống và món ăn Katinat.</p>
						<div class="row mt-4">
							<div class="col-md-6">
								<div class="form-group d-flex">
		              <div class="select-wrap">
	                  <div class="icon"><span class="ion-ios-arrow-down"></span></div>
	                  <select name="" id="" class="form-control">
<option value=""><?= app_escape($selectedProduct['KichCo'] ?? 'Mặc định') ?></option>
	                  </select>
	                </div>
		            </div>
							</div>
							<div class="w-100"></div>
							<form action="cart.php" method="post">
								<input type="hidden" name="csrf_token" value="<?= app_escape(app_csrf_token()) ?>">
								<input type="hidden" name="action" value="add">
								<input type="hidden" name="product_id" value="<?= (int) $selectedProduct['MaSP'] ?>">
							<div class="input-group col-md-6 d-flex mb-3">
	             	<span class="input-group-btn mr-2">
	                	<button type="button" class="quantity-left-minus btn"  data-type="minus" data-field="">
	                   <i class="icon-minus"></i>
	                	</button>
	            		</span>
<input type="number" id="quantity" name="quantity" class="form-control input-number" value="1" min="1" max="99">
	             	<span class="input-group-btn ml-2">
	                	<button type="button" class="quantity-right-plus btn" data-type="plus" data-field="">
	                     <i class="icon-plus"></i>
	                 </button>
	             	</span>
	          	</div>
          	</div>
<button type="submit" class="btn btn-primary py-3 px-5">Thêm vào giỏ</button>
							</form>
					<?php else: ?>
						<p>Sản phẩm không tồn tại hoặc hiện không được bán.</p>
					<?php endif; ?>
    			</div>
    		</div>
    	</div>
    </section>

    <?php
$relatedProducts = array_slice(array_merge(...array_map(
    static fn (array $category): array => $category['products'],
    array_values($catalogGroups)
)), 0, 4);
if ($selectedProduct !== null) {
    $relatedProducts = array_values(array_filter(
        $relatedProducts,
        static fn (array $product): bool => (int) $product['MaSP'] !== (int) $selectedProduct['MaSP']
    ));
}
include __DIR__ . '/includes/related_products.php';
?>


<?php include "src/footer.php"; ?>
