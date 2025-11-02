using BookManager.Application.Services;
using BookManager.Application.Services.Interfaces;
using BookManager.Application.Validators;
using BookManager.Infrastructure.Data;
using BookManager.Infrastructure.Repositories;
using BookManager.Infrastructure.Repositories.Interfaces;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") // origem do Angular
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection Factory
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton(new SqlConnectionFactory(connectionString!));

// Repositories
builder.Services.AddScoped<IAssuntoRepository, AssuntoRepository>();
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<IFormaPagamentoRepository, FormaPagamentoRepository>();
builder.Services.AddScoped<ILivroAutorRepository, LivroAutorRepository>();
builder.Services.AddScoped<ILivroPrecoRepository, LivroPrecoRepository>();
builder.Services.AddScoped<ILivroRepository, LivroRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();

// Services
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<IAssuntoService, AssuntoService>();
builder.Services.AddScoped<IFormaPagamentoService, FormaPagamentoService>();
builder.Services.AddScoped<ILivroService, LivroService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining(typeof(CreateAutorDtoValidator));

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();