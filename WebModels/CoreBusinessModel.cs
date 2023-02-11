using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebModels
{
    [Table("BuyAndSellBonds")]
    public partial class BuyAndSellBond
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Ngày mua")]
        public DateTime? PurchaseDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        [Display(Name = "Khách hàng")]
        public int? CustomerId { get; set; }

        [Display(Name = "Loại tài sản")]
        [Required(ErrorMessage = "Vui lòng chọn Loại tài sản")]
        public int? AssetTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mã trái phiếu")]
        [Display(Name = "Mã trái phiếu")]
        public int? AssetCategorysId { get; set; }

        [Display(Name = "Nguồn tiền")]
        public int? FundId { get; set; }

        [Display(Name = "Thời gian được chuyển nhượng")]
        public DateTime? TransferredTime { get; set; }

        [Required(ErrorMessage = "Vui lòng không để trống tên hợp đồng")]
        [Display(Name = "Tên hợp đồng")]
        public string ContractName { get; set; }

        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantily { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị")]
        [Display(Name = "Giá trị")]
        public long? Value { get; set; }

        [Display(Name = "Lãi suất cố định")]
        public Boolean FixedInterestRate { get; set; }

        [Display(Name = "Kỳ trả lãi")]
        public Double? InterestPayPeriod { get; set; }

        [Display(Name = "Lãi suất")]
        public Double? InterestRate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập kỳ hạn")]
        [Display(Name = "Kỳ hạn (Tháng)")]
        public Double? Period { get; set; }

        [Display(Name = "Ngày mua + kỳ hạn")]
        public DateTime? PeriodDate { get; set; }
        public int? BuyAndSellBondId { get; set; }

        [Required(ErrorMessage = "Vui lòng không để trống mã hợp đồng")]
        public string ContractCode { get; set; }

        [Display(Name = "Lãi suất đầu vào")]
        public Double? InputInterestRate { get; set; }

        [Display(Name = "Ngày mua sơ cấp")]
        public DateTime? PrimaryPurchaseDate { get; set; }

        [Display(Name = "Ngày mua thứ cấp")]
        public DateTime? SecondaryPurchaseDate { get; set; }

        [Display(Name = "Tiền lãi đầu tư")]
        public Double? InvestmentReturn { get; set; }

        [Display(Name = "Tỷ suất lãi suất")]
        public Double? Rates { get; set; }

        [Display(Name = "% Thuế thu nhập cá nhân")]
        public Double? PersonalIncomeTax { get; set; }

        [Display(Name = "Lợi tức trước thuế")]
        public Double? PreTaxProfit { get; set; }

        [Display(Name = "Mệnh giá")]
        public Double? Denominations { get; set; }

        [Display(Name = "Lợi tức sau thuế")]
        public Double? ProfitAfterTax { get; set; }

        [Display(Name = "% Phí quản lý tài sản")]
        public Double? PropertyManagementFees { get; set; }

        [Display(Name = "Lợi tức quản lý tài sản")]
        public Double? WealthManageBenefits { get; set; }

        [Display(Name = "Tiền lãi net")]
        public Double? NetInterest { get; set; }

        [Display(Name = "Lợi tức lãi suất IN/OUT")]
        public Double? InterestRateInOut { get; set; }

        [Display(Name = "Ngày phát hành trái phiếu")]
        public DateTime? BondIssueDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
        [Display(Name = "Đơn giá")]
        public Double? UnitPrice { get; set; }

        [Display(Name = "Tiền thuế TNCN")]
        public Double? PersonalIncomeTaxCalculation { get; set; }

        [Display(Name = "Giá trị khi mua làm tròn")]
        public Double? RoundedPurchaseValue { get; set; }

        [Display(Name = "Tổng giá trị khi bán")]
        public Double? TotalValueSold { get; set; }

        [Display(Name = "Giá trị thực nhận")]
        public Double? RealValueReceived { get; set; }


        public Boolean? IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedAt { get; set; }

        [ForeignKey("CustomerId")]
        public virtual UserProfile Customer { get; set; }

        [ForeignKey("AssetTypeId")]
        public virtual TypeOfAsset TypeOfAsset { get; set; }

        [ForeignKey("AssetCategorysId")]
        public virtual AssetCategory AssetCategorys { get; set; }

    }


    [Table("AssetCategorys")]
    public partial class AssetCategory
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã trái phiếu/Tài sản")]
        [Display(Name = "Mã trái phiếu/Tài sản")]
        public string AssetCode { get; set; }

        //[Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        [Display(Name = "Tên trái phiếu/tài sản/tổ chức phát hành")]
        public string Name { get; set; }

        [Display(Name = "Mệnh giá (tiền/trái phiếu)")]
        public long? Price { get; set; }

        [Display(Name = "Ngày phát hành")]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "Tổ chức giao dịch")]
        public string TradeOrganization { get; set; }

        [Display(Name = "Hình thức bảo đảm tài sản")]
        public string PropertySecurity { get; set; }

        [Display(Name = "Tổ chức tư vấn và phát hành")]
        public string ConsulAndPublishOrg { get; set; }


        [Display(Name = "Đại lý thanh toán")]
        public string PaymentAgent { get; set; }

        [Display(Name = "Đại lý lưu ký")]
        public string DepositoryAgent { get; set; }

        [Display(Name = "Kỳ trả lãi/số lần trả lãi/coupon")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Vui lòng nhập đúng định dạng số")]
        public int? InterestPayment { get; set; }

        [Display(Name = "Kỳ hạn")]
        [DisplayFormat(DataFormatString = "{0:n1}", ApplyFormatInEditMode = true)]
        public Decimal? Period { get; set; }

        [Display(Name = "Lãi suất")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        //[Column(TypeName ="decimal(18,2)")]
        public Decimal? InterestRate { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        public Boolean? IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedAt { get; set; }
    }

    [Table("ContactDisbursementDetails")]
    public partial class ContactDisbursementDetail
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Vui lòng chọn ngày thực hiện")]
        [Display(Name = "Ngày thực hiện")]
        public DateTime? ImplementationDate { get; set; }

        [Required(ErrorMessage = "Vui lòng không để trống giá trị")]
        [Display(Name = "Giá trị")]
        public long? Value { get; set; }
        public int? BuyAndSellBondId { get; set; }
        public Boolean? IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedAt { get; set; }

        [ForeignKey("BuyAndSellBondId")]
        public virtual BuyAndSellBond BuyAndSellBond { get; set; }
    }

    [Table("InterestPaymentPeriods")]
    public partial class InterestPaymentPeriod
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int Id { get; set; }

        //[Required(ErrorMessage = "Vui lòng không để trống nội dung")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả lãi")]
        [Display(Name = "Ngày trả lãi")]
        public DateTime? InterestPaymentDate { get; set; }

        [Display(Name = "Số ngày tính lãi")]
        public int? CalculateInterestNumber { get; set; }

        [Display(Name = "Lãi dự thu")]
        public long? AccruedInterest { get; set; }

        public int? BuyAndSellBondId { get; set; }
        public Boolean? IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedAt { get; set; }

        [ForeignKey("BuyAndSellBondId")]
        public virtual BuyAndSellBond BuyAndSellBond { get; set; }
    }

    [Table("Periods")]
    public partial class Period
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Giá trị")]
        public long? Value { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        public int? BuyAndSellBondId { get; set; }
        public int? IncurredId { get; set; }
        public Boolean? IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedAt { get; set; }

        [ForeignKey("BuyAndSellBondId")]
        public virtual BuyAndSellBond BuyAndSellBond { get; set; }

        [ForeignKey("IncurredId")]
        public virtual IncurredPurchase Incurred { get; set; }

    }

    [Table("SourceInBuyAndSellBonds")]
    public partial class SourceInBuyAndSellBond
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int? BuyAndSellBondId { get; set; }
        public Boolean? IsActive { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedAt { get; set; }

        [ForeignKey("SourceId")]
        public virtual BuyAndSellBond Source { get; set; }

        [ForeignKey("BuyAndSellBondId")]
        public virtual BuyAndSellBond BuyAndSellBond { get; set; }
    }

    [Table("IncurredPurchases")]
    public partial class IncurredPurchase
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        /*[Required(ErrorMessage = "Vui lòng chọn ngày phát sinh")]*/
        public DateTime? IncurredDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phát sinh")]
        [StringLength(250)]
        public string IncurredName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hợp đồng")]
        public int? BuyAndSellBondId { get; set; }

        public int? ContactDisbursementId { get; set; }

        public int? InterestPaymentPeriodsId { get; set; }

        /* [StringLength(250)]
         public string CustomerName { get; set; }*/

        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]

        public int? CustomerId { get; set; }

        public int? TransactionType { get; set; }

        //[RegularExpression(@"^([1-9]\d*|0)$", ErrorMessage = "Số tiền phải là số!")]
        public long? AmountOfMoney { get; set; }

        [DataType(DataType.Text)]
        public string Note { get; set; }

        public bool? IsActive { get; set; }


        public int? CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        [ForeignKey("ContactDisbursementId")]
        public virtual ContactDisbursementDetail ContactDisbursement { get; set; }

        [ForeignKey("CustomerId")]
        public virtual UserProfile Customer { get; set; }

        [ForeignKey("BuyAndSellBondId")]
        public virtual BuyAndSellBond BuyAndSellBond { get; set; }
    }

    [Table("ExchangeOfAssets")]
    public partial class ExchangeOfAsset
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        public DateTime? IncurredDate { get; set; }


        public int? CustomerId { get; set; }

        public int? BuyAndSellBondId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn người bán")]
        public int? Seller { get; set; }


        public int? Buyer { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tài sản")]
        public int? Asset { get; set; }

        /*[RegularExpression(@"^([1-9]\d*|0)$", ErrorMessage = "Giá trị phải là số!")]*/
        [Required(ErrorMessage = "Vui lòng nhập giá trị")]
        public long? Value { get; set; }

        /* [RegularExpression(@"^([1-9]\d*|0)$", ErrorMessage = "Giá bán phải là số!")]*/
        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        public long? Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá chuyển nhượng")]
        public long? UnitPrice { get; set; }



        //[RegularExpression(@"^([1-9]\d*|0)$", ErrorMessage = "Số lượng phải là số!")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int? Number { get; set; }

        /*[RegularExpression(@"^(((\d{1,3})(,\d{3})*)|(\d+))(.\d+)?$", ErrorMessage = "loi")]*/
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        //[Column(TypeName ="decimal(18,2)")]
        public decimal? Interest { get; set; }

        [DataType(DataType.Text)]
        public string Note { get; set; }

        public bool? IsActive { get; set; }


        public int? CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        [ForeignKey("Seller")]
        public virtual UserProfile SellerUser { get; set; }

        [ForeignKey("Buyer")]
        public virtual UserProfile BuyerUser { get; set; }

        [ForeignKey("Asset")]
        public virtual BuyAndSellBond BuyAndSellBond { get; set; }
    }

    [Table("News")]
    public partial class New
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int Id { get; set; }
        [Display(Name = "Khách hàng")]
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int? CustomerId { get; set; }
        public int? BuyAndSellBondId { get; set; }

        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Vui lòng không bỏ trống tiêu đề")]
        [RegularExpression(@"^.{1,99}$", ErrorMessage = "Vui lòng nhập tiêu đề không quá 99 kí tự")]
        public string Title { get; set; }

        //[Required(ErrorMessage = "Vui lòng không bỏ trống tiêu đề")]
        [Display(Name = "Ảnh bìa")]
        public string Image { get; set; }

        //[Required(ErrorMessage = "Vui lòng không bỏ trống nội dung")]
        [Display(Name = "Nội dung")]
        public string Body { get; set; }

        [Display(Name = "Is Publish")]
        public bool? IsPublish { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        [ForeignKey("CustomerId")]
        public virtual UserProfile Customer { get; set; }

        [ForeignKey("BuyAndSellBondId")]
        public virtual BuyAndSellBond BuyAndSellBond { get; set; }
    }

    [Table("AuditLog")]
    public partial class AuditLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        public string UserName { get; set; }
        public string ActiveType { get; set; }

        public string FunctionName { get; set; }
        public string DataTable { get; set; }
        //[DataType(DataType.DateTime)]
        public string Information { get; set; }

        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }

        //[DataType(DataType.DateTime)]
        public DateTime? CreatedAt { get; set; }
    }

    [Table("LogData")]
    public partial class LogData
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        public DateTime date { get; set; }
        public string sIp { get; set; }
        public string csMethod { get; set; }
        public string csUriStem { get; set; }
        public string csUriQuery { get; set; }
        public string sPort { get; set; }
        public string csUsername { get; set; }
        public string cIp { get; set; }
        public string csVersion { get; set; }
        public string csUserAgent { get; set; }
        public string csReferer { get; set; }
        public string csHost { get; set; }
        public int? scStatus { get; set; }
        public int? scSubstatus { get; set; }
        public long? scWin32Status { get; set; }
        public int? scBytes { get; set; }
        public int? csBytes { get; set; }
        public int? timeTaken { get; set; }
    }

}
