using System.ComponentModel.DataAnnotations;
using WebModels;

namespace WEB.Models
{
    public class ImportModel
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Time { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
    }
}
