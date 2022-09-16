using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.S3;
using Imagegram.WebApi.Services;
using Microsoft.Extensions.Configuration;

namespace Imagegram.WebApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();

        var clientId = Configuration.GetValue<string>("ClientId"); ;
        var clientSecret = Configuration.GetValue<string>("ClientSecret");

        var credentials = new BasicAWSCredentials(clientId, clientSecret);
        var config = new AmazonDynamoDBConfig()
        {
            RegionEndpoint = RegionEndpoint.APSoutheast1
        };
        var client = new AmazonDynamoDBClient(credentials, config);
        services.AddSingleton<IAmazonDynamoDB>(client);
        services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

        var s3Client = new AmazonS3Client(credentials, new AmazonS3Config { RegionEndpoint = RegionEndpoint.APSoutheast1});
        services.AddSingleton<IAmazonS3>(s3Client);

        services.AddSingleton<IPostService, PostService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app
        .UseSwagger()
        .UseSwaggerUI(setup =>
        {
            string swaggerJsonBasePath = string.IsNullOrWhiteSpace(setup.RoutePrefix) ? "." : "..";
            setup.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "Version 1.0");
            setup.OAuthAppName("Lambda Api");
            setup.OAuthUsePkce();
            setup.OAuthScopeSeparator(" ");
        });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}