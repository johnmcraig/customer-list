﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerApi.Data;
using CustomerApi.Dtos;
using CustomerApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace CustomerApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICustomerRepository, CustomerRepository>(); // use AddScoped when going to production or have a larger test database
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddApiVersioning(config => {
                    config.ReportApiVersions = true;
                    config.AssumeDefaultVersionWhenUnspecified = true;
                    config.DefaultApiVersion = new ApiVersion(1, 0);
                    config.ApiVersionReader = new HeaderApiVersionReader("api-version");
            }).AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV"); 

            services.AddSwaggerGen(options => {
                
                var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, new Info
                        { 
                            Title = $"Sample API {description.ApiVersion}", 
                            Version = description.ApiVersion.ToString() 
                        });
                    }
            });

            services.AddCors();
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            AutoMapper.Mapper.Initialize(mapper => {
                mapper.CreateMap<Customer, CustomerDto>().ReverseMap();
                mapper.CreateMap<Customer, CustomerCreateDto>().ReverseMap();
            });

            // Swagger API testing setup and config
            app.UseSwagger();
            app.UseSwaggerUI(options => {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());

                }
            });

            app.UseCors(x => {
                x.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
