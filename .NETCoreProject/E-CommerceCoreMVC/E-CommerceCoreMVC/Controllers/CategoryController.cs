using E_CommerceCoreMVC.Models;
using E_CommerceCoreMVC.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceCoreMVC.Controllers
{
	public class CategoryController : Controller
	{
		private readonly DataContext _dataContext;
		public CategoryController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}
		// GET: CategoryController
		public async Task<IActionResult> Index(string slug="")
		{
			CategoryModel cate = _dataContext.Categories.Where(x => x.Slug == slug).FirstOrDefault();
			if(cate == null) return RedirectToAction("Index");
			var produectByCate = _dataContext.Products.Where(c => c.CategoryId == cate.Id);
			return View(await produectByCate.OrderByDescending(c => c.Id).ToListAsync());
		}

	}
}
