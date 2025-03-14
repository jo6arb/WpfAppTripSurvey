using System;
using System.IO;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WpfAppTrip.Models;
using System.Windows;
using System.Text;

namespace WpfAppTrip.Services
{
    public interface IPdfTicketService
    {
        /// <summary>
        /// Создает PDF-билет и сохраняет его в указанном месте
        /// </summary>
        /// <param name="ticketData">Данные для билета</param>
        /// <returns>Путь к сохраненному файлу</returns>
        string GenerateTicket(TicketData ticketData);
    }

    public class PdfTicketService : IPdfTicketService
    {
        private readonly string _baseDirectory;
        private readonly BaseFont _baseFont;

        public PdfTicketService()
        {
            // Создаем директорию для билетов в папке Documents пользователя
            _baseDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "TripSurveyTickets");
            
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
            
            // Инициализируем базовый шрифт с поддержкой кириллицы
            try
            {
                _baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\Arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при инициализации шрифта: {ex.Message}");
                // Используем стандартный шрифт, если не удалось загрузить Arial
                _baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            }
        }

        public string GenerateTicket(TicketData ticketData)
        {
            try
            {
                // Обработка значений из ComboBox
                string passengerGender = ticketData.PassengerGender;
                string documentType = ticketData.DocumentType;
                
                // Извлекаем только текстовое значение из ComboBoxItem
                if (passengerGender != null && passengerGender.Contains("System.Windows.Controls.ComboBoxItem:"))
                {
                    passengerGender = passengerGender.Split(':')[1].Trim();
                }
                
                if (documentType != null && documentType.Contains("System.Windows.Controls.ComboBoxItem:"))
                {
                    documentType = documentType.Split(':')[1].Trim();
                }
                
                // Генерируем уникальное имя файла
                string fileName = $"Ticket_{ticketData.PassengerLastName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(_baseDirectory, fileName);

                // Создаем документ
                using (Document document = new Document(PageSize.A4, 50, 50, 50, 50))
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                    document.Open();
                    
                    // Создаем шрифты с поддержкой кириллицы
                    Font titleFont = new Font(_baseFont, 24, Font.BOLD, new BaseColor(41, 128, 185)); // Синий
                    Font sectionFont = new Font(_baseFont, 16, Font.BOLD, new BaseColor(52, 73, 94)); // Темно-серый
                    Font labelFont = new Font(_baseFont, 11, Font.BOLD, new BaseColor(44, 62, 80)); // Почти черный
                    Font valueFont = new Font(_baseFont, 11, Font.NORMAL, BaseColor.BLACK);
                    Font noteFont = new Font(_baseFont, 9, Font.ITALIC, new BaseColor(149, 165, 166)); // Светло-серый

                    // Добавляем верхний колонтитул с логотипом и заголовком
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 2, 1 });
                    
                    // Ячейка с заголовком
                    PdfPCell titleCell = new PdfPCell();
                    titleCell.Border = Rectangle.NO_BORDER;
                    titleCell.BackgroundColor = new BaseColor(236, 240, 241); // Светло-серый фон
                    titleCell.PaddingTop = 20;
                    titleCell.PaddingBottom = 20;
                    titleCell.PaddingLeft = 20;
                    
                    Paragraph titleParagraph = new Paragraph();
                    titleParagraph.Add(new Chunk("ЭЛЕКТРОННЫЙ БИЛЕТ", titleFont));
                    titleParagraph.Add(Chunk.NEWLINE);
                    titleParagraph.Add(new Chunk($"№ {ticketData.TicketNumber ?? GenerateTicketNumber()}", new Font(_baseFont, 14, Font.BOLD, new BaseColor(231, 76, 60)))); // Красный
                    titleCell.AddElement(titleParagraph);
                    headerTable.AddCell(titleCell);
                    
                    // Ячейка с логотипом
                    PdfPCell logoCell = new PdfPCell();
                    logoCell.Border = Rectangle.NO_BORDER;
                    logoCell.BackgroundColor = new BaseColor(236, 240, 241); // Светло-серый фон
                    logoCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    
                    try
                    {
                        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "logo.jpeg");
                        if (File.Exists(logoPath))
                        {
                            Image logo = Image.GetInstance(logoPath);
                            logo.ScaleToFit(100, 100);
                            logoCell.AddElement(logo);
                        }
                        else
                        {
                            // Если логотип не найден, добавляем текст
                            Paragraph logoText = new Paragraph("TRIP SURVEY", new Font(_baseFont, 16, Font.BOLD, new BaseColor(52, 152, 219)));
                            logoCell.AddElement(logoText);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при добавлении логотипа: {ex.Message}");
                        // Если произошла ошибка, добавляем текст
                        Paragraph logoText = new Paragraph("TRIP SURVEY", new Font(_baseFont, 16, Font.BOLD, new BaseColor(52, 152, 219)));
                        logoCell.AddElement(logoText);
                    }
                    
                    headerTable.AddCell(logoCell);
                    document.Add(headerTable);
                    
                    // Добавляем информацию о маршруте в красивой рамке
                    PdfPTable routeBoxTable = new PdfPTable(1);
                    routeBoxTable.WidthPercentage = 100;
                    routeBoxTable.SpacingBefore = 20;
                    
                    PdfPCell routeBoxCell = new PdfPCell();
                    routeBoxCell.BackgroundColor = new BaseColor(235, 245, 251); // Светло-голубой
                    routeBoxCell.BorderColor = new BaseColor(52, 152, 219); // Синий
                    routeBoxCell.BorderWidth = 2f;
                    routeBoxCell.Padding = 10;
                    
                    Paragraph routeInfo = new Paragraph();
                    routeInfo.Add(new Chunk("МАРШРУТ: ", new Font(_baseFont, 14, Font.BOLD, new BaseColor(41, 128, 185))));
                    routeInfo.Add(new Chunk(ticketData.RouteInfo, new Font(_baseFont, 14, Font.NORMAL, BaseColor.BLACK)));
                    routeInfo.Add(Chunk.NEWLINE);
                    routeInfo.Add(new Chunk("ДАТА: ", new Font(_baseFont, 12, Font.BOLD, new BaseColor(41, 128, 185))));
                    routeInfo.Add(new Chunk(DateTime.Now.ToString("dd.MM.yyyy"), new Font(_baseFont, 12, Font.NORMAL, BaseColor.BLACK)));
                    routeInfo.Add(Chunk.NEWLINE);
                    routeInfo.Add(new Chunk("КЛАСС: ", new Font(_baseFont, 12, Font.BOLD, new BaseColor(41, 128, 185))));
                    routeInfo.Add(new Chunk(ticketData.ClassInfo ?? "Эконом", new Font(_baseFont, 12, Font.NORMAL, BaseColor.BLACK)));
                    
                    routeBoxCell.AddElement(routeInfo);
                    routeBoxTable.AddCell(routeBoxCell);
                    document.Add(routeBoxTable);

                    // Добавляем информацию о пассажире
                    Paragraph passengerSection = new Paragraph("ИНФОРМАЦИЯ О ПАССАЖИРЕ", sectionFont);
                    passengerSection.SpacingBefore = 20;
                    passengerSection.SpacingAfter = 10;
                    document.Add(passengerSection);

                    // Создаем таблицу для информации о пассажире
                    PdfPTable passengerTable = new PdfPTable(2);
                    passengerTable.WidthPercentage = 100;
                    passengerTable.SetWidths(new float[] { 1, 3 });

                    AddTableRow(passengerTable, "Фамилия:", ticketData.PassengerLastName, labelFont, valueFont);
                    AddTableRow(passengerTable, "Имя:", ticketData.PassengerFirstName, labelFont, valueFont);
                    AddTableRow(passengerTable, "Пол:", passengerGender, labelFont, valueFont);
                    AddTableRow(passengerTable, "Дата рождения:", ticketData.PassengerBirthDate?.ToString("dd.MM.yyyy") ?? "", labelFont, valueFont);
                    AddTableRow(passengerTable, "Документ:", documentType, labelFont, valueFont);
                    AddTableRow(passengerTable, "Номер документа:", ticketData.DocumentNumber, labelFont, valueFont);
                    document.Add(passengerTable);

                    // Добавляем информацию о покупателе
                    Paragraph buyerSection = new Paragraph("ИНФОРМАЦИЯ О ПОКУПАТЕЛЕ", sectionFont);
                    buyerSection.SpacingBefore = 20;
                    buyerSection.SpacingAfter = 10;
                    document.Add(buyerSection);

                    // Создаем таблицу для информации о покупателе
                    PdfPTable buyerTable = new PdfPTable(2);
                    buyerTable.WidthPercentage = 100;
                    buyerTable.SetWidths(new float[] { 1, 3 });

                    AddTableRow(buyerTable, "Фамилия:", ticketData.BuyerLastName, labelFont, valueFont);
                    AddTableRow(buyerTable, "Имя:", ticketData.BuyerFirstName, labelFont, valueFont);
                    AddTableRow(buyerTable, "Email:", ticketData.BuyerEmail, labelFont, valueFont);
                    AddTableRow(buyerTable, "Телефон:", ticketData.BuyerPhone, labelFont, valueFont);
                    document.Add(buyerTable);

                    // Добавляем информацию о стоимости в выделенном блоке
                    PdfPTable priceBoxTable = new PdfPTable(1);
                    priceBoxTable.WidthPercentage = 100;
                    priceBoxTable.SpacingBefore = 20;
                    
                    PdfPCell priceBoxCell = new PdfPCell();
                    priceBoxCell.BackgroundColor = new BaseColor(235, 245, 251); // Светло-голубой
                    priceBoxCell.BorderColor = new BaseColor(52, 152, 219); // Синий
                    priceBoxCell.BorderWidth = 2f;
                    priceBoxCell.Padding = 10;
                    
                    Paragraph priceInfo = new Paragraph();
                    priceInfo.Add(new Chunk("СТОИМОСТЬ: ", new Font(_baseFont, 14, Font.BOLD, new BaseColor(41, 128, 185))));
                    priceInfo.Add(new Chunk($"{ticketData.TotalPrice:N0}₽", new Font(_baseFont, 14, Font.BOLD, new BaseColor(231, 76, 60)))); // Красный
                    
                    priceBoxCell.AddElement(priceInfo);
                    priceBoxTable.AddCell(priceBoxCell);
                    document.Add(priceBoxTable);

                    // Добавляем QR-код (имитация)
                    try
                    {
                        // Создаем простую таблицу для имитации QR-кода
                        PdfPTable qrTable = new PdfPTable(1);
                        qrTable.WidthPercentage = 30;
                        qrTable.HorizontalAlignment = Element.ALIGN_CENTER;
                        qrTable.SpacingBefore = 20;

                        PdfPCell qrCell = new PdfPCell(new Phrase("QR-код билета", new Font(_baseFont, 10, Font.NORMAL, BaseColor.BLACK)));
                        qrCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        qrCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        qrCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        qrCell.FixedHeight = 100;
                        qrCell.Border = Rectangle.BOX;
                        qrTable.AddCell(qrCell);

                        document.Add(qrTable);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при добавлении QR-кода: {ex.Message}");
                    }

                    // Добавляем примечание
                    Paragraph note = new Paragraph("Этот документ является электронным билетом. " +
                        "Для посадки необходимо предъявить распечатанный билет или его электронную версию вместе с документом, удостоверяющим личность.", noteFont);
                    note.SpacingBefore = 30;
                    note.Alignment = Element.ALIGN_CENTER;
                    document.Add(note);

                    // Добавляем нижний колонтитул
                    PdfPTable footerTable = new PdfPTable(1);
                    footerTable.WidthPercentage = 100;
                    footerTable.SpacingBefore = 20;
                    
                    PdfPCell footerCell = new PdfPCell();
                    footerCell.Border = Rectangle.ALIGN_TOP;
                    footerCell.BorderColor = new BaseColor(189, 195, 199); // Светло-серый
                    footerCell.BorderWidth = 1f;
                    footerCell.Padding = 10;
                    
                    Paragraph footerText = new Paragraph("© TRIP SURVEY " + DateTime.Now.Year.ToString(), 
                        new Font(_baseFont, 8, Font.NORMAL, new BaseColor(149, 165, 166)));
                    footerText.Alignment = Element.ALIGN_CENTER;
                    
                    footerCell.AddElement(footerText);
                    footerTable.AddCell(footerCell);
                    document.Add(footerTable);

                    document.Close();
                }

                Debug.WriteLine($"Билет успешно создан: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при создании билета: {ex.Message}");
                MessageBox.Show($"Ошибка при создании билета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void AddTableRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.BackgroundColor = new BaseColor(236, 240, 241); // Светло-серый
            labelCell.Border = Rectangle.BOX;
            labelCell.BorderColor = new BaseColor(189, 195, 199); // Серый
            labelCell.Padding = 8;
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value ?? "", valueFont));
            valueCell.Border = Rectangle.BOX;
            valueCell.BorderColor = new BaseColor(189, 195, 199); // Серый
            valueCell.Padding = 8;
            table.AddCell(valueCell);
        }

        private string GenerateTicketNumber()
        {
            // Генерируем случайный номер билета
            Random random = new Random();
            return $"TKT{random.Next(100000, 999999)}";
        }
    }
} 