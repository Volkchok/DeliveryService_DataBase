-- 1. Создаем базу данных если не существует
\c postgres;
CREATE DATABASE "PolyclinicDB"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'Russian_Russia.1251'
    LC_CTYPE = 'Russian_Russia.1251'
    CONNECTION LIMIT = -1;

-- 2. Подключаемся к базе
\c "PolyclinicDB";

-- 3. Создаем таблицы
CREATE TABLE IF NOT EXISTS patients (
    patient_id SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    birthdate DATE,
    address TEXT,
    passport VARCHAR(20) UNIQUE,
    policy VARCHAR(20) UNIQUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS doctors (
    doctor_id SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    specialization VARCHAR(100) NOT NULL,
    license_number VARCHAR(50) UNIQUE
);

CREATE TABLE IF NOT EXISTS employees (
    employee_id SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    position VARCHAR(50) NOT NULL,
    phone VARCHAR(15),
    email VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS appointments (
    appointment_id SERIAL PRIMARY KEY,
    patient_id INTEGER NOT NULL REFERENCES patients(patient_id) ON DELETE RESTRICT,
    doctor_id INTEGER NOT NULL REFERENCES doctors(doctor_id) ON DELETE RESTRICT,
    employee_id INTEGER REFERENCES employees(employee_id) ON DELETE SET NULL,
    appointment_date TIMESTAMP NOT NULL,
    complaints TEXT,
    diagnosis VARCHAR(200),
    prescription TEXT,
    status VARCHAR(20) DEFAULT 'scheduled'
);

CREATE TABLE IF NOT EXISTS medical_procedures (
    procedure_id SERIAL PRIMARY KEY,
    appointment_id INTEGER NOT NULL REFERENCES appointments(appointment_id) ON DELETE CASCADE,
    procedure_name VARCHAR(100) NOT NULL,
    procedure_result TEXT,
    procedure_date DATE NOT NULL,
    doctor_id INTEGER REFERENCES doctors(doctor_id)
);

-- 4. Создаем процедуры
CREATE OR REPLACE PROCEDURE insert_patient(
    p_full_name VARCHAR(100),
    p_birthdate DATE,
    p_address TEXT,
    p_passport VARCHAR(20),
    p_policy VARCHAR(20)
)
LANGUAGE SQL
AS $$
    INSERT INTO patients (full_name, birthdate, address, passport, policy)
    VALUES (p_full_name, p_birthdate, p_address, p_passport, p_policy);
$$;

CREATE OR REPLACE PROCEDURE update_patient(
    p_patient_id INTEGER,
    p_full_name VARCHAR(100),
    p_address TEXT
)
LANGUAGE SQL
AS $$
    UPDATE patients
    SET full_name = p_full_name, address = p_address
    WHERE patient_id = p_patient_id;
$$;

-- 5. Создаем функции
CREATE OR REPLACE FUNCTION get_longest_patient_name()
RETURNS VARCHAR
LANGUAGE SQL
AS $$
    SELECT full_name
    FROM patients
    ORDER BY LENGTH(full_name) DESC
    LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION get_top10_patients()
RETURNS TABLE (
    patient_id INTEGER,
    full_name VARCHAR(100),
    birthdate DATE,
    address TEXT
)
LANGUAGE SQL
AS $$
    SELECT patient_id, full_name, birthdate, address
    FROM patients
    ORDER BY patient_id DESC
    LIMIT 10;
$$;

-- 6. Создаем представление
CREATE OR REPLACE VIEW patient_appointment_details AS
SELECT
    p.full_name AS patient_name,
    p.birthdate,
    d.full_name AS doctor_name,
    d.specialization,
    a.appointment_date,
    a.complaints,
    a.diagnosis,
    a.prescription,
    EXTRACT(YEAR FROM AGE(CURRENT_DATE, p.birthdate)) AS patient_age
FROM appointments a
JOIN patients p ON a.patient_id = p.patient_id
JOIN doctors d ON a.doctor_id = d.doctor_id
WHERE a.appointment_date >= CURRENT_DATE - INTERVAL '30 days';

-- 7. Добавляем тестовые данные

-- Врачи
INSERT INTO doctors (full_name, specialization, license_number) VALUES
('Иванов Петр Сергеевич', 'Терапевт', 'ТЕРА-001'),
('Сидорова Анна Владимировна', 'Хирург', 'ХИРУ-001'),
('Козлов Дмитрий Иванович', 'Кардиолог', 'КАРД-001'),
('Петрова Ольга Николаевна', 'Невролог', 'НЕВР-001'),
('Смирнов Алексей Николаевич', 'Офтальмолог', 'ОФТ-001');

-- Сотрудники регистратуры
INSERT INTO employees (full_name, position, phone, email) VALUES
('Смирнова Елена Алексеевна', 'Регистратор', '+79161234567', 'smirnova@polyclinic.ru'),
('Васильев Игорь Петрович', 'Администратор', '+79167654321', 'vasilev@polyclinic.ru'),
('Попова Наталья Владимировна', 'Старший регистратор', '+79165554433', 'popova@polyclinic.ru');

-- Пациенты через процедуру insert_patient
CALL insert_patient('Иванов Иван Иванович', '1980-05-15', 'Москва, ул. Ленина, д. 10, кв. 25', '4510 123456', '1234567890123456');
CALL insert_patient('Петрова Мария Сергеевна', '1992-08-20', 'Москва, ул. Новая, д. 45, кв. 12', '4510 654321', '2345678901234567');
CALL insert_patient('Сидоров Алексей Владимирович', '1975-11-10', 'Москва, пр. Мира, д. 120, кв. 78', '4511 123456', '3456789012345678');
CALL insert_patient('Козлова Елена Петровна', '1988-03-22', 'Москва, ул. Гагарина, д. 15, кв. 34', '4511 654321', '4567890123456789');
CALL insert_patient('Новикова Анна Сергеевна', '1995-12-05', 'Москва, ул. Пушкина, д. 8, кв. 15', '4512 123456', '5678901234567890');
CALL insert_patient('Морозов Дмитрий Игоревич', '1982-07-30', 'Москва, ул. Тверская, д. 25, кв. 42', '4512 654321', '6789012345678901');
CALL insert_patient('Волкова Ольга Александровна', '1990-01-14', 'Москва, ул. Арбат, д. 33, кв. 19', '4513 123456', '7890123456789012');
CALL insert_patient('Федоров Сергей Петрович', '1978-09-18', 'Москва, ул. Садовая, д. 67, кв. 56', '4513 654321', '8901234567890123');

-- Приемы (только будние дни)
INSERT INTO appointments (patient_id, doctor_id, employee_id, appointment_date, complaints, diagnosis, prescription, status) VALUES
(1, 1, 1, '2024-01-15 09:00:00', 'Головная боль, слабость, температура 37.8', 'ОРВИ', 'Постельный режим, обильное питье, парацетамол 500мг 3 раза в день', 'completed'),
(2, 2, 1, '2024-01-16 10:30:00', 'Острая боль в правом колене, отек', 'Травматический артрит', 'Рентген колена, противовоспалительные препараты, фиксация эластичным бинтом', 'completed'),
(3, 3, 2, '2024-01-17 14:00:00', 'Боль в груди, одышка при физической нагрузке', 'Артериальная гипертензия I степени', 'Диета с ограничением соли, мониторинг АД, амлодипин 5мг 1 раз в день', 'completed'),
(4, 4, 1, '2024-01-18 11:15:00', 'Головокружение, шум в ушах, периодические головные боли', 'Вегетососудистая дистония', 'Контроль режима дня, легкие седативные препараты, консультация невролога', 'scheduled'),
(5, 1, 2, '2024-01-19 13:45:00', 'Кашель с мокротой, боль в горле, насморк', 'Острый бронхит', 'Антибиотикотерапия, отхаркивающие средства, ингаляции', 'completed'),
(6, 5, 1, CURRENT_DATE + INTERVAL '1 day' + INTERVAL '15:00', 'Ухудшение зрения, боли при чтении', 'Миопия средней степени', 'Подбор очков, гимнастика для глаз, контроль зрения через 6 месяцев', 'scheduled'),
(7, 3, 2, CURRENT_DATE + INTERVAL '2 days' + INTERVAL '10:30', 'Перебои в работе сердца, сердцебиение', 'Экстрасистолия', 'Холтер-мониторирование ЭКГ, ограничение кофеина', 'scheduled'),
(8, 2, 1, CURRENT_DATE + INTERVAL '3 days' + INTERVAL '16:20', 'Боль в пояснице после физической нагрузки', 'Остеохондроз поясничного отдела', 'ЛФК, физиотерапия, противовоспалительные мази', 'scheduled');

-- Медицинские процедуры
INSERT INTO medical_procedures (appointment_id, procedure_name, procedure_result, procedure_date, doctor_id) VALUES
(1, 'Измерение температуры тела', '37.8°C', '2024-01-15', 1),
(1, 'Измерение артериального давления', '120/80 мм рт.ст.', '2024-01-15', 1),
(2, 'Рентгенография правого коленного сустава', 'Признаки травматического артрита, остеофиты', '2024-01-16', 2),
(3, 'Электрокардиография (ЭКГ)', 'Синусовый ритм, 75 уд/мин, признаки гипертрофии левого желудочка', '2024-01-17', 3),
(3, 'Измерение артериального давления', '145/95 мм рт.ст.', '2024-01-17', 3),
(4, 'Неврологический осмотр', 'Нистагм, легкая атаксия', '2024-01-18', 4),
(5, 'Аускультация легких', 'Жесткое дыхание, рассеянные хрипы', '2024-01-19', 1),
(6, 'Проверка остроты зрения', 'OD: -2.5D, OS: -2.75D', CURRENT_DATE, 5),
(6, 'Осмотр глазного дна', 'Периферическая дегенерация сетчатки', CURRENT_DATE, 5),
(7, 'Холтер-мониторирование ЭКГ', 'Назначено на следующую неделю', CURRENT_DATE, 3);

-- 8. Создаем триггеры
CREATE OR REPLACE FUNCTION prevent_weekend_appointments()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXTRACT(DOW FROM NEW.appointment_date) IN (0, 6) THEN
        RAISE EXCEPTION 'Запрещено создавать записи на прием в выходные дни (суббота и воскресенье). Указана дата: %',
        TO_CHAR(NEW.appointment_date, 'DD.MM.YYYY');
    END IF;
    RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER before_insert_appointment
BEFORE INSERT ON appointments
FOR EACH ROW
EXECUTE FUNCTION prevent_weekend_appointments();

CREATE OR REPLACE FUNCTION after_delete_appointment()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE NOTICE 'Удалена запись на прием с ID: %. Дата приема: %, Пациент ID: %, Врач ID: %',
    OLD.appointment_id,
    OLD.appointment_date,
    OLD.patient_id,
    OLD.doctor_id;
    RETURN OLD;
END;
$$;

CREATE OR REPLACE TRIGGER after_delete_appointment
AFTER DELETE ON appointments
FOR EACH ROW
EXECUTE FUNCTION after_delete_appointment();