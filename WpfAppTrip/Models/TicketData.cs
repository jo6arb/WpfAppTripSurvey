using System;

namespace WpfAppTrip.Models
{
    /// <summary>
    /// Модель данных для билета
    /// </summary>
    public class TicketData
    {
        // Информация о маршруте
        public string RouteInfo { get; set; }
        public string ClassInfo { get; set; }
        
        // Информация о пассажире
        public string PassengerLastName { get; set; }
        public string PassengerFirstName { get; set; }
        public string PassengerGender { get; set; }
        public DateTime? PassengerBirthDate { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentExpiryDate { get; set; }
        
        // Информация о покупателе
        public string BuyerLastName { get; set; }
        public string BuyerFirstName { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
        
        // Информация о стоимости
        public decimal TotalPrice { get; set; }
        
        // Информация о билете
        public string TicketNumber { get; set; }
        public string TicketFilePath { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
} 