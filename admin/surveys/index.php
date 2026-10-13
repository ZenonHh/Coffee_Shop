<?php
require_once __DIR__ . '/../includes/auth.php';
admin_require_login();

if (!isset($_SESSION['admin_survey_csrf']) || !is_string($_SESSION['admin_survey_csrf'])) {
    $_SESSION['admin_survey_csrf'] = bin2hex(random_bytes(32));
}

$message = $_SESSION['admin_survey_message'] ?? '';
unset($_SESSION['admin_survey_message']);
$error = '';
$selectedSurveyId = filter_input(INPUT_GET, 'survey', FILTER_VALIDATE_INT);

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $postedToken = $_POST['csrf_token'] ?? null;
    if (!is_string($postedToken) || !hash_equals($_SESSION['admin_survey_csrf'], $postedToken)) {
        $error = 'Phiên làm việc hết hạn. Hãy tải lại trang và thử lại.';
    } else {
        try {
            $title = trim((string) ($_POST['title'] ?? ''));
            $description = trim((string) ($_POST['description'] ?? ''));
            $startInput = trim((string) ($_POST['start_at'] ?? ''));
            $endInput = trim((string) ($_POST['end_at'] ?? ''));
            $questions = $_POST['questions'] ?? null;
            $startDate = DateTimeImmutable::createFromFormat('Y-m-d\TH:i', $startInput);
            $startDateErrors = DateTimeImmutable::getLastErrors();
            $endDate = DateTimeImmutable::createFromFormat('Y-m-d\TH:i', $endInput);
            $endDateErrors = DateTimeImmutable::getLastErrors();

            if ($title === '' || mb_strlen($title, 'UTF-8') > 255) {
                throw new InvalidArgumentException('Tiêu đề khảo sát bắt buộc và tối đa 255 ký tự.');
            }
            if ($description !== '' && mb_strlen($description, 'UTF-8') > 5000) {
                throw new InvalidArgumentException('Mô tả khảo sát tối đa 5.000 ký tự.');
            }
            if (
                !$startDate
                || !$endDate
                || ($startDateErrors !== false && ($startDateErrors['warning_count'] > 0 || $startDateErrors['error_count'] > 0))
                || ($endDateErrors !== false && ($endDateErrors['warning_count'] > 0 || $endDateErrors['error_count'] > 0))
                || $endDate <= $startDate
            ) {
                throw new InvalidArgumentException('Thời gian kết thúc phải sau thời gian bắt đầu.');
            }
            if (!is_array($questions) || $questions === [] || count($questions) > 30) {
                throw new InvalidArgumentException('Khảo sát cần có từ 1 đến 30 câu hỏi.');
            }

            $validatedQuestions = [];
            foreach ($questions as $question) {
                if (!is_array($question)) {
                    throw new InvalidArgumentException('Dữ liệu câu hỏi không hợp lệ.');
                }
                $questionText = trim((string) ($question['text'] ?? ''));
                $type = (string) ($question['type'] ?? '');
                if ($questionText === '' || mb_strlen($questionText, 'UTF-8') > 255) {
                    throw new InvalidArgumentException('Nội dung mỗi câu hỏi bắt buộc và tối đa 255 ký tự.');
                }
                if (!in_array($type, ['single', 'multiple', 'text'], true)) {
                    throw new InvalidArgumentException('Loại câu hỏi không hợp lệ.');
                }

                $options = [];
                if ($type !== 'text') {
                    $rawOptions = preg_split('/\r\n|\r|\n/', (string) ($question['options'] ?? '')) ?: [];
                    $options = array_values(array_unique(array_filter(
                        array_map('trim', $rawOptions),
                        static fn (string $option): bool => $option !== ''
                    )));
                    if (count($options) < 2 || count($options) > 20) {
                        throw new InvalidArgumentException('Câu hỏi lựa chọn cần từ 2 đến 20 phương án.');
                    }
                    foreach ($options as $option) {
                        if (mb_strlen($option, 'UTF-8') > 255) {
                            throw new InvalidArgumentException('Mỗi phương án tối đa 255 ký tự.');
                        }
                    }
                }
                $validatedQuestions[] = ['text' => $questionText, 'type' => $type, 'options' => $options];
            }

            $admin = admin_user();
            if ((int) ($admin['id'] ?? 0) < 1) {
                throw new RuntimeException('Tài khoản quản trị không có mã tài khoản hợp lệ.');
            }
            $conn->beginTransaction();
            $insertSurvey = $conn->prepare(
                'INSERT INTO KhaoSat (MaTaiKhoan, TieuDe, MoTa, NgayBatDau, NgayKetThuc)
                 VALUES (:accountId, :title, :description, :startAt, :endAt)'
            );
            $insertSurvey->execute([
                'accountId' => (int) ($admin['id'] ?? 0),
                'title' => $title,
                'description' => $description !== '' ? $description : null,
                'startAt' => $startDate->format('Y-m-d H:i:s'),
                'endAt' => $endDate->format('Y-m-d H:i:s'),
            ]);
            $surveyId = (int) $conn->lastInsertId();

            $insertQuestion = $conn->prepare(
                'INSERT INTO CauHoiKhaoSat (MaKhaoSat, NoiDungCauHoi, LoaiCauHoi)
                 VALUES (:surveyId, :questionText, :type)'
            );
            $insertOption = $conn->prepare(
                'INSERT INTO DanhMucLuaChon (MaCauHoi, NoiDungLuaChon, ThuTuHienThi)
                 VALUES (:questionId, :optionText, :sortOrder)'
            );
            foreach ($validatedQuestions as $question) {
                $insertQuestion->execute([
                    'surveyId' => $surveyId,
                    'questionText' => $question['text'],
                    'type' => $question['type'],
                ]);
                $questionId = (int) $conn->lastInsertId();
                foreach ($question['options'] as $index => $option) {
                    $insertOption->execute([
                        'questionId' => $questionId,
                        'optionText' => $option,
                        'sortOrder' => $index + 1,
                    ]);
                }
            }

            $inviteCustomers = $conn->prepare(
                "INSERT INTO LoiMoiKhaoSat (MaKhaoSat, MaKH)
                 SELECT :surveyId, kh.MaKH FROM KhachHang AS kh
                 WHERE NOT EXISTS (
                     SELECT 1 FROM LoiMoiKhaoSat AS lm
                     WHERE lm.MaKhaoSat = :existingSurveyId AND lm.MaKH = kh.MaKH
                 )"
            );
            $inviteCustomers->execute([
                'surveyId' => $surveyId,
                'existingSurveyId' => $surveyId,
            ]);
            $invitationCount = $inviteCustomers->rowCount();
            $conn->commit();

            $_SESSION['admin_survey_message'] = 'Đã tạo khảo sát và gửi lời mời đến ' . $invitationCount . ' khách hàng.';
            header('Location: index.php?survey=' . $surveyId, true, 303);
            exit;
        } catch (InvalidArgumentException $exception) {
            if ($conn->inTransaction()) {
                $conn->rollBack();
            }
            $error = $exception->getMessage();
        } catch (Throwable $exception) {
            if ($conn->inTransaction()) {
                $conn->rollBack();
            }
            error_log('Admin survey creation failed: ' . $exception->getMessage());
            $error = 'Không thể tạo khảo sát. Vui lòng kiểm tra cấu trúc cơ sở dữ liệu và thử lại.';
        }
    }
}

$surveyQuery = $conn->query(
    "SELECT ks.MaKhaoSat, ks.TieuDe, ks.MoTa, ks.NgayBatDau, ks.NgayKetThuc,
            COUNT(DISTINCT lm.MaLoiMoi) AS SoLuotMoi,
            COUNT(DISTINCT CASE WHEN lm.TrangThai = 'Đã trả lời' THEN lm.MaLoiMoi END) AS SoLuotHoanThanh
     FROM KhaoSat AS ks
     LEFT JOIN LoiMoiKhaoSat AS lm ON lm.MaKhaoSat = ks.MaKhaoSat
     GROUP BY ks.MaKhaoSat, ks.TieuDe, ks.MoTa, ks.NgayBatDau, ks.NgayKetThuc
     ORDER BY ks.MaKhaoSat DESC"
);
$surveys = $surveyQuery->fetchAll(PDO::FETCH_ASSOC);

$selectedSurvey = null;
$responses = [];
$surveyQuestionResults = [];
if ($selectedSurveyId && $selectedSurveyId > 0) {
    $selectedQuery = $conn->prepare(
        'SELECT MaKhaoSat, TieuDe, MoTa, NgayBatDau, NgayKetThuc
         FROM KhaoSat WHERE MaKhaoSat = :surveyId'
    );
    $selectedQuery->execute(['surveyId' => $selectedSurveyId]);
    $selectedSurvey = $selectedQuery->fetch(PDO::FETCH_ASSOC);
    if ($selectedSurvey) {
        $questionQuery = $conn->prepare(
            'SELECT MaCauHoi, NoiDungCauHoi, LoaiCauHoi
             FROM CauHoiKhaoSat
             WHERE MaKhaoSat = :surveyId
             ORDER BY MaCauHoi'
        );
        $questionQuery->execute(['surveyId' => $selectedSurveyId]);
        $questionResultsQuery = $questionQuery->fetchAll(PDO::FETCH_ASSOC);

        $choiceResultsQuery = $conn->prepare(
            'SELECT dl.MaDanhMuc, dl.NoiDungLuaChon,
                    COUNT(DISTINCT CASE WHEN lm.MaKhaoSat = :surveyId THEN ct.MaLoiMoi END) AS SoLuot
             FROM DanhMucLuaChon AS dl
             LEFT JOIN CauTraLoi AS ct ON ct.MaDanhMuc = dl.MaDanhMuc
             LEFT JOIN LoiMoiKhaoSat AS lm ON lm.MaLoiMoi = ct.MaLoiMoi
             WHERE dl.MaCauHoi = :questionId
             GROUP BY dl.MaDanhMuc, dl.NoiDungLuaChon, dl.ThuTuHienThi
             ORDER BY dl.ThuTuHienThi, dl.MaDanhMuc'
        );
        $textResultsQuery = $conn->prepare(
            "SELECT ct.NoiDungTraLoi AS GiaTri, COUNT(DISTINCT ct.MaLoiMoi) AS SoLuot
             FROM CauTraLoi AS ct
             INNER JOIN LoiMoiKhaoSat AS lm ON lm.MaLoiMoi = ct.MaLoiMoi
             WHERE lm.MaKhaoSat = :surveyId
               AND ct.MaCauHoi = :questionId
               AND ct.NoiDungTraLoi IS NOT NULL
               AND TRIM(ct.NoiDungTraLoi) <> ''
             GROUP BY ct.NoiDungTraLoi
             ORDER BY SoLuot DESC, GiaTri"
        );
        foreach ($questionResultsQuery as $questionResult) {
            $questionId = (int) $questionResult['MaCauHoi'];
            $questionResult['answers'] = [];
            if ($questionResult['LoaiCauHoi'] === 'text') {
                $textResultsQuery->execute([
                    'surveyId' => $selectedSurveyId,
                    'questionId' => $questionId,
                ]);
                $questionResult['answers'] = $textResultsQuery->fetchAll(PDO::FETCH_ASSOC);
            } else {
                $choiceResultsQuery->execute([
                    'surveyId' => $selectedSurveyId,
                    'questionId' => $questionId,
                ]);
                $questionResult['answers'] = $choiceResultsQuery->fetchAll(PDO::FETCH_ASSOC);
            }
            $surveyQuestionResults[] = $questionResult;
        }

        $responseQuery = $conn->prepare(
            'SELECT lm.MaLoiMoi, lm.TrangThai, lm.ThoiGianGui, kh.HoTen,
                    ch.MaCauHoi, ch.NoiDungCauHoi, ch.LoaiCauHoi,
                    ct.NoiDungTraLoi, dl.NoiDungLuaChon, ct.NgayTraLoi
             FROM LoiMoiKhaoSat AS lm
             INNER JOIN KhachHang AS kh ON kh.MaKH = lm.MaKH
             LEFT JOIN CauTraLoi AS ct ON ct.MaLoiMoi = lm.MaLoiMoi
             LEFT JOIN CauHoiKhaoSat AS ch ON ch.MaCauHoi = ct.MaCauHoi
             LEFT JOIN DanhMucLuaChon AS dl ON dl.MaDanhMuc = ct.MaDanhMuc
             WHERE lm.MaKhaoSat = :surveyId
             ORDER BY lm.MaLoiMoi DESC, ch.MaCauHoi, ct.MaCauTraLoi'
        );
        $responseQuery->execute(['surveyId' => $selectedSurveyId]);
        foreach ($responseQuery->fetchAll(PDO::FETCH_ASSOC) as $row) {
            $invitationId = (int) $row['MaLoiMoi'];
            if (!isset($responses[$invitationId])) {
                $responses[$invitationId] = [
                    'customer' => (string) $row['HoTen'],
                    'status' => (string) ($row['TrangThai'] ?? 'Chưa làm'),
                    'submitted_at' => null,
                    'answers' => [],
                ];
            }
            if ($row['MaCauHoi'] !== null) {
                $questionId = (int) $row['MaCauHoi'];
                if (!isset($responses[$invitationId]['answers'][$questionId])) {
                    $responses[$invitationId]['answers'][$questionId] = [
                        'question' => (string) $row['NoiDungCauHoi'],
                        'values' => [],
                    ];
                }
                $answer = $row['NoiDungTraLoi'] !== null && $row['NoiDungTraLoi'] !== ''
                    ? (string) $row['NoiDungTraLoi']
                    : (string) ($row['NoiDungLuaChon'] ?? '');
                if ($answer !== '') {
                    $responses[$invitationId]['answers'][$questionId]['values'][] = $answer;
                    $responses[$invitationId]['submitted_at'] = $row['NgayTraLoi'];
                }
            }
        }
    }
}

function survey_admin_escape(mixed $value): string
{
    return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
}

$defaultStart = (new DateTimeImmutable())->format('Y-m-d\TH:i');
$defaultEnd = (new DateTimeImmutable('+7 days'))->format('Y-m-d\TH:i');
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Khảo sát | Coffee Admin</title>
    <link rel="stylesheet" href="../admin.css?v=3">
    <style>
        .survey-page { max-width: 1120px; }
        .survey-panel { margin-bottom: 18px; padding: 22px; }
        .survey-panel .panel-heading { margin-bottom: 18px; }
        .survey-form { max-width: 850px; }
        .survey-field { display: grid; gap: 7px; margin-bottom: 16px; }
        .survey-field label, .survey-question label { color: #514a42; font-size: 11px; font-weight: 700; }
        .survey-field input, .survey-field textarea, .survey-question input, .survey-question textarea, .survey-question select {
            width: 100%; min-height: 39px; padding: 9px 11px; border: 1px solid #eae7e1;
            border-radius: 8px; outline: 0; background: #fff; color: var(--ink); font: 12px 'DM Sans', Arial, sans-serif;
        }
        .survey-field textarea, .survey-question textarea { min-height: 82px; resize: vertical; }
        .survey-field input:focus, .survey-field textarea:focus, .survey-question input:focus, .survey-question textarea:focus, .survey-question select:focus {
            border-color: #b89b81; box-shadow: 0 0 0 3px rgba(111,78,55,.08);
        }
        .survey-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 14px; }
        .survey-question { margin: 14px 0; padding: 17px; border: 1px solid #efede9; border-radius: 10px; background: #fcfbf9; }
        .survey-question h3 { margin: 0 0 14px; color: var(--coffee-dark); font: 700 13px 'Manrope', sans-serif; }
        .survey-question .survey-field:last-child { margin-bottom: 0; }
        .survey-actions { display: flex; flex-wrap: wrap; gap: 9px; margin-top: 16px; }
        .survey-notice { margin-bottom: 16px; padding: 12px 14px; border: 1px solid #eadbca; border-radius: 8px; background: #f8f1e8; color: #6f4e37; font-size: 12px; }
        .survey-notice.error { border-color: #f1dada; background: #fff4f3; color: #9a3d37; }
        .survey-table-wrap { overflow-x: auto; }
        .survey-table { width: 100%; border-collapse: collapse; }
        .survey-table th, .survey-table td { padding: 12px 10px; text-align: left; vertical-align: top; border-bottom: 1px solid #f1efec; font-size: 11px; }
        .survey-table th { color: #918b82; font-size: 9px; font-weight: 700; letter-spacing: .6px; white-space: nowrap; }
        .survey-table td { color: #514a42; }
        .survey-answer { margin: 0 0 10px; color: #514a42; font-size: 12px; line-height: 1.6; }
        .survey-answer:last-child { margin-bottom: 0; }
        .survey-meta { color: var(--muted); font-size: 10px; }
        .survey-result-card { margin: 12px 0; padding: 15px; border: 1px solid #efede9; border-radius: 9px; background: #fcfbf9; }
        .survey-result-card h3 { margin: 0 0 6px; color: var(--ink); font: 700 13px 'Manrope', sans-serif; }
        .survey-topbar-user { margin-left: auto; }
        @media (max-width: 700px) {
            .survey-grid { grid-template-columns: 1fr; gap: 0; }
            .survey-panel { padding: 15px; }
            .survey-question { padding: 13px; }
            .survey-table { min-width: 650px; }
        }
    </style>
</head>
<body>
<div class="admin-shell">
    <aside class="sidebar" id="sidebar">
        <a class="brand" href="../index.php" aria-label="Coffee Blend Admin">
            <span class="brand-mark">C</span>
            <span class="brand-copy">Coffee<small>Blend · ADMIN</small></span>
        </a>
        <div class="side-caption">MENU CHÍNH</div>
        <nav class="side-nav" aria-label="Điều hướng quản trị">
            <a class="side-link" href="../index.php?page=dashboard"><span class="side-icon" aria-hidden="true">◫</span><span>Tổng quan</span></a>
            <a class="side-link" href="../index.php?page=products"><span class="side-icon" aria-hidden="true">▤</span><span>Sản phẩm</span></a>
            <a class="side-link" href="../index.php?page=orders"><span class="side-icon" aria-hidden="true">▣</span><span>Đơn hàng</span></a>
            <a class="side-link" href="../index.php?page=customers"><span class="side-icon" aria-hidden="true">♙</span><span>Khách hàng</span></a>
            <a class="side-link active" href="../index.php?page=surveys"><span class="side-icon" aria-hidden="true">▧</span><span>Khảo sát ý kiến</span></a>
        </nav>
        <div class="side-caption side-caption-spaced">HỆ THỐNG</div>
        <a class="side-link" href="../auth/logout.php"><span class="side-icon" aria-hidden="true">↪</span><span>Đăng xuất</span></a>
        <div class="sidebar-bottom">
            <a class="store-link" href="../../index.php"><span aria-hidden="true">↗</span> Xem website cửa hàng</a>
            <?php $adminProfile = admin_user() ?? []; ?>
            <div class="admin-profile">
                <div class="avatar avatar-admin">AD</div>
                <div class="profile-copy"><strong><?= survey_admin_escape($adminProfile['full_name'] ?? 'Quản trị viên') ?></strong><span><?= survey_admin_escape($adminProfile['username'] ?? '') ?> · <?= survey_admin_escape($adminProfile['role'] ?? 'Admin') ?></span></div>
            </div>
        </div>
    </aside>
    <main class="main-area">
        <header class="topbar">
            <button class="icon-button menu-toggle" id="menuToggle" type="button" aria-label="Mở menu" aria-expanded="false">☰</button>
            <div class="breadcrumb"><span>Admin</span><span class="crumb-separator">/</span><strong>Khảo sát ý kiến</strong></div>
            <div class="topbar-user survey-topbar-user"><div class="avatar avatar-admin">AD</div><span><?= survey_admin_escape($adminProfile['full_name'] ?? 'Quản trị viên') ?></span></div>
        </header>

        <div class="page-content survey-page">
        <?php if ($message !== ''): ?><div class="survey-notice" role="status"><?= survey_admin_escape($message) ?></div><?php endif; ?>
        <?php if ($error !== ''): ?><div class="survey-notice error" role="alert"><?= survey_admin_escape($error) ?></div><?php endif; ?>

        <section class="welcome-row page-title-row">
            <div><div class="eyebrow">THẤU HIỂU KHÁCH HÀNG</div><h1>Khảo sát khách hàng</h1><p class="page-subtitle">Tạo khảo sát, gửi lời mời đến khách hàng và theo dõi câu trả lời.</p></div>
            <a class="button button-outline" href="../index.php?page=surveys">← Quay lại</a>
        </section>

        <section class="panel survey-panel">
            <div class="panel-heading"><div><h2>Tạo khảo sát mới</h2><p>Khách hàng có thể tham gia trong khoảng thời gian đã chọn.</p></div></div>
            <form method="post" action="index.php">
                <input type="hidden" name="csrf_token" value="<?= survey_admin_escape($_SESSION['admin_survey_csrf']) ?>">
                <div class="survey-form">
                <div class="survey-field"><label for="survey-title">Tiêu đề khảo sát</label><input id="survey-title" name="title" maxlength="255" placeholder="Ví dụ: Đánh giá trải nghiệm tại cửa hàng" required></div>
                <div class="survey-field"><label for="survey-description">Mô tả</label><textarea id="survey-description" name="description" maxlength="5000" placeholder="Cho khách hàng biết mục đích của khảo sát"></textarea></div>
                <div class="survey-grid">
                    <div class="survey-field"><label for="survey-start">Ngày bắt đầu</label><input id="survey-start" type="datetime-local" name="start_at" value="<?= survey_admin_escape($defaultStart) ?>" required></div>
                    <div class="survey-field"><label for="survey-end">Ngày kết thúc</label><input id="survey-end" type="datetime-local" name="end_at" value="<?= survey_admin_escape($defaultEnd) ?>" required></div>
                </div>
                <div id="survey-questions">
                    <div class="survey-question" data-question>
                        <h3>Câu hỏi 1</h3>
                        <div class="survey-field"><label>Nội dung câu hỏi</label><input name="questions[0][text]" maxlength="255" placeholder="Nhập câu hỏi" required></div>
                        <div class="survey-field">
                            <label>Kiểu trả lời</label>
                            <select name="questions[0][type]" data-question-type>
                                <option value="single">Chọn một phương án</option>
                                <option value="multiple">Chọn nhiều phương án</option>
                                <option value="text">Trả lời tự do</option>
                            </select>
                        </div>
                        <div class="survey-field" data-options-field><label>Các phương án (mỗi dòng một lựa chọn, tối thiểu 2)</label><textarea name="questions[0][options]" placeholder="Rất hài lòng&#10;Bình thường&#10;Chưa hài lòng" required></textarea></div>
                    </div>
                </div>
                <div class="survey-actions">
                    <button class="button button-outline" type="button" id="add-survey-question">＋ Thêm câu hỏi</button>
                    <button class="button button-primary" type="submit">Tạo khảo sát và gửi lời mời</button>
                </div>
                </div>
            </form>
        </section>

        <section class="panel survey-panel">
            <div class="panel-heading"><div><h2>Danh sách khảo sát</h2><p>Theo dõi thời gian, lời mời và tiến độ phản hồi.</p></div></div>
            <div class="survey-table-wrap">
                <table class="survey-table">
                    <thead><tr><th>Mã</th><th>Tiêu đề</th><th>Thời gian</th><th>Lượt mời</th><th>Hoàn thành</th><th></th></tr></thead>
                    <tbody>
                    <?php foreach ($surveys as $survey): ?>
                        <tr>
                            <td><?= (int) $survey['MaKhaoSat'] ?></td>
                            <td><?= survey_admin_escape($survey['TieuDe']) ?></td>
                            <td><?= survey_admin_escape($survey['NgayBatDau']) ?><br>— <?= survey_admin_escape($survey['NgayKetThuc']) ?></td>
                            <td><?= (int) $survey['SoLuotMoi'] ?></td>
                            <td><?= (int) $survey['SoLuotHoanThanh'] ?></td>
                            <td><a class="button button-outline button-small" href="index.php?survey=<?= (int) $survey['MaKhaoSat'] ?>">Xem kết quả</a></td>
                        </tr>
                    <?php endforeach; ?>
                    <?php if ($surveys === []): ?><tr><td colspan="6" class="muted-cell">Chưa có khảo sát nào.</td></tr><?php endif; ?>
                    </tbody>
                </table>
            </div>
        </section>

        <?php if (is_array($selectedSurvey)): ?>
            <section class="panel survey-panel">
                <div class="panel-heading"><div><h2>Kết quả: <?= survey_admin_escape($selectedSurvey['TieuDe']) ?></h2><p><?= nl2br(survey_admin_escape($selectedSurvey['MoTa'] ?? '')) ?></p></div></div>
                <?php foreach ($surveyQuestionResults as $questionResult): ?>
                    <?php $questionAnswerTotal = array_sum(array_map(static fn (array $answer): int => (int) $answer['SoLuot'], $questionResult['answers'])); ?>
                    <article class="survey-result-card">
                        <h3><?= survey_admin_escape($questionResult['NoiDungCauHoi']) ?></h3>
                        <p class="survey-meta"><?= $questionAnswerTotal ?> lượt trả lời<?= $questionResult['LoaiCauHoi'] === 'multiple' ? ' (mỗi phương án được tính riêng)' : '' ?></p>
                        <?php if ($questionResult['answers'] === [] || $questionAnswerTotal === 0): ?>
                            <p class="subtle">Chưa có câu trả lời cho câu hỏi này.</p>
                        <?php elseif ($questionResult['LoaiCauHoi'] === 'text'): ?>
                            <?php foreach ($questionResult['answers'] as $answer): ?>
                                <p class="survey-answer"><?= nl2br(survey_admin_escape($answer['GiaTri'])) ?> <span class="survey-meta">· <?= (int) $answer['SoLuot'] ?> lượt</span></p>
                            <?php endforeach; ?>
                        <?php else: ?>
                            <?php foreach ($questionResult['answers'] as $answer): ?>
                                <?php $answerCount = (int) $answer['SoLuot']; $answerPercent = $questionAnswerTotal > 0 ? (int) round($answerCount / $questionAnswerTotal * 100) : 0; ?>
                                <p class="survey-answer"><?= survey_admin_escape($answer['NoiDungLuaChon']) ?> — <strong><?= $answerCount ?></strong> lượt (<?= $answerPercent ?>%)</p>
                            <?php endforeach; ?>
                        <?php endif; ?>
                    </article>
                <?php endforeach; ?>
                <?php if ($responses === []): ?>
                    <p class="survey-meta">Chưa có khách hàng nào được mời hoặc chưa có câu trả lời.</p>
                <?php else: ?>
                    <?php foreach ($responses as $response): ?>
                        <article class="survey-result-card">
                            <h3><?= survey_admin_escape($response['customer']) ?></h3>
                            <p class="survey-meta">Trạng thái: <?= survey_admin_escape($response['status']) ?>
                                <?= $response['submitted_at'] ? ' · Hoàn thành: ' . survey_admin_escape($response['submitted_at']) : '' ?>
                            </p>
                            <?php if ($response['answers'] === []): ?>
                                <p class="subtle">Chưa gửi câu trả lời.</p>
                            <?php else: foreach ($response['answers'] as $answer): ?>
                                <div class="survey-answer">
                                    <strong><?= survey_admin_escape($answer['question']) ?></strong>
                                    <div><?= nl2br(survey_admin_escape(implode(' · ', $answer['values']))) ?></div>
                                </div>
                            <?php endforeach; endif; ?>
                        </article>
                    <?php endforeach; ?>
                <?php endif; ?>
            </section>
        <?php endif; ?>
        <footer class="page-footer">© 2026 Coffee Blend Admin <span>Quản lý và tổng hợp phản hồi khảo sát</span></footer>
        </div>
    </main>
</div>
<div class="sidebar-backdrop" id="sidebarBackdrop"></div>
<script>
    (function () {
      var container = document.getElementById('survey-questions');
      var addButton = document.getElementById('add-survey-question');
      var nextQuestion = 1;

      function setOptionsVisibility(editor) {
        var type = editor.querySelector('[data-question-type]').value;
        var optionsField = editor.querySelector('[data-options-field]');
        var optionsInput = optionsField.querySelector('textarea');
        optionsField.hidden = type === 'text';
        optionsInput.required = type !== 'text';
      }

      container.addEventListener('change', function (event) {
        if (event.target.matches('[data-question-type]')) {
          setOptionsVisibility(event.target.closest('[data-question]'));
        }
      });

      addButton.addEventListener('click', function () {
        if (nextQuestion >= 30) return;
        var index = nextQuestion++;
        var editor = document.createElement('div');
        editor.className = 'survey-question';
        editor.setAttribute('data-question', '');
        editor.innerHTML =
          '<h3>Câu hỏi ' + (index + 1) + '</h3>' +
          '<div class="survey-field"><label>Nội dung câu hỏi</label><input name="questions[' + index + '][text]" maxlength="255" required></div>' +
          '<div class="survey-field"><label>Kiểu trả lời</label><select name="questions[' + index + '][type]" data-question-type>' +
          '<option value="single">Chọn một phương án</option><option value="multiple">Chọn nhiều phương án</option><option value="text">Trả lời tự do</option></select></div>' +
          '<div class="survey-field" data-options-field><label>Các phương án (mỗi dòng một lựa chọn, tối thiểu 2)</label>' +
          '<textarea name="questions[' + index + '][options]" required></textarea></div>';
        container.appendChild(editor);
      });
    })();
</script>
<script src="../admin.js?v=3"></script>
</body>
</html>
