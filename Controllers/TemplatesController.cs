using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseProject.Areas.Identity.Data;
using CourseProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CourseProject.Controllers
{
    [Authorize]
    public class TemplatesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public TemplatesController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        // GET: Templates
        public async Task<IActionResult> Index()
        {
            return context.Templates != null ?
                        View(await context.Templates.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Templates'  is null.");
        }

        [Authorize]
        public async Task<IActionResult> My()
        {
            var userId = userManager.GetUserId(User);

            return context.Templates != null ?
                View(await context.Templates.Where(t => t.UserId == userId).ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Templates'  is null.");
        }

        // GET: Templates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || context.Templates == null)
            {
                return NotFound();
            }

            var template = await context.Templates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (template == null)
            {
                return NotFound();
            }

            return View(template);
        }

        // POST: Templates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CreateDate,Image,Title,Desription,Access")] Template template)
        {
            if (ModelState.IsValid)
            {
                context.Add(template);
                template.UserId = userManager.GetUserId(User);
                template.CreateDate = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = template.Id });
            }

            return View(template);
        }

        // GET: Templates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || context.Templates == null)
            {
                return NotFound();
            }

            var template = await context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            if (CheckAccess(template.UserId) == false)
            {
                return new RedirectResult("~/Identity/Account/AccessDenied");
            }

            template.Questions = await GetQuestionsList(id);
            return View(template);
        }

        public async Task<List<Question>> GetQuestionsList(int? templateId)
        {
            return context.Questions != null ?
                await context.Questions
                    .Where(q => q.TemplateId == templateId)
                    .ToListAsync() :
                    new List<CourseProject.Models.Question>();
        }

        // POST: Templates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CreateDate,Image,Title,Desription,Access")] Template template)
        {

            if (id != template.Id)
            {
                return NotFound();
            }

            if (CheckAccess(template.UserId) == false)
            {
                return new RedirectResult("~/Identity/Account/AccessDenied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(template);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TemplateExists(template.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                if (TempData["Route"]?.ToString() == "Index")
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(My));
            }
            return View(template);
        }

        // GET: Templates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || context.Templates == null)
            {
                return NotFound();
            }

            var template = await context.Templates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (template == null)
            {
                return NotFound();
            }

            if (CheckAccess(template.UserId) == false)
            {
                return new RedirectResult("~/Identity/Account/AccessDenied");
            }

            return View(template);
        }

        // POST: Templates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (context.Templates == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Templates'  is null.");
            }
            var questions = await GetQuestionsList(id);
            var template = await context.Templates.FindAsync(id);
            if (template != null)
            {
                if (CheckAccess(template.UserId) == false)
                {
                    return new RedirectResult("~/Identity/Account/AccessDenied");
                }
                context.Templates.Remove(template);
                context.Questions.RemoveRange(questions);
            }

            await context.SaveChangesAsync();
            if (TempData["Route"]?.ToString() == "Index")
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(My));
        }

        private bool TemplateExists(int id)
        {
            return (context.Templates?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool CheckAccess(string? ownerId)
        {
            return this.User.FindFirstValue(ClaimTypes.NameIdentifier) == ownerId || this.User.IsInRole("Admin");
        }
    }
}
