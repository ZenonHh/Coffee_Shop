    <footer class="ftco-footer ftco-section img">
    	<div class="overlay"></div>
      <div class="container">
        <div class="row mb-5">
          <div class="col-lg-3 col-md-6 mb-5 mb-md-5">
            <div class="ftco-footer-widget mb-4">
              <h2 class="ftco-heading-2">Về chúng tôi</h2>
              <p>Coffee Blend mang đến cà phê, trà và những món bánh để bạn thưởng thức mỗi ngày.</p>
              <a href="shop.php" class="py-2 d-block">Khám phá thực đơn</a>
            </div>
          </div>
          <div class="col-lg-4 col-md-6 mb-5 mb-md-5">
            <div class="ftco-footer-widget mb-4">
              <h2 class="ftco-heading-2">Mua sắm</h2>
              <ul class="list-unstyled">
                <li><a href="shop.php" class="py-2 d-block">Tất cả sản phẩm</a></li>
                <li><a href="cart.php" class="py-2 d-block">Giỏ hàng</a></li>
                <li><a href="order.php" class="py-2 d-block">Đơn hàng của tôi</a></li>
              </ul>
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
	                <li><a href="contact.php"><span class="icon icon-map-marker"></span><span class="text">Xem địa chỉ cửa hàng tại trang Liên hệ.</span></a></li>
	                <li><a href="contact.php"><span class="icon icon-phone"></span><span class="text">Liên hệ với chúng tôi</span></a></li>
	                <li><a href="contact.php"><span class="icon icon-envelope"></span><span class="text">Gửi câu hỏi hoặc góp ý</span></a></li>
	              </ul>
	            </div>
            </div>
          </div>
        </div>
        <div class="row">
          <div class="col-md-12 text-center">

            <p><!-- Link back to Colorlib can't be removed. Template is licensed under CC BY 3.0. -->
  Bản quyền &copy;<script>document.write(new Date().getFullYear());</script> Coffee Blend. Thiết kế giao diện bởi <a href="https://colorlib.com" target="_blank">Colorlib</a>
  <!-- Link back to Colorlib can't be removed. Template is licensed under CC BY 3.0. --></p>
          </div>
        </div>
      </div>
    </footer>

  <aside id="customer-survey-popup" class="customer-survey-popup" role="dialog" aria-label="Khảo sát ý kiến khách hàng" aria-live="polite">
    <button class="customer-survey-popup__close" type="button" aria-label="Đóng thông báo">&times;</button>
    <div class="customer-survey-popup__icon" aria-hidden="true">☕</div>
    <div class="customer-survey-popup__content">
      <h2>Khảo sát ý kiến khách hàng</h2>
      <p>Hãy chia sẻ trải nghiệm của bạn để Coffee Blend phục vụ tốt hơn.</p>
      <a class="customer-survey-popup__action" href="surveys.php">Tham gia ngay</a>
    </div>
  </aside>
  <button id="customer-survey-reopen" class="customer-survey-reopen" type="button" aria-label="Mở lại lời mời khảo sát" aria-controls="customer-survey-popup" hidden>
    <span aria-hidden="true">☕</span>
    <span>Khảo sát</span>
  </button>

  <style>
    .customer-survey-popup {
      position: fixed; right: 25px; bottom: 25px; z-index: 9999;
      box-sizing: border-box; display: flex; align-items: flex-start; gap: 13px;
      width: 330px; max-width: calc(100vw - 30px); padding: 20px 18px 18px;
      color: #f4eee7; background: #1e1b18; border: 1px solid #c49b63;
      border-radius: 10px; box-shadow: 0 12px 32px rgba(0, 0, 0, .3);
      animation: customerSurveySlideUp .45s ease-out both;
    }
    .customer-survey-popup[hidden], .customer-survey-reopen[hidden] { display: none; }
    .customer-survey-popup__icon {
      display: grid; flex: 0 0 42px; width: 42px; height: 42px;
      place-items: center; color: #c49b63; background: rgba(196, 155, 99, .12);
      border: 1px solid rgba(196, 155, 99, .4); border-radius: 10px; font-size: 23px;
    }
    .customer-survey-popup__content { min-width: 0; padding-right: 10px; }
    .customer-survey-popup__content h2 {
      margin: 0 0 6px; color: #e1bd8a; font-size: 15px; font-weight: 600; line-height: 1.4;
    }
    .customer-survey-popup__content p { margin: 0 0 12px; color: #ddd4cb; font-size: 12px; line-height: 1.5; }
    .customer-survey-popup__action {
      display: inline-block; padding: 8px 13px; color: #171411; background: #c49b63;
      border-radius: 6px; font-size: 12px; font-weight: 600; text-decoration: none;
    }
    .customer-survey-popup__action:hover, .customer-survey-popup__action:focus {
      color: #171411; background: #d6ad78; text-decoration: none;
    }
    .customer-survey-popup__close {
      position: absolute; top: 7px; right: 9px; width: 24px; height: 24px; padding: 0;
      color: #d5c5b2; background: transparent; border: 0; border-radius: 50%;
      font-size: 19px; line-height: 1; cursor: pointer;
    }
    .customer-survey-popup__close:hover, .customer-survey-popup__close:focus {
      color: #fff; background: rgba(255, 255, 255, .08);
    }
    .customer-survey-reopen {
      position: fixed; right: 25px; bottom: 25px; z-index: 9998;
      display: inline-flex; align-items: center; gap: 8px; padding: 11px 15px;
      color: #171411; background: #c49b63; border: 1px solid #dfbd8f; border-radius: 24px;
      box-shadow: 0 8px 22px rgba(0, 0, 0, .25); font: inherit; font-size: 13px;
      font-weight: 600; cursor: pointer; transition: background-color .2s ease, transform .2s ease;
    }
    .customer-survey-reopen:hover, .customer-survey-reopen:focus {
      color: #171411; background: #d6ad78; transform: translateY(-2px);
    }
    .customer-survey-reopen span:first-child { font-size: 17px; line-height: 1; }
    @keyframes customerSurveySlideUp {
      from { opacity: 0; transform: translateY(16px); }
      to { opacity: 1; transform: translateY(0); }
    }
    @media (max-width: 575.98px) {
      .customer-survey-popup, .customer-survey-reopen { right: 15px; bottom: 15px; }
      .customer-survey-popup { padding: 16px 14px; }
    }
    @media (prefers-reduced-motion: reduce) { .customer-survey-popup { animation: none; } }
  </style>

  <script>
    (function () {
      var popup = document.getElementById('customer-survey-popup');
      var reopenButton = document.getElementById('customer-survey-reopen');
      if (!popup || !reopenButton) return;

      var storageKey = 'coffee_blend_survey_popup_closed';
      var closeButton = popup.querySelector('.customer-survey-popup__close');
      try {
        if (window.sessionStorage.getItem(storageKey) === '1') {
          popup.hidden = true;
          reopenButton.hidden = false;
        }
      } catch (error) {
        // Keep the popup available if browser storage is disabled.
      }

      closeButton.addEventListener('click', function () {
        popup.hidden = true;
        reopenButton.hidden = false;
        try {
          window.sessionStorage.setItem(storageKey, '1');
        } catch (error) {
          // The launcher still works for this page if browser storage is disabled.
        }
      });

      reopenButton.addEventListener('click', function () {
        popup.hidden = false;
        reopenButton.hidden = true;
        try {
          window.sessionStorage.removeItem(storageKey);
        } catch (error) {
          // The popup remains open for this page if browser storage is disabled.
        }
      });
    })();
  </script>

  

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
    document.addEventListener('change', function (event) {
      var input = event.target;
      if (!input.matches || !input.matches('.product-review__choices input[type="radio"]')) {
        return;
      }

      var selectedRating = Number(input.value);
      input.closest('.product-review__choices').querySelectorAll('label').forEach(function (label) {
        label.classList.toggle('is-selected', Number(label.querySelector('input').value) <= selectedRating);
      });
    });
  </script>
  <?php if (!empty($extraScriptsFile) && file_exists($extraScriptsFile)) {
      include $extraScriptsFile;
  } ?>
    
  </body>
</html>
