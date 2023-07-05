using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Bulky.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.product.GetAll(includeProperties: "Category").ToList();
            return View(products);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart shoppingCart = new ShoppingCart
            {
                Product = _unitOfWork.product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId

            };
            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart cartfromDB = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);
            if (cartfromDB != null)
            {
                cartfromDB.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartfromDB);
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }

            TempData["success"]="Cart updated successfully";
            _unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}