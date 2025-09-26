using iFood.Models;
using System.Collections.Generic;

namespace iFood.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
    }
}
