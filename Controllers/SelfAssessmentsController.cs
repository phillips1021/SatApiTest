using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SatApiTest.Models;

namespace SatApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfAssessmentsController : ControllerBase
    {
        private readonly SatContext _context;

        public SelfAssessmentsController(SatContext context)
        {
            _context = context;
        }

        // GET: api/SelfAssessments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SelfAssessment>>> GetSelfAssessments()
        {
            return await _context.SelfAssessments.ToListAsync();
        }

        // GET: api/SelfAssessments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SelfAssessment>> GetSelfAssessment(Guid id)
        {
            var selfAssessment = await _context.SelfAssessments.FindAsync(id);

            if (selfAssessment == null)
            {
                return NotFound();
            }

            return selfAssessment;
        }

        // PUT: api/SelfAssessments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSelfAssessment(Guid id, SelfAssessment selfAssessment)
        {
            if (id != selfAssessment.Id)
            {
                return BadRequest();
            }

            _context.Entry(selfAssessment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SelfAssessmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SelfAssessments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SelfAssessment>> PostSelfAssessment(SelfAssessment selfAssessment)
        {
            _context.SelfAssessments.Add(selfAssessment);
            await _context.SaveChangesAsync();

            // return CreatedAtAction("GetSelfAssessment", new { id = selfAssessment.Id }, selfAssessment);
            return CreatedAtAction(nameof(GetSelfAssessment), new { id = selfAssessment.Id }, selfAssessment);
        }

        // DELETE: api/SelfAssessments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSelfAssessment(Guid id)
        {
            var selfAssessment = await _context.SelfAssessments.FindAsync(id);
            if (selfAssessment == null)
            {
                return NotFound();
            }

            _context.SelfAssessments.Remove(selfAssessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SelfAssessmentExists(Guid id)
        {
            return _context.SelfAssessments.Any(e => e.Id == id);
        }
    }
}
