using Amazon.CDK;

namespace NukeCdkCICDExampleCdk
{
    public class NukeCdkCICDExampleCdkStack : Stack
    {
        internal NukeCdkCICDExampleCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var awsRegion = new CfnParameter(this, "awsRegion",
                new CfnParameterProps
                {
                    Type = "String",
                    Description = "Aws region everything is running in"
                });
            
            var gitBranchName = new CfnParameter(this, "gitBranchName",
                new CfnParameterProps
                {
                    Type = "String",
                    Description = "Git Branch Name"
                });
            
            var awsAccount = new CfnParameter(this, "awsAccount",
                new CfnParameterProps
                {
                    Type = "String",
                    Description = "Aws Account"
                });
            
            
            var createCodePipelineRequest = new CreateCodePipelineRequest()
            {
                AwsRegion = awsRegion.ValueAsString,
                GitBranchName = gitBranchName.ValueAsString,
                AwsAccount = awsAccount.ValueAsString,
            };

            var nukeCdkCICDExampleCdkCodePipeline = new NukeCdkCICDExampleCodePipeline(this,
                "NukeCdkCICDExampleCdk CodePipeline", createCodePipelineRequest);
        }
    }
}
