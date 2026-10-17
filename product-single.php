<?php
$title = 'Sản phẩm - Katinat Coffee';
$activePage = 'shop';
$extraScriptsFile = 'src/quantity-script.php';

if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

include "database/connect.php";

// =========================================================================
// 1. ĐỒNG BỘ TOÀN BỘ BIẾN KẾT NỐI DATABASE VÀO GLOBALS
// =========================================================================
if (!isset($conn) || !($conn instanceof mysqli)) {
    if (isset($connect) && ($connect instanceof mysqli)) {
        $conn = $connect;
    } elseif (isset($con) && ($con instanceof mysqli)) {
        $conn = $con;
    } elseif (isset($db) && ($db instanceof mysqli)) {
        $conn = $db;
    } elseif (file_exists(__DIR__ . '/connect.php')) {
        include_once __DIR__ . '/connect.php';
        if (isset($connect) && ($connect instanceof mysqli)) {
            $conn = $connect;
        }
    }
}

// Kết nối dự phòng nếu chưa có
if (!isset($conn) || !($conn instanceof mysqli)) {
    $conn = @mysqli_connect('localhost', 'root', '', 'coffee_shop_db') 
            ?: @mysqli_connect('localhost', 'root', '', 'coffee_shop');
    if ($conn) {
        mysqli_set_charset($conn, 'utf8mb4');
    }
}

// Gán đồng thời vào cả $GLOBALS['conn'] và $GLOBALS['connect'] để các Model không bị null
$GLOBALS['conn'] = $conn;
$GLOBALS['connect'] = $conn;

include "src/products.php";

$maSP = (int)($_GET['id'] ?? 0);

$productModel = new Product();
$sp = $maSP > 0 ? $productModel->getById($maSP) : null;
$bienThe = $sp ? $productModel->getVariants($maSP) : [];

// NẾU MÓN CÓ TRONG MENU NHƯNG CHƯA CÓ BIẾN THỂ SIZE TRONG BẢNG BienThe:
// TỰ ĐỘNG TẠO 1 SIZE TIÊU CHUẨN ĐỂ KHÔNG BỊ LỖI DỪNG TRANG
if ($sp && empty($bienThe)) {
    $bienThe = [
        [
            'MaBienThe' => 0,
            'KichCo'    => !empty($sp['KichCo']) ? $sp['KichCo'] : 'Tiêu chuẩn',
            'GiaBan'    => (float)($sp['GiaBan'] ?? 0)
        ]
    ];
}

// Nếu thực sự không tìm thấy sản phẩm trong CSDL -> Chuyển về shop.php an toàn
if (!$sp) {
    header('Location: shop.php');
    exit;
}

// =========================================================================
// 2. XỬ LÝ LƯU ĐÁNH GIÁ (POST / REDIRECT / GET CHỐNG GỬI LẶP KHI F5)
// =========================================================================
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['gui_danh_gia'])) {
    if (!$conn || !($conn instanceof mysqli)) {
        $_SESSION['review_error'] = 'Không thể kết nối cơ sở dữ liệu. Vui lòng thử lại sau!';
    } else {
        $danhGia = max(1, min(5, (int)($_POST['danh_gia'] ?? 5)));
        $noiDung = trim($_POST['noi_dung'] ?? '');
        $hoTen   = trim($_POST['ho_ten'] ?? '');
        $sdt     = trim($_POST['sdt'] ?? '');

        if ($noiDung === '' || $hoTen === '') {
            $_SESSION['review_error'] = 'Vui lòng nhập đầy đủ họ tên và nội dung nhận xét!';
        } else {
            $maKH = $_SESSION['ma_kh'] ?? $_SESSION['MaKH'] ?? null;

            if (!$maKH && !empty($sdt)) {
                $stmtCheck = $conn->prepare("SELECT MaKH FROM KhachHang WHERE Sdt = ? LIMIT 1");
                if ($stmtCheck) {
                    $stmtCheck->bind_param("s", $sdt);
                    $stmtCheck->execute();
                    $resCheck = $stmtCheck->get_result();
                    if ($rowCheck = $resCheck->fetch_assoc()) {
                        $maKH = (int)$rowCheck['MaKH'];
                    } else {
                        $stmtInsertKH = $conn->prepare("INSERT INTO KhachHang (HoTen, Sdt, HangTV, TongChiTieu, DiemTichLuy) VALUES (?, ?, 'Thành viên', 0, 0)");
                        if ($stmtInsertKH) {
                            $stmtInsertKH->bind_param("ss", $hoTen, $sdt);
                            if ($stmtInsertKH->execute()) {
                                $maKH = (int)$conn->insert_id;
                            }
                            $stmtInsertKH->close();
                        }
                    }
                    $stmtCheck->close();
                }
            }

            if (!$maKH) {
                $resDefault = $conn->query("SELECT MaKH FROM KhachHang LIMIT 1");
                if ($resDefault && ($rowDef = $resDefault->fetch_assoc())) {
                    $maKH = (int)$rowDef['MaKH'];
                } else {
                    $maKH = 1;
                }
            }

            $ngayGui = date('Y-m-d H:i:s');
            $trangThaiXL = 'Chưa xử lý';

            $stmtPH = $conn->prepare("INSERT INTO PhanHoi (MaKH, MaSP, DanhGia, NoiDung, NgayGui, TrangThaiXL) VALUES (?, ?, ?, ?, ?, ?)");
            if ($stmtPH) {
                $stmtPH->bind_param("iiisss", $maKH, $maSP, $danhGia, $noiDung, $ngayGui, $trangThaiXL);
                if ($stmtPH->execute()) {
                    $_SESSION['review_success'] = 'Cảm ơn bạn đã đánh giá món uống! Nhận xét đã được gửi thành công.';
                } else {
                    $_SESSION['review_error'] = 'Lỗi lưu đánh giá: ' . $conn->error;
                }
                $stmtPH->close();
            } else {
                $_SESSION['review_error'] = 'Lỗi truy vấn: ' . $conn->error;
            }
        }
    }

    header("Location: product-single.php?id=" . $maSP . "#phan-danh-gia");
    exit;
}

// =========================================================================
// 3. NẠP HEADER SAU KHI ĐÃ ĐẢM BẢO DỮ LIỆU SẢN PHẨM HỢP LỆ
// =========================================================================
include "src/header.php";

$reviewSuccess = $_SESSION['review_success'] ?? '';
$reviewError   = $_SESSION['review_error'] ?? '';
unset($_SESSION['review_success'], $_SESSION['review_error']);

// Tải danh sách đánh giá từ bảng PhanHoi
$reviewsList = [];
$totalRatingSum = 0;

if ($conn && ($conn instanceof mysqli)) {
    $stmtGetReviews = $conn->prepare("
        SELECT ph.*, kh.HoTen, kh.Sdt 
        FROM PhanHoi ph
        LEFT JOIN KhachHang kh ON ph.MaKH = kh.MaKH
        WHERE ph.MaSP = ?
        ORDER BY ph.NgayGui DESC
    ");
    if ($stmtGetReviews) {
        $stmtGetReviews->bind_param("i", $maSP);
        $stmtGetReviews->execute();
        $reviewsResult = $stmtGetReviews->get_result();
        while ($row = $reviewsResult->fetch_assoc()) {
            $reviewsList[] = $row;
            $totalRatingSum += (int)$row['DanhGia'];
        }
        $stmtGetReviews->close();
    }
}

$reviewCount = count($reviewsList);
$avgRating = $reviewCount > 0 ? round($totalRatingSum / $reviewCount, 1) : 5.0;

// Topping (nếu có class Topping)
$toppingList = [];
if (defined('MA_LOAI_CO_TOPPING') && is_array(MA_LOAI_CO_TOPPING) && isset($sp['MaLoaiSP']) && in_array((int)$sp['MaLoaiSP'], MA_LOAI_CO_TOPPING, true)) {
    if (class_exists('Topping')) {
        try {
            $toppingList = (new Topping())->getAll() ?: [];
        } catch (Throwable $e) {
            $toppingList = [];
        }
    }
}

// Sản phẩm liên quan (kiểm tra an toàn kiểu mảng)
$rawLienQuan = (isset($sp['MaLoaiSP']) && method_exists($productModel, 'getByCategory')) 
    ? $productModel->getByCategory($sp['MaLoaiSP']) 
    : [];

$lienQuan = is_array($rawLienQuan) ? array_filter(
    $rawLienQuan,
    fn($r) => (int)($r['MaSP'] ?? 0) !== $maSP
) : [];
$lienQuan = array_slice($lienQuan, 0, 4);

$hinh = htmlspecialchars($sp['HinhAnh'] ?? 'menu-1.jpg');

$descriptionByCategory = [
    1 => 'Đậm đà, thơm nồng và phù hợp cho những ai yêu hương vị cà phê nguyên bản.',
    2 => 'Cà phê pha phin mang hương vị truyền thống, cân bằng và đặc trưng của quán.',
    3 => 'Cà phê ủ lạnh êm dịu, thanh mát, thích hợp để thưởng thức trong ngày nóng.',
    4 => 'Matcha thơm dịu, béo nhẹ và mang đến trải nghiệm mới mẻ cho người yêu đồ uống xanh.',
    5 => 'Trà sữa thơm béo, dễ uống, là lựa chọn lý tưởng cho những phút thư giãn.',
    6 => 'Trà trái cây thanh mát, kết hợp vị trà nhẹ cùng hương trái cây tự nhiên.',
    7 => 'Thức uống trái cây tươi mát, nổi bật với màu sắc bắt mắt và vị chua ngọt hài hòa.',
    8 => 'Món bánh thơm ngon, phù hợp dùng kèm cà phê trong những buổi gặp gỡ và thư giãn.',
];
$moTa = !empty($sp['MoTa']) ? $sp['MoTa'] : ($descriptionByCategory[(int)($sp['MaLoaiSP'] ?? 1)] ?? 'Một lựa chọn thơm ngon dành cho bạn tại Coffee Blend.');
?>

<section class="home-slider owl-carousel">
  <div class="slider-item" style="background-image: url(images/bg_3.jpg);" data-stellar-background-ratio="0.5">
    <div class="overlay"></div>
    <div class="container">
      <div class="row slider-text justify-content-center align-items-center">
        <div class="col-md-7 col-sm-12 text-center ftco-animate">
          <h1 class="mb-3 mt-5 bread"><?= htmlspecialchars($sp['TenSP']) ?></h1>
          <p class="breadcrumbs">
            <span class="mr-2"><a href="index.php">Trang chủ</a></span>
            <span class="mr-2"><a href="shop.php">Thực đơn</a></span>
            <span><?= htmlspecialchars($sp['TenLoaiSP'] ?? 'Đồ uống') ?></span>
          </p>
        </div>
      </div>
    </div>
  </div>
</section>

<section class="ftco-section">
  <div class="container">
    <div class="row">
      <!-- Cột trái: Ảnh và mô tả món -->
      <div class="col-lg-6 mb-5 ftco-animate">
        <a href="picture/<?= $hinh ?>" class="image-popup">
          <img src="picture/<?= $hinh ?>" class="img-fluid" style="border-radius:12px;box-shadow:0 10px 25px rgba(0,0,0,0.25);" alt="<?= htmlspecialchars($sp['TenSP']) ?>">
        </a>

        <?php if (!empty($moTa)): ?>
        <div class="product-description mt-4 mb-4" style="background:rgba(255,255,255,0.03);padding:15px 20px;border-left:3px solid #c49b63;border-radius:4px;">
          <h4 style="color:#c49b63;font-size:1.1rem;margin-bottom:8px;">Mô tả món</h4>
          <p style="color:#bbb;margin-bottom:0;line-height:1.6;"><?= nl2br(htmlspecialchars($moTa, ENT_QUOTES, 'UTF-8')) ?></p>
        </div>
        <?php endif; ?>
      </div>

      <!-- Cột phải: Chọn size, topping, số lượng -->
      <div class="col-lg-6 product-details pl-md-5 ftco-animate">
        <h3><?= htmlspecialchars($sp['TenSP']) ?></h3>
        
        <div class="rating-wrap mb-3" style="display:flex;align-items:center;gap:8px;">
          <span style="color:#c49b63;font-size:1.2rem;">
            <?= str_repeat('★', (int)round($avgRating)) ?><?= str_repeat('☆', 5 - (int)round($avgRating)) ?>
          </span>
          <span style="color:#aaa;font-size:0.95rem;">(<?= $avgRating ?>/5 · <?= $reviewCount ?> nhận xét)</span>
        </div>

        <p class="price"><span id="gia-hien-thi"></span></p>

        <form action="cart.php" method="post" id="form-them-gio">
          <input type="hidden" name="MaSP" value="<?= $maSP ?>">

          <!-- Chọn size -->
          <div class="row mt-4">
            <div class="col-md-6">
              <label class="font-weight-bold">Kích cỡ</label>
              <div class="form-group d-flex">
                <div class="select-wrap">
                  <div class="icon"><span class="ion-ios-arrow-down"></span></div>
                  <select name="MaBienThe" id="chon-size" class="form-control">
                    <?php foreach ($bienThe as $bt): ?>
                      <option value="<?= (int)$bt['MaBienThe'] ?>" data-price="<?= (int)$bt['GiaBan'] ?>">
                        <?= htmlspecialchars($bt['KichCo']) ?> - <?= number_format($bt['GiaBan'], 0, ',', '.') ?>đ
                      </option>
                    <?php endforeach; ?>
                  </select>
                </div>
              </div>
            </div>
          </div>

          <!-- Chọn topping -->
          <?php if (!empty($toppingList)): ?>
            <div class="mb-3">
              <label class="font-weight-bold d-block">Thêm topping</label>
              <?php foreach ($toppingList as $tp): ?>
                <div class="custom-control custom-checkbox">
                  <input type="checkbox" class="custom-control-input chon-topping"
                         name="topping[]" value="<?= (int)$tp['MaTopping'] ?>"
                         id="tp-<?= (int)$tp['MaTopping'] ?>" data-price="<?= (int)$tp['GiaTopping'] ?>">
                  <label class="custom-control-label" for="tp-<?= (int)$tp['MaTopping'] ?>">
                    <?= htmlspecialchars($tp['TenTopping']) ?> (+<?= number_format($tp['GiaTopping'], 0, ',', '.') ?>đ)
                  </label>
                </div>
              <?php endforeach; ?>
            </div>
          <?php endif; ?>

          <!-- Số lượng -->
          <div class="row">
            <div class="input-group col-md-6 d-flex mb-3">
              <span class="input-group-btn mr-2">
                <button type="button" class="quantity-left-minus btn" data-type="minus">
                  <i class="icon-minus"></i>
                </button>
              </span>
              <input type="text" id="quantity" name="quantity" class="form-control input-number" value="1" min="1" max="100">
              <span class="input-group-btn ml-2">
                <button type="button" class="quantity-right-plus btn" data-type="plus">
                  <i class="icon-plus"></i>
                </button>
              </span>
            </div>
          </div>
          <p><button type="submit" class="btn btn-primary py-3 px-5" style="color:#fff !important;background-color:#c49b63;border-color:#c49b63;">Thêm vào giỏ - <span id="tong-tien"></span></button></p>
        </form>
      </div>
    </div>
  </div>
</section>

<!-- ========================================================================= -->
<!-- KHU VỰC ĐÁNH GIÁ & NHẬN XÉT CỦA MÓN ĐỒ UỐNG                                -->
<!-- ========================================================================= -->
<section class="ftco-section pt-0" id="phan-danh-gia">
  <div class="container">
    <div class="row">
      <div class="col-md-12">
        <div style="background:#14100c;border:1px solid #33261a;border-radius:12px;padding:35px;">
          
          <div class="row mb-4 align-items-center">
            <div class="col-md-8">
              <span style="color:#c49b63;font-size:0.85rem;letter-spacing:2px;text-transform:uppercase;font-weight:600;">Ý kiến khách hàng</span>
              <h2 style="color:#fff;font-size:1.8rem;margin:5px 0 0 0;">Đánh giá & Nhận xét món uống</h2>
            </div>
            <div class="col-md-4 text-md-right mt-3 mt-md-0">
              <div style="display:inline-block;background:#1e1711;padding:8px 18px;border-radius:25px;border:1px solid #4a3826;">
                <span style="color:#c49b63;font-size:1.3rem;font-weight:bold;"><?= $avgRating ?></span>
                <span style="color:#c49b63;">★</span>
                <span style="color:#aaa;font-size:0.85rem;">/ 5.0 (<?= $reviewCount ?> lượt)</span>
              </div>
            </div>
          </div>

          <?php if (!empty($reviewSuccess)): ?>
            <div class="alert alert-success" style="background:#1c3822;color:#4ade80;border:1px solid #22c55e;"><?= htmlspecialchars($reviewSuccess) ?></div>
          <?php endif; ?>
          <?php if (!empty($reviewError)): ?>
            <div class="alert alert-danger" style="background:#3d1818;color:#f87171;border:1px solid #ef4444;"><?= htmlspecialchars($reviewError) ?></div>
          <?php endif; ?>

          <div class="row">
            <!-- Form gửi nhận xét -->
            <div class="col-lg-5 mb-5 mb-lg-0">
              <div style="background:#1c1611;padding:25px;border-radius:10px;border:1px solid #3d2c1c;">
                <h4 style="color:#c49b63;font-size:1.15rem;margin-bottom:15px;font-weight:600;">Gửi cảm nhận của bạn</h4>
                
                <form action="product-single.php?id=<?= $maSP ?>#phan-danh-gia" method="post">
                  <div class="form-group mb-3">
                    <label style="color:#ddd;font-size:0.9rem;display:block;margin-bottom:5px;">Mức độ hài lòng của bạn:</label>
                    <div class="star-rating-picker" style="font-size:1.8rem;color:#666;cursor:pointer;">
                      <span class="star-item" data-value="1">★</span>
                      <span class="star-item" data-value="2">★</span>
                      <span class="star-item" data-value="3">★</span>
                      <span class="star-item" data-value="4">★</span>
                      <span class="star-item" data-value="5">★</span>
                    </div>
                    <input type="hidden" name="danh_gia" id="rating-value" value="5">
                    <small id="rating-desc" style="color:#c49b63;font-weight:500;">Tuyệt vời (5/5 sao)</small>
                  </div>

                  <div class="form-group mb-3">
                    <input type="text" name="ho_ten" class="form-control" placeholder="Họ và tên của bạn *" required
                           value="<?= htmlspecialchars($_SESSION['ho_ten'] ?? $_SESSION['fullname'] ?? '') ?>"
                           style="background:#251e17;border:1px solid #4a3622;color:#fff;">
                  </div>

                  <div class="form-group mb-3">
                    <input type="text" name="sdt" class="form-control" placeholder="Số điện thoại (tùy chọn)"
                           value="<?= htmlspecialchars($_SESSION['sdt'] ?? $_SESSION['phone'] ?? '') ?>"
                           style="background:#251e17;border:1px solid #4a3622;color:#fff;">
                  </div>

                  <div class="form-group mb-3">
                    <textarea name="noi_dung" rows="3" class="form-control" placeholder="Cảm nhận về hương vị, độ ngọt, đá của món đồ uống... *" required
                              style="background:#251e17;border:1px solid #4a3622;color:#fff;"></textarea>
                  </div>

                  <button type="submit" name="gui_danh_gia" class="btn btn-primary py-2 px-4 w-100" 
                          style="background:#c49b63;border-color:#c49b63;color:#111;font-weight:bold;letter-spacing:1px;">
                    GỬI ĐÁNH GIÁ NGAY
                  </button>
                </form>
              </div>
            </div>

            <!-- Danh sách nhận xét -->
            <div class="col-lg-7">
              <h4 style="color:#fff;font-size:1.15rem;margin-bottom:20px;font-weight:600;">
                Nhận xét gần đây (<?= $reviewCount ?>)
              </h4>

              <div style="max-height:480px;overflow-y:auto;padding-right:10px;">
                <?php if (empty($reviewsList)): ?>
                  <div style="text-align:center;padding:40px 20px;background:#1a140f;border-radius:8px;border:1px dashed #3a2a1b;">
                    <span style="font-size:2rem;color:#c49b63;">☕</span>
                    <p style="color:#888;margin-top:10px;">Chưa có nhận xét nào cho món này. Hãy là người đầu tiên chia sẻ cảm nhận!</p>
                  </div>
                <?php else: ?>
                  <?php foreach ($reviewsList as $rev): ?>
                    <?php 
                      $stars = max(1, min(5, (int)$rev['DanhGia']));
                      $initial = mb_substr((string)($rev['HoTen'] ?? 'K'), 0, 1, 'UTF-8');
                    ?>
                    <div style="background:#19130e;border:1px solid #2e2114;border-radius:8px;padding:16px;margin-bottom:12px;">
                      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:8px;">
                        <div style="display:flex;align-items:center;gap:10px;">
                          <div style="width:36px;height:36px;border-radius:50%;background:#c49b63;color:#111;display:flex;align-items:center;justify-content:center;font-weight:bold;font-size:0.95rem;">
                            <?= htmlspecialchars($initial) ?>
                          </div>
                          <div>
                            <strong style="color:#fff;display:block;font-size:0.95rem;"><?= htmlspecialchars($rev['HoTen'] ?? 'Khách hàng') ?></strong>
                            <small style="color:#777;font-size:0.8rem;"><?= date('d/m/Y H:i', strtotime((string)$rev['NgayGui'])) ?></small>
                          </div>
                        </div>
                        <div style="color:#c49b63;font-size:1rem;letter-spacing:1px;">
                          <?= str_repeat('★', $stars) ?><span style="color:#443424;"><?= str_repeat('★', 5 - $stars) ?></span>
                        </div>
                      </div>
                      <p style="color:#ccc;font-size:0.9rem;margin:0;line-height:1.5;">
                        “<?= htmlspecialchars($rev['NoiDung']) ?>”
                      </p>
                    </div>
                  <?php endforeach; ?>
                <?php endif; ?>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</section>

<!-- Script tương tác chọn sao -->
<script>
document.addEventListener('DOMContentLoaded', function () {
  const stars = document.querySelectorAll('.star-item');
  const ratingInput = document.getElementById('rating-value');
  const ratingDesc = document.getElementById('rating-desc');
  const descs = {
    1: 'Rất không hài lòng (1/5 sao)',
    2: 'Cần cải thiện (2/5 sao)',
    3: 'Tạm ổn (3/5 sao)',
    4: 'Ngon, hài lòng (4/5 sao)',
    5: 'Tuyệt vời (5/5 sao)'
  };

  function updateStars(val) {
    stars.forEach(s => {
      const sVal = parseInt(s.getAttribute('data-value'), 10);
      if (sVal <= val) {
        s.style.color = '#c49b63';
      } else {
        s.style.color = '#443424';
      }
    });
    if (ratingDesc) {
      ratingDesc.textContent = descs[val] || '';
    }
  }

  stars.forEach(s => {
    s.addEventListener('click', function () {
      const val = parseInt(this.getAttribute('data-value'), 10);
      ratingInput.value = val;
      updateStars(val);
    });

    s.addEventListener('mouseenter', function () {
      const val = parseInt(this.getAttribute('data-value'), 10);
      updateStars(val);
    });
  });

  const ratingPicker = document.querySelector('.star-rating-picker');
  if (ratingPicker) {
    ratingPicker.addEventListener('mouseleave', function () {
      updateStars(parseInt(ratingInput.value, 10));
    });
  }

  updateStars(5);
});
</script>

<?php if (!empty($lienQuan)): ?>
<section class="ftco-section">
  <div class="container">
    <div class="row justify-content-center mb-5 pb-3">
      <div class="col-md-7 heading-section ftco-animate text-center">
        <span class="subheading">Khám phá</span>
        <h2 class="mb-4">Sản phẩm liên quan</h2>
      </div>
    </div>
    <div class="row">
      <?php foreach ($lienQuan as $r): ?>
        <div class="col-md-3">
          <div class="menu-entry">
            <a href="product-single.php?id=<?= (int)$r['MaSP'] ?>" class="img" style="background-image: url(picture/<?= htmlspecialchars($r['HinhAnh'] ?? 'menu-1.jpg') ?>);"></a>
            <div class="text text-center pt-4">
              <h3><a href="product-single.php?id=<?= (int)$r['MaSP'] ?>"><?= htmlspecialchars($r['TenSP']) ?></a></h3>
              <p class="price"><span>Từ <?= number_format($r['GiaThapNhat'] ?? 0, 0, ',', '.') ?>đ</span></p>
              <p><a href="product-single.php?id=<?= (int)$r['MaSP'] ?>" class="btn btn-primary btn-outline-primary">Chọn món</a></p>
            </div>
          </div>
        </div>
      <?php endforeach; ?>
    </div>
  </div>
</section>
<?php endif; ?>

<script>
(function () {
  var size = document.getElementById('chon-size');
  var qty = document.getElementById('quantity');
  var out = document.getElementById('gia-hien-thi');

  function tinhGia() {
    var gia = parseInt(size.options[size.selectedIndex].dataset.price, 10) || 0;
    document.querySelectorAll('.chon-topping:checked').forEach(function (cb) {
      gia += parseInt(cb.dataset.price, 10) || 0;
    });
    var sl = parseInt(qty.value, 10) || 1;
    out.textContent = gia.toLocaleString('vi-VN') + 'đ';
    document.getElementById('tong-tien').textContent = (gia * sl).toLocaleString('vi-VN') + 'đ';
  }

  size.addEventListener('change', tinhGia);
  qty.addEventListener('input', tinhGia);
  document.querySelectorAll('.chon-topping').forEach(function (cb) {
    cb.addEventListener('change', tinhGia);
  });
  document.querySelectorAll('.quantity-left-minus, .quantity-right-plus').forEach(function (b) {
    b.addEventListener('click', function () { setTimeout(tinhGia, 0); });
  });
  tinhGia();
})();
</script>

<?php include "src/footer.php"; ?>