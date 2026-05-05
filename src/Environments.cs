namespace csharp_fastapi_template;

using Amazon.DynamoDBv2;
using Amazon.Runtime;
using csharp_fastapi_template.errors;
using csharp_fastapi_template.repo;
using csharp_fastapi_template.repo.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public enum STAGE
{
    DOTENV,
    DEV,
    PROD,
    TEST
}

public static class Environments
{
    public static STAGE GetStage()
    {
        var stageValue = Environment.GetEnvironmentVariable("STAGE");

        // Equivalente ao fallback do Python: se não vier stage, assume TEST localmente.
        if (string.IsNullOrWhiteSpace(stageValue) ||
            string.Equals(stageValue, STAGE.DOTENV.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return STAGE.TEST;
        }

        if (!Enum.TryParse<STAGE>(stageValue, ignoreCase: true, out var stage))
        {
            throw new EnvironmentNotFoundException("STAGE");
        }

        return stage;
    }

    public static void ConfigureItemRepository(IServiceCollection services, IConfiguration configuration)
    {
        var stage = GetStage();

        if (stage == STAGE.TEST)
        {
            services.AddSingleton<IItemRepository, ItemRepositoryMock>();
            return;
        }

        if (stage == STAGE.DEV || stage == STAGE.PROD)
        {
            var tableName = configuration["DYNAMO_TABLE_NAME"];
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new EnvironmentNotFoundException("DYNAMO_TABLE_NAME");
            }

            var endpointUrl = configuration["DYNAMO_ENDPOINT_URL"];
            var awsRegion = configuration["AWS_REGION"] ?? "us-east-1";

            services.AddSingleton<IAmazonDynamoDB>(_ =>
            {
                if (!string.IsNullOrWhiteSpace(endpointUrl))
                {
                    var config = new AmazonDynamoDBConfig
                    {
                        ServiceURL = endpointUrl,
                        AuthenticationRegion = awsRegion
                    };
                    var credentials = new BasicAWSCredentials("dummy", "dummy");
                    return new AmazonDynamoDBClient(credentials, config);
                }

                return new AmazonDynamoDBClient();
            });
            services.AddSingleton<IItemRepository>(sp =>
                new ItemRepositoryDynamo(
                    sp.GetRequiredService<IAmazonDynamoDB>(),
                    tableName
                )
            );
            return;
        }

        throw new EnvironmentNotFoundException("STAGE");
    }
}
