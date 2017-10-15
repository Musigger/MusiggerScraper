using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ReleaseCrawler;
using ReleaseCrowler.Models;
using System.Net.Http.Headers;

namespace ReleaseCrowler.Controllers
{
    public class ReleasesController : ApiController
    {
        private DataContext db = new DataContext();

        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, db.Releases.ToList(), MediaTypeHeaderValue.Parse("application/json"));
        }

        //// GET: api/Releases
        //public IQueryable<Release> GetReleases()
        //{
        //    return db.Releases;
        //}

        //// GET: api/Releases/5
        //[ResponseType(typeof(Release))]
        //public async Task<IHttpActionResult> GetRelease(int id)
        //{
        //    Release release = await db.Releases.FindAsync(id);
        //    if (release == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(release);
        //}

        // PUT: api/Releases/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutRelease(int id, Release release)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != release.Id)
            {
                return BadRequest();
            }

            db.Entry(release).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReleaseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Releases
        [ResponseType(typeof(Release))]
        public async Task<IHttpActionResult> PostRelease(Release release)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Releases.Add(release);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = release.Id }, release);
        }

        // DELETE: api/Releases/5
        [ResponseType(typeof(Release))]
        public async Task<IHttpActionResult> DeleteRelease(int id)
        {
            Release release = await db.Releases.FindAsync(id);
            if (release == null)
            {
                return NotFound();
            }

            db.Releases.Remove(release);
            await db.SaveChangesAsync();

            return Ok(release);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ReleaseExists(int id)
        {
            return db.Releases.Count(e => e.Id == id) > 0;
        }
    }
}