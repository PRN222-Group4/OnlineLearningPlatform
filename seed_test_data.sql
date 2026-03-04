-- ══════════════════════════════════════════════════════
-- SEED TEST DATA
-- Student:    2ceff858-9a3c-4f75-94bc-18b3c3984378  (Role 1)
-- Instructor: b0170017-0cbd-4d3e-b4fa-54d0633d1cc8  (Role 2)
-- ══════════════════════════════════════════════════════

-- ── 1. COURSES ────────────────────────────────────────
INSERT INTO "Courses" ("CourseId","LanguageId","Title","Description","Image","Status","Price","RejectReason","Level","CreatedBy","CreatedAt","IsDeleted","UpdatedBy","UpdatedAt")
VALUES
(
    'cc000001-0000-0000-0000-cc0000000001',
    '00000000-0000-0000-0000-000000000001',
    'Lập Trình Web với ASP.NET Core',
    'Khóa học toàn diện về ASP.NET Core từ cơ bản đến nâng cao. Bạn sẽ học cách xây dựng web app hoàn chỉnh với Razor Pages, Entity Framework, và RESTful API.',
    'https://images.unsplash.com/photo-1593720213428-28a5b9e94613?w=800&q=80',
    2, 299000, '', 1,
    'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',
    NOW(), false, NULL, NULL
),
(
    'cc000002-0000-0000-0000-cc0000000002',
    '00000000-0000-0000-0000-000000000001',
    'Python cho Data Science',
    'Học Python từ đầu và ứng dụng vào phân tích dữ liệu thực tế với Pandas, NumPy và Matplotlib.',
    'https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=800&q=80',
    2, 199000, '', 0,
    'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',
    NOW(), false, NULL, NULL
),
(
    'cc000003-0000-0000-0000-cc0000000003',
    '00000000-0000-0000-0000-000000000001',
    'UI/UX Design cơ bản với Figma',
    'Học thiết kế giao diện người dùng từ zero với Figma. Bao gồm wireframe, prototype và design system.',
    'https://images.unsplash.com/photo-1561070791-2526d30994b5?w=800&q=80',
    2, 0, '', 0,
    'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',
    NOW(), false, NULL, NULL
)
ON CONFLICT ("CourseId") DO NOTHING;


-- ── 2. MODULES ────────────────────────────────────────
INSERT INTO "Modules" ("ModuleId","CourseId","Name","Description","Index","IsPublished","CreatedBy","CreatedAt","IsDeleted")
VALUES
('aa000001-0000-0000-0000-aa0000000001','cc000001-0000-0000-0000-cc0000000001','Giới thiệu ASP.NET Core','Tổng quan về framework',1,true,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('aa000002-0000-0000-0000-aa0000000002','cc000001-0000-0000-0000-cc0000000001','Razor Pages','Xây dựng UI với Razor Pages',2,true,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('aa000003-0000-0000-0000-aa0000000003','cc000001-0000-0000-0000-cc0000000001','Entity Framework Core','ORM và làm việc với database',3,true,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('aa000004-0000-0000-0000-aa0000000004','cc000002-0000-0000-0000-cc0000000002','Python Cơ Bản','Biến, kiểu dữ liệu, vòng lặp',1,true,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('aa000005-0000-0000-0000-aa0000000005','cc000002-0000-0000-0000-cc0000000002','Pandas & NumPy','Xử lý dữ liệu',2,true,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('aa000006-0000-0000-0000-aa0000000006','cc000003-0000-0000-0000-cc0000000003','Figma Cơ Bản','Giao diện và công cụ',1,true,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('aa000007-0000-0000-0000-aa0000000007','cc000003-0000-0000-0000-cc0000000003','Wireframe & Prototype','Tạo wireframe tương tác',2,true,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false)
ON CONFLICT ("ModuleId") DO NOTHING;


-- ── 3. LESSONS ────────────────────────────────────────
INSERT INTO "Lessons" ("LessonId","ModuleId","Title","Description","EstimatedMinutes","OrderIndex","CreatedBy","CreatedAt","IsDeleted")
VALUES
('bb000001-0000-0000-0000-bb0000000001','aa000001-0000-0000-0000-aa0000000001','ASP.NET Core là gì?','Tổng quan về .NET ecosystem',10,1,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000002-0000-0000-0000-bb0000000002','aa000001-0000-0000-0000-aa0000000001','Cài đặt môi trường','Cài .NET SDK và Visual Studio',15,2,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000003-0000-0000-0000-bb0000000003','aa000001-0000-0000-0000-aa0000000001','Tạo project đầu tiên','Tạo và chạy project đầu tiên',20,3,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000004-0000-0000-0000-bb0000000004','aa000002-0000-0000-0000-aa0000000002','Razor Pages là gì?','Giới thiệu page model',12,1,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000005-0000-0000-0000-bb0000000005','aa000002-0000-0000-0000-aa0000000002','Tag Helpers','Sử dụng Tag Helpers',15,2,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000006-0000-0000-0000-bb0000000006','aa000003-0000-0000-0000-aa0000000003','DbContext và Migrations','Cấu hình DbContext',20,1,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000007-0000-0000-0000-bb0000000007','aa000003-0000-0000-0000-aa0000000003','CRUD với EF Core','CRUD operations',25,2,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000008-0000-0000-0000-bb0000000008','aa000004-0000-0000-0000-aa0000000004','Cài đặt Python','Cài Python và môi trường',10,1,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000009-0000-0000-0000-bb0000000009','aa000004-0000-0000-0000-aa0000000004','Biến và kiểu dữ liệu','Kiểu dữ liệu cơ bản',15,2,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000010-0000-0000-0000-bb0000000010','aa000005-0000-0000-0000-aa0000000005','Pandas DataFrame','Tạo và thao tác DataFrame',20,1,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000011-0000-0000-0000-bb0000000011','aa000006-0000-0000-0000-aa0000000006','Giao diện Figma','Khám phá panel và toolbar',10,1,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000012-0000-0000-0000-bb0000000012','aa000006-0000-0000-0000-aa0000000006','Shapes và Text','Tạo shape, text và group',12,2,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false),
('bb000013-0000-0000-0000-bb0000000013','aa000007-0000-0000-0000-aa0000000007','Tạo Wireframe','Wireframe cho màn hình Login',15,1,'b0170017-0cbd-4d3e-b4fa-54d0633d1cc8',NOW(),false)
ON CONFLICT ("LessonId") DO NOTHING;


-- ── 4. ENROLLMENTS ────────────────────────────────────
INSERT INTO "Enrollments" ("EnrollmentId","UserId","CourseId","Status","ProgressPercent","EnrolledAt","CreatedBy","CreatedAt","IsDeleted")
VALUES
(
    'dd000001-0000-0000-0000-dd0000000001',
    '2ceff858-9a3c-4f75-94bc-18b3c3984378',
    'cc000001-0000-0000-0000-cc0000000001',
    1, 45, NOW() - INTERVAL '10 days',
    '2ceff858-9a3c-4f75-94bc-18b3c3984378',
    NOW() - INTERVAL '10 days', false
),
(
    'dd000002-0000-0000-0000-dd0000000002',
    '2ceff858-9a3c-4f75-94bc-18b3c3984378',
    'cc000003-0000-0000-0000-cc0000000003',
    1, 100, NOW() - INTERVAL '20 days',
    '2ceff858-9a3c-4f75-94bc-18b3c3984378',
    NOW() - INTERVAL '20 days', false
)
ON CONFLICT ("EnrollmentId") DO NOTHING;


-- ── 5. USER LESSON PROGRESS ───────────────────────────
INSERT INTO "UserLessonProgresses" ("LessonProgressId","UserId","LessonId","IsCompleted","CompletedAt","LastAccessedAt","LastWatchedSecond","CompletionPercent")
VALUES
('ee000001-0000-0000-0000-ee0000000001','2ceff858-9a3c-4f75-94bc-18b3c3984378','bb000001-0000-0000-0000-bb0000000001',true, NOW()-INTERVAL '9 days', NOW()-INTERVAL '9 days',  0,  100),
('ee000002-0000-0000-0000-ee0000000002','2ceff858-9a3c-4f75-94bc-18b3c3984378','bb000002-0000-0000-0000-bb0000000002',true, NOW()-INTERVAL '8 days', NOW()-INTERVAL '8 days',  0,  100),
('ee000003-0000-0000-0000-ee0000000003','2ceff858-9a3c-4f75-94bc-18b3c3984378','bb000003-0000-0000-0000-bb0000000003',true, NOW()-INTERVAL '7 days', NOW()-INTERVAL '7 days',  0,  100),
('ee000004-0000-0000-0000-ee0000000004','2ceff858-9a3c-4f75-94bc-18b3c3984378','bb000004-0000-0000-0000-bb0000000004',false,NULL,                  NOW()-INTERVAL '1 day',  180, 60),
('ee000005-0000-0000-0000-ee0000000005','2ceff858-9a3c-4f75-94bc-18b3c3984378','bb000005-0000-0000-0000-bb0000000005',false,NULL,                  NULL,                    0,   0),
('ee000006-0000-0000-0000-ee0000000006','2ceff858-9a3c-4f75-94bc-18b3c3984378','bb000006-0000-0000-0000-bb0000000006',false,NULL,                  NULL,                    0,   0),
('ee000007-0000-0000-0000-ee0000000007','2ceff858-9a3c-4f75-94bc-18b3c3984378','bb000007-0000-0000-0000-bb0000000007',false,NULL,                  NULL,                    0,   0)
ON CONFLICT ("LessonProgressId") DO NOTHING;


-- ── 6. PAYMENT ────────────────────────────────────────
INSERT INTO "Payments" ("PaymentId","UserId","CourseId","EnrollmentId","OrderCode","PaymentLinkId","CheckoutUrl","Amount","Currency","Reference","CounterAccountNumber","CounterAccountName","CounterAccountBankName","Method","Status","PaidAt","RawWebhookData","CreatedBy","CreatedAt","IsDeleted","Type")
VALUES
(
    'ff000001-0000-0000-0000-ff0000000001',
    '2ceff858-9a3c-4f75-94bc-18b3c3984378',
    'cc000001-0000-0000-0000-cc0000000001',
    'dd000001-0000-0000-0000-dd0000000001',
    123456789,
    'payos-link-001',
    'https://pay.payos.vn/web/payos-link-001',
    299000, 'VND',
    'REF001', '1234567890', 'TA MINH HOANG', 'VietcomBank',
    'BANK_TRANSFER', 2,
    NOW() - INTERVAL '10 days',
    '{}',
    '2ceff858-9a3c-4f75-94bc-18b3c3984378',
    NOW() - INTERVAL '10 days',
    false, 0
)
ON CONFLICT ("PaymentId") DO NOTHING;


-- ── VERIFY ────────────────────────────────────────────
SELECT 'Courses'     AS tbl, COUNT(*) FROM "Courses"              WHERE "CourseId"::text          LIKE 'cc000%'
UNION ALL
SELECT 'Modules',            COUNT(*) FROM "Modules"              WHERE "ModuleId"::text          LIKE 'aa000%'
UNION ALL
SELECT 'Lessons',            COUNT(*) FROM "Lessons"              WHERE "LessonId"::text          LIKE 'bb000%'
UNION ALL
SELECT 'Enrollments',        COUNT(*) FROM "Enrollments"          WHERE "EnrollmentId"::text      LIKE 'dd000%'
UNION ALL
SELECT 'Progress',           COUNT(*) FROM "UserLessonProgresses" WHERE "LessonProgressId"::text  LIKE 'ee000%'
UNION ALL
SELECT 'Payments',           COUNT(*) FROM "Payments"             WHERE "PaymentId"::text         LIKE 'ff000%';
