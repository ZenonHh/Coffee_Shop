<?php
$title = 'Cửa hàng - Katinat Coffee';
$activePage = 'shop';
include "database/connect.php";
include "src/header.php";
include "src/products.php";

$productModel = new Product();
$loaiSanPhamModel = new LoaiSanPham();

$danhMucList = $loaiSanPhamModel->getAll();

// Gom sẵn sản phẩm theo từng danh mục vào 1 mảng, dùng key = MaLoaiSP
$sanPhamTheoDanhMuc = [];
foreach ($danhMucList as $dm) {
    $sanPhamTheoDanhMuc[$dm['MaLoaiSP']] = $productModel->getByCategory($dm['MaLoaiSP']);
}
?>

<section class="home-slider owl-carousel">

      <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
      	<div class="overlay"></div>
        <div class="container">
          <div class="row slider-text justify-content-center align-items-center">

            <div class="col-md-7 col-sm-12 text-center ftco-animate">
            	<h1 class="mb-3 mt-5 bread">Order Online</h1>
	            <p class="breadcrumbs"><span class="mr-2"><a href="index.php">Trang chủ</a></span> <span>Cửa hàng</span></p>
            </div>

          </div>
        </div>
      </div>
    </section>


    <section class="ftco-menu mb-5 pb-5">
    	<div class="container">
    		<div class="row d-md-flex">
	    		<div class="col-lg-12 ftco-animate p-md-5">
		    		<div class="row">
		          <div class="col-md-12 nav-link-wrap mb-5">
		            <div class="nav ftco-animate nav-pills justify-content-center" id="v-pills-tab" role="tablist" aria-orientation="vertical">
		            <?php foreach ($danhMucList as $i => $dm): ?>
		              <a class="nav-link <?= $i === 0 ? 'active' : '' ?>"
		                 id="v-pills-<?= $dm['MaLoaiSP'] ?>-tab"
		                 data-toggle="pill"
		                 href="#v-pills-<?= $dm['MaLoaiSP'] ?>"
		                 role="tab"
		                 aria-controls="v-pills-<?= $dm['MaLoaiSP'] ?>"
		                 aria-selected="<?= $i === 0 ? 'true' : 'false' ?>">
		                <?= htmlspecialchars($dm['TenLoaiSP']) ?>
		              </a>
		            <?php endforeach; ?>
		            </div>
		          </div>
		          <div class="col-md-12 d-flex align-items-center">

		            <div class="tab-content ftco-animate" id="v-pills-tabContent">
		            <?php foreach ($danhMucList as $i => $dm): ?>
		              <div class="tab-pane fade <?= $i === 0 ? 'show active' : '' ?>"
		                   id="v-pills-<?= $dm['MaLoaiSP'] ?>"
		                   role="tabpanel"
		                   aria-labelledby="v-pills-<?= $dm['MaLoaiSP'] ?>-tab">
		                <div class="row d-flex flex-wrap">
		                <?php $dsSanPham = $sanPhamTheoDanhMuc[$dm['MaLoaiSP']]; ?>
		                <?php if (empty($dsSanPham)): ?>
		                  <div class="col-12 text-center"><p>Chưa có sản phẩm nào trong danh mục này.</p></div>
		                <?php else: ?>
		                  <?php foreach ($dsSanPham as $sp): ?>
		                  <div class="col-md-3">
		                    <div class="menu-entry">
		                      <a href="product-single.php?id=<?= (int)$sp['MaSP'] ?>" class="img" style="background-image: url(picture/<?= htmlspecialchars($sp['HinhAnh'] ?? 'menu-1.jpg') ?>);"></a>
		                      <div class="text text-center pt-4">
		                        <h3><a href="product-single.php?id=<?= (int)$sp['MaSP'] ?>"><?= htmlspecialchars($sp['TenSP']) ?></a></h3>
		                        <p class="price"><span>Từ <?= number_format($sp['GiaThapNhat'], 0, ',', '.') ?>đ</span></p>
		                        <p><a href="cart.php" class="btn btn-primary btn-outline-primary">Thêm vào giỏ</a></p>
		                      </div>
		                    </div>
		                  </div>
		                  <?php endforeach; ?>
		                <?php endif; ?>
		                </div>
		              </div>
		            <?php endforeach; ?>
		            </div>
		          </div>
		        </div>
		      </div>
		    </div>
    	</div>
    </section>


<?php include "src/footer.php"; ?>