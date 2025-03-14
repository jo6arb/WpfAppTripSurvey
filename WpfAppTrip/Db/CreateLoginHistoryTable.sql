-- Создание таблицы для хранения истории входов пользователей
CREATE TABLE LoginHistory (
    LoginID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    LoginDate DATETIME NOT NULL DEFAULT GETDATE(),
    ComputerName NVARCHAR(100) NULL,
    UserWindowsName NVARCHAR(100) NULL,
    CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Индекс для ускорения поиска по UserID
CREATE INDEX IX_LoginHistory_UserID ON LoginHistory(UserID);

-- Индекс для ускорения поиска по дате входа
CREATE INDEX IX_LoginHistory_LoginDate ON LoginHistory(LoginDate);

PRINT 'Таблица LoginHistory успешно создана'; 