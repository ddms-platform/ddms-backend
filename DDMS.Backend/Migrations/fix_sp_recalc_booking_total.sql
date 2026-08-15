-- Sua sp_recalc_booking_total: tong tien bo quen tien dich vu.
--
-- Trieu chung: khach dat tour het 2.600.000d (500k tour + 300k cabin +
-- 1.800.000d dich vu) nhung man "Tour da dat" chi hien 800.000d.
--
-- Nguyen nhan: bang booking_cabins va booking_services deu co trigger AFTER
-- INSERT/UPDATE/DELETE goi sp_recalc_booking_total. Thu tuc nay tinh:
--
--     total_price = base_price + SUM(booking_cabins) - discount_amount
--
-- Khong cong tien dich vu. Ung dung (BookingPricingService) tinh dung
-- base + cabin + service - discount roi ghi vao bookings, nhung ngay sau do EF
-- insert cac dong booking_services, trigger chay va GHI DE total_price bang con
-- so thieu tien dich vu. Database chay sau nen database thang.
--
-- Ket qua: moi don co dich vu deu bi ghi thieu dung bang tien dich vu.
--
-- Luu y: thu tuc nay khong nam trong repo, chi ton tai trong DB (co trong
-- boat_tour_clean.sql). Day la lan dau no duoc dua vao version control.
--
-- ============================================================
-- BUOC 1: Xem truoc cac don dang sai (chi doc)
-- ============================================================
SELECT
    LEFT(id, 8) AS booking_id,
    created_at,
    status,
    base_price,
    cabin_price,
    service_price,
    discount_amount,
    total_price,
    (base_price + cabin_price + service_price - discount_amount) AS total_dung,
    (base_price + cabin_price + service_price - discount_amount - total_price) AS chenh_lech
FROM bookings
WHERE ABS(total_price - (base_price + cabin_price + service_price - discount_amount)) > 0.01
ORDER BY created_at DESC;

-- Ghi chu khi doc ket qua tren:
--   * chenh_lech > 0 va dung bang service_price  -> loi trigger nay, buoc 3 se va.
--   * chenh_lech < 0 (don truoc thang 8/2026)    -> loi cu khac: base_price hoi
--     do luu don gia thay vi don gia * so khach. Buoc 3 KHONG dung toi cac don
--     nay, xu ly rieng neu can.

-- ============================================================
-- BUOC 2: Sua thu tuc
-- ============================================================
-- DROP PROCEDURE IF EXISTS sp_recalc_booking_total;
--
-- DELIMITER $$
-- CREATE PROCEDURE sp_recalc_booking_total(IN p_booking_id CHAR(36))
-- BEGIN
--     UPDATE bookings
--     SET
--         cabin_price = (
--             SELECT COALESCE(SUM(quantity * unit_price), 0)
--             FROM booking_cabins WHERE booking_id = p_booking_id
--         ),
--         service_price = (
--             SELECT COALESCE(SUM(quantity * unit_price), 0)
--             FROM booking_services WHERE booking_id = p_booking_id
--         ),
--         total_price = base_price
--             + (SELECT COALESCE(SUM(quantity * unit_price), 0)
--                FROM booking_cabins WHERE booking_id = p_booking_id)
--             + (SELECT COALESCE(SUM(quantity * unit_price), 0)
--                FROM booking_services WHERE booking_id = p_booking_id)
--             - discount_amount
--     WHERE id = p_booking_id;
-- END$$
-- DELIMITER ;

-- ============================================================
-- BUOC 3: Va lai cac don da bi ghi thieu tien dich vu
-- ============================================================
-- Chi dung toi cac don thieu dung bang service_price, tranh dung vao nhom loi
-- cu (chenh_lech am) o buoc 1.
--
-- UPDATE bookings
-- SET total_price = base_price + cabin_price + service_price - discount_amount,
--     updated_at  = NOW(6)
-- WHERE service_price > 0
--   AND ABS((base_price + cabin_price + service_price - discount_amount) - total_price - service_price) < 0.01;

-- ============================================================
-- BUOC 4: Kiem tra lai
-- ============================================================
-- Chay lai BUOC 1. Chi con lai nhom chenh_lech am (don cu truoc thang 8/2026).
