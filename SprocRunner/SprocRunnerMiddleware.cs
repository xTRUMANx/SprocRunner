using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SprocRunner
{
    public class SprocRunnerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly SprocRunnerOptions options;

        public SprocRunnerMiddleware(RequestDelegate next, SprocRunnerOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method))
            {
                if (context.Request.Path.StartsWithSegments(new PathString("/" + options.RoutePrefix)))
                {
                    var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    var sprocExecRequest = JsonSerializer.Deserialize<SprocExecRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (sprocExecRequest == null)
                    {
                        await next(context);
                        return;
                    }

                    var validationResults = ValidateRequest(sprocExecRequest);

                    if (validationResults.Count > 0)
                    {
                        context.Response.StatusCode = 400;

                        var validationErrors = validationResults.Select(r => new { r.MemberNames, r.ErrorMessage });

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(validationErrors));

                        await next(context);

                        return;
                    }

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";

                    var simpleSqlRunner = new SimpleSqlRunner.SqlRunner(options.ConnectionString);

                    var resultSets = await simpleSqlRunner.RunSqlAsync(sprocExecRequest.FullyQualifiedSproc, isSproc: true);

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(resultSets));
                    return;
                }
            }
            await next(context);
        }

        List<ValidationResult> ValidateRequest(SprocExecRequest sprocExecRequest)
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();

            if (sprocExecRequest == null)
            {
                validationResults.Add(new ValidationResult("Invalid request."));
                return validationResults;
            }

            var isValid = Validator.TryValidateObject(sprocExecRequest, new ValidationContext(sprocExecRequest), validationResults);

            return validationResults;
        }
    }

    public class SprocExecRequest
    {
        [Required]
        public string Db { get; set; } = "";

        [Required]
        public string Schema { get; set; } = "";

        [Required]
        public string Sproc { get; set; } = "";

        public string FullyQualifiedSproc => $"{Db}.{Schema}.{Sproc}";
    }

}
