using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseCalculator.Data;
using ExpenseCalculator.Models;

namespace ExpenseCalculator.Controllers
{
    public class UserTripsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserTripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserTrips
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserTrip.ToListAsync());
        }

        // GET: UserTrips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTrip = await _context.UserTrip
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userTrip == null)
            {
                return NotFound();
            }

            return View(userTrip);
        }

        // GET: UserTrips/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserTrips/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,TripId,Owner")] UserTrip userTrip)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userTrip);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userTrip);
        }

        // GET: UserTrips/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTrip = await _context.UserTrip.FindAsync(id);
            if (userTrip == null)
            {
                return NotFound();
            }
            return View(userTrip);
        }

        // POST: UserTrips/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,TripId,Owner")] UserTrip userTrip)
        {
            if (id != userTrip.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userTrip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTripExists(userTrip.Id))
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
            return View(userTrip);
        }

        // GET: UserTrips/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTrip = await _context.UserTrip
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userTrip == null)
            {
                return NotFound();
            }

            return View(userTrip);
        }

        // POST: UserTrips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userTrip = await _context.UserTrip.FindAsync(id);
            if (userTrip != null)
            {
                _context.UserTrip.Remove(userTrip);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserTripExists(int id)
        {
            return _context.UserTrip.Any(e => e.Id == id);
        }
    }
}
