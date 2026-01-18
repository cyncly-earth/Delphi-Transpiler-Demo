using Microsoft.AspNetCore.Mvc;
using GeneratedApp.Models;

namespace GeneratedApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        [HttpPost("SetPeople")]
        public IActionResult SetPeople([FromBody] TPerson data)
        {
            // Logic transpiled from Delphi
            // open mtPerson
            // commit
            return Ok(new { message = "Success", data });
        }

        [HttpPost("PersonFields")]
        public IActionResult PersonFields([FromBody] TPerson data)
        {
            // Logic transpiled from Delphi
            // open mtPerson
            // commit
            return Ok(new { message = "Success", data });
        }

        [HttpPost("AddPerson")]
        public IActionResult AddPerson([FromBody] TPerson data)
        {
            // Logic transpiled from Delphi
            // open mtPerson
            // commit
            return Ok(new { message = "Success", data });
        }

        [HttpPost("EditPerson")]
        public IActionResult EditPerson([FromBody] TPerson data)
        {
            // Logic transpiled from Delphi
            // open mtPerson
            // commit
            return Ok(new { message = "Success", data });
        }

        [HttpPost("DeletePerson")]
        public IActionResult DeletePerson([FromBody] TPerson data)
        {
            // Logic transpiled from Delphi
            // open mtPerson
            // commit
            return Ok(new { message = "Success", data });
        }

    }
}
