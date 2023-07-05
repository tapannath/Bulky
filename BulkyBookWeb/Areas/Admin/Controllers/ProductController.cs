using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;


namespace Bulky.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;


        }
        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.product.GetAll(includeProperties: "Category").ToList();
            return View(products);
        }
        public IActionResult upsert(int? Id)
        {
            IEnumerable<SelectListItem> category = _unitOfWork.Category
              .GetAll().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
            //ViewBag.Categorylist = category;
            //ViewData["CategoryList"] = category;
            ProductVM productVM = new ProductVM() { CategoryList = category, Product = new Product() };
            if (Id == null || Id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _unitOfWork.product.Get(c => c.Id == Id);
                return View(productVM);
            }


        }
        [HttpPost]
        public IActionResult upsert(ProductVM productVM, IFormFile? file)
        {
            //if (obj.Name == obj.DisplayOrder.ToString()) { ModelState.AddModelError("Name", "Display Order should not match with Name"); }

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    #region DeleteOldFile
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    #endregion
                    #region UploadFile                    
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                    #endregion
                }
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.product.Add(productVM.Product);
                    TempData["success"] = "Product updated successfully";
                    //return RedirectToAction("Index");
                }
                else
                {
                    _unitOfWork.product.Update(productVM.Product);
                    TempData["success"] = "Product created successfully";

                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                //This part is written, if the model state is not valid
                //product dropdown is to be populated
                IEnumerable<SelectListItem> categoryList = _unitOfWork.Category
        .GetAll().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                productVM.CategoryList = categoryList;
                return View(productVM);
            }

        }

        //public IActionResult Edit(int? Id)
        //{
        //    if (Id == null || Id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product product = _unitOfWork.product.Get(c => c.Id == Id);
        //    if (product == null) { return NotFound(); };
        //    return View(product);
        //}
        //[HttpPost]
        //public IActionResult Edit(Product obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.product.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product updated successfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}
        //public IActionResult Delete(int? Id)
        //{
        //    if (Id == null || Id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product product = _unitOfWork.product.Get(c => c.Id == Id);
        //    if (product == null) { return NotFound(); };
        //    return View(product);
        //}
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? Id)
        {
            Product? product = _unitOfWork.product.Get(c => c.Id == Id);
            if (product == null) { return NotFound(); }
            _unitOfWork.product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product Deleted successfully";
            return RedirectToAction("Index");

        }
        #region APICalls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitOfWork.product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });

        }
        #endregion
    }
}
