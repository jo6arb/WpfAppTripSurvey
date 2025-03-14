-- Скрипт для обновления таблицы Users
USE TourSurveyDB
GO

-- Проверка наличия колонок
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'LastName')
BEGIN
    ALTER TABLE Users ADD LastName NVARCHAR(100) NULL
    PRINT 'Колонка LastName добавлена'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'FirstName')
BEGIN
    ALTER TABLE Users ADD FirstName NVARCHAR(100) NULL
    PRINT 'Колонка FirstName добавлена'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'MiddleName')
BEGIN
    ALTER TABLE Users ADD MiddleName NVARCHAR(100) NULL
    PRINT 'Колонка MiddleName добавлена'
END

-- Обновление существующих записей
UPDATE Users
SET LastName = CASE 
                WHEN CHARINDEX(' ', Username) > 0 
                THEN LEFT(Username, CHARINDEX(' ', Username) - 1) 
                ELSE Username 
              END,
    FirstName = CASE 
                 WHEN CHARINDEX(' ', Username) > 0 
                 THEN SUBSTRING(Username, 
                               CHARINDEX(' ', Username) + 1, 
                               CASE 
                                 WHEN CHARINDEX(' ', Username, CHARINDEX(' ', Username) + 1) > 0 
                                 THEN CHARINDEX(' ', Username, CHARINDEX(' ', Username) + 1) - CHARINDEX(' ', Username) - 1
                                 ELSE LEN(Username) - CHARINDEX(' ', Username)
                               END)
                 ELSE ''
               END,
    MiddleName = CASE 
                  WHEN CHARINDEX(' ', Username, CHARINDEX(' ', Username) + 1) > 0 
                  THEN SUBSTRING(Username, 
                                CHARINDEX(' ', Username, CHARINDEX(' ', Username) + 1) + 1, 
                                LEN(Username) - CHARINDEX(' ', Username, CHARINDEX(' ', Username) + 1))
                  ELSE NULL
                END
WHERE LastName IS NULL OR FirstName IS NULL

-- Изменение колонок на NOT NULL
ALTER TABLE Users ALTER COLUMN LastName NVARCHAR(100) NOT NULL
ALTER TABLE Users ALTER COLUMN FirstName NVARCHAR(100) NOT NULL

-- Изменение колонки Phone на NOT NULL и добавление UNIQUE
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Phone')
BEGIN
    -- Обновление NULL значений в Phone
    UPDATE Users SET Phone = CONCAT('+7', CAST(CAST(RAND() * 9000000000 + 1000000000 AS BIGINT) AS VARCHAR(10)))
    WHERE Phone IS NULL
    
    -- Изменение колонки Phone
    ALTER TABLE Users ALTER COLUMN Phone NVARCHAR(20) NOT NULL
    
    -- Добавление UNIQUE constraint
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_Phone' AND object_id = OBJECT_ID('Users'))
    BEGIN
        ALTER TABLE Users ADD CONSTRAINT UQ_Users_Phone UNIQUE (Phone)
        PRINT 'UNIQUE constraint для Phone добавлен'
    END
END

PRINT 'Обновление таблицы Users завершено' 