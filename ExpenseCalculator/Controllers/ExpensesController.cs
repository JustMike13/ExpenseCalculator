using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseCalculator.Data;
using ExpenseCalculator.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseCalculator.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(int id)
        {
            return View(await _context.Expense.Where(e => e.EquallyDivided && e.TripId == id).ToListAsync());
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            //Get the UserName of the user that owns the expense
            System.FormattableString query = FormattableStringFactory.
                Create("SELECT UserName FROM AspNetUsers WHERE Id = '" + expense.PayedBy + "'");
            ViewBag.ExpenseUserName = _context.Database
                        .SqlQuery<string>(query)
                        .ToList().First();

            //Get the list of payments, but replace userId with UserName
            query = FormattableStringFactory.
                Create("SELECT p.Id Id, u.UserName Payer, p.Name Name, p.Ammount Ammount, p.ExpenseId ExpenseId " +
                "FROM Payment p, AspNetUsers u WHERE u.Id = p.Payer and p.ExpenseId = '" + expense.Id + "'");
            ViewBag.Payments = _context.Database
                                    .SqlQuery<Payment>(query)
                                    .ToList();

            //Get the UserName and Id of current User
            ViewBag.MyUserName = User.Identity.Name;
            //ViewBag.MyUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //Get the UserName of the creator of the trip, he can edit anything in the trip
            query = FormattableStringFactory.
                Create("SELECT u.UserName " +
                "FROM Trip t, AspNetUsers u WHERE t.CreatorId = u.Id and t.Id = '" + expense.TripId +"'");
            ViewBag.CreatorUserName = _context.Database
                                    .SqlQuery<string>(query)
                                    .ToList().First();

            //Check if current user is admin
            ViewBag.IamAdmin = User.IsInRole("Admin");

            return View(expense);
        }

        // GET: Expenses/Create
        public IActionResult Create(int Id)
        {
            ViewData["TripId"] = Id;
            ViewData["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View();
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TripId,PayedBy,Name,TotalAmmount,EquallyDivided,OwnContribution")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                _context.Add(expense);
                await _context.SaveChangesAsync();
                Payment pmt = new Payment();
                pmt.ExpenseId = expense.Id;
                pmt.Payer = User.FindFirstValue(ClaimTypes.NameIdentifier);
                pmt.Name = expense.Name;
                pmt.Ammount = expense.TotalAmmount - expense.OwnContribution;
                _context.Add(pmt);
                await _context.SaveChangesAsync();
                if (expense.EquallyDivided)
                {
                    UpdateGroupExpenses(expense.TripId);
                }
                return RedirectToAction(nameof(Details), "Trips", new { id = expense.TripId });
            }
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PayedBy,Name,TotalAmmount,EquallyDivided,OwnContribution")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
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
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expense.Any(e => e.Id == id);
        }

        public void UpdateGroupExpenses(int TripId)
        {
            //Get total ammount of group expenses and no of users
            float totalAmmount = _context.Expense.Where(e => e.EquallyDivided && e.TripId == TripId && e.Name != "Group Expense").Sum(e => e.TotalAmmount);
            Expense groupExp = _context.Expense.Where(e => e.Name == "Group Expense" && e.TripId == TripId).ToList().First();
            List<string> userIds = _context.UserTrip.Where(ut => ut.TripId == TripId).Select(ut=>ut.UserId).ToList();
            if (totalAmmount == 0 || userIds.IsNullOrEmpty())
            {
                return;
            }

            //create or update payments
            foreach(string userId in userIds)
            {
                Payment pmt;
                if (!_context.Payment.Any(p => p.Payer == userId && groupExp.Id == p.ExpenseId))
                {
                    pmt = new Payment();
                    pmt.ExpenseId = groupExp.Id;
                    pmt.Payer = userId;
                    pmt.Name = "Group Expense Payment";
                    pmt.Ammount = totalAmmount / userIds.Count();
                    _context.Add(pmt);
                }
                else
                {
                    pmt = _context.Payment.Where(p => p.Payer == userId && groupExp.Id == p.ExpenseId).ToList().First();
                    pmt.Ammount = - totalAmmount / userIds.Count();
                    _context.SaveChanges();
                }
            }
            //update group expense
            groupExp.TotalAmmount = totalAmmount;
            _context.SaveChanges();
        }
    }
}
