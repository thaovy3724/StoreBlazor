namespace StoreBlazor.DTO.Client
{
    public class CheckOutDTO
    {
        public int? CustomerId { get; set; }
        public int? UserId { get; set; }

        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string PaymentMethod { get; set; } 

        public int? PromoId { get; set; }
    }
}
