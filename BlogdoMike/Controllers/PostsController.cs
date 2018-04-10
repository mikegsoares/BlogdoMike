using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlogdoMike.Models;
using BlogdoMike.ViewModels;

namespace BlogdoMike.Controllers
{
    public class PostsController : Controller
    {
        private BlogContext _context = new BlogContext();

        // GET: Posts
        public ActionResult Index()
        {
            var content = _context.Posts.Select(s => new
            {
                s.Id,
                s.Title,
                s.DateAdded,
                s.DateUpdated,
                s.Image,
                s.PostingBody
            });
            var viewModel = content.Select(p => new PostViewModel()
            {
                Id = p.Id,
                Title = p.Title,
                DateAdded = p.DateAdded,
                DateUpdated = p.DateUpdated,
                Image = p.Image,
                PostingBody = p.PostingBody
            }).ToList();

            return View(viewModel);
        }

        public ActionResult Details(int id)
        {
            var post = _context.Posts.SingleOrDefault(p => p.Id == id);

            if (post == null)
            {
                return HttpNotFound();
            }

            return View(post);
        }

        public ActionResult Edit(int id)
        {
            var postInDb = _context.Posts.SingleOrDefault(p => p.Id == id);
            return View(postInDb);
        }

        public ActionResult New()
        {
            return View();
        }

        public ActionResult Delete(int id)
        {
            var postInDb = _context.Posts.Single(p => p.Id == id);

            if (postInDb == null)
            {
                return HttpNotFound();
            }

            return View(postInDb);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteComplete(int id)
        {
            var postInDb = _context.Posts.Single(p => p.Id == id);
            if (postInDb == null)
                return HttpNotFound();

            _context.Posts.Remove(postInDb);
            _context.SaveChanges();

            return RedirectToAction("Index", "Posts");
        }

        [HttpPost]
        public ActionResult Save(PostViewModel post)
        {
            var file = Request.Files["ImageData"];

            if (post.Id == 0)
            {
                var service = new PostsController();
                var i = service.UploadImageInDb(file, post);
                if (i == 1)
                {
                    return RedirectToAction("Index");
                }

                post.DateAdded = DateTime.Now;
              //  _context.Posts.Add(post); - Removido!
            }
            else
            {
                var postInDb = _context.Posts.Single(p => p.Id == post.Id);
                postInDb.Title = post.Title;
                postInDb.DateUpdated = DateTime.Now;
                // postInDb.Image = post.Image; 
                postInDb.PostingBody = post.PostingBody;

                if (file.ContentLength > 0)
                {
                    postInDb.Image = ConvertToBytes(file);
                }
                else
                {
                    _context.Entry(postInDb).Property(i => i.Image).IsModified = false;
                }
            }
            _context.SaveChanges();

            return RedirectToAction("Index", "Posts");
        }

        public ActionResult RetrieveImage(int id)
        {
            var cover = GetImageFromDatabase(id);
            if (cover != null)
            {
                return File(cover, "image/png");
            }
            else
            {
                return null;
            }
         //   return cover != null ? File(cover, "image/png") : null;
        }


        public byte[] GetImageFromDatabase(int id)
        {
            var q = _context.Posts.Where(i => i.Id == id).Select(i => i.Image);
            var cover = q.First();
            return cover;
        }

        public int UploadImageInDb(HttpPostedFileBase file, PostViewModel viewModel)
        {
            viewModel.Image = ConvertToBytes(file);
            var post = new Post
            {
                Title = viewModel.Title,
                DateAdded = DateTime.Now,
                DateUpdated = DateTime.Now,
                PostingBody = viewModel.PostingBody,
                Image = viewModel.Image
            };

            _context.Posts.Add(post);
            var i = _context.SaveChanges();
            return i == 1 ? 1 : 0;
        }

        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
            var reader = new BinaryReader(image.InputStream);
            var imageBytes = reader.ReadBytes(image.ContentLength);
            return imageBytes;
        }

    }
}