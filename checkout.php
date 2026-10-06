<?php
require_once __DIR__ . '/includes/app_helpers.php';
app_start_session();

$checkoutError = '';
$checkoutSuccess = $_SESSION['checkout_success'] ?? '';
unset($_SESSION['checkout_success']);
$cart = is_array($_SESSION['cart'] ?? null) ? $_SESSION['cart'] : [];
$checkoutProducts = app_fetch_products($conn, array_map('intval', array_keys($cart)));
$checkoutRows = [];
$checkoutTotal = 0.0;
foreach ($checkoutProducts as $id => $product) {
    $quantity = (int) $cart[$id];
    $lineTotal = (float) $product['GiaBan'] * $quantity;
    $checkoutRows[] = ['product' => $product, 'quantity' => $quantity, 'line_total' => $lineTotal];
    $checkoutTotal += $lineTotal;
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    if (!app_verify_csrf($_POST['csrf_token'] ?? null)) {
        $checkoutError = 'Phiên làm việc không hợp lệ. Vui lòng tải lại trang.';
    } elseif ($checkoutRows === [] || count($checkoutProducts) !== count($cart)) {
        $checkoutError = 'Giỏ hàng trống hoặc có món không còn được bán. Vui lòng kiểm tra lại giỏ hàng.';
    } else {
        $recipientName = app_post_string($_POST, 'recipient_name');
        $recipientPhone = app_post_string($_POST, 'recipient_phone');
        $deliveryAddress = app_post_string($_POST, 'delivery_address');
        $deliveryNote = app_post_string($_POST, 'delivery_note');

        if ($recipientName === '' || mb_strlen($recipientName, 'UTF-8') > 100) {
            $checkoutError = 'Vui lòng nhập tên người nhận (tối đa 100 ký tự).';
        } elseif (!preg_match('/^[0-9+\s().-]{8,20}$/', $recipientPhone)) {
            $checkoutError = 'Vui lòng nhập số điện thoại hợp lệ.';
        } elseif ($deliveryAddress === '' || mb_strlen($deliveryAddress, 'UTF-8') > 255) {
            $checkoutError = 'Vui lòng nhập địa chỉ giao hàng (tối đa 255 ký tự).';
        } elseif (mb_strlen($deliveryNote, 'UTF-8') > 500) {
            $checkoutError = 'Ghi chú giao hàng không được vượt quá 500 ký tự.';
        } else {
            try {
                $conn->beginTransaction();
                $freshProducts = app_fetch_products($conn, array_map('intval', array_keys($cart)));
                if (count($freshProducts) !== count($cart)) {
                    throw new RuntimeException('A cart product is no longer available.');
                }

                $total = 0.0;
                foreach ($freshProducts as $id => $product) {
                    $total += (float) $product['GiaBan'] * (int) $cart[$id];
                }

                $orderStatement = $conn->prepare(
                    "INSERT INTO HoaDon
                     (MaCaLam, MaKH, MaNV, MaCTKM, LoaiDonHang, TongTien, TienGiamGia, TrangThai, NgayTao,
                      TenNguoiNhan, SdtNguoiNhan, DiaChiGiaoHang, GhiChuGiaoHang)
                     VALUES (NULL, NULL, NULL, NULL, 'Giao hàng', :total, 0, 'Chờ xác nhận', NOW(),
                             :recipientName, :recipientPhone, :deliveryAddress, :deliveryNote)"
                );
                $orderStatement->execute([
                    'total' => $total,
                    'recipientName' => $recipientName,
                    'recipientPhone' => $recipientPhone,
                    'deliveryAddress' => $deliveryAddress,
                    'deliveryNote' => $deliveryNote !== '' ? $deliveryNote : null,
                ]);
                $orderId = (int) $conn->lastInsertId();
                if ($orderId < 1) {
                    throw new RuntimeException('MySQL did not return the new order ID.');
                }

                $detailStatement = $conn->prepare(
                    'INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan, ThanhTien)
                     VALUES (:orderId, :productId, :quantity, :unitPrice, :lineTotal)'
                );
                foreach ($freshProducts as $id => $product) {
                    $quantity = (int) $cart[$id];
                    $unitPrice = (float) $product['GiaBan'];
                    $detailStatement->execute([
                        'orderId' => $orderId,
                        'productId' => (int) $id,
                        'quantity' => $quantity,
                        'unitPrice' => $unitPrice,
                        'lineTotal' => $unitPrice * $quantity,
                    ]);
                }
                $conn->commit();
                $_SESSION['cart'] = [];
                $_SESSION['checkout_success'] = 'Đặt hàng thành công. Mã đơn hàng: #' . $orderId;
                app_redirect('checkout.php');
            } catch (Throwable $exception) {
                if ($conn->inTransaction()) {
                    $conn->rollBack();
                }
                error_log('Checkout failed: ' . $exception->getMessage());
                $checkoutError = 'Không thể hoàn tất đơn hàng do lỗi hệ thống. Vui lòng thử lại sau.';
            }
        }
    }
}
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

      <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
      	<div class="overlay"></div>
        <div class="container">
          <div class="row slider-text justify-content-center align-items-center">

            <div class="col-md-7 col-sm-12 text-center ftco-animate">
            	<h1 class="mb-3 mt-5 bread">Thanh toán</h1>
	            <p class="breadcrumbs"><span class="mr-2"><a href="index.php">Trang chủ</a></span> <span>Checout</span></p>
            </div>

          </div>
        </div>
      </div>
    </section>

    <section class="ftco-section">
      <div class="container">
        <div class="row">
          <div class="col-xl-8 ftco-animate">
						<?php if ($checkoutError !== ''): ?><div class="alert alert-danger" role="alert"><?= app_escape($checkoutError) ?></div><?php endif; ?>
						<?php if ($checkoutSuccess !== ''): ?><div class="alert alert-success" role="status"><?= app_escape($checkoutSuccess) ?></div><?php endif; ?>
						<form method="post" action="checkout.php" class="billing-form ftco-bg-dark p-3 p-md-5">
							<input type="hidden" name="csrf_token" value="<?= app_escape(app_csrf_token()) ?>">
							<h3 class="mb-4 billing-heading">Thông tin người nhận</h3>
	          	<div class="row align-items-end">
	          		<div class="col-md-6">
	                <div class="form-group">
<label for="firstname">Họ và tên *</label>
	                  <input id="firstname" name="recipient_name" type="text" class="form-control" maxlength="100" required value="<?= app_escape($_POST['recipient_name'] ?? '') ?>">
	                </div>
	              </div>
	              <div class="col-md-6">
	                <div class="form-group">
	                	<label for="lastname">Last Name</label>
	                  <input type="text" class="form-control" placeholder="">
	                </div>
                </div>
                <div class="w-100"></div>
		            <div class="col-md-12">
		            	<div class="form-group">
		            		<label for="country">State / Country</label>
		            		<div class="select-wrap">
		                  <div class="icon"><span class="ion-ios-arrow-down"></span></div>
		                  <select name="" id="" class="form-control">
		                  	<option value="">France</option>
		                    <option value="">Italy</option>
		                    <option value="">Philippines</option>
		                    <option value="">South Korea</option>
		                    <option value="">Hongkong</option>
		                    <option value="">Japan</option>
		                  </select>
		                </div>
		            	</div>
		            </div>
		            <div class="w-100"></div>
		            <div class="col-md-6">
		            	<div class="form-group">
<label for="streetaddress">Địa chỉ giao hàng *</label>
	                  <input id="streetaddress" name="delivery_address" type="text" class="form-control" maxlength="255" placeholder="Số nhà, tên đường, phường/xã, quận/huyện, tỉnh/thành" required value="<?= app_escape($_POST['delivery_address'] ?? '') ?>">
	                </div>
		            </div>
		            <div class="col-md-6">
		            	<div class="form-group">
	                  <input type="text" class="form-control" placeholder="Appartment, suite, unit etc: (optional)">
	                </div>
		            </div>
		            <div class="w-100"></div>
		            <div class="col-md-6">
		            	<div class="form-group">
	                	<label for="towncity">Town / City</label>
	                  <input type="text" class="form-control" placeholder="">
	                </div>
		            </div>
		            <div class="col-md-6">
		            	<div class="form-group">
		            		<label for="postcodezip">Postcode / ZIP *</label>
	                  <input type="text" class="form-control" placeholder="">
	                </div>
		            </div>
		            <div class="w-100"></div>
		            <div class="col-md-6">
	                <div class="form-group">
<label for="phone">Số điện thoại *</label>
	                  <input id="phone" name="recipient_phone" type="tel" class="form-control" maxlength="20" required value="<?= app_escape($_POST['recipient_phone'] ?? '') ?>">
	                </div>
	                <div class="col-md-12">
	                  <div class="form-group">
	                    <label for="delivery-note">Ghi chú giao hàng</label>
	                    <textarea id="delivery-note" name="delivery_note" class="form-control" maxlength="500" rows="3"><?= app_escape($_POST['delivery_note'] ?? '') ?></textarea>
	                  </div>
	                </div>
	              </div>
	              <div class="col-md-6">
	                <div class="form-group">
	                	<label for="emailaddress">Email Address</label>
	                  <input type="text" class="form-control" placeholder="">
	                </div>
                </div>
                <div class="w-100"></div>
                <div class="col-md-12">
                	<div class="form-group mt-4">
										<div class="radio">
										  <label class="mr-3"><input type="radio" name="optradio"> Create an Account? </label>
										  <label><input type="radio" name="optradio"> Ship to different address</label>
										</div>
									</div>
                </div>
	            </div>
	          <div class="row mt-5 pt-3 d-flex">
	          	<div class="col-md-6 d-flex">
	          		<div class="cart-detail cart-total ftco-bg-dark p-3 p-md-4">
<h3 class="billing-heading mb-4">Tổng đơn hàng</h3>
	          			<p class="d-flex">
<span>Món trong giỏ</span>
<span><?= count($checkoutRows) ?></span>
		    					</p>
		    					<p class="d-flex">
		    						<span>Giao hàng</span>
<span>Tính khi xác nhận</span>
		    					</p>
		    					<p class="d-flex">
		    						<span>Giảm giá</span>
<span>0đ</span>
		    					</p>
		    					<hr>
		    					<p class="d-flex total-price">
		    						<span>Tổng cộng</span>
<span><?= app_money($checkoutTotal) ?></span>
		    					</p>
								</div>
	          	</div>
	          	<div class="col-md-6">
	          		<div class="cart-detail ftco-bg-dark p-3 p-md-4">
	          			<h3 class="billing-heading mb-4">Payment Method</h3>
									<div class="form-group">
										<div class="col-md-12">
											<div class="radio">
											   <label><input type="radio" name="optradio" class="mr-2"> Direct Bank Tranfer</label>
											</div>
										</div>
									</div>
									<div class="form-group">
										<div class="col-md-12">
											<div class="radio">
											   <label><input type="radio" name="optradio" class="mr-2"> Check Payment</label>
											</div>
										</div>
									</div>
									<div class="form-group">
										<div class="col-md-12">
											<div class="radio">
											   <label><input type="radio" name="optradio" class="mr-2"> Paypal</label>
											</div>
										</div>
									</div>
									<div class="form-group">
										<div class="col-md-12">
											<div class="checkbox">
											   <label><input type="checkbox" value="" class="mr-2"> I have read and accept the terms and conditions</label>
											</div>
										</div>
									</div>
									<p><button type="submit" class="btn btn-primary py-3 px-4" <?= $checkoutRows === [] ? 'disabled' : '' ?>>Đặt hàng</button></p>
								</div>
	          	</div>
						</form>
	          </div>
          </div> <!-- .col-md-8 -->




          <div class="col-xl-4 sidebar ftco-animate">
            <div class="sidebar-box">
              <form action="#" class="search-form">
                <div class="form-group">
                	<div class="icon">
	                  <span class="icon-search"></span>
                  </div>
                  <input type="text" class="form-control" placeholder="Tìm kiếm...">
                </div>
              </form>
            </div>
            <div class="sidebar-box ftco-animate">
              <div class="categories">
                <h3>Categories</h3>
                <li><a href="#">Tour <span>(12)</span></a></li>
                <li><a href="#">Hotel <span>(22)</span></a></li>
                <li><a href="#">Coffee <span>(37)</span></a></li>
                <li><a href="#">Drinks <span>(42)</span></a></li>
                <li><a href="#">Foods <span>(14)</span></a></li>
                <li><a href="#">Travel <span>(140)</span></a></li>
              </div>
            </div>

            <div class="sidebar-box ftco-animate">
              <h3>Bài viết gần đây</h3>
              <div class="block-21 mb-4 d-flex">
                <a class="blog-img mr-4" style="background-image: url(images/image_1.jpg);"></a>
                <div class="text">
                  <h3 class="heading"><a href="#">Even the all-powerful Pointing has no control about the blind texts</a></h3>
                  <div class="meta">
                    <div><a href="#"><span class="icon-calendar"></span> July 12, 2018</a></div>
                    <div><a href="#"><span class="icon-person"></span> Admin</a></div>
                    <div><a href="#"><span class="icon-chat"></span> 19</a></div>
                  </div>
                </div>
              </div>
              <div class="block-21 mb-4 d-flex">
                <a class="blog-img mr-4" style="background-image: url(images/image_2.jpg);"></a>
                <div class="text">
                  <h3 class="heading"><a href="#">Even the all-powerful Pointing has no control about the blind texts</a></h3>
                  <div class="meta">
                    <div><a href="#"><span class="icon-calendar"></span> July 12, 2018</a></div>
                    <div><a href="#"><span class="icon-person"></span> Admin</a></div>
                    <div><a href="#"><span class="icon-chat"></span> 19</a></div>
                  </div>
                </div>
              </div>
              <div class="block-21 mb-4 d-flex">
                <a class="blog-img mr-4" style="background-image: url(images/image_3.jpg);"></a>
                <div class="text">
                  <h3 class="heading"><a href="#">Even the all-powerful Pointing has no control about the blind texts</a></h3>
                  <div class="meta">
                    <div><a href="#"><span class="icon-calendar"></span> July 12, 2018</a></div>
                    <div><a href="#"><span class="icon-person"></span> Admin</a></div>
                    <div><a href="#"><span class="icon-chat"></span> 19</a></div>
                  </div>
                </div>
              </div>
            </div>

            <div class="sidebar-box ftco-animate">
              <h3>Tag Cloud</h3>
              <div class="tagcloud">
                <a href="#" class="tag-cloud-link">dish</a>
                <a href="#" class="tag-cloud-link">menu</a>
                <a href="#" class="tag-cloud-link">food</a>
                <a href="#" class="tag-cloud-link">sweet</a>
                <a href="#" class="tag-cloud-link">tasty</a>
                <a href="#" class="tag-cloud-link">delicious</a>
                <a href="#" class="tag-cloud-link">desserts</a>
                <a href="#" class="tag-cloud-link">drinks</a>
              </div>
            </div>

            <div class="sidebar-box ftco-animate">
              <h3>Paragraph</h3>
              <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit. Ducimus itaque, autem necessitatibus voluptate quod mollitia delectus aut, sunt placeat nam vero culpa sapiente consectetur similique, inventore eos fugit cupiditate numquam!</p>
            </div>
          </div>

        </div>
      </div>
    </section> <!-- .section -->

    <footer class="ftco-footer ftco-section img">
    	<div class="overlay"></div>
      <div class="container">
        <div class="row mb-5">
          <div class="col-lg-3 col-md-6 mb-5 mb-md-5">
            <div class="ftco-footer-widget mb-4">
              <h2 class="ftco-heading-2">Về chúng tôi</h2>
              <p>Far far away, behind the word mountains, far from the countries Vokalia and Consonantia, there live the blind texts.</p>
              <ul class="ftco-footer-social list-unstyled float-md-left float-lft mt-5">
                <li class="ftco-animate"><a href="#"><span class="icon-twitter"></span></a></li>
                <li class="ftco-animate"><a href="#"><span class="icon-facebook"></span></a></li>
                <li class="ftco-animate"><a href="#"><span class="icon-instagram"></span></a></li>
              </ul>
            </div>
          </div>
          <div class="col-lg-4 col-md-6 mb-5 mb-md-5">
            <div class="ftco-footer-widget mb-4">
              <h2 class="ftco-heading-2">Bài viết gần đây</h2>
              <div class="block-21 mb-4 d-flex">
                <a class="blog-img mr-4" style="background-image: url(images/image_1.jpg);"></a>
                <div class="text">
                  <h3 class="heading"><a href="#">Even the all-powerful Pointing has no control about</a></h3>
                  <div class="meta">
                    <div><a href="#"><span class="icon-calendar"></span> Sept 15, 2018</a></div>
                    <div><a href="#"><span class="icon-person"></span> Admin</a></div>
                    <div><a href="#"><span class="icon-chat"></span> 19</a></div>
                  </div>
                </div>
              </div>
              <div class="block-21 mb-4 d-flex">
                <a class="blog-img mr-4" style="background-image: url(images/image_2.jpg);"></a>
                <div class="text">
                  <h3 class="heading"><a href="#">Even the all-powerful Pointing has no control about</a></h3>
                  <div class="meta">
                    <div><a href="#"><span class="icon-calendar"></span> Sept 15, 2018</a></div>
                    <div><a href="#"><span class="icon-person"></span> Admin</a></div>
                    <div><a href="#"><span class="icon-chat"></span> 19</a></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-2 col-md-6 mb-5 mb-md-5">
             <div class="ftco-footer-widget mb-4 ml-md-4">
              <h2 class="ftco-heading-2">Dịch vụ</h2>
              <ul class="list-unstyled">
                <li><a href="#" class="py-2 d-block">Chế biến</a></li>
                <li><a href="#" class="py-2 d-block">Giao hàng</a></li>
                <li><a href="#" class="py-2 d-block">Thực phẩm chất lượng</a></li>
                <li><a href="#" class="py-2 d-block">Pha chế</a></li>
              </ul>
            </div>
          </div>
          <div class="col-lg-3 col-md-6 mb-5 mb-md-5">
            <div class="ftco-footer-widget mb-4">
            	<h2 class="ftco-heading-2">Bạn có câu hỏi?</h2>
            	<div class="block-23 mb-3">
	              <ul>
	                <li><span class="icon icon-map-marker"></span><span class="text">203 Fake St. Mountain View, San Francisco, California, USA</span></li>
	                <li><a href="#"><span class="icon icon-phone"></span><span class="text">+2 392 3929 210</span></a></li>
	                <li><a href="#"><span class="icon icon-envelope"></span><span class="text">info@yourdomain.com</span></a></li>
	              </ul>
	            </div>
            </div>
          </div>
        </div>
        <div class="row">
          <div class="col-md-12 text-center">

            <p><!-- Link back to Colorlib can't be removed. Template is licensed under CC BY 3.0. -->
  Copyright &copy;<script>document.write(new Date().getFullYear());</script> All rights reserved | This template is made with <i class="icon-heart" aria-hidden="true"></i> by <a href="https://colorlib.com" target="_blank">Colorlib</a>
  <!-- Link back to Colorlib can't be removed. Template is licensed under CC BY 3.0. --></p>
          </div>
        </div>
      </div>
    </footer>
    
  

  <!-- loader -->
  <div id="ftco-loader" class="show fullscreen"><svg class="circular" width="48px" height="48px"><circle class="path-bg" cx="24" cy="24" r="22" fill="none" stroke-width="4" stroke="#eeeeee"/><circle class="path" cx="24" cy="24" r="22" fill="none" stroke-width="4" stroke-miterlimit="10" stroke="#F96D00"/></svg></div>


  <script src="js/jquery.min.js?v=20261004"></script>
  <script src="js/jquery-migrate-3.0.1.min.js"></script>
  <script src="js/popper.min.js"></script>
  <script src="js/bootstrap.min.js"></script>
  <script src="js/jquery.easing.1.3.js"></script>
  <script src="js/jquery.waypoints.min.js"></script>
  <script src="js/jquery.stellar.min.js"></script>
  <script src="js/owl.carousel.min.js?v=20261004"></script>
  <script src="js/jquery.magnific-popup.min.js?v=20261004"></script>
  <script src="js/aos.js"></script>
  <script src="js/jquery.animateNumber.min.js"></script>
  <script src="js/bootstrap-datepicker.js?v=20261004"></script>
  <script src="js/jquery.timepicker.min.js"></script>
  <script src="js/scrollax.min.js"></script>
  <script src="https://maps.googleapis.com/maps/api/js?key=AIzaSyBVWaKrjvy3MaE7SQ74_uJiULgl1JY0H2s&sensor=false"></script>
  <script src="js/google-map.js?v=20261004"></script>
  <script src="js/main.js"></script>

  <script>
		$(document).ready(function(){

		var quantitiy=0;
		   $('.quantity-right-plus').click(function(e){
		        
		        // Stop acting like a button
		        e.preventDefault();
		        // Get the field name
		        var quantity = parseInt($('#quantity').val());
		        
		        // If is not undefined
		            
		            $('#quantity').val(quantity + 1);

		          
		            // Increment
		        
		    });

		     $('.quantity-left-minus').click(function(e){
		        // Stop acting like a button
		        e.preventDefault();
		        // Get the field name
		        var quantity = parseInt($('#quantity').val());
		        
		        // If is not undefined
		      
		            // Increment
		            if(quantity>0){
		            $('#quantity').val(quantity - 1);
		            }
		    });
		    
		});
	</script>

    
  </body>
</html>