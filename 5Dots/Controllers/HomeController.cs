using _5Dots.Data;
using _5Dots.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;

namespace _5Dots.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public HomeController(ApplicationDbContext context,RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

     

        public IActionResult Index(string selectedCategory)
        {
            ViewBag.products = _context.Products.Include(product => product.Category).ToList();
            ViewBag.Categories = _context.Categories.Include(category => category.Products).ToList();
            ViewBag.SelectedCategory = selectedCategory;
            ViewBag.ProductImages = _context.ProductImages.ToList();
            ViewBag.Testimonials = _context.Testimonials.Include(testimonial => testimonial.User).Where(testimonial => testimonial.TestimonialStatus == "Accept").Take(3).ToList();
            return View();
        }




        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Contact()
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
