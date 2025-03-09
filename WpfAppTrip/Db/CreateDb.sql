-- Создание базы данных
CREATE DATABASE TourSurveyDB
GO

USE TourSurveyDB
GO

-- Таблица пользователей
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Role NVARCHAR(20) DEFAULT 'User' CHECK (Role IN ('User', 'Admin')),
    RegistrationDate DATETIME DEFAULT GETDATE()
)
GO

-- Таблица вопросов
CREATE TABLE Questions (
    QuestionID INT PRIMARY KEY IDENTITY(1,1),
    QuestionText NVARCHAR(500) NOT NULL,
    ImagePath NVARCHAR(200),
    OrderNumber INT NOT NULL
)
GO

-- Таблица вариантов ответов
CREATE TABLE AnswerOptions (
    OptionID INT PRIMARY KEY IDENTITY(1,1),
    QuestionID INT FOREIGN KEY REFERENCES Questions(QuestionID),
    OptionText NVARCHAR(200) NOT NULL,
    ImagePath NVARCHAR(200),
    Weight INT -- вес для подбора тура
)
GO

-- Таблица ответов пользователей
CREATE TABLE UserAnswers (
    AnswerID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    QuestionID INT FOREIGN KEY REFERENCES Questions(QuestionID),
    SelectedOptionID INT FOREIGN KEY REFERENCES AnswerOptions(OptionID),
    AnswerDate DATETIME DEFAULT GETDATE()
)
GO

-- Таблица категорий туров
CREATE TABLE TourCategories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200)
)
GO

-- Таблица туров
CREATE TABLE Tours (
    TourID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    CategoryID INT FOREIGN KEY REFERENCES TourCategories(CategoryID),
    Price DECIMAL(10,2) NOT NULL,
    Duration INT NOT NULL, -- количество дней
    ImagePath NVARCHAR(200),
    Season NVARCHAR(50) CHECK (Season IN (N'Лето', N'Зима', N'Весна', N'Осень', N'Всесезонный')),
    Difficulty NVARCHAR(20) CHECK (Difficulty IN (N'Легкий', N'Средний', N'Сложный')),
    MaxGroupSize INT,
    IsActive BIT DEFAULT 1
)
GO

-- Таблица характеристик тура
CREATE TABLE TourCharacteristics (
    CharacteristicID INT PRIMARY KEY IDENTITY(1,1),
    TourID INT FOREIGN KEY REFERENCES Tours(TourID),
    BudgetLevel INT, -- 1-5
    ActivityLevel INT, -- 1-5
    ComfortLevel INT, -- 1-5
    CulturalFocus INT, -- 1-5
    NatureFocus INT -- 1-5
)
GO

-- Таблица подобранных туров
CREATE TABLE MatchedTours (
    MatchID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    TourID INT FOREIGN KEY REFERENCES Tours(TourID),
    MatchDate DATETIME DEFAULT GETDATE(),
    MatchScore DECIMAL(5,2), -- процент совпадения
    IsBooked BIT DEFAULT 0
)
GO

-- Таблица бронирований
CREATE TABLE Bookings (
    BookingID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    TourID INT FOREIGN KEY REFERENCES Tours(TourID),
    BookingDate DATETIME DEFAULT GETDATE(),
    TravelDate DATE NOT NULL,
    NumberOfPeople INT NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN (N'Ожидание', N'Подтверждено', N'Отменено'))
)
GO

-- Добавление тестовых данных: Вопросы
INSERT INTO Questions (QuestionText, OrderNumber) VALUES 
(N'Какой тип отдыха Вас больше привлекает ?', 1),
(N'С кем вы планируете отправиться в тур ?', 2),
(N'Какой бюджет у Вас запланирован на тур?', 3),
(N'Какие выды транспорта вы рассматриваете для путешествия?', 4),
(N'В какое время года вы планируете свой отдых?', 5),
(N'Какие условия по проживанию?', 6),
(N'На какой уровень в отеле вы ориентируетесь?', 7),
(N'Предпочитаете ли Вы путешествовать самостоятельно или предпочитаете групповые туры с лидом?', 8),
(N'Какое количество времени вы готовы уделить путешествию?', 9),
(N'Планируете ли вы заниматься какой-либо спортивной или экстремальной деятельностью? во время путешествий', 10)
GO

-- Добавление тестовых данных: Варианты ответов (пример для первого вопроса)
INSERT INTO AnswerOptions (QuestionID, OptionText, ImagePath, Weight) VALUES 
-- Вопрос 1: Тип отдыха
(1, N'Пляжный отдых', N'Images/Quest1/1.jpg', 1),
(1, N'Тур-поход', N'Images/Quest1/2.jpg', 2),
(1, N'Экскурсии', N'Images/Quest1/3.jpg', 3),

-- Вопрос 2: С кем планируете
(2, N'Один', N'Images/Quest2/1.jpg', 1),
(2, N'Семьей', N'Images/Quest2/2.jpg', 2),
(2, N'Тургруппа', N'Images/Quest2/3.jpg', 3),

-- Вопрос 3: Бюджет (без картинок)
(3, N'До 30 000 руб.', NULL, 1),
(3, N'30 000 - 70 000 руб.', NULL, 2),
(3, N'Более 70 000 руб.', NULL, 3),

-- Вопрос 4: Транспорт
(4, N'Самолет', N'Images/Quest4/1.jpg', 3),
(4, N'Поезд', N'Images/Quest4/2.jpg', 2),
(4, N'Автомобиль', N'Images/Quest4/3.jpg', 1),

-- Вопрос 5: Время года
(5, N'Лето', N'Images/Quest5/1.jpg', 1),
(5, N'Зима', N'Images/Quest5/2.jpg', 2),
(5, N'Осень/Весна', N'Images/Quest5/3.jpg', 3),

-- Вопрос 6: Проживание
(6, N'Отель', N'Images/Quest6/1.jpg', 3),
(6, N'Дом/Апартаменты', N'Images/Quest6/2.jpg', 2),
(6, N'Палатка/Хостел', N'Images/Quest6/3.jpg', 1),

-- Вопрос 7: Уровень отеля
(7, N'Эконом', N'Images/Quest7/1.jpg', 1),
(7, N'Стандарт', N'Images/Quest7/2.jpg', 2),
(7, N'Люкс', N'Images/Quest7/3.jpg', 3),

-- Вопрос 8: Самостоятельно или с гидом (без картинок)
(8, N'Самостоятельно', NULL, 1),
(8, N'С гидом', NULL, 2),
(8, N'Комбинированно', NULL, 3),

-- Вопрос 9: Продолжительность (без картинок)
(9, N'До 7 дней', NULL, 1),
(9, N'7-14 дней', NULL, 2),
(9, N'Более 14 дней', NULL, 3),

-- Вопрос 10: Спортивная активность
(10, N'Серфинг/Водные виды', N'Images/Quest10/1.jpg', 1),
(10, N'Скалолазание/Горные виды', N'Images/Quest10/2.jpg', 2),
(10, N'Без активностей', N'Images/Quest10/3.jpg', 3)
GO


-- Добавление тестовых данных: Категории туров
INSERT INTO TourCategories (CategoryName, Description) VALUES 
(N'Пляжный отдых', N'Отдых на море'),
(N'Экскурсионный', N'Культурно-познавательные туры'),
(N'Активный', N'Спортивные и приключенческие туры'),
(N'Горнолыжный', N'Зимний отдых в горах')
GO

-- Добавление администратора
INSERT INTO Users (Username, Password, Email, Role) VALUES 
('admin', 'hashed_password_here', 'admin@example.com', 'Admin')
GO