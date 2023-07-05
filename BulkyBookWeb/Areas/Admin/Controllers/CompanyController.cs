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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return View(objCompanyList);
        }
        public IActionResult upsert(int? Id)
        {
            if (Id == null || Id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company companyObj = _unitOfWork.Company.Get(c => c.Id == Id);
                return View(companyObj);
            }


        }
        [HttpPost]
        public IActionResult upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                if (companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);
                    TempData["success"] = "Company updated successfully";
                    //return RedirectToAction("Index");
                }
                else
                {
                    _unitOfWork.Company.Update(companyObj);
                    TempData["success"] = "Company created successfully";

                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return View(companyObj);
            }

        }

        //public IActionResult Edit(int? Id)
        //{
        //    if (Id == null || Id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company Company = _unitOfWork.Company.Get(c => c.Id == Id);
        //    if (Company == null) { return NotFound(); };
        //    return View(Company);
        //}
        //[HttpPost]
        //public IActionResult Edit(Company obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Company.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Company updated successfully";
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
        //    Company Company = _unitOfWork.Company.Get(c => c.Id == Id);
        //    if (Company == null) { return NotFound(); };
        //    return View(Company);
        //}
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? Id)
        {
            Company? Company = _unitOfWork.Company.Get(c => c.Id == Id);
            if (Company == null) { return NotFound(); }
            _unitOfWork.Company.Remove(Company);
            _unitOfWork.Save();
            TempData["success"] = "Company Deleted successfully";
            return RedirectToAction("Index");

        }
        #region APICalls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(CompanyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });

        }
        #endregion
    }
}
