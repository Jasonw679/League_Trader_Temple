using Microsoft.AspNetCore.Mvc;

namespace League_Trader_Temple.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RiftboundCardsController(CardDatabase cardDatabase) : ControllerBase
    {
        private readonly CardDatabase cardDatabase = cardDatabase;

        [HttpGet(Name = "GetRiftboundCards")]
        public async Task<ActionResult<RiftboundCardPage>> Get(
            [FromQuery] string? search = null,
            [FromQuery] string? id = null,
            [FromQuery] string? setId = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 6,
            [FromQuery] string sort = "collector_number",
            [FromQuery] int dir = 1,
            CancellationToken cancellationToken = default)
        {
            if (page < 1)
            {
                return BadRequest("Page must be greater than zero.");
            }

            if (size is < 1 or > 100)
            {
                return BadRequest("Size must be between 1 and 100.");
            }

            if (dir is not 1 and not -1)
            {
                return BadRequest("Dir must be 1 for ascending or -1 for descending.");
            }

            var cardPage = await cardDatabase.GetCardsAsync(
                search,
                id,
                setId,
                page,
                size,
                sort,
                dir,
                cancellationToken);

            return Ok(cardPage);
        }

        [HttpPost("{id}/visits", Name = "RiftboundCardVisit")]
        public async Task<IActionResult> RecordVisit(
            string id,
            [FromBody] RiftboundCardVisitRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Card id is required.");
            }

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest("User id is required.");
            }

            if (request.UserId.Length > 128)
            {
                return BadRequest("User id must be 128 characters or fewer.");
            }

            var recorded = await cardDatabase.RecordVisitAsync(
                id.Trim(),
                request.UserId.Trim(),
                cancellationToken);

            return recorded ? NoContent() : NotFound();
        }
    }

    public class RiftboundCardVisitRequest
    {
        public string UserId { get; set; } = "";
    }
}
