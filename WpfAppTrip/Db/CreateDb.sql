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
    Season NVARCHAR(50) CHECK (Season IN ('Лето', 'Зима', 'Весна', 'Осень', 'Всесезонный')),
    Difficulty NVARCHAR(20) CHECK (Difficulty IN ('Легкий', 'Средний', 'Сложный')),
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
    Status NVARCHAR(20) CHECK (Status IN ('Ожидание', 'Подтверждено', 'Отменено'))
)
GO

-- Добавление тестовых данных: Вопросы
INSERT INTO Questions (QuestionText, OrderNumber) VALUES 
('Какой тип отдыха Вас больше привлекает ?', 1),
('С кем вы планируете отправиться в тур ?', 2),
('Какой бюджет у Вас запланирован на тур?', 3),
('Какие выды транспорта вы рассматриваете для путешествия?', 4),
('В какое время года вы планируете свой отдых?', 5),
('Какие условия по проживанию?', 6),
('На какой уровень в отеле вы ориентируетесь?', 7),
('Предпочитаете ли Вы путешествовать самостоятельно или предпочитаете групповые туры с лидом?', 8),
('Какое количество времени вы готовы уделить путешествию?', 9),
('Планируете ли вы заниматься какой-либо спортивной или экстремальной деятельностью? во время путешествий', 10)
GO

-- Добавление тестовых данных: Варианты ответов (пример для первого вопроса)
INSERT INTO AnswerOptions (QuestionID, OptionText, Weight) VALUES 
(1, 'пляжный отдых.', 1),
(1, 'тур поход', 1),
(1, 'экскурсии.', 1),
(2, 'Один.', 1),
(2, 'Семьей', 1),
(2, 'Тургруппа', 2),
(4, 'Самолет', 3),
(4, 'Поезд', 4),
(4, 'Автомобиль', 4),
(5, 'Лето', 2),
(5, 'Зима', 2),
(5, 'Осень', 2),
(6, 'Отель', 2),
(6, 'Дом', 2),
(6, 'Палатка', 2),
(8, 'Самостоятельно', 2),
(8, 'с Экскурсиями', 2),
(8, 'с Экскурсиями', 2),
(10, 'серфинг', 2),
(10, 'скалолазаниие', 2),
(10, 'плавание', 2)
GO

-- Добавление тестовых данных: Категории туров
INSERT INTO TourCategories (CategoryName, Description) VALUES 
('Пляжный отдых', 'Отдых на море'),
('Экскурсионный', 'Культурно-познавательные туры'),
('Активный', 'Спортивные и приключенческие туры'),
('Горнолыжный', 'Зимний отдых в горах')
GO

-- Добавление администратора
INSERT INTO Users (Username, Password, Email, Role) VALUES 
('admin', 'hashed_password_here', 'admin@example.com', 'Admin')
GO