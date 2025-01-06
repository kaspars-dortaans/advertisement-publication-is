using AutoMapper;
using BusinessLogic.Entities;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Dto.Advertisement;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AdvertisementController(
    ILogger<AdvertisementController> logger,
    IMapper mapper,
    IBaseService<Category> categoryService) : ControllerBase
{
    private readonly ILogger<AdvertisementController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IBaseService<Category> _categoryService = categoryService;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IEnumerable<CategoryItem>> GetCategories(string locale)
    {
        var normalizedLocale = locale.ToUpper();
        return await _categoryService
            .GetAll()
            .Select(c => new CategoryItem()
            {
                Id = c.Id,
                ParentCategoryId = c.ParentCategoryId,
                AdvertisementCount = c.AdvertisementCount,
                CanContainAdvertisements = c.CanContainAdvertisements,
                Name = c.LocalisedNames.First(n => n.Locale == normalizedLocale).Text,
            })
            .ToListAsync();
    }

    //[HttpGet]
    //public IEnumerable<WeatherForecast> Get()
    //{
    //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //    {
    //        Date = DateTime.Now.AddDays(index),
    //        TemperatureC = Random.Shared.Next(-20, 55),
    //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //    })
    //    .ToArray();
    //}
}