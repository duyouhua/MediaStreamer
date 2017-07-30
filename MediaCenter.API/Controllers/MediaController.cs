using MediaCenter.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Http;

namespace MediaCenter.API.Controllers
{
    public class MediaController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<Video> Get()
        {
            var videos = new List<Video>();

            var DvdPath = HttpContext.Current.Server.MapPath("~/Content/DVDs");
            var MainDirectory = Directory.GetDirectories(DvdPath);

            foreach (var folder in MainDirectory)
            {
                var directory = new DirectoryInfo(folder);
                var name = directory.Name;

                videos.Add(new Video()
                {
                    Name = name,
                    CoverPhoto = "/Content/DVDs/" + name + "/photo.jpg",
                    MovieURL = "/Content/DVDs/" + name + "/movie.mp4"
                });
            }

            return videos;
        }
   }
}