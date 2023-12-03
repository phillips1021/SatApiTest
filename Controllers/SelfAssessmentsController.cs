using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using DinkToPdf;
using DinkToPdf.Contracts;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.EntityFrameworkCore;
using SatApiTest.Models;
using Stfm.Generator.SelfAssessmentTool;

namespace SatApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfAssessmentsController : ControllerBase
    {
        private readonly SatContext _context;

        private HttpClient client = new HttpClient();

        private readonly IConverter converter;

        public SelfAssessmentsController(SatContext context, IConverter converter)
        {
            _context = context;

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            this.converter = converter;
        }

        // GET: api/SelfAssessments
        [EnableCors]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SelfAssessment>>> GetSelfAssessments()
        {
            return await _context.SelfAssessments.ToListAsync();
        }

        // GET: api/SelfAssessments/5
        [EnableCors]
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
        [EnableCors]
        [HttpPost]
        public async Task<ActionResult<SelfAssessment>> PostSelfAssessment(SelfAssessment selfAssessment)
        {
            _context.SelfAssessments.Add(selfAssessment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSelfAssessment), new { id = selfAssessment.Id }, selfAssessment);
        }

        // POST: api/SelfAssessmentsCsv
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [EnableCors]
        [HttpPost("~/SelfAssessmentCsv")]
        public async Task<ActionResult<SelfAssessment>> SelfAssessmentCsv(String selfAssessmentCsv)
        {


            SelfAssessment selfAssessment = SelfAssessment.FromCsv(selfAssessmentCsv);

            _context.SelfAssessments.Add(selfAssessment);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSelfAssessment), new { id = selfAssessment.Id }, selfAssessment);



        }

        // POST: api/SelfAssessmentJson
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [EnableCors]
        [HttpPost("~/SelfAssessmentJson")]
        public async Task<ActionResult<SelfAssessment>> SelfAssessmentJson(SelfAssessmentDTO selfAssessmentDTO)
        {

            SelfAssessment selfAssessment = SelfAssessment.FromJson(selfAssessmentDTO);

            _context.SelfAssessments.Add(selfAssessment);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSelfAssessment), new { id = selfAssessment.Id }, selfAssessment);



        }

        // DELETE: api/SelfAssessments/5
        [EnableCors]
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

        [HttpGet]
        [EnableCors]
        [Route("GenerateSelfAssessmentForm")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GenerateSelfAssessmentForm(Guid id, bool returnHtml = false)
        {

            var selfAssessment = _context.SelfAssessments.Find(id);

            if (selfAssessment == null)
            {
                throw new Exception("Unable to locate data for Id " + id.ToString());
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            //Load the Resource URLs file
            List<ResourceLink> resources;
            using (var reader = new StreamReader("resources/sat_files/resources.csv"))
            using (var csvReader = new CsvHelper.CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<ResourceLinkMap>();
                resources = csvReader.GetRecords<ResourceLink>().ToList();
            }

            //Let's prepare the template data
            var cleanedUpData = PrepareSelfAssessmentData(selfAssessment, resources);

            cleanedUpData.Add("RootFolder", "/");
            cleanedUpData.Add("ImagesRootFolder", "resources/sat_files/images");
            cleanedUpData.Add("CommunityFacultyComptenciesSelfAssessmentUrl", "https://teachingphysician.org");

            string selfAssessmentTemplatePath = "resources/sat_files";

            //Load all the HTML templates for each Domain and Competency; we'll reorder them below after populating the templates
            var domain1Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain1.html");
            var domain2Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain2.html");
            var domain3Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain3.html");
            var domain4Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain4.html");
            var domain5Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain5.html");

            var domain1Competency1Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain1Competency1.html");
            var domain1Competency2Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain1Competency2.html");
            var domain1Competency3Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain1Competency3.html");
            var domain1Competency4Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain1Competency4.html");
            var domain1Competency5Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain1Competency5.html");
            var domain1Competency6Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain1Competency6.html");

            var domain2Competency1Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain2Competency1.html");
            var domain2Competency2Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain2Competency2.html");
            var domain2Competency3Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain2Competency3.html");
            var domain2Competency4Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain2Competency4.html");

            var domain3Competency1Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain3Competency1.html");
            var domain3Competency2Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain3Competency2.html");
            var domain3Competency3Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain3Competency3.html");
            var domain3Competency4Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain3Competency4.html");

            var domain4Competency1Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain4Competency1.html");
            var domain4Competency2Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain4Competency2.html");
            var domain4Competency3Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain4Competency3.html");
            var domain4Competency4Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain4Competency4.html");
            var domain4Competency5Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain4Competency5.html");

            var domain5Competency1Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain5Competency1.html");
            var domain5Competency2Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain5Competency2.html");
            var domain5Competency3Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain5Competency3.html");
            var domain5Competency4Html = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\Domain5Competency4.html");


            var domain1CompetenciesHtml = "";

            //Clear out Next Competency / Resource removal placeholder for answers that aren't the rightmost (since the rightmost answers have no "next resource"s)
            if (selfAssessment.Domain1Competency1 != 5) domain1Competency1Html = domain1Competency1Html.Replace("{{ hided1c1Start }}", "").Replace("{{ hided1c1End }}", "");
            if (selfAssessment.Domain1Competency2 != 5) domain1Competency2Html = domain1Competency2Html.Replace("{{ hided1c2Start }}", "").Replace("{{ hided1c2End }}", "");
            if (selfAssessment.Domain1Competency3 != 5) domain1Competency3Html = domain1Competency3Html.Replace("{{ hided1c3Start }}", "").Replace("{{ hided1c3End }}", "");
            if (selfAssessment.Domain1Competency4 != 5) domain1Competency4Html = domain1Competency4Html.Replace("{{ hided1c4Start }}", "").Replace("{{ hided1c4End }}", "");
            if (selfAssessment.Domain1Competency5 != 5) domain1Competency5Html = domain1Competency5Html.Replace("{{ hided1c5Start }}", "").Replace("{{ hided1c5End }}", "");
            if (selfAssessment.Domain1Competency6 != 5) domain1Competency6Html = domain1Competency6Html.Replace("{{ hided1c6Start }}", "").Replace("{{ hided1c6End }}", "");

            if (selfAssessment.Domain2Competency1 != 5) domain2Competency1Html = domain2Competency1Html.Replace("{{ hided2c1Start }}", "").Replace("{{ hided2c1End }}", "");
            if (selfAssessment.Domain2Competency2 != 5) domain2Competency2Html = domain2Competency2Html.Replace("{{ hided2c2Start }}", "").Replace("{{ hided2c2End }}", "");
            if (selfAssessment.Domain2Competency3 != 5) domain2Competency3Html = domain2Competency3Html.Replace("{{ hided2c3Start }}", "").Replace("{{ hided2c3End }}", "");
            if (selfAssessment.Domain2Competency4 != 5) domain2Competency4Html = domain2Competency4Html.Replace("{{ hided2c4Start }}", "").Replace("{{ hided2c4End }}", "");

            if (selfAssessment.Domain3Competency1 != 5) domain3Competency1Html = domain3Competency1Html.Replace("{{ hided3c1Start }}", "").Replace("{{ hided3c1End }}", "");
            if (selfAssessment.Domain3Competency2 != 5) domain3Competency2Html = domain3Competency2Html.Replace("{{ hided3c2Start }}", "").Replace("{{ hided3c2End }}", "");
            if (selfAssessment.Domain3Competency3 != 5) domain3Competency3Html = domain3Competency3Html.Replace("{{ hided3c3Start }}", "").Replace("{{ hided3c3End }}", "");
            if (selfAssessment.Domain3Competency4 != 5) domain3Competency4Html = domain3Competency4Html.Replace("{{ hided3c4Start }}", "").Replace("{{ hided3c4End }}", "");

            if (selfAssessment.Domain4Competency1 != 5) domain4Competency1Html = domain4Competency1Html.Replace("{{ hided4c1Start }}", "").Replace("{{ hided4c1End }}", "");
            if (selfAssessment.Domain4Competency2 != 5) domain4Competency2Html = domain4Competency2Html.Replace("{{ hided4c2Start }}", "").Replace("{{ hided4c2End }}", "");
            if (selfAssessment.Domain4Competency3 != 5) domain4Competency3Html = domain4Competency3Html.Replace("{{ hided4c3Start }}", "").Replace("{{ hided4c3End }}", "");
            if (selfAssessment.Domain4Competency4 != 5) domain4Competency4Html = domain4Competency4Html.Replace("{{ hided4c4Start }}", "").Replace("{{ hided4c4End }}", "");
            if (selfAssessment.Domain4Competency5 != 5) domain4Competency5Html = domain4Competency5Html.Replace("{{ hided4c5Start }}", "").Replace("{{ hided4c5End }}", "");

            if (selfAssessment.Domain5Competency1 != 5) domain5Competency1Html = domain5Competency1Html.Replace("{{ hided5c1Start }}", "").Replace("{{ hided5c1End }}", "");
            if (selfAssessment.Domain5Competency2 != 5) domain5Competency2Html = domain5Competency2Html.Replace("{{ hided5c2Start }}", "").Replace("{{ hided5c2End }}", "");
            if (selfAssessment.Domain5Competency3 != 5) domain5Competency3Html = domain5Competency3Html.Replace("{{ hided5c3Start }}", "").Replace("{{ hided5c3End }}", "");
            if (selfAssessment.Domain5Competency4 != 5) domain5Competency4Html = domain5Competency4Html.Replace("{{ hided5c4Start }}", "").Replace("{{ hided5c4End }}", "");

            //Order the Competencies within the Domains according to their scores. There's a foreach for each Domain
            foreach (var domain in domain1Scores.OrderByDescending(x => x.Value))
            {
                switch (domain.Key)
                {
                    case 1: domain1CompetenciesHtml += Regex.Replace(domain1Competency1Html.Replace("\r\n", " "), "{{ hided1c1Start }}.*{{ hided1c1End }}", ""); break;
                    case 2: domain1CompetenciesHtml += Regex.Replace(domain1Competency2Html.Replace("\r\n", " "), "{{ hided1c2Start }}.*{{ hided1c2End }}", ""); break;
                    case 3: domain1CompetenciesHtml += Regex.Replace(domain1Competency3Html.Replace("\r\n", " "), "{{ hided1c3Start }}.*{{ hided1c3End }}", ""); break;
                    case 4: domain1CompetenciesHtml += Regex.Replace(domain1Competency4Html.Replace("\r\n", " "), "{{ hided1c4Start }}.*{{ hided1c4End }}", ""); break;
                    case 5: domain1CompetenciesHtml += Regex.Replace(domain1Competency5Html.Replace("\r\n", " "), "{{ hided1c5Start }}.*{{ hided1c5End }}", ""); break;
                    case 6: domain1CompetenciesHtml += Regex.Replace(domain1Competency6Html.Replace("\r\n", " "), "{{ hided1c6Start }}.*{{ hided1c6End }}", ""); break;
                }
            }

            domain1Html = domain1Html.Replace("{{ domain1Competencies }}", domain1CompetenciesHtml);


            var domain2CompetenciesHtml = "";

            foreach (var domain in domain2Scores.OrderByDescending(x => x.Value))
            {
                switch (domain.Key)
                {
                    case 1: domain2CompetenciesHtml += Regex.Replace(domain2Competency1Html.Replace("\r\n", " "), "{{ hided2c1Start }}.*{{ hided2c1End }}", ""); break;
                    case 2: domain2CompetenciesHtml += Regex.Replace(domain2Competency2Html.Replace("\r\n", " "), "{{ hided2c2Start }}.*{{ hided2c2End }}", ""); break;
                    case 3: domain2CompetenciesHtml += Regex.Replace(domain2Competency3Html.Replace("\r\n", " "), "{{ hided2c3Start }}.*{{ hided2c3End }}", ""); break;
                    case 4: domain2CompetenciesHtml += Regex.Replace(domain2Competency4Html.Replace("\r\n", " "), "{{ hided2c4Start }}.*{{ hided2c4End }}", ""); break;
                }
            }

            domain2Html = domain2Html.Replace("{{ domain2Competencies }}", domain2CompetenciesHtml);


            var domain3CompetenciesHtml = "";

            foreach (var domain in domain3Scores.OrderByDescending(x => x.Value))
            {
                switch (domain.Key)
                {
                    case 1: domain3CompetenciesHtml += Regex.Replace(domain3Competency1Html.Replace("\r\n", " "), "{{ hided3c1Start }}.*{{ hided3c1End }}", ""); break;
                    case 2: domain3CompetenciesHtml += Regex.Replace(domain3Competency2Html.Replace("\r\n", " "), "{{ hided3c2Start }}.*{{ hided3c2End }}", ""); break;
                    case 3: domain3CompetenciesHtml += Regex.Replace(domain3Competency3Html.Replace("\r\n", " "), "{{ hided3c3Start }}.*{{ hided3c3End }}", ""); break;
                    case 4: domain3CompetenciesHtml += Regex.Replace(domain3Competency4Html.Replace("\r\n", " "), "{{ hided3c4Start }}.*{{ hided3c4End }}", ""); break;
                }
            }

            domain3Html = domain3Html.Replace("{{ domain3Competencies }}", domain3CompetenciesHtml);


            var domain4CompetenciesHtml = "";

            foreach (var domain in domain4Scores.OrderByDescending(x => x.Value))
            {
                switch (domain.Key)
                {
                    case 1: domain4CompetenciesHtml += Regex.Replace(domain4Competency1Html.Replace("\r\n", " "), "{{ hided4c1Start }}.*{{ hided4c1End }}", ""); break;
                    case 2: domain4CompetenciesHtml += Regex.Replace(domain4Competency2Html.Replace("\r\n", " "), "{{ hided4c2Start }}.*{{ hided4c2End }}", ""); break;
                    case 3: domain4CompetenciesHtml += Regex.Replace(domain4Competency3Html.Replace("\r\n", " "), "{{ hided4c3Start }}.*{{ hided4c3End }}", ""); break;
                    case 4: domain4CompetenciesHtml += Regex.Replace(domain4Competency4Html.Replace("\r\n", " "), "{{ hided4c4Start }}.*{{ hided4c4End }}", ""); break;
                    case 5: domain4CompetenciesHtml += Regex.Replace(domain4Competency5Html.Replace("\r\n", " "), "{{ hided4c5Start }}.*{{ hided4c5End }}", ""); break;
                }
            }

            domain4Html = domain4Html.Replace("{{ domain4Competencies }}", domain4CompetenciesHtml);


            var domain5CompetenciesHtml = "";

            foreach (var domain in domain5Scores.OrderByDescending(x => x.Value))
            {
                switch (domain.Key)
                {
                    case 1: domain5CompetenciesHtml += Regex.Replace(domain5Competency1Html.Replace("\r\n", " "), "{{ hided5c1Start }}.*{{ hided5c1End }}", ""); break;
                    case 2: domain5CompetenciesHtml += Regex.Replace(domain5Competency2Html.Replace("\r\n", " "), "{{ hided5c2Start }}.*{{ hided5c2End }}", ""); break;
                    case 3: domain5CompetenciesHtml += Regex.Replace(domain5Competency3Html.Replace("\r\n", " "), "{{ hided5c3Start }}.*{{ hided5c3End }}", ""); break;
                    case 4: domain5CompetenciesHtml += Regex.Replace(domain5Competency4Html.Replace("\r\n", " "), "{{ hided5c4Start }}.*{{ hided5c4End }}", ""); break;
                }
            }

            domain5Html = domain5Html.Replace("{{ domain5Competencies }}", domain5CompetenciesHtml);

            //Page break in the PDF
            var pb = "<div style='page-break-after: always'></div>";

            //Order the Domains in the output according to each Domain's score based on its Competency selections
            var domainsSection = "";
            var howManyPages = 0;
            foreach (var domain in domainScores.OrderByDescending(x => x.Value))
            {
                switch (domain.Key)
                {
                    case 1:
                        domainsSection += domain1Html;
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString(), "Teacher and Professionalism");
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString() + "Url", "https://www.teachingphysician.org/content/learning-paths/teacher-and-professionalism");
                        break;
                    case 2:
                        domainsSection += domain2Html;
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString(), "Teacher and Learner");
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString() + "Url", "https://www.teachingphysician.org/content/learning-paths/teacher-and-learner");
                        break;
                    case 3:
                        domainsSection += domain3Html;
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString(), "Teacher and Assessment");
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString() + "Url", "https://www.teachingphysician.org/content/learning-paths/teacher-and-assessment");
                        break;
                    case 4:
                        domainsSection += domain4Html;
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString(), "Teacher and Content");
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString() + "Url", "https://www.teachingphysician.org/content/learning-paths/teacher-and-content");
                        break;
                    case 5:
                        domainsSection += domain5Html;
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString(), "Teacher and Environment");
                        cleanedUpData.Add("Domain" + (howManyPages + 1).ToString() + "Url", "https://www.teachingphysician.org/content/learning-paths/teacher-and-environment");

                        break;
                }

                howManyPages += 1;
                if (howManyPages < 5)
                {
                    domainsSection += pb;
                }
            }

            //Here, we place the Domain html into the main template html
            var htmlTemplate = System.IO.File.ReadAllText(selfAssessmentTemplatePath + "\\SelfAssessmentTemplate.html");
            htmlTemplate = htmlTemplate.Replace("{{ domainsSection }}", domainsSection);

            //Populate all the data into the template
            var template = Handlebars.Compile(htmlTemplate);
            var html = template(cleanedUpData);

           


            if (!returnHtml)    //make a PDF!
            {
                try
                {
                    html.Replace("\r\n", "");
                    var doc = new HtmlToPdfDocument()
                    {
                        GlobalSettings = {
                        ColorMode = DinkToPdf.ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = DinkToPdf.PaperKind.A4,
                        Margins = new MarginSettings() { Top = 10 },
                        ViewportSize = "1024x768"
                },
                        Objects = {
                    new ObjectSettings() {
                        // PagesCount = true,
                        // HtmlContent = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In consectetur mauris eget ultrices  iaculis. Ut                               odio viverra, molestie lectus nec, venenatis turpis.",
                        HtmlContent = html,
                        //Page = "https://purecss.io/forms/",
                        //Page = "file:///c:/temp/SampleStudentPassport.html",
                        WebSettings = { DefaultEncoding = "utf-8" }
                        // HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                    }
                }
                    };


                    return File(
                                      converter.Convert(doc),
                                        "application/pdf",
                                        "self-assessment.pdf");



                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading PDF for some reason\n" + ex.ToString());
                    throw;
                }

            }
            else
            {

                //Just return the HTML so it can be displayed on a webpage
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.OK,
                    Content = html.Replace("\r\n", "")
                };
            }

        }



        //These probably don't need to be class vars anymore
        private Dictionary<int, float> domainScores = new Dictionary<int, float>();
        private Dictionary<int, int> domain1Scores = new Dictionary<int, int>();
        private Dictionary<int, int> domain2Scores = new Dictionary<int, int>();
        private Dictionary<int, int> domain3Scores = new Dictionary<int, int>();
        private Dictionary<int, int> domain4Scores = new Dictionary<int, int>();
        private Dictionary<int, int> domain5Scores = new Dictionary<int, int>();
        private int domain1Competency1Score = 0;
        private int domain1Competency2Score = 0;
        private int domain1Competency3Score = 0;
        private int domain1Competency4Score = 0;
        private int domain1Competency5Score = 0;
        private int domain1Competency6Score = 0;
        private int domain2Competency1Score = 0;
        private int domain2Competency2Score = 0;
        private int domain2Competency3Score = 0;
        private int domain2Competency4Score = 0;
        private int domain3Competency1Score = 0;
        private int domain3Competency2Score = 0;
        private int domain3Competency3Score = 0;
        private int domain3Competency4Score = 0;
        private int domain4Competency1Score = 0;
        private int domain4Competency2Score = 0;
        private int domain4Competency3Score = 0;
        private int domain4Competency4Score = 0;
        private int domain4Competency5Score = 0;
        private int domain5Competency1Score = 0;
        private int domain5Competency2Score = 0;
        private int domain5Competency3Score = 0;
        private int domain5Competency4Score = 0;

        //The template contains a grid for each Domain; each row represents a Competency within the Domain; each cell in a row is an answer or selection.
        //Each cell will have a template value of something like d1c1a1 (for Domain 1, Competency 1, Answer 1) or d4c3a2 (for Domain 4, Competency 3, Answer 2),
        //and each cell will get populated with a checkmark if selected or remain blank. The getCompetencyLevel() method returns the checkmark or a blank.
        private Dictionary<string, string> PrepareSelfAssessmentData(SelfAssessment selfAssessment, List<ResourceLink> resources)
        {
            //preparedData contains all the template placeholders and the values to fill them with
            var preparedData = new Dictionary<string, String>();

            preparedData.Add("Date", selfAssessment.Created.ToShortDateString());

            //Domain 1 Competencies
            preparedData.Add("d1c1a1", getCompetencyLevel(selfAssessment.Domain1Competency1, 1));
            preparedData.Add("d1c1a2", getCompetencyLevel(selfAssessment.Domain1Competency1, 2));
            preparedData.Add("d1c1a3", getCompetencyLevel(selfAssessment.Domain1Competency1, 3));
            preparedData.Add("d1c1a4", getCompetencyLevel(selfAssessment.Domain1Competency1, 4));
            preparedData.Add("d1c1a5", getCompetencyLevel(selfAssessment.Domain1Competency1, 5));

            preparedData.Add("d1c2a1", getCompetencyLevel(selfAssessment.Domain1Competency2, 1));
            preparedData.Add("d1c2a2", getCompetencyLevel(selfAssessment.Domain1Competency2, 2));
            preparedData.Add("d1c2a3", getCompetencyLevel(selfAssessment.Domain1Competency2, 3));
            preparedData.Add("d1c2a4", getCompetencyLevel(selfAssessment.Domain1Competency2, 4));
            preparedData.Add("d1c2a5", getCompetencyLevel(selfAssessment.Domain1Competency2, 5));

            preparedData.Add("d1c3a1", getCompetencyLevel(selfAssessment.Domain1Competency3, 1));
            preparedData.Add("d1c3a2", getCompetencyLevel(selfAssessment.Domain1Competency3, 2));
            preparedData.Add("d1c3a3", getCompetencyLevel(selfAssessment.Domain1Competency3, 3));
            preparedData.Add("d1c3a4", getCompetencyLevel(selfAssessment.Domain1Competency3, 4));
            preparedData.Add("d1c3a5", getCompetencyLevel(selfAssessment.Domain1Competency3, 5));

            preparedData.Add("d1c4a1", getCompetencyLevel(selfAssessment.Domain1Competency4, 1));
            preparedData.Add("d1c4a2", getCompetencyLevel(selfAssessment.Domain1Competency4, 2));
            preparedData.Add("d1c4a3", getCompetencyLevel(selfAssessment.Domain1Competency4, 3));
            preparedData.Add("d1c4a4", getCompetencyLevel(selfAssessment.Domain1Competency4, 4));
            preparedData.Add("d1c4a5", getCompetencyLevel(selfAssessment.Domain1Competency4, 5));

            preparedData.Add("d1c5a1", getCompetencyLevel(selfAssessment.Domain1Competency5, 1));
            preparedData.Add("d1c5a2", getCompetencyLevel(selfAssessment.Domain1Competency5, 2));
            preparedData.Add("d1c5a3", getCompetencyLevel(selfAssessment.Domain1Competency5, 3));
            preparedData.Add("d1c5a4", getCompetencyLevel(selfAssessment.Domain1Competency5, 4));
            preparedData.Add("d1c5a5", getCompetencyLevel(selfAssessment.Domain1Competency5, 5));

            preparedData.Add("d1c6a1", getCompetencyLevel(selfAssessment.Domain1Competency6, 1));
            preparedData.Add("d1c6a2", getCompetencyLevel(selfAssessment.Domain1Competency6, 2));
            preparedData.Add("d1c6a3", getCompetencyLevel(selfAssessment.Domain1Competency6, 3));
            preparedData.Add("d1c6a4", getCompetencyLevel(selfAssessment.Domain1Competency6, 4));
            preparedData.Add("d1c6a5", getCompetencyLevel(selfAssessment.Domain1Competency6, 5));

            //Domain 2 Competencies
            preparedData.Add("d2c1a1", getCompetencyLevel(selfAssessment.Domain2Competency1, 1));
            preparedData.Add("d2c1a2", getCompetencyLevel(selfAssessment.Domain2Competency1, 2));
            preparedData.Add("d2c1a3", getCompetencyLevel(selfAssessment.Domain2Competency1, 3));
            preparedData.Add("d2c1a4", getCompetencyLevel(selfAssessment.Domain2Competency1, 4));
            preparedData.Add("d2c1a5", getCompetencyLevel(selfAssessment.Domain2Competency1, 5));

            preparedData.Add("d2c2a1", getCompetencyLevel(selfAssessment.Domain2Competency2, 1));
            preparedData.Add("d2c2a2", getCompetencyLevel(selfAssessment.Domain2Competency2, 2));
            preparedData.Add("d2c2a3", getCompetencyLevel(selfAssessment.Domain2Competency2, 3));
            preparedData.Add("d2c2a4", getCompetencyLevel(selfAssessment.Domain2Competency2, 4));
            preparedData.Add("d2c2a5", getCompetencyLevel(selfAssessment.Domain2Competency2, 5));

            preparedData.Add("d2c3a1", getCompetencyLevel(selfAssessment.Domain2Competency3, 1));
            preparedData.Add("d2c3a2", getCompetencyLevel(selfAssessment.Domain2Competency3, 2));
            preparedData.Add("d2c3a3", getCompetencyLevel(selfAssessment.Domain2Competency3, 3));
            preparedData.Add("d2c3a4", getCompetencyLevel(selfAssessment.Domain2Competency3, 4));
            preparedData.Add("d2c3a5", getCompetencyLevel(selfAssessment.Domain2Competency3, 5));

            preparedData.Add("d2c4a1", getCompetencyLevel(selfAssessment.Domain2Competency4, 1));
            preparedData.Add("d2c4a2", getCompetencyLevel(selfAssessment.Domain2Competency4, 2));
            preparedData.Add("d2c4a3", getCompetencyLevel(selfAssessment.Domain2Competency4, 3));
            preparedData.Add("d2c4a4", getCompetencyLevel(selfAssessment.Domain2Competency4, 4));
            preparedData.Add("d2c4a5", getCompetencyLevel(selfAssessment.Domain2Competency4, 5));

            //Domain 3 Competencies
            preparedData.Add("d3c1a1", getCompetencyLevel(selfAssessment.Domain3Competency1, 1));
            preparedData.Add("d3c1a2", getCompetencyLevel(selfAssessment.Domain3Competency1, 2));
            preparedData.Add("d3c1a3", getCompetencyLevel(selfAssessment.Domain3Competency1, 3));
            preparedData.Add("d3c1a4", getCompetencyLevel(selfAssessment.Domain3Competency1, 4));
            preparedData.Add("d3c1a5", getCompetencyLevel(selfAssessment.Domain3Competency1, 5));

            preparedData.Add("d3c2a1", getCompetencyLevel(selfAssessment.Domain3Competency2, 1));
            preparedData.Add("d3c2a2", getCompetencyLevel(selfAssessment.Domain3Competency2, 2));
            preparedData.Add("d3c2a3", getCompetencyLevel(selfAssessment.Domain3Competency2, 3));
            preparedData.Add("d3c2a4", getCompetencyLevel(selfAssessment.Domain3Competency2, 4));
            preparedData.Add("d3c2a5", getCompetencyLevel(selfAssessment.Domain3Competency2, 5));

            preparedData.Add("d3c3a1", getCompetencyLevel(selfAssessment.Domain3Competency3, 1));
            preparedData.Add("d3c3a2", getCompetencyLevel(selfAssessment.Domain3Competency3, 2));
            preparedData.Add("d3c3a3", getCompetencyLevel(selfAssessment.Domain3Competency3, 3));
            preparedData.Add("d3c3a4", getCompetencyLevel(selfAssessment.Domain3Competency3, 4));
            preparedData.Add("d3c3a5", getCompetencyLevel(selfAssessment.Domain3Competency3, 5));

            preparedData.Add("d3c4a1", getCompetencyLevel(selfAssessment.Domain3Competency4, 1));
            preparedData.Add("d3c4a2", getCompetencyLevel(selfAssessment.Domain3Competency4, 2));
            preparedData.Add("d3c4a3", getCompetencyLevel(selfAssessment.Domain3Competency4, 3));
            preparedData.Add("d3c4a4", getCompetencyLevel(selfAssessment.Domain3Competency4, 4));
            preparedData.Add("d3c4a5", getCompetencyLevel(selfAssessment.Domain3Competency4, 5));

            //Domain 4 Competencies
            preparedData.Add("d4c1a1", getCompetencyLevel(selfAssessment.Domain4Competency1, 1));
            preparedData.Add("d4c1a2", getCompetencyLevel(selfAssessment.Domain4Competency1, 2));
            preparedData.Add("d4c1a3", getCompetencyLevel(selfAssessment.Domain4Competency1, 3));
            preparedData.Add("d4c1a4", getCompetencyLevel(selfAssessment.Domain4Competency1, 4));
            preparedData.Add("d4c1a5", getCompetencyLevel(selfAssessment.Domain4Competency1, 5));

            preparedData.Add("d4c2a1", getCompetencyLevel(selfAssessment.Domain4Competency2, 1));
            preparedData.Add("d4c2a2", getCompetencyLevel(selfAssessment.Domain4Competency2, 2));
            preparedData.Add("d4c2a3", getCompetencyLevel(selfAssessment.Domain4Competency2, 3));
            preparedData.Add("d4c2a4", getCompetencyLevel(selfAssessment.Domain4Competency2, 4));
            preparedData.Add("d4c2a5", getCompetencyLevel(selfAssessment.Domain4Competency2, 5));

            preparedData.Add("d4c3a1", getCompetencyLevel(selfAssessment.Domain4Competency3, 1));
            preparedData.Add("d4c3a2", getCompetencyLevel(selfAssessment.Domain4Competency3, 2));
            preparedData.Add("d4c3a3", getCompetencyLevel(selfAssessment.Domain4Competency3, 3));
            preparedData.Add("d4c3a4", getCompetencyLevel(selfAssessment.Domain4Competency3, 4));
            preparedData.Add("d4c3a5", getCompetencyLevel(selfAssessment.Domain4Competency3, 5));

            preparedData.Add("d4c4a1", getCompetencyLevel(selfAssessment.Domain4Competency4, 1));
            preparedData.Add("d4c4a2", getCompetencyLevel(selfAssessment.Domain4Competency4, 2));
            preparedData.Add("d4c4a3", getCompetencyLevel(selfAssessment.Domain4Competency4, 3));
            preparedData.Add("d4c4a4", getCompetencyLevel(selfAssessment.Domain4Competency4, 4));
            preparedData.Add("d4c4a5", getCompetencyLevel(selfAssessment.Domain4Competency4, 5));

            preparedData.Add("d4c5a1", getCompetencyLevel(selfAssessment.Domain4Competency5, 1));
            preparedData.Add("d4c5a2", getCompetencyLevel(selfAssessment.Domain4Competency5, 2));
            preparedData.Add("d4c5a3", getCompetencyLevel(selfAssessment.Domain4Competency5, 3));
            preparedData.Add("d4c5a4", getCompetencyLevel(selfAssessment.Domain4Competency5, 4));
            preparedData.Add("d4c5a5", getCompetencyLevel(selfAssessment.Domain4Competency5, 5));

            //Domain 5 Competencies
            preparedData.Add("d5c1a1", getCompetencyLevel(selfAssessment.Domain5Competency1, 1));
            preparedData.Add("d5c1a2", getCompetencyLevel(selfAssessment.Domain5Competency1, 2));
            preparedData.Add("d5c1a3", getCompetencyLevel(selfAssessment.Domain5Competency1, 3));
            preparedData.Add("d5c1a4", getCompetencyLevel(selfAssessment.Domain5Competency1, 4));
            preparedData.Add("d5c1a5", getCompetencyLevel(selfAssessment.Domain5Competency1, 5));

            preparedData.Add("d5c2a1", getCompetencyLevel(selfAssessment.Domain5Competency2, 1));
            preparedData.Add("d5c2a2", getCompetencyLevel(selfAssessment.Domain5Competency2, 2));
            preparedData.Add("d5c2a3", getCompetencyLevel(selfAssessment.Domain5Competency2, 3));
            preparedData.Add("d5c2a4", getCompetencyLevel(selfAssessment.Domain5Competency2, 4));
            preparedData.Add("d5c2a5", getCompetencyLevel(selfAssessment.Domain5Competency2, 5));

            preparedData.Add("d5c3a1", getCompetencyLevel(selfAssessment.Domain5Competency3, 1));
            preparedData.Add("d5c3a2", getCompetencyLevel(selfAssessment.Domain5Competency3, 2));
            preparedData.Add("d5c3a3", getCompetencyLevel(selfAssessment.Domain5Competency3, 3));
            preparedData.Add("d5c3a4", getCompetencyLevel(selfAssessment.Domain5Competency3, 4));
            preparedData.Add("d5c3a5", getCompetencyLevel(selfAssessment.Domain5Competency3, 5));

            preparedData.Add("d5c4a1", getCompetencyLevel(selfAssessment.Domain5Competency4, 1));
            preparedData.Add("d5c4a2", getCompetencyLevel(selfAssessment.Domain5Competency4, 2));
            preparedData.Add("d5c4a3", getCompetencyLevel(selfAssessment.Domain5Competency4, 3));
            preparedData.Add("d5c4a4", getCompetencyLevel(selfAssessment.Domain5Competency4, 4));
            preparedData.Add("d5c4a5", getCompetencyLevel(selfAssessment.Domain5Competency4, 5));

            //Resource Titles
            preparedData.Add("d1c1RecommendedResource", getResourceTitle("d1c1", resources, selfAssessment.Domain1Competency1));
            preparedData.Add("d1c2RecommendedResource", getResourceTitle("d1c2", resources, selfAssessment.Domain1Competency2));
            preparedData.Add("d1c3RecommendedResource", getResourceTitle("d1c3", resources, selfAssessment.Domain1Competency3));
            preparedData.Add("d1c4RecommendedResource", getResourceTitle("d1c4", resources, selfAssessment.Domain1Competency4));
            preparedData.Add("d1c5RecommendedResource", getResourceTitle("d1c5", resources, selfAssessment.Domain1Competency5));
            preparedData.Add("d1c6RecommendedResource", getResourceTitle("d1c6", resources, selfAssessment.Domain1Competency6));

            preparedData.Add("d2c1RecommendedResource", getResourceTitle("d2c1", resources, selfAssessment.Domain2Competency1));
            preparedData.Add("d2c2RecommendedResource", getResourceTitle("d2c2", resources, selfAssessment.Domain2Competency2));
            preparedData.Add("d2c3RecommendedResource", getResourceTitle("d2c3", resources, selfAssessment.Domain2Competency3));
            preparedData.Add("d2c4RecommendedResource", getResourceTitle("d2c4", resources, selfAssessment.Domain2Competency4));

            preparedData.Add("d3c1RecommendedResource", getResourceTitle("d3c1", resources, selfAssessment.Domain3Competency1));
            preparedData.Add("d3c2RecommendedResource", getResourceTitle("d3c2", resources, selfAssessment.Domain3Competency2));
            preparedData.Add("d3c3RecommendedResource", getResourceTitle("d3c3", resources, selfAssessment.Domain3Competency3));
            preparedData.Add("d3c4RecommendedResource", getResourceTitle("d3c4", resources, selfAssessment.Domain3Competency4));

            preparedData.Add("d4c1RecommendedResource", getResourceTitle("d4c1", resources, selfAssessment.Domain4Competency1));
            preparedData.Add("d4c2RecommendedResource", getResourceTitle("d4c2", resources, selfAssessment.Domain4Competency2));
            preparedData.Add("d4c3RecommendedResource", getResourceTitle("d4c3", resources, selfAssessment.Domain4Competency3));
            preparedData.Add("d4c4RecommendedResource", getResourceTitle("d4c4", resources, selfAssessment.Domain4Competency4));
            preparedData.Add("d4c5RecommendedResource", getResourceTitle("d4c5", resources, selfAssessment.Domain4Competency5));

            preparedData.Add("d5c1RecommendedResource", getResourceTitle("d5c1", resources, selfAssessment.Domain5Competency1));
            preparedData.Add("d5c2RecommendedResource", getResourceTitle("d5c2", resources, selfAssessment.Domain5Competency2));
            preparedData.Add("d5c3RecommendedResource", getResourceTitle("d5c3", resources, selfAssessment.Domain5Competency3));
            preparedData.Add("d5c4RecommendedResource", getResourceTitle("d5c4", resources, selfAssessment.Domain5Competency4));

            //Resource URLs
            preparedData.Add("d1c1RecommendedResourceUrl", getResourceUrl("d1c1", resources, selfAssessment.Domain1Competency1));
            preparedData.Add("d1c2RecommendedResourceUrl", getResourceUrl("d1c2", resources, selfAssessment.Domain1Competency2));
            preparedData.Add("d1c3RecommendedResourceUrl", getResourceUrl("d1c3", resources, selfAssessment.Domain1Competency3));
            preparedData.Add("d1c4RecommendedResourceUrl", getResourceUrl("d1c4", resources, selfAssessment.Domain1Competency4));
            preparedData.Add("d1c5RecommendedResourceUrl", getResourceUrl("d1c5", resources, selfAssessment.Domain1Competency5));
            preparedData.Add("d1c6RecommendedResourceUrl", getResourceUrl("d1c6", resources, selfAssessment.Domain1Competency6));

            preparedData.Add("d2c1RecommendedResourceUrl", getResourceUrl("d2c1", resources, selfAssessment.Domain2Competency1));
            preparedData.Add("d2c2RecommendedResourceUrl", getResourceUrl("d2c2", resources, selfAssessment.Domain2Competency2));
            preparedData.Add("d2c3RecommendedResourceUrl", getResourceUrl("d2c3", resources, selfAssessment.Domain2Competency3));
            preparedData.Add("d2c4RecommendedResourceUrl", getResourceUrl("d2c4", resources, selfAssessment.Domain2Competency4));

            preparedData.Add("d3c1RecommendedResourceUrl", getResourceUrl("d3c1", resources, selfAssessment.Domain3Competency1));
            preparedData.Add("d3c2RecommendedResourceUrl", getResourceUrl("d3c2", resources, selfAssessment.Domain3Competency2));
            preparedData.Add("d3c3RecommendedResourceUrl", getResourceUrl("d3c3", resources, selfAssessment.Domain3Competency3));
            preparedData.Add("d3c4RecommendedResourceUrl", getResourceUrl("d3c4", resources, selfAssessment.Domain3Competency4));

            preparedData.Add("d4c1RecommendedResourceUrl", getResourceUrl("d4c1", resources, selfAssessment.Domain4Competency1));
            preparedData.Add("d4c2RecommendedResourceUrl", getResourceUrl("d4c2", resources, selfAssessment.Domain4Competency2));
            preparedData.Add("d4c3RecommendedResourceUrl", getResourceUrl("d4c3", resources, selfAssessment.Domain4Competency3));
            preparedData.Add("d4c4RecommendedResourceUrl", getResourceUrl("d4c4", resources, selfAssessment.Domain4Competency4));
            preparedData.Add("d4c5RecommendedResourceUrl", getResourceUrl("d4c5", resources, selfAssessment.Domain4Competency5));

            preparedData.Add("d5c1RecommendedResourceUrl", getResourceUrl("d5c1", resources, selfAssessment.Domain5Competency1));
            preparedData.Add("d5c2RecommendedResourceUrl", getResourceUrl("d5c2", resources, selfAssessment.Domain5Competency2));
            preparedData.Add("d5c3RecommendedResourceUrl", getResourceUrl("d5c3", resources, selfAssessment.Domain5Competency3));
            preparedData.Add("d5c4RecommendedResourceUrl", getResourceUrl("d5c4", resources, selfAssessment.Domain5Competency4));

            //Competencies & Next Competencies
            preparedData.Add("d1c1Level", getCompetency("d1c1", resources, selfAssessment.Domain1Competency1));
            preparedData.Add("d1c2Level", getCompetency("d1c2", resources, selfAssessment.Domain1Competency2));
            preparedData.Add("d1c3Level", getCompetency("d1c3", resources, selfAssessment.Domain1Competency3));
            preparedData.Add("d1c4Level", getCompetency("d1c4", resources, selfAssessment.Domain1Competency4));
            preparedData.Add("d1c5Level", getCompetency("d1c5", resources, selfAssessment.Domain1Competency5));
            preparedData.Add("d1c6Level", getCompetency("d1c6", resources, selfAssessment.Domain1Competency6));

            preparedData.Add("d2c1Level", getCompetency("d2c1", resources, selfAssessment.Domain2Competency1));
            preparedData.Add("d2c2Level", getCompetency("d2c2", resources, selfAssessment.Domain2Competency2));
            preparedData.Add("d2c3Level", getCompetency("d2c3", resources, selfAssessment.Domain2Competency3));
            preparedData.Add("d2c4Level", getCompetency("d2c4", resources, selfAssessment.Domain2Competency4));

            preparedData.Add("d3c1Level", getCompetency("d3c1", resources, selfAssessment.Domain3Competency1));
            preparedData.Add("d3c2Level", getCompetency("d3c2", resources, selfAssessment.Domain3Competency2));
            preparedData.Add("d3c3Level", getCompetency("d3c3", resources, selfAssessment.Domain3Competency3));
            preparedData.Add("d3c4Level", getCompetency("d3c4", resources, selfAssessment.Domain3Competency4));

            preparedData.Add("d4c1Level", getCompetency("d4c1", resources, selfAssessment.Domain4Competency1));
            preparedData.Add("d4c2Level", getCompetency("d4c2", resources, selfAssessment.Domain4Competency2));
            preparedData.Add("d4c3Level", getCompetency("d4c3", resources, selfAssessment.Domain4Competency3));
            preparedData.Add("d4c4Level", getCompetency("d4c4", resources, selfAssessment.Domain4Competency4));
            preparedData.Add("d4c5Level", getCompetency("d4c5", resources, selfAssessment.Domain4Competency5));

            preparedData.Add("d5c1Level", getCompetency("d5c1", resources, selfAssessment.Domain5Competency1));
            preparedData.Add("d5c2Level", getCompetency("d5c2", resources, selfAssessment.Domain5Competency2));
            preparedData.Add("d5c3Level", getCompetency("d5c3", resources, selfAssessment.Domain5Competency3));
            preparedData.Add("d5c4Level", getCompetency("d5c4", resources, selfAssessment.Domain5Competency4));

            preparedData.Add("d1c1NextLevel", getNextCompetency("d1c1", resources, selfAssessment.Domain1Competency1));
            preparedData.Add("d1c2NextLevel", getNextCompetency("d1c2", resources, selfAssessment.Domain1Competency2));
            preparedData.Add("d1c3NextLevel", getNextCompetency("d1c3", resources, selfAssessment.Domain1Competency3));
            preparedData.Add("d1c4NextLevel", getNextCompetency("d1c4", resources, selfAssessment.Domain1Competency4));
            preparedData.Add("d1c5NextLevel", getNextCompetency("d1c5", resources, selfAssessment.Domain1Competency5));
            preparedData.Add("d1c6NextLevel", getNextCompetency("d1c6", resources, selfAssessment.Domain1Competency6));

            preparedData.Add("d2c1NextLevel", getNextCompetency("d2c1", resources, selfAssessment.Domain2Competency1));
            preparedData.Add("d2c2NextLevel", getNextCompetency("d2c2", resources, selfAssessment.Domain2Competency2));
            preparedData.Add("d2c3NextLevel", getNextCompetency("d2c3", resources, selfAssessment.Domain2Competency3));
            preparedData.Add("d2c4NextLevel", getNextCompetency("d2c4", resources, selfAssessment.Domain2Competency4));

            preparedData.Add("d3c1NextLevel", getNextCompetency("d3c1", resources, selfAssessment.Domain3Competency1));
            preparedData.Add("d3c2NextLevel", getNextCompetency("d3c2", resources, selfAssessment.Domain3Competency2));
            preparedData.Add("d3c3NextLevel", getNextCompetency("d3c3", resources, selfAssessment.Domain3Competency3));
            preparedData.Add("d3c4NextLevel", getNextCompetency("d3c4", resources, selfAssessment.Domain3Competency4));

            preparedData.Add("d4c1NextLevel", getNextCompetency("d4c1", resources, selfAssessment.Domain4Competency1));
            preparedData.Add("d4c2NextLevel", getNextCompetency("d4c2", resources, selfAssessment.Domain4Competency2));
            preparedData.Add("d4c3NextLevel", getNextCompetency("d4c3", resources, selfAssessment.Domain4Competency3));
            preparedData.Add("d4c4NextLevel", getNextCompetency("d4c4", resources, selfAssessment.Domain4Competency4));
            preparedData.Add("d4c5NextLevel", getNextCompetency("d4c5", resources, selfAssessment.Domain4Competency5));

            preparedData.Add("d5c1NextLevel", getNextCompetency("d5c1", resources, selfAssessment.Domain5Competency1));
            preparedData.Add("d5c2NextLevel", getNextCompetency("d5c2", resources, selfAssessment.Domain5Competency2));
            preparedData.Add("d5c3NextLevel", getNextCompetency("d5c3", resources, selfAssessment.Domain5Competency3));
            preparedData.Add("d5c4NextLevel", getNextCompetency("d5c4", resources, selfAssessment.Domain5Competency4));


            //Domain & Competency Scores
            var domain1Score = 0;
            domain1Competency1Score = getCompetencyScore(selfAssessment.Domain1Competency1, selfAssessment.Domain1Competency1Important, ref domain1Score);
            domain1Competency2Score = getCompetencyScore(selfAssessment.Domain1Competency2, selfAssessment.Domain1Competency2Important, ref domain1Score);
            domain1Competency3Score = getCompetencyScore(selfAssessment.Domain1Competency3, selfAssessment.Domain1Competency3Important, ref domain1Score);
            domain1Competency4Score = getCompetencyScore(selfAssessment.Domain1Competency4, selfAssessment.Domain1Competency4Important, ref domain1Score);
            domain1Competency5Score = getCompetencyScore(selfAssessment.Domain1Competency5, selfAssessment.Domain1Competency5Important, ref domain1Score);
            domain1Competency6Score = getCompetencyScore(selfAssessment.Domain1Competency6, selfAssessment.Domain1Competency6Important, ref domain1Score);
            domain1Scores.Add(1, domain1Competency1Score);
            domain1Scores.Add(2, domain1Competency2Score);
            domain1Scores.Add(3, domain1Competency3Score);
            domain1Scores.Add(4, domain1Competency4Score);
            domain1Scores.Add(5, domain1Competency5Score);
            domain1Scores.Add(6, domain1Competency6Score);

            var domain2Score = 0;
            domain2Competency1Score = getCompetencyScore(selfAssessment.Domain2Competency1, selfAssessment.Domain2Competency1Important, ref domain2Score);
            domain2Competency2Score = getCompetencyScore(selfAssessment.Domain2Competency2, selfAssessment.Domain2Competency2Important, ref domain2Score);
            domain2Competency3Score = getCompetencyScore(selfAssessment.Domain2Competency3, selfAssessment.Domain2Competency3Important, ref domain2Score);
            domain2Competency4Score = getCompetencyScore(selfAssessment.Domain2Competency4, selfAssessment.Domain2Competency4Important, ref domain2Score);
            domain2Scores.Add(1, domain2Competency1Score);
            domain2Scores.Add(2, domain2Competency2Score);
            domain2Scores.Add(3, domain2Competency3Score);
            domain2Scores.Add(4, domain2Competency4Score);

            var domain3Score = 0;
            domain3Competency1Score = getCompetencyScore(selfAssessment.Domain3Competency1, selfAssessment.Domain3Competency1Important, ref domain3Score);
            domain3Competency2Score = getCompetencyScore(selfAssessment.Domain3Competency2, selfAssessment.Domain3Competency2Important, ref domain3Score);
            domain3Competency3Score = getCompetencyScore(selfAssessment.Domain3Competency3, selfAssessment.Domain3Competency3Important, ref domain3Score);
            domain3Competency4Score = getCompetencyScore(selfAssessment.Domain3Competency4, selfAssessment.Domain3Competency4Important, ref domain3Score);
            domain3Scores.Add(1, domain3Competency1Score);
            domain3Scores.Add(2, domain3Competency2Score);
            domain3Scores.Add(3, domain3Competency3Score);
            domain3Scores.Add(4, domain3Competency4Score);

            var domain4Score = 0;
            domain4Competency1Score = getCompetencyScore(selfAssessment.Domain4Competency1, selfAssessment.Domain4Competency1Important, ref domain4Score);
            domain4Competency2Score = getCompetencyScore(selfAssessment.Domain4Competency2, selfAssessment.Domain4Competency2Important, ref domain4Score);
            domain4Competency3Score = getCompetencyScore(selfAssessment.Domain4Competency3, selfAssessment.Domain4Competency3Important, ref domain4Score);
            domain4Competency4Score = getCompetencyScore(selfAssessment.Domain4Competency4, selfAssessment.Domain4Competency4Important, ref domain4Score);
            domain4Competency5Score = getCompetencyScore(selfAssessment.Domain4Competency5, selfAssessment.Domain4Competency5Important, ref domain4Score);
            domain4Scores.Add(1, domain4Competency1Score);
            domain4Scores.Add(2, domain4Competency2Score);
            domain4Scores.Add(3, domain4Competency3Score);
            domain4Scores.Add(4, domain4Competency4Score);
            domain4Scores.Add(5, domain4Competency5Score);

            var domain5Score = 0;
            domain5Competency1Score = getCompetencyScore(selfAssessment.Domain5Competency1, selfAssessment.Domain5Competency1Important, ref domain5Score);
            domain5Competency2Score = getCompetencyScore(selfAssessment.Domain5Competency2, selfAssessment.Domain5Competency2Important, ref domain5Score);
            domain5Competency3Score = getCompetencyScore(selfAssessment.Domain5Competency3, selfAssessment.Domain5Competency3Important, ref domain5Score);
            domain5Competency4Score = getCompetencyScore(selfAssessment.Domain5Competency4, selfAssessment.Domain5Competency4Important, ref domain5Score);
            domain5Scores.Add(1, domain5Competency1Score);
            domain5Scores.Add(2, domain5Competency2Score);
            domain5Scores.Add(3, domain5Competency3Score);
            domain5Scores.Add(4, domain5Competency4Score);

            //Domain Order
            domainScores.Add(1, (float)(domain1Score / 6.00));
            domainScores.Add(2, (float)(domain2Score / 4.00));
            domainScores.Add(3, (float)(domain3Score / 4.00));
            domainScores.Add(4, (float)(domain4Score / 5.00));
            domainScores.Add(5, (float)(domain5Score / 4.00));

            //Determine how to order the Competencies within each Domain based on answer selection and importance
            int order = 0;
            var letters = new List<string> { "a", "b", "c", "d", "e", "f" };
            foreach (var domain in domainScores.OrderByDescending(x => x.Value))
            {
                preparedData.Add($"domain{domain.Key}CompetencyOrder", letters[order]);
                order += 1;
            }

            return preparedData;
        }

        //Literally return a checkmark if the provided values match; otherwise return a blank space
        private string getCompetencyLevel(int answer, int level)
        {
            return answer == level ? "✓" : " ";
        }

        //Return the Resource Link for a Competency based on the selected answer
        private string getCompetency(string level, List<ResourceLink> resources, int answer)
        {
            if (answer < 6)
            {
                var resource = resources.FirstOrDefault(x => x.Code == level + "a" + answer);

                return resource?.Competency ?? "";
            }
            return "";
        }

        //Return the next Competency listed based on the current Competency selected
        private string getNextCompetency(string level, List<ResourceLink> resources, int answer)
        {
            if (answer < 6)
            {
                var resource = resources.FirstOrDefault(x => x.Code == level + "a" + answer);

                return resource?.NextCompetency ?? "";
            }
            return "";
        }

        //Get the title of the Resource for the selected answer
        private string getResourceTitle(string level, List<ResourceLink> resources, int answer)
        {
            if (answer < 6)
            {
                var resource = resources.FirstOrDefault(x => x.Code == level + "a" + answer);

                return resource?.Title ?? "";
            }
            return "";
        }
        //Get the URL for the recommended Resource for the selected answer
        private string getResourceUrl(string level, List<ResourceLink> resources, int answer)
        {
            if (answer < 6)
            {
                var resource = resources.FirstOrDefault(x => x.Code == level + "a" + answer);

                return resource?.Url ?? "";
            }
            return "";
        }

        //Calculate the Competency score for the selected answer; add to the Competency's Domain's score, by ref
        private int getCompetencyScore(int answer, bool isImportant, ref int domainScore)
        {
            var score = 1;
            switch (answer)
            {
                case 1: score = 4 * (isImportant ? 2 : 1); break;
                case 2: score = 3 * (isImportant ? 2 : 1); break;
                case 3: score = 2 * (isImportant ? 2 : 1); break;
                case 4:
                case 5: score = 1 * (isImportant ? 2 : 1); break;
            }

            domainScore += score;

            return score;
        }
    }
}
