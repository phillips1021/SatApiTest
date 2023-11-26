using Newtonsoft.Json;

namespace SatApiTest.Models
{
    public class SelfAssessmentDTO
    {
        public string Selected { get; set; }

        public string Important { get; set; }

        public string Date { get; set; }

        public string Institution { get; set; }

        public string getJsonData()
        {

            return JsonConvert.SerializeObject(this);

        }

        public List<string> Competencies()
        {
            return Selected.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public List<string> ImportantCompetencies()
        {
            return Important.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
