using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitBasket.Models
{
    public class Fruit
    {
        public string Name { get; set; }
        public PricingMethodEnum PricingModel { get; set; }
        public decimal BasePrice { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }
}
