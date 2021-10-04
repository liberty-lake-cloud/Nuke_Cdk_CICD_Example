namespace Temp
{
    public class CreateCodePipelineRequest
    {
        public string AwsRegion { get; set; }
        public string GitBranchName { get; set; }
        public string AwsAccount { get; set; }
    }
}