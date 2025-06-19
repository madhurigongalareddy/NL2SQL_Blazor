using NL2SQL_Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();

// Register the NlQueryEngine as a scoped service
builder.Services.AddScoped<NLQueryEngine>(sp => new NLQueryEngine (sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<IConfiguration>()));
builder.Services.AddScoped<DatabaseEngine>(sp => new DatabaseEngine(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddSingleton<QueryHistoryService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run(); 
