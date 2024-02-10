using Homies.Data;
using Homies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Claims;

namespace Homies.Controllers
{
    public class EventController : Controller
    {
        private readonly HomiesDbContext data;

        public EventController(HomiesDbContext context)
        {
            data = context;
        }
        public async Task<IActionResult> All()
        {
            var events = await data.Events
              .Select(e => new EventInfoViewModel(
              e.Id,
              e.Name,
              e.StartDate,
              e.Type.Name,
              e.Organiser.UserName
            )).ToListAsync();

            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int id)
        {
            var e = await data.Events
                .Where(e => e.Id == id)
                .Include(e => e.EventsParticipants)
                .FirstOrDefaultAsync();
            if (e == null)
            {
                return BadRequest();
            }

            string userId = GetUserId();

            if (e.EventsParticipants.Any(p => p.HelperId == userId))
            {
                return BadRequest();
            }

            e.EventsParticipants.Add(new EventParticipant
            {
                HelperId = userId,
                EventId = id
            });

            await data.SaveChangesAsync();
            return RedirectToAction(nameof(Join));
        }

        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {

            var e = await data.Events
                .Where(e => e.Id == id)
                .Include(e => e.EventsParticipants)
                .FirstOrDefaultAsync();

            if (e == null)
            {
                return BadRequest();
            }

            string userId = GetUserId();

            if (!e.EventsParticipants.Any(p => p.HelperId == userId))
            {
                e.EventsParticipants.Add(new EventParticipant
                {
                    EventId = e.Id,
                    HelperId = userId
                });
            }

            await data.SaveChangesAsync();

            return RedirectToAction("Joined");
        }

        public async Task<IActionResult> Joined()
        {
            string userId = GetUserId();

            var events = await data.EventsParticipants
                .Where(p => p.HelperId == userId)
                .AsNoTracking()
                .Select(p => new EventInfoViewModel(
                p.EventId,
                p.Event.Name,
                p.Event.StartDate,
                p.Event.Type.Name,
                p.Event.Organiser.UserName))
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> Leave(int id)
        {
            var e = await data.Events
                          .Where(e => e.Id == id)
                          .Include(e => e.EventsParticipants)
                          .FirstOrDefaultAsync();

            if (e == null)
            {
                return BadRequest();
            }

            string userId = GetUserId();

            var participant = e.EventsParticipants
                .FirstOrDefault(p => p.HelperId == userId);

            if (participant == null)
            {
                return BadRequest();
            }

            e.EventsParticipants.Remove(participant);

            await data.SaveChangesAsync();

            return RedirectToAction(nameof(All));

        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new EventFormViewModel
            {
                Types = await GetTypes()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(EventFormViewModel model)
        {
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            if (!DateTime.TryParseExact(
                model.Start,
                DataConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out start))
            {
                ModelState.AddModelError(nameof(EventFormViewModel.Start), $"Invalid start date:{DataConstants.DateFormat}");
            }
            if (!DateTime.TryParseExact(
                model.End,
                DataConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out end))
            {
                ModelState.AddModelError(nameof(EventFormViewModel.End), $"Invalid start date:{DataConstants.DateFormat}");
            }

            if (!ModelState.IsValid)
            {
                model.Types = await GetTypes();
                return View(model);
            }

            var e = new Event()
            {
                CreatedOn = DateTime.Now,
                Description = model.Description,
                Name = model.Name,
                OrganiserId = GetUserId(),
                TypeId = model.TypeId,
                StartDate = start,
                EndDate = end
            };

            await data.Events.AddAsync(e);
            await data.SaveChangesAsync();
            return RedirectToAction(nameof(All));
        }

        public async Task<IActionResult> Details(int id)
        {
            var e = await data.Events
                .Where(e => e.Id == id)
                .Select(e => new EventDetailsViewModel
                {
                    Description = e.Description,
                    Name = e.Name,
                    Start = e.StartDate.ToString(DataConstants.DateFormat),
                    End = e.EndDate.ToString(DataConstants.DateFormat),
                    Organiser = e.Organiser.UserName,
                    CreatedOn = e.CreatedOn.ToString(DataConstants.DateFormat),
                    Type = e.Type.Name
                })
                .FirstOrDefaultAsync();

            if (e == null)
            {
                return BadRequest();
            }

            return View(e);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var e = await data.Events.FindAsync(id);

            if (e == null)
            {
                return BadRequest();
            }

            if (e.OrganiserId != GetUserId())
            {
                return Unauthorized();
            }

            var model = new EventFormViewModel
            {

                Description = e.Description,
                Name = e.Name,
                End = e.EndDate.ToString(DataConstants.DateFormat),
                Start = e.StartDate.ToString(DataConstants.DateFormat),
                TypeId = e.TypeId
            };

            model.Types = await GetTypes();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EventFormViewModel model)
        {
            var e = await data.Events.FindAsync(id);

            if (e == null)
            {
                return BadRequest();
            }

            if (e.OrganiserId != GetUserId())
            {
                return BadRequest();
            }

            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            if (!DateTime.TryParseExact(
                             model.Start,
                             DataConstants.DateFormat,
                             CultureInfo.InvariantCulture,
                             DateTimeStyles.None,
                             out start))
            {
                ModelState.AddModelError(nameof(EventFormViewModel.Start), $"Invalid start date:{DataConstants.DateFormat}");
            }
            if (!DateTime.TryParseExact(
                            model.End,
                            DataConstants.DateFormat,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out end))
            {
                ModelState.AddModelError(nameof(EventFormViewModel.End), $"Invalid start date:{DataConstants.DateFormat}");
            }

            if (!ModelState.IsValid)
            {
                model.Types = await GetTypes();
                return View(model);
            }

            e.Description = model.Description;
            e.Name = model.Name;
            e.TypeId = model.TypeId;
            e.StartDate = start;
            e.EndDate = end;

            await data.SaveChangesAsync();
            return RedirectToAction(nameof(All));
        }
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        private async Task<ICollection<TypeViewModel>> GetTypes()
        {
            return await data
                .Types
                .AsNoTracking()
                .Select(t => new TypeViewModel
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();
        }
    }
}
