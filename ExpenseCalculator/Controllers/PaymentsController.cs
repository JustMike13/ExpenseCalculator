﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseCalculator.Data;
using ExpenseCalculator.Models;
using System.Security.Claims;
using System.Runtime.CompilerServices;

namespace ExpenseCalculator.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index(int? id)
        {
            System.FormattableString query = FormattableStringFactory.
                Create("SELECT p.Id FROM Payment p, Expense e " +
                "WHERE p.Payer = '" + User.FindFirstValue(ClaimTypes.NameIdentifier) + 
                "' and p.ExpenseId = e.Id and e.TripId = " + id.ToString());
            var result = _context.Database
                        .SqlQuery<int>(query)
                        .ToList();
            return View(await _context.Payment.Where(p => result.Contains(p.Id)).ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        // id: 1 - Add Payment with current users id
        //     2 - Show dropdown with all users
        //     3 - a user paying his debt
        public IActionResult Create(int? id, int ExpenseId = 0, string? ExpenseName = " ")
        {
            Payment p = new Payment();
            p.ExpenseId = ExpenseId;
            p.Name = ExpenseName;
            ViewBag.PaymentType = -1;
            if (id == 1)
            {
                ViewBag.UserId = "Assigned";
                p.Payer = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.Users = "noList";
            }
            else if (id == 2)
            {
                ViewBag.UserId = "NoUserAssigned";
                var users = _context.Users.Select(u => new { u.Id, u.UserName }).ToList();
                ViewBag.Users = new SelectList(users, "Id", "UserName");
            }
            else if (id == 3)
            {
                ViewBag.PaymentType = 1;
            }
            return View(p);
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Payer,Name,Ammount,ExpenseId")] Payment payment, int PaymentType)
        {
            if (ModelState.IsValid)
            {
                payment.Ammount = payment.Ammount * PaymentType >= 0 ? payment.Ammount : payment.Ammount * -1;
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Expenses", new { id = payment.ExpenseId });
            }
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Payer,Name,Ammount,ExpenseId")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
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
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payment.FindAsync(id);
            if (payment != null)
            {
                _context.Payment.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payment.Any(e => e.Id == id);
        }
    }
}
