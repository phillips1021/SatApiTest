
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.EntityFrameworkCore;
using SatApiTest.Models;

namespace SatApiTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddCors();

            builder.Services.AddControllers();
            builder.Services.AddDbContext<SatContext>(options =>
                           options.UseInMemoryDatabase("Data Source=sat.db"));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.UseCors(builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });

            app.Run();


        }
    }
}