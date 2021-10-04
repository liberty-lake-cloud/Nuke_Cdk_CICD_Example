using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using Amazon.JSII.Runtime.Deputy;
using IStageProps = Amazon.CDK.AWS.CodePipeline.IStageProps;

namespace NukeCdkCICDExampleCdk
{
    public class NukeCdkCICDExampleCodePipeline: Construct
    {
        public NukeCdkCICDExampleCodePipeline(Constructs.Construct scope, string id, CreateCodePipelineRequest createCodePipelineRequest) : base(scope, id)
        {
            Bucket codeBuildBucket = new Bucket(this, "NukeCdkCICDExample Code Pipeline Bucket");
            
            var codeBuildRole = new Role(this, "NukeCdkCICDExample Codebuild Role", new RoleProps{AssumedBy = new ServicePrincipal("codebuild.amazonaws.com")});
            codeBuildRole.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this,"S3 full access managed policy codebuild","arn:aws:iam::aws:policy/AmazonS3FullAccess"));
            codeBuildRole.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this,"SecretsManager full access managed policy codebuild","arn:aws:iam::aws:policy/SecretsManagerReadWrite"));
            codeBuildRole.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this,"CodePipeline full access managed policy codebuild","arn:aws:iam::aws:policy/AWSCodePipeline_FullAccess"));
            codeBuildRole.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this,"CodeBuild full access managed policy codebuild","arn:aws:iam::aws:policy/AWSCodeBuildAdminAccess"));
            codeBuildRole.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this, "IAM full access managed policy codebuild", "arn:aws:iam::aws:policy/IAMFullAccess"));
            
            //Code Build
            
            PipelineProject project = new PipelineProject(this, "NukeCdkCICDExample Pipeline Project", new PipelineProjectProps
            {
                ProjectName = "NukeCdkCICDExample-CodeBuild",
                Role = codeBuildRole,
                BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                {
                    ["version"] = "0.2",
                    ["phases"] = new Dictionary<string, object>
                    {
                        ["install"] = new Dictionary<string, object>
                        {
                            ["runtime-versions"] = new Dictionary<string, object>()
                            {
                                {"dotnet", "5.0"}
                            },
                            ["commands"] = new []
                            {
                                "export PATH=\"$PATH:/root/.dotnet/tools\"",
                                "dotnet tool install --global Nuke.GlobalTool",
                                "npm install -g aws-cdk"
                            }
                        },
                        ["build"] = new Dictionary<string, object>
                        {
                            ["commands"] = $"nuke DeployNukeCdkCICDExampleCdkStack " +
                                                $"--awsregion {createCodePipelineRequest.AwsRegion} " +
                                                $"--gitbranchname {createCodePipelineRequest.GitBranchName} " +
                                                $"--awsaccount {createCodePipelineRequest.AwsAccount} "
                        }
                    }
                }),
                Environment = new BuildEnvironment
                {
                    EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>{
                        {"DOTNET_ROOT", new BuildEnvironmentVariable
                        {
                            Value = "/root/.dotnet"
                        }},
                        {"CDK_DEFAULT_ACCOUNT", new BuildEnvironmentVariable
                        {
                            Value = createCodePipelineRequest.AwsAccount
                        }},
                        {"CDK_DEFAULT_REGION", new BuildEnvironmentVariable
                        {
                            Value = createCodePipelineRequest.AwsRegion
                        }}
                    },
                    BuildImage = LinuxBuildImage.STANDARD_5_0
                }
            });
            
            var sourceOutput = new Artifact_();
            
            var githubToken = SecretValue.SecretsManager("Github_Token");

            Pipeline pipeline = new Pipeline(this, "NukeCdkCICDExample Pipeline", new PipelineProps
            {
                ArtifactBucket = codeBuildBucket,
                Stages = new IStageProps[]{new StageOptions
                    {
                        StageName = "Source",
                        Actions = new IAction[]{new GitHubSourceAction(new GitHubSourceActionProps
                        {
                            ActionName = "Github_Source",
                            OauthToken = githubToken,
                            Owner = "liberty-lake-cloud",
                            Repo = "Nuke_Cdk_CICD_Example",
                            Branch = createCodePipelineRequest.GitBranchName,
                            Output = sourceOutput,
                            Trigger = GitHubTrigger.WEBHOOK
                        }) }
                    },
                    new StageOptions
                    {
                        StageName = "Build",
                        Actions = new IAction[]{ new CodeBuildAction(new CodeBuildActionProps
                        {
                            ActionName = "NukeCdkCICDExample_Build",
                            Project = project,
                            Input = sourceOutput,
                        })}
                    }
                }
            });
        }

        protected NukeCdkCICDExampleCodePipeline(ByRefValue reference) : base(reference)
        {
        }

        protected NukeCdkCICDExampleCodePipeline(DeputyProps props) : base(props)
        {
        }
    }
}