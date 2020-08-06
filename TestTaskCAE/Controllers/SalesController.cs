using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TestTaskCAE.BaseClasses;
using TestTaskCAE.Services;

namespace TestTaskCAE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        ISalesService salesService;
        public SalesController(ISalesService salesService)
        {
            this.salesService = salesService ?? throw new ArgumentNullException(nameof(salesService));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return new OkObjectResult(await salesService.Get());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Int32 id)
        {
            var result = (await salesService.Get(id)).FirstOrDefault();

            if (result == null)
                return NotFound();

            return new OkObjectResult(result);
        }        

        [HttpGet("{startDate:DateTime}/{endDate:DateTime}")]
        public async Task<IActionResult> Get(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            return new OkObjectResult(await salesService.Get(null, startDate.Value.UtcDateTime, endDate.Value.UtcDateTime));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }


            await salesService.Add(order);
            return new OkResult();
        }

        [HttpPut]
        public IActionResult Put([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            salesService.Update(order);
            return new OkResult();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Int32 id)
        {
            salesService.Remove(id);

            return new OkResult();
        }

    }
}
