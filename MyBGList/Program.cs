using MyBGList.Attributes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.Models;
using Microsoft.AspNetCore.Diagnostics;
using MyBGList.Constants;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
// Add Logging providers chapter 7 
builder.Logging
    .ClearProviders()
    //.AddSimpleConsole(opts => //Configuring the providers to timestamp its log entries using the HH:mm: ss format and the UTC time zone
    //{
    //    opts.SingleLine = true;
    //    opts.TimestampFormat = "HH:mm:ss";
    //    opts.UseUtcTimestamp = true;
    //})
    .AddSimpleConsole()
    .AddDebug();
// azure chapter 7
//.AddApplicationInsights(/*telemetry => telemetry
//    .ConnectionString = */builder
//    .Configuration["Azure:ApplicationInsights:ConnectionString"],
//    loggerOptions => { });

builder.Host.UseSerilog((ctx, lc) => {
    lc.MinimumLevel.Is(Serilog.Events.LogEventLevel.Warning); 
    lc.MinimumLevel.Override( 
        "MyBGList", Serilog.Events.LogEventLevel.Information);
    lc.ReadFrom.Configuration(ctx.Configuration);
    lc.Enrich.WithMachineName();
    lc.Enrich.WithThreadId(); // Now the MachineName and ThreadId properties will be created automatically in all our log records.
    lc.WriteTo.File("Logs/log.txt",
        outputTemplate: 
            "{Timestamp:HH:mm:ss} [{Level:u3}] " +
            "[{MachineName} #{ThreadId}] " +
            "{Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day);
    /*The above settings will instruct the sink to create a log.txt file in the /Logs/
folder (creating it if it doesn't exist) with a rolling interval of one day*/
    lc.WriteTo.MSSqlServer(
        connectionString:
            ctx.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions
        {
            //configured the SQL Server sink to auto-create it in case it doesn't exist.
            TableName = "LogEvents",
            AutoCreateSqlTable = true
        },
        columnOptions: new ColumnOptions()
        {
            AdditionalColumns = new SqlColumn[]
            {
                new SqlColumn() 
                {
                    ColumnName = "SourceContext",
                    PropertyName = "SourceContext",
                    DataType = System.Data.SqlDbType.NVarChar
                }
            }
        });
        },
        writeToProviders: true);
/* writeToProviders : true => this option ensures that Serilog will pass the log events not only to its sinks, but
also to the logging providers registered through the Microsoft.Extensions.Logging API -*/

// Add services to the container.

builder.Services.AddControllers(opts =>
{
    /*Customize the Model Binding errors*/
    opts.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value '{x}' is invalid");
    opts.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The field {x} must be a number.");
    opts.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    opts.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => $"A value is required.");

    // CACHE PROFILES from chapter 8
    /*ControllerMiddleware’s configuration option that allows us to set up some predefined caching directives and then apply them using a convenient namebased reference, instead of having to repeat them.*/
    opts.CacheProfiles.Add("NoCache",
        new CacheProfile() { NoStore = true });
    opts.CacheProfiles.Add("Any-60",
        new CacheProfile()
        {
            Location = ResponseCacheLocation.Any,
            Duration = 60
        });

    /* 8.5.1 response caching: private and must expire after 120 sec.*/
    opts.CacheProfiles.Add("Client-120",
        new CacheProfile()
        {
            Location = ResponseCacheLocation.Client,
            Duration = 120
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.ParameterFilter<SortColumnFilter>(); //using MyBGList.Attributes;
    opts.ParameterFilter<SortOrderFilter>(); //Binding the IParameterFilters

    opts.ResolveConflictingActions(apiDesc => apiDesc.First());/*basically telling Swagger to resolve all conflicts related
to duplicate routing handlers by always taking the first one found (and ignoring the others).*/
   
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {// chapter 9 Updating Swagger configuration
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            //Array.Empty<string>()
            new string[]{ }
        }
    });
});


//Implementing CORS
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(cfg => {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    options.AddPolicy(name: "AnyOrigin",
        cfg => {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
    );

////Configuring the ApiController's behavior
/*Code replaced by the [ManualValidationFilter] attribute*/

/*builder.Services.Configure< ApiBehaviorOptions > (options =>
options.SuppressModelStateInvalidFilter = true);*/

builder.Services.AddIdentity<ApiUser, IdentityRole> (opts =>
{ //Adding the Identity service
    opts.Password.RequireDigit = true; 
    opts.Password.RequireUppercase = true;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireNonAlphanumeric = true;
    opts.Password.RequiredLength = 12;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddAuthentication(opts =>
{ //Adding the Authentication service
    opts.DefaultAuthenticateScheme =
    opts.DefaultChallengeScheme =
    opts.DefaultForbidScheme =
    opts.DefaultScheme =
    opts.DefaultSignInScheme =
    opts.DefaultSignOutScheme =
        JwtBearerDefaults.AuthenticationScheme; 
}).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        RequireExpirationTime = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
          System.Text.Encoding.UTF8.GetBytes(
              builder.Configuration["JWT:SigningKey"])
        )
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ModeratorWithMobilePhone", policy =>
        policy
            .RequireClaim(ClaimTypes.Role, RoleNames.Moderator) 
            .RequireClaim(ClaimTypes.MobilePhone));

    options.AddPolicy("MinAge18", policy =>
        policy
            .RequireAssertion(ctx =>
                ctx.User.HasClaim(c => c.Type == ClaimTypes.DateOfBirth)
                && DateTime.ParseExact(
                    "yyyyMMdd",
                    ctx.User.Claims.First(c =>
                        c.Type == ClaimTypes.DateOfBirth).Value,
                    System.Globalization.CultureInfo.InvariantCulture)
                    >= DateTime.Now.AddYears(-18)));
});


//Response Caching Middleware | settings
builder.Services.AddResponseCaching(opts => //fine-tune the middleware’s caching strategies by changing its default settings
    {
        opts.MaximumBodySize = 32 * 1024 * 1024;
        opts.SizeLimit = 50 * 1024 * 1024;

        // server-side response caching exercise 8.5.3
        /*opts.MaximumBodySize = 128 * 1024 * 1024; //128mb
        opts.SizeLimit = 200 * 1024 * 1024; // 200mb
        opts.UseCaseSensitivePaths = true;*/ //sensitive
    });

//Setting up the in-memory cache
builder.Services.AddMemoryCache(); //IMemoryCache interface

//Distributed Caching
builder.Services.AddDistributedSqlServerCache(opts =>
{
    opts.ConnectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");
    opts.SchemaName = "dbo";
    opts.TableName = "AppCache";
    //opts.TableName = "SQLCache";// 8.5.5. disturbed caching name SQLCAche
});
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //will be only included if the app is run in the Development environment
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//if (app.Configuration.GetValue<bool>("UseSwagger")) // 4. current initialization strategy to ensure that they will be used only if the UseSwagger configuration setting is set to True.
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage")) 
    app.UseDeveloperExceptionPage(); /* the /error string parameter, meaning that we want to handle these errors with a
    dedicated HTTP route - which now we need to implement.
    with a Controller or using the Minimal API.*/

else
{
    app.UseExceptionHandler("/error");
    /* <- handles HTTPlevel
exceptions, but it’s better suited for non-development
environments since it sends all the relevant error info to a customizable
handler instead of generating a detailed error response and automatically
present it to the end-user*/

    ////from chapter about exeptions handling:
    //app.UseExceptionHandler(action => {
    //    action.Run(async context =>
    //    {
    //        var exceptionHandler =
    //        context.Features.Get<IExceptionHandlerPathFeature>();
    //        var details = new ProblemDetails();
    //        details.Detail = exceptionHandler?.Error.Message;
    //        details.Extensions["traceId"] =
    //        System.Diagnostics.Activity.Current?.Id
    //        ?? context.TraceIdentifier;
    //        details.Type =
    //        "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    //        details.Status = StatusCodes.Status500InternalServerError;
    //        await context.Response.WriteAsync(
    //        System.Text.Json.JsonSerializer.Serialize(details)); 
    //    });
    //});

}
/*the ExceptionHandlerMiddleware will be used instead of the DeveloperExceptionPageMiddleware, since the value of the
UseDeveloperExceptionPage key has been previously set to false in the appsetting.json file.*/

app.UseHttpsRedirection();
app.UseCors(); //Applying CORS

/*The Response Caching Middleware will only cache HTTP responses using
GET or HEAD methods and resulting in a 200 - OK status code: any other
responses, including error pages, will be ignored.*/
app.UseResponseCaching(); //CORS Middleware must be called after the Response Caching Middleware in order to work

app.UseAuthentication(); //chapter 9 Adding the Authentication middleware
app.UseAuthorization();

//Implementing a no-cache default behavior | implementing custom middleware.
/*app.Use((context, next) =>
{
    context.Response.Headers["cache-control"] =
        "no-cache, no-store";
    return next.Invoke();
});*/ // that is literal approach, so it is vulnerable to human error risk of mistyping them, below strongly-typed values instead

// implementing custom middleware using strongly-typed values
app.Use((context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl =
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true
        };
    return next.Invoke();
});


/*MINIMAL  A P I   */
app.MapGet("/error", 
    [EnableCors("AnyOrigin")]
    //[ResponseCache(NoStore = true)] () => Results.Problem());// handle app.UseExceptionHandler("/error") Using Minimal API
    
    ////from chapter 6.2.4.
    [ResponseCache(NoStore = true)] (HttpContext context) =>
    {
        var exceptionHandler =
        context.Features.Get< IExceptionHandlerPathFeature > ();
        // TODO : logging, sending,
        var details = new ProblemDetails();
        details.Detail = exceptionHandler?.Error.Message;
        details.Extensions["tradeId"] =
            System.Diagnostics.Activity.Current?.Id
            ?? context.TraceIdentifier;

        details.Type =
            "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        details.Status = StatusCodes.Status500InternalServerError;


        /* 6.3.5 Exception Handling exercise */
       /* if (exceptionHandler?.Error is NotImplementedException)
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2";
            details.Status = StatusCodes.Status501NotImplemented;
        }
        else if( exceptionHandler?.Error is TimeoutException)
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5";
            details.Status = StatusCodes.Status504GatewayTimeout;
        }
        else
        {
            details.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            details.Status = StatusCodes.Status500InternalServerError;
            
        }
        */
        ////implement our exception logging change request     
        //app.Logger.LogError(
        //    exceptionHandler?.Error,
        //    "An unhandled exception occurred.");
       
        app.Logger.LogError(
            CustomLogEvents.Error_Get,        //// using MyBGList.Constants; 
            exceptionHandler?.Error,
            "An unhandled exception occurred.");
        return Results.Problem(details);

    });

/*  6.3.5 Exception Handling exercise */
// => /error/test/501 for the HTTP 501 - Not Implemented statuscode
/*app.MapGet("/error/test/501", 
    [EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] () => 
    { throw new NotImplementedException("test 501"); });
//=> /error/test/504 for the HTTP 504 - Gateway Timeout statuscode.
app.MapGet("/error/test/504",
    [EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] () =>
    { throw new TimeoutException("test 504"); });*/

app.MapGet("/error/test", [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)] () => { throw new Exception("test"); }); // we want to produce an error to test the way its handled

app.MapGet("/cod/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () =>
    Results.Text("<script>" +
            "window.alert('Your client supports JavaScript!" +
            "\\r\\n\\r\\n" +
            $"Server time (UTC): {DateTime.UtcNow.ToString("o")}" +
            "\\r\\n" +
            "Client time (UTC): ' + new Date().toISOString());" +
            "</script>" +
            "<noscript>Your client does not support JavaScript</noscript>",
            "text/html"));

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

//Manually setting the cache-control header
app.MapGet("/cache/test/1",
    [EnableCors("AnyOrigin")]
    (HttpContext context) =>
    {
        context.Response.Headers["cache-control"] =
            "no-cache, no-store";
        return Results.Ok();
    });

//Manually setting the cache-control header
app.MapGet("cache/test/2",//ccaching test method without specifying any caching strategy
    [EnableCors("AnyOrigin")]
    (HttpContext context) =>
    {
        return Results.Ok();
    });

app.MapGet("/auth/test/1",
    [Authorize] // chapter 9 Minimal API Authorization
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () =>
    {
        return Results.Ok("You are authorized!");
    });

app.MapGet("/auth/test/2",
    [Authorize(Roles = RoleNames.Moderator)]
    [EnableCors("AnyOrigin")]
    [ResponseCacheAttribute(NoStore = true)] () =>
    {
        return Results.Ok("Tou are authorized!");
    });

app.MapGet("/auth/test/3",
    [Authorize(Roles = RoleNames.Administrator)]
    [EnableCors("AnyOrigin")]
    [ResponseCacheAttribute(NoStore = true)] () =>
    {
        return Results.Ok("You are authorized!");
    });



app.MapControllers()
    .RequireCors("AnyOrigin"); 

app.Run();

//