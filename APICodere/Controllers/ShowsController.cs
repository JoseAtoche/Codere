using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("Show/{id}")]
        public async Task<IActionResult> GetShowById(int id)
        {
            var show = await _showsService.GetShowById(id);
            return show != null ? Ok(show) : NotFound();
        }

        [HttpGet("ShowAllData")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllData()
        {
            var allData = await _showsService.GetAllData();
            if (allData == null) {

                return BadRequest("La base de datos no tiene Datos, por favor, ejecute antes el metodo de importación: ShowsMainInformationAndImport");
            }
            
            return Ok(allData);
        }

        [HttpGet("GetDataByQuery")]
        public async Task<IActionResult> GetDataByQuery([FromBody] string sqlQuery)
        {
            var data = await _showsService.GetDataByQuery(sqlQuery);
            return data ;
        }
    }

}
