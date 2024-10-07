using Microsoft.AspNetCore.Mvc;
using SalesOrders.Models;
using SalesOrders.Services;

namespace SalesOrders.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;

        public ProductsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var products = context.Products.ToList(); // Retrieve all products without any sorting
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    ProductCode = productDto.ProductCode,
                    ProductType = productDto.ProductType,
                    CostPrice = productDto.CostPrice,
                    SalesPrice = productDto.SalesPrice,
                    StaffDiscountedPrice = productDto.StaffDiscountedPrice,
                    StockQuantity = productDto.StockQuantity,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.Products.Add(product);
                context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(productDto);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var productDto = new ProductDto
            {
                Id = product.Id, // Set the Id here
                Name = product.Name,
                Description = product.Description,
                ProductCode = product.ProductCode,
                ProductType = product.ProductType,
                CostPrice = product.CostPrice,
                SalesPrice = product.SalesPrice,
                StaffDiscountedPrice = product.StaffDiscountedPrice,
                StockQuantity = product.StockQuantity
            };

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                var product = context.Products.Find(productDto.Id); // Use Id from ProductDto
                if (product == null)
                {
                    return NotFound();
                }

                // Update product properties
                product.Name = productDto.Name;
                product.Description = productDto.Description;
                product.ProductCode = productDto.ProductCode;
                product.ProductType = productDto.ProductType;
                product.CostPrice = productDto.CostPrice;
                product.SalesPrice = productDto.SalesPrice;
                product.StaffDiscountedPrice = productDto.StaffDiscountedPrice;
                product.StockQuantity = productDto.StockQuantity;
                product.UpdatedAt = DateTime.Now;

                // Save changes to the database
                context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(productDto); // Return the productDto back if validation fails
        }
        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product); // Pass the product to the view
        }

        [HttpPost]
        [ActionName("DeleteProduct")]
        public IActionResult DeleteProductConfirmed(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
