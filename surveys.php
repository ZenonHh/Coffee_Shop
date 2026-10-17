<?php
$title = 'Khảo sát của tôi - Coffee Blend';
$activePage = 'surveys';
require_once __DIR__ . '/includes/customer_portal.php';
$customer = customer_portal_require_login($conn);

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    $inviteActiveSurveys = $conn->prepare(
        "INSERT INTO LoiMoiKhaoSat (MaKhaoSat, MaKH)
         SELECT ks.MaKhaoSat, :customerId
         FROM KhaoSat AS ks
         WHERE NOW() BETWEEN ks.NgayBatDau AND ks.NgayKetThuc
           AND EXISTS (
               SELECT 1 FROM CauHoiKhaoSat AS ch WHERE ch.MaKhaoSat = ks.MaKhaoSat
           )
           AND NOT EXISTS (
               SELECT 1 FROM LoiMoiKhaoSat AS lm
               WHERE lm.MaKhaoSat = ks.MaKhaoSat AND lm.MaKH = :existingCustomerId
           )"
    );
    $inviteActiveSurveys->execute([
        'customerId' => $customer['MaKH'],
        'existingCustomerId' => $customer['MaKH'],
    ]);
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    try {
        customer_portal_validate_csrf();
        $invitationId = filter_var($_POST['invitation_id'] ?? null, FILTER_VALIDATE_INT);
        $answers = $_POST['answers'] ?? null;
        if (!$invitationId || $invitationId < 1 || !is_array($answers)) {
            throw new InvalidArgumentException('Lời mời hoặc nội dung trả lời không hợp lệ.');
        }
        $invitationQuery = $conn->prepare(
            "SELECT lm.MaLoiMoi, lm.MaKhaoSat
             FROM LoiMoiKhaoSat AS lm
             INNER JOIN KhaoSat AS ks ON ks.MaKhaoSat = lm.MaKhaoSat
             WHERE lm.MaLoiMoi = :invitationId AND lm.MaKH = :customerId
               AND (lm.TrangThai IS NULL OR lm.TrangThai <> 'Đã trả lời')
               AND NOW() BETWEEN ks.NgayBatDau AND ks.NgayKetThuc
             FOR UPDATE"
        );
        $conn->beginTransaction();
        $invitationQuery->execute(['invitationId' => $invitationId, 'customerId' => $customer['MaKH']]);
        $invitation = $invitationQuery->fetch();
        if (!is_array($invitation)) {
            throw new InvalidArgumentException('Lời mời không còn khả dụng hoặc đã được trả lời.');
        }
        $questionQuery = $conn->prepare(
            'SELECT MaCauHoi, LoaiCauHoi FROM CauHoiKhaoSat WHERE MaKhaoSat = :surveyId ORDER BY MaCauHoi'
        );
        $questionQuery->execute(['surveyId' => $invitation['MaKhaoSat']]);
        $questions = $questionQuery->fetchAll();
        if ($questions === []) {
            throw new InvalidArgumentException('Khảo sát này chưa có câu hỏi để trả lời.');
        }
        $optionQuery = $conn->prepare('SELECT MaDanhMuc FROM DanhMucLuaChon WHERE MaCauHoi = :questionId');
        $insertAnswer = $conn->prepare(
            'INSERT INTO CauTraLoi (MaLoiMoi, MaCauHoi, MaDanhMuc, NoiDungTraLoi, NgayTraLoi)
             VALUES (:invitationId, :questionId, :optionId, :answerText, NOW())'
        );
        $validatedAnswers = [];
        foreach ($questions as $question) {
            $questionId = (int) $question['MaCauHoi'];
            if (!array_key_exists((string) $questionId, $answers) && !array_key_exists($questionId, $answers)) {
                throw new InvalidArgumentException('Vui lòng trả lời tất cả câu hỏi.');
            }
            $value = $answers[$questionId] ?? $answers[(string) $questionId];
            $optionQuery->execute(['questionId' => $questionId]);
            $allowedOptions = array_map('intval', $optionQuery->fetchAll(PDO::FETCH_COLUMN));
            $type = (string) $question['LoaiCauHoi'];
            if ($type === 'text') {
                $text = is_string($value) ? trim($value) : '';
                if ($text === '' || mb_strlen($text, 'UTF-8') > 5000) {
                    throw new InvalidArgumentException('Câu trả lời văn bản bắt buộc và tối đa 5.000 ký tự.');
                }
                $validatedAnswers[] = [$questionId, null, $text];
            } elseif ($type === 'multiple') {
                if (!is_array($value) || $value === []) {
                    throw new InvalidArgumentException('Vui lòng chọn ít nhất một phương án cho mỗi câu hỏi.');
                }
                $selectedOptions = array_values(array_unique(array_map('intval', $value)));
                if (array_diff($selectedOptions, $allowedOptions) !== []) {
                    throw new InvalidArgumentException('Phương án trả lời không hợp lệ.');
                }
                foreach ($selectedOptions as $optionId) {
                    $validatedAnswers[] = [$questionId, $optionId, null];
                }
            } else {
                $optionId = filter_var($value, FILTER_VALIDATE_INT);
                if (!is_int($optionId) || !in_array($optionId, $allowedOptions, true)) {
                    throw new InvalidArgumentException('Vui lòng chọn một phương án trả lời hợp lệ.');
                }
                $validatedAnswers[] = [$questionId, $optionId, null];
            }
        }

        try {
            foreach ($validatedAnswers as [$questionId, $optionId, $answerText]) {
                $insertAnswer->execute([
                    'invitationId' => $invitation['MaLoiMoi'],
                    'questionId' => $questionId,
                    'optionId' => $optionId,
                    'answerText' => $answerText,
                ]);
            }
            $complete = $conn->prepare("UPDATE LoiMoiKhaoSat SET TrangThai = 'Đã trả lời' WHERE MaLoiMoi = :invitationId");
            $complete->execute(['invitationId' => $invitation['MaLoiMoi']]);
            $conn->commit();
        } catch (Throwable $exception) {
            if ($conn->inTransaction()) {
                $conn->rollBack();
            }
            throw $exception;
        }
        $_SESSION['success'] = 'Cảm ơn bạn đã hoàn thành khảo sát.';
        customer_portal_redirect('surveys.php');
    } catch (InvalidArgumentException $exception) {
        if ($conn->inTransaction()) {
            $conn->rollBack();
        }
        $_SESSION['error'] = $exception->getMessage();
        customer_portal_redirect('surveys.php');
    } catch (Throwable $exception) {
        if ($conn->inTransaction()) {
            $conn->rollBack();
        }
        error_log('Customer survey submission failed: ' . $exception->getMessage());
        $_SESSION['error'] = 'Không thể lưu câu trả lời lúc này. Vui lòng thử lại sau.';
        customer_portal_redirect('surveys.php');
    }
}

$invitationQuery = $conn->prepare(
    'SELECT lm.MaLoiMoi, lm.TrangThai, ks.MaKhaoSat, ks.TieuDe, ks.MoTa, ks.NgayBatDau, ks.NgayKetThuc
     FROM LoiMoiKhaoSat AS lm INNER JOIN KhaoSat AS ks ON ks.MaKhaoSat = lm.MaKhaoSat
     WHERE lm.MaKH = :customerId ORDER BY ks.NgayBatDau DESC'
);
$invitationQuery->execute(['customerId' => $customer['MaKH']]);
$invitations = $invitationQuery->fetchAll();
foreach ($invitations as &$invitationRow) {
    $questionQuery = $conn->prepare(
        'SELECT MaCauHoi, NoiDungCauHoi, LoaiCauHoi FROM CauHoiKhaoSat WHERE MaKhaoSat = :surveyId ORDER BY MaCauHoi'
    );
    $questionQuery->execute(['surveyId' => $invitationRow['MaKhaoSat']]);
    $invitationRow['questions'] = $questionQuery->fetchAll();
    foreach ($invitationRow['questions'] as &$questionRow) {
        $optionQuery = $conn->prepare(
            'SELECT MaDanhMuc, NoiDungLuaChon FROM DanhMucLuaChon WHERE MaCauHoi = :questionId ORDER BY ThuTuHienThi, MaDanhMuc'
        );
        $optionQuery->execute(['questionId' => $questionRow['MaCauHoi']]);
        $questionRow['options'] = $optionQuery->fetchAll();
    }
    unset($questionRow);
}
unset($invitationRow);
include __DIR__ . '/src/header.php';
?>
<section class="ftco-section">
  <div class="container mt-5">
    <h2>Khảo sát dành cho bạn</h2><p>Các khảo sát do cửa hàng gửi đến tài khoản của bạn.</p>
    <?php foreach (['success' => 'alert-success', 'error' => 'alert-danger'] as $notice => $class): ?>
      <?php if (isset($_SESSION[$notice])): ?><div class="alert <?= $class ?>"><?= customer_portal_escape($_SESSION[$notice]) ?></div><?php unset($_SESSION[$notice]); endif; ?>
    <?php endforeach; ?>
    <?php if ($invitations === []): ?><div class="alert alert-info">Hiện chưa có lời mời khảo sát. Cửa hàng sẽ thông báo tại đây khi có khảo sát phù hợp.</div><?php endif; ?>
    <?php foreach ($invitations as $invitation): ?>
      <?php $isOpen = $invitation['TrangThai'] !== 'Đã trả lời' && strtotime((string) $invitation['NgayBatDau']) <= time() && strtotime((string) $invitation['NgayKetThuc']) >= time(); ?>
      <article class="p-4 mb-4" style="background:#fff;border-radius:10px">
        <div class="d-flex justify-content-between flex-wrap"><h3><?= customer_portal_escape($invitation['TieuDe']) ?></h3><span class="badge badge-<?= $invitation['TrangThai'] === 'Đã trả lời' ? 'success' : ($isOpen ? 'primary' : 'secondary') ?>"><?= customer_portal_escape($invitation['TrangThai'] === 'Đã trả lời' ? 'Đã hoàn thành' : ($isOpen ? 'Đang mở' : 'Chưa mở / Đã kết thúc')) ?></span></div>
        <p><?= nl2br(customer_portal_escape($invitation['MoTa'] ?? '')) ?></p>
        <small>Thời gian: <?= date('d/m/Y', strtotime((string) $invitation['NgayBatDau'])) ?> – <?= date('d/m/Y', strtotime((string) $invitation['NgayKetThuc'])) ?></small>
        <?php if ($isOpen && $invitation['questions'] !== []): ?>
          <form method="post" action="surveys.php" class="contact-form mt-4">
            <input type="hidden" name="csrf_token" value="<?= customer_portal_escape($_SESSION['customer_csrf']) ?>">
            <input type="hidden" name="invitation_id" value="<?= (int) $invitation['MaLoiMoi'] ?>">
            <?php foreach ($invitation['questions'] as $question): $questionId = (int) $question['MaCauHoi']; ?>
              <fieldset class="form-group"><legend><?= customer_portal_escape($question['NoiDungCauHoi']) ?></legend>
                <?php if ($question['LoaiCauHoi'] === 'text'): ?>
                  <textarea class="form-control" name="answers[<?= $questionId ?>]" rows="3" maxlength="5000" required></textarea>
                <?php elseif ($question['LoaiCauHoi'] === 'multiple'): ?>
                  <?php foreach ($question['options'] as $option): ?><label class="d-block"><input type="checkbox" name="answers[<?= $questionId ?>][]" value="<?= (int) $option['MaDanhMuc'] ?>"> <?= customer_portal_escape($option['NoiDungLuaChon']) ?></label><?php endforeach; ?>
                <?php else: ?>
                  <?php foreach ($question['options'] as $option): ?><label class="d-block"><input type="radio" name="answers[<?= $questionId ?>]" value="<?= (int) $option['MaDanhMuc'] ?>" required> <?= customer_portal_escape($option['NoiDungLuaChon']) ?></label><?php endforeach; ?>
                <?php endif; ?>
              </fieldset>
            <?php endforeach; ?>
            <button type="submit" class="btn btn-primary py-2 px-4">Gửi câu trả lời</button>
          </form>
        <?php elseif ($invitation['TrangThai'] === 'Đã trả lời'): ?><p class="text-success mt-3">Cảm ơn bạn đã tham gia khảo sát này.</p><?php endif; ?>
      </article>
    <?php endforeach; ?>
  </div>
</section>
<?php include __DIR__ . '/src/footer.php'; ?>
