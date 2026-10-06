<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}
if (!isset($title)) {
    $title = 'Katinat Coffee';
}
if (!isset($activePage)) {
    $activePage = '';
}
?>
<!DOCTYPE html>
<html lang="vi">
  <head>
    <title><?= htmlspecialchars($title) ?></title>
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
    <link rel="stylesheet" href="css/style.css?v=20261004-2">
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
	          <li class="nav-item <?= $activePage === 'home' ? 'active' : '' ?>"><a href="index.php" class="nav-link">Trang chủ</a></li>
	          <li class="nav-item <?= $activePage === 'menu' ? 'active' : '' ?>"><a href="menu.php" class="nav-link">Thực đơn</a></li>
	          <li class="nav-item <?= $activePage === 'blog' ? 'active' : '' ?>"><a href="blog.php" class="nav-link">Bài viết</a></li>
	          <li class="nav-item <?= $activePage === 'about' ? 'active' : '' ?>"><a href="about.php" class="nav-link">Giới thiệu</a></li>
	          <li class="nav-item dropdown <?= $activePage === 'shop' ? 'active' : '' ?>">
              <a class="nav-link dropdown-toggle" href="shop.php" id="dropdown04" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">Cửa hàng</a>
              <div class="dropdown-menu" aria-labelledby="dropdown04">
              	<a class="dropdown-item" href="shop.php">Cửa hàng</a>
                <a class="dropdown-item" href="product-single.php">Sản phẩm</a>
                <a class="dropdown-item" href="cart.php">Giỏ hàng</a>
                <a class="dropdown-item" href="checkout.php">Thanh toán</a>
              </div>
            </li>
	          <li class="nav-item <?= $activePage === 'contact' ? 'active' : '' ?>"><a href="contact.php" class="nav-link">Liên hệ</a></li>
	          <li class="nav-item cart"><a href="cart.php" class="nav-link"><span class="icon icon-shopping_cart"></span><span class="bag d-flex justify-content-center align-items-center"><small>1</small></span></a></li>
	          <?php if (isset($_SESSION['user_id'])): ?>
	          <li class="nav-item dropdown">
	            <a class="nav-link dropdown-toggle" href="#" id="dropdownUser" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
	              <span class="icon-person"></span> <?= htmlspecialchars($_SESSION['fullname']) ?>
	            </a>
	            <div class="dropdown-menu dropdown-menu-right" aria-labelledby="dropdownUser">
	              <a class="dropdown-item" href="profile.php">Thông tin cá nhân</a>
	              <a class="dropdown-item" href="orders.php">Đơn hàng của tôi</a>
	              <a class="dropdown-item" href="logout.php">Đăng xuất</a>
	            </div>
	          </li>
	          <?php else: ?>
	          <li class="nav-item <?= $activePage === 'login' || $activePage === 'register' ? 'active' : '' ?>"><a href="login.php" class="nav-link">Đăng nhập</a></li>
	          <?php endif; ?>
			  </ul>
	      </div>
		  </div>
	  </nav>
    <!-- END nav -->