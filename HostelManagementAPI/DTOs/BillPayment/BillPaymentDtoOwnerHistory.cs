namespace DTOs.BillPayment
{
    public class BillPaymentDtoOwnerHistory
    {
        public int? BillPaymentID { get; set; }
        public int? ContractId { get; set; }
        public double? BillAmount { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public double? TotalAmount { get; set; }
        public int? BillPaymentStatus { get; set; }
        public int? BillType { get; set; }
        public string? TnxRef { get; set; }
        public List<BillPaymentDetailResponseDto> BillPaymentDetails { get; set; }
        public string? RoomName { get; set; }
        public string? RenterName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? RoomId { get; set; }
        public int? AccountPayId { get; set; }
        public int? AccountReceiveId { get; set; }
        public bool? IsFirstBill { get; set; }
    }
}
