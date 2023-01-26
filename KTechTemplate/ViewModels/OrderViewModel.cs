using KTechTemplateDAL.Models;

namespace KTechTemplate.ViewModels
{
    public class OrderViewModel
    {
        public OrderHeader orderHeader { get; set; }
        public IEnumerable<OrderDetail> orderDetail { get; set; }
    }
}
