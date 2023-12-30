using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _5Dots.Data;
using _5Dots.Models;
using System.Security.Claims;

namespace _5Dots.Controllers
{
    public class CartProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CartProducts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CartProducts.Include(c => c.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CartProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CartProducts == null)
            {
                return NotFound();
            }

            var cartProduct = await _context.CartProducts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cartProduct == null)
            {
                return NotFound();
            }

            return View(cartProduct);
        }

        // GET: CartProducts/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductDescription");
            return View();
        }

        // POST: CartProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create_(int id,int quantity)
        {
            //if (ModelState.IsValid)
            //{
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Product product = _context.Products.Where(product => product.ProductId == id).SingleOrDefault();
            Cart cart = _context.Carts.Where(cart => cart.UserId == userId).SingleOrDefault();
            CartProduct cartProduct_ = _context.CartProducts.Where(cartP=>cartP.CartId == cart.CartId && cartP.ProductId == id).SingleOrDefault();
            if(cartProduct_ == null)
            {
                CartProduct cartProduct = new CartProduct();
                cartProduct.ProductId = id;
                cartProduct.ProductQuantity = quantity;
                cartProduct.CartId = cart.CartId;
                _context.Add(cartProduct);
               await _context.SaveChangesAsync();
                if (product.ProductSale > 0)
                {
                    cart.TotalPrice = quantity *(@product.ProductPrice - (@product.ProductPrice *@product.ProductSale / 100));
                }
                else
                {
                    cart.TotalPrice = quantity * product.ProductPrice;
                }
                cart.TotalQuantity++;
                _context.Update(cart);
                await _context.SaveChangesAsync();
            }
            else
            {
                cartProduct_.ProductQuantity = quantity+cartProduct_.ProductQuantity;
                _context.Update(cartProduct_);
                await _context.SaveChangesAsync();
                if (product.ProductSale > 0)
                {
                    cart.TotalPrice += quantity * (@product.ProductPrice - (@product.ProductPrice * @product.ProductSale / 100));
                }
                else
                {
                    cart.TotalPrice += quantity * product.ProductPrice;
                }
                _context.Update(cart);
                await _context.SaveChangesAsync();
            }
            product.ProductQuantityStock-=quantity;
            _context.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Shop");
            //}
            //ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductDescription", cartProduct.ProductId);
            //return View(cartProduct);
        }

        // GET: CartProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CartProducts == null)
            {
                return NotFound();
            }

            var cartProduct = await _context.CartProducts.FindAsync(id);
            if (cartProduct == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductDescription", cartProduct.ProductId);
            return View(cartProduct);
        }

        // POST: CartProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,ProductQuantity,ProductId")] CartProduct cartProduct)
        {
            if (id != cartProduct.CartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cartProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartProductExists(cartProduct.CartId))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductDescription", cartProduct.ProductId);
            return View(cartProduct);
        }

        // GET: CartProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CartProducts == null)
            {
                return NotFound();
            }

            var cartProduct = await _context.CartProducts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cartProduct == null)
            {
                return NotFound();
            }

            return View(cartProduct);
        }

        // POST: CartProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CartProducts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CartProducts'  is null.");
            }
            var cartProduct = await _context.CartProducts.FindAsync(id);
            if (cartProduct != null)
            {
                _context.CartProducts.Remove(cartProduct);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartProductExists(int id)
        {
          return (_context.CartProducts?.Any(e => e.CartId == id)).GetValueOrDefault();
        }
    }
}
