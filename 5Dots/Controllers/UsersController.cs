using _5Dots.Data;
using _5Dots.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace _5Dots.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;


        public UsersController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Users;
            return View(await applicationDbContext.ToListAsync());
        }
        // GET: Users/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var User = _context.Users
                .Where(m => m.Id == id).SingleOrDefault();

            if (User == null)
            {
                return NotFound();
            }

            return View(User);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User User, string Password, string ConfirmPassword, IFormFile FormFile)
        {

            //if (ModelState.IsValid)
            //{
            using (var stream = FormFile.OpenReadStream())
            using (var reader = new BinaryReader(stream))
            {
                var byteFile = reader.ReadBytes((int)stream.Length);
                User.Image = byteFile;
            }
            User.ImageName = FormFile.FileName;
            User.ContentType = FormFile.ContentType;
            User.EmailConfirmed = true;
            User.NormalizedEmail = User.Email.ToUpper();
            User.NormalizedUserName = User.Email.ToUpper();
            User.UserName = User.Email;
            User.Role = "User";

            var result = await _userManager.CreateAsync(User, Password);

            var Id = User.Id;
            var roleId = "2";
            var userRole = new IdentityUserRole<string>
            {
                UserId = Id,
                RoleId = roleId
            };
            _context.UserRoles.Add(userRole);
            _context.SaveChanges();
            Cart cart = new Cart();
            cart.UserId=User.Id;
            cart.TotalQuantity = 0;
            cart.TotalPrice = 0;
            _context.Carts.Add(cart);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
            //}
            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", User.Id);
            //return View(User);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var User = await _context.Users.FindAsync(id);
            if (User == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", User.Id);
            return View(User);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User User, IFormFile FormFile)
        {
            if (id != User.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using (var stream = FormFile.OpenReadStream())
                    using (var reader = new BinaryReader(stream))
                    {
                        var byteFile = reader.ReadBytes((int)stream.Length);
                        User.Image = byteFile;
                    }
                    User.ImageName = FormFile.FileName;
                    User.ContentType = FormFile.ContentType;
                    _context.Update(User);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(User.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", User.Id);
            return View(User);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var User = _context.Users
                .Where(u => u.Id == id)
                .SingleOrDefault();
            if (User == null)
            {
                return NotFound();
            }

            return View(User);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.User'  is null.");
            }
            var User = await _context.Users.FindAsync(id);
            if (User != null)
            {
                _context.Users.Remove(User);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        #region Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                    var visa = _context.Visa
                        .Where(v => v.UserId == user.Id)
                        .FirstOrDefault();

                    if (visa != null)
                    {
                        ViewBag.VisaNumber = visa.VisaNumber;
                        ViewBag.ExpDate = visa.ExpDate;
                    }

                    List<User> users = new List<User>();
                    users.AddRange(_context.Users);

                    foreach (var existingUser in users)
                    {
                        if (user.Email == existingUser.Email && user.UserName == existingUser.UserName)
                        {
                            return View(existingUser);
                        }
                    }

                    return View();
                
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion



        #region editProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {

                return RedirectToAction("Profile");
            }
            var user = await _userManager.GetUserAsync(User);
            
                   
                    if (user != null)
                    {
                       var visa = _context.Visa
                      .Where(v => v.UserId == user.Id)
                      .FirstOrDefault();
                        if (visa != null)
                        {
                            ViewBag.VisaNumber = visa.VisaNumber;
                            ViewBag.ExpDate = visa.ExpDate;
                        }
                        else
                        {
                            ViewBag.VisaNumber = null;
                            ViewBag.ExpDate = null;
                        }
                        return View(user);
                    }

            
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(User updatedUser)
        {
            var existingUser = await _userManager.GetUserAsync(User);

            if (existingUser != null)
            {
                existingUser.FirstName = updatedUser.FirstName;
                existingUser.LastName = updatedUser.LastName;
                existingUser.Email = updatedUser.Email;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;

                var result = await _userManager.UpdateAsync(existingUser);


                if (result.Succeeded)
                {

                    return RedirectToAction("Profile");
                }
                else
                {

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }


            return RedirectToAction("Profile");
        }

        #endregion

    }
}
