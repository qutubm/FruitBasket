using FruitBasket.Models;
using FruitBasket.PricingStrategy;

var basket = new Basket(timeProvider : TimeProvider.System);
basket.Add(new Fruit { Name = "Apple", BasePrice = 2, Qty = 5, PricingModel = PricingMethodEnum.PerKg });
basket.Add(new Fruit { Name = "Pineapple", BasePrice = 1.5M, Qty = 15M, PricingModel = PricingMethodEnum.PerItem });
basket.Add(new Fruit { Name = "Grapes", BasePrice = 3M, Qty = 4, PricingModel = PricingMethodEnum.PerKg });

Console.WriteLine($"Total amount of the basket is {basket.TotalBill}");


//namespace FruitBasket
//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Hello, World!");
//        }
//    }
//}
