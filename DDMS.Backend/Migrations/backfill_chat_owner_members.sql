-- Backfill owner vao cac cuoc hoi thoai bi tao thieu thanh vien.
--
-- Nguyen nhan: ChatService.StartConversationAsync truoc day lay owner tu
-- tour.created_by, ma TourService.CreateAsync (api/legacy/tours) khong bao gio
-- gan truong nay. ownerId = NULL nen nhanh `if (ownerId.HasValue)` bi bo qua
-- im lang, tao ra conversation chi co mot thanh vien la khach. Owner khong
-- thay cuoc hoi thoai do trong /inbox va khong nhan duoc tin nhan nao.
--
-- Code da sua de uu tien schedule.boat.owner_id, nhung du lieu cu van thieu.
-- Script nay them owner con thieu vao conversation_members.
--
-- CHAY BUOC 1 TRUOC de xem se dong nao bi anh huong, roi moi chay BUOC 2.

-- ============================================================
-- BUOC 1: Kiem tra (chi doc, khong sua gi)
-- ============================================================
SELECT
    c.id                AS conversation_id,
    c.booking_id,
    b.user_id           AS customer_id,
    COALESCE(s.boat_id, NULL) AS boat_id,
    COALESCE(bo.owner_id, t.created_by) AS owner_id,
    (SELECT COUNT(*) FROM conversation_members cm WHERE cm.conversation_id = c.id) AS member_count
FROM conversations c
JOIN bookings b       ON b.id = c.booking_id
JOIN tour_schedules s ON s.id = b.schedule_id
JOIN tours t          ON t.id = s.tour_id
LEFT JOIN boats bo    ON bo.id = s.boat_id
WHERE COALESCE(bo.owner_id, t.created_by) IS NOT NULL
  AND COALESCE(bo.owner_id, t.created_by) <> b.user_id
  AND NOT EXISTS (
      SELECT 1 FROM conversation_members cm
      WHERE cm.conversation_id = c.id
        AND cm.user_id = COALESCE(bo.owner_id, t.created_by)
  );

-- ============================================================
-- BUOC 2: Va du lieu
-- ============================================================
-- last_read_at de NULL de owner thay dung so tin chua doc.
-- INSERT INTO conversation_members (id, conversation_id, user_id, joined_at, last_read_at)
-- SELECT
--     UUID(),
--     c.id,
--     COALESCE(bo.owner_id, t.created_by),
--     NOW(6),
--     NULL
-- FROM conversations c
-- JOIN bookings b       ON b.id = c.booking_id
-- JOIN tour_schedules s ON s.id = b.schedule_id
-- JOIN tours t          ON t.id = s.tour_id
-- LEFT JOIN boats bo    ON bo.id = s.boat_id
-- WHERE COALESCE(bo.owner_id, t.created_by) IS NOT NULL
--   AND COALESCE(bo.owner_id, t.created_by) <> b.user_id
--   AND NOT EXISTS (
--       SELECT 1 FROM conversation_members cm
--       WHERE cm.conversation_id = c.id
--         AND cm.user_id = COALESCE(bo.owner_id, t.created_by)
--   );

-- ============================================================
-- Con lai: cac tour khong xac dinh duoc owner
-- ============================================================
-- Nhung dong hien ra o day co ca boat.owner_id lan tour.created_by deu NULL.
-- Script khong doan duoc owner cho chung - phai gan chu tau cho lich trinh
-- hoac dien tours.created_by bang tay roi chay lai BUOC 2.
-- SELECT c.id AS conversation_id, c.booking_id, t.id AS tour_id, t.name AS tour_name
-- FROM conversations c
-- JOIN bookings b       ON b.id = c.booking_id
-- JOIN tour_schedules s ON s.id = b.schedule_id
-- JOIN tours t          ON t.id = s.tour_id
-- LEFT JOIN boats bo    ON bo.id = s.boat_id
-- WHERE COALESCE(bo.owner_id, t.created_by) IS NULL;
