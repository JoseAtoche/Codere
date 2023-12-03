using Microsoft.AspNetCore.Mvc;
using APICodere.Repository;
using System.Text.Json;
using APICodere.Models.Models;
using APICodere.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using APICodere.Services;

namespace APICodere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly ShowsService _showsService;

        public ShowsController(ShowsService showsService)
        {
            _showsService = showsService;
        }

        [HttpGet("ShowsMainInformationAndImport")]
        public async Task<IActionResult> GetShowsMainInformationAndImport()
        {
            try
            {
                var showInfoList = await _showsService.GetShowsMainInformation();

                if (showInfoList!= null && await _showsService.ImportShows(showInfoList))
                {
                    return Ok(showInfoList);
                }
                return StatusCode(500, "Error Importando Shows");
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception: {ex.Message}");
            }
        }

        [HttpGet("show/{id}")]
        public async Task<IActionResult> GetShowById(int id)
        {
            var show = await _showsService.GetShowById(id);
            return show != null ? Ok(show) : NotFound();
        }

        [HttpGet("showAllData")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllData()
        {
            var allData = await _showsService.GetAllData();
            return Ok(allData);
        }

        [HttpGet("GetDataByQuery")]
        public IActionResult GetDataByQuery([FromBody] string sqlQuery)
        {
            return _showsService.GetDataByQuery(sqlQuery);
        }
    }

}
