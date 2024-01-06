using MyBGList;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
opts.ResolveConflictingActions(apiDesc => apiDesc.First()) /*basically telling Swagger to resolve all conflicts related
to duplicate routing handlers by always taking the first one found (and ignoring the others).*/
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //will be only included if the app is run in the Development environment
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseSwagger")) // 4. current initialization strategy to ensure that they will be used only if the UseSwagger configuration setting is set to True.
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage")) 
    app.UseDeveloperExceptionPage(); /* the /error string parameter, meaning that we want to handle these errors with a
    dedicated HTTP route - which now we need to implement.
    with a Controller or using the Minimal API.*/

else
{
    app.UseExceptionHandler("/error");/* <- handles HTTPlevel
exceptions, but it�s better suited for non-development
environments since it sends all the relevant error info to a customizable
handler instead of generating a detailed error response and automatically
present it to the end-user*/
}
/*the ExceptionHandlerMiddleware will be used instead of the DeveloperExceptionPageMiddleware, since the value of the
UseDeveloperExceptionPage key has been previously set to false in the appsetting.json file.*/

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/error", () => Results.Problem()); // handle app.UseExceptionHandler("/error") Using Minimal API
app.MapGet("/error/test", () => { throw new Exception("test"); }); // we want to produce an error to test the way its handled

//app.MapGet("/BoardGames", () => new[] { // we keep the controler instead of this Minimal API cose :)
//    new BoardGame() {
//        Id = 1,
//        Name = "Axis & Allies",
//        Year = 1981
//    },
//    new BoardGame() {
//        Id = 2,
//        Name = "Citadels",
//        Year = 2000
//    },
//    new BoardGame() {
//        Id = 3,
//        Name = "Terraforming Mars",
//        Year = 2016
//    }
//});

app.MapControllers();

app.Run();


//74 RESTful rozdzial 3