﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseCalculator.Data;
using ExpenseCalculator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Runtime.CompilerServices;
using System.Data;

namespace ExpenseCalculator.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; 
        private readonly SignInManager<IdentityUser> _signInManager;

        public TripsController(ApplicationDbContext         context, 
                               UserManager<IdentityUser>    userManager, 
                               SignInManager<IdentityUser>  signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Trips
        public async Task<IActionResult> Index()
        {
            System.FormattableString query = FormattableStringFactory.
                Create("SELECT TripId FROM UserTrip WHERE UserId = '" + User.FindFirstValue(ClaimTypes.NameIdentifier) + "'");
            var result = _context.Database
                        .SqlQuery<int>(query)
                        .ToList();
            return View(await _context.Trip.Where(t => (result.Contains(t.Id) || User.IsInRole("Admin")) && t.Active).ToListAsync());
        }

        // GET: Trips/Details/5
        [Authorize(Roles = "User, Creator, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trip == null)
            {
                return NotFound();
            }
            System.FormattableString query = FormattableStringFactory.
                Create("SELECT e.Id ExpenseId, e.Name, u.UserName UserName, e.TotalAmmount Ammount FROM Expense e, AspNetUsers u WHERE u.Id = e.PayedBy and e.TripId = '" + trip.Id + "'");
            var result = _context.Database
                        .SqlQuery<ExpenseView>(query)
                        .ToList();
            ViewBag.TripExpenses = result;
            ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(trip);
        }

        // GET: Trips/Create
        [Authorize(Roles = "Admin, Creator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Trips/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Creator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,InviteCode,CreatorId")] Trip trip)
        {
            if (ModelState.IsValid)
            {
                trip.Active = true;
                _context.Add(trip);
                await _context.SaveChangesAsync();
                UserTrip userTrip = new UserTrip();
                userTrip.TripId = trip.Id;
                userTrip.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(userTrip);
                Expense exp = new Expense();
                exp.Name = "Group Expense";
                exp.EquallyDivided = true;
                exp.TripId = trip.Id;
                exp.TotalAmmount = 0;
                exp.OwnContribution = 0;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trip);
        }

        // GET: Trips/Edit/5
        [Authorize(Roles = "Admin, Creator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != trip.CreatorId && !User.IsInRole("Admin"))
            {
                return NotFound();
            }

            return View(trip);
        }

        // POST: Trips/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Creator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatorId,InviteCode")] Trip trip)
        {
            if (id != trip.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TripExists(trip.Id))
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
            return View(trip);
        }

        // GET: Trips/Delete/5
        [Authorize(Roles = "Admin, Creator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trip == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != trip.CreatorId && !User.IsInRole("Admin"))
            {
                return NotFound();
            }

            return View(trip);
        }

        // POST: Trips/Delete/5
        [Authorize(Roles = "Admin, Creator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trip.FindAsync(id);
            if (trip != null)
            {
                trip.Active = false;
                _context.Update(trip);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RegisterToTrip()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles == null || !roles.Any())
            {
                await _userManager.AddToRoleAsync(user, "User");
                // Refresh the user's authentication cookie
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            return View("RegisterToTrip");
        }

        [HttpPost]
        public IActionResult RegisterToTrip(string InviteCode)
        {
            Trip trip = _context.Trip.Where(t => t.InviteCode == InviteCode).First();
            UserTrip userTrip = new UserTrip();
            userTrip.TripId = trip.Id;
            userTrip.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(userTrip);
            _context.SaveChangesAsync();
            return View("Details", trip);
        }

        private bool TripExists(int id)
        {
            return _context.Trip.Any(e => e.Id == id);
        }
    }
}
