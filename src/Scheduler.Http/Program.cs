

using Scheduler.Http.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(opts =>
{
  opts.AddPolicy("AllowFrontend", policy =>
    policy.WithOrigins("")
      .AllowAnyHeader()
      .AllowAnyMethod());
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
app.UseConfigurationPage(opts =>
{
  opts.RoutePath = "/configuration";
});
app.Run();
