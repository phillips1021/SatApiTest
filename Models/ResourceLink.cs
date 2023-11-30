using CsvHelper.Configuration;

namespace Stfm.Generator.SelfAssessmentTool
{
    public class ResourceLink
    {
        public string Code { get; set; }
        public string Competency { get; set; }
        public string NextCompetency { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public sealed class ResourceLinkMap : ClassMap<ResourceLink>
    {
        public ResourceLinkMap()
        {
            Map(m => m.Code);
            Map(m => m.Competency);
            Map(m => m.NextCompetency);
            Map(m => m.Title);
            Map(m => m.Url);
        }
    }
}