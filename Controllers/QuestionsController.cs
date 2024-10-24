using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CourseProject.Areas.Identity.Data;
using CourseProject.Models;
using static CourseProject.Models.Question;
using Microsoft.AspNetCore.Authorization;

namespace CourseProject
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext context;

        public QuestionsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize(Roles = "Admin")]
        // GET: Questions
        public async Task<IActionResult> Index()
        {
            return context.Questions != null ?
                        View(await context.Questions.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Questions'  is null.");
        }

        [HttpPost]
        public async Task<IActionResult> Create(int templateId, string label, QestionTypes type)
        {
            if (label == null)
                label = "Untitled question";

            var question = new Question
            {
                TemplateId = templateId,
                Label = label,
                Type = type

            };

            context.Questions.Add(question);
            await context.SaveChangesAsync();

            var questions = await GetQuestionsList(templateId);

            return PartialView("_QuestionsListPartial", questions);
        }


        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || context.Questions == null)
            {
                return NotFound();
            }

            var question = await context.Questions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || context.Questions == null)
            {
                return NotFound();
            }

            var question = await context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Label,Type")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(question);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
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
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int templateId)
        {
            var question = await context.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            context.Questions.Remove(question);

            await context.SaveChangesAsync();
            var questions = await GetQuestionsList(templateId);
            return PartialView("_QuestionsListPartial", questions);
        }

        private bool QuestionExists(int id)
        {
            return (context.Questions?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<List<Question>> GetQuestionsList(int? templateId)
        {
            return context.Questions != null ?
                await context.Questions
                    .Where(q => q.TemplateId == templateId)
                    .ToListAsync() :
                    new List<CourseProject.Models.Question>();
        }
    }
}
