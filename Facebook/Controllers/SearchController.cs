﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facebook.Contracts;
using Facebook.Mappers;
using Facebook.Models.ViewModels;
using Facebook.Utilities;
using Facebook.Utilities.Enums;
using FaceBook.Models;
using FacebookDbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nancy.Json;

namespace Facebook.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FacebookDataContext facebookDataContext;
        private readonly IUserData userData;

        public SearchController(ILogger<HomeController> logger, FacebookDataContext _facebookDataContext, IUserData _userData)
        {
            userData = _userData;
            _logger = logger;
            facebookDataContext = _facebookDataContext;
        }



        [AuthorizedAction]
        public IActionResult Index(string search)
        {

            if (string.IsNullOrEmpty(search))
            {
                return RedirectToAction("/Home/Index");
            }
            ViewData["LayoutData"] = userData.GetLayoutData(HttpContext);
            ViewData["Users"] = userData.GetUser(HttpContext);

            var loggedUserData = userData.GetUser(HttpContext);

            List<User> searchUsrs = facebookDataContext.Users.Include(x => x.UserRelationsDesider).Include(x => x.UserRelationsInitiator).Include(x => x.ProfilePhotos)
                .Where(usr => (usr.FirstName.Trim().Contains(search.Trim()) || usr.LastName.Trim().Contains(search.Trim()) || (usr.FirstName.Trim() + " " + usr.LastName.Trim() == search.Trim())) && usr.Id != loggedUserData.Id && usr.IsDeleted == false  && usr.RoleId != (int)UserType.SuperAdmin).ToList();

            List<SearchUserDto> searchUserDtos = SearchUserMapper.Map(searchUsrs, loggedUserData.Id).ToList();

            ViewData["LoggedUser"] = loggedUserData.Id;

            return View(searchUserDtos);
        }

        [HttpPut]
        [AuthorizedAction]
        public IActionResult ConfirmFriendAction(int InitiatorId, int DesiderId)
        {

            var userRelSelect = facebookDataContext.UserRelations.Where(usrRel => usrRel.InitiatorId == InitiatorId && usrRel.DesiderId == DesiderId).FirstOrDefault();
            if (userRelSelect == null)
                return Json(new { success = false });

            userRelSelect.SocialStatusId = (int)SocialStatuses.Friend;
            facebookDataContext.SaveChanges();
            //return RedirectToAction("index", searchStr);
            return Json(new { success = true });


        }

        [HttpDelete]
        [AuthorizedAction]
        public IActionResult UnFriendAction(int InitiatorId, int DesiderId)
        {
            var userRelSelect = facebookDataContext.UserRelations.Where(usrRel => (usrRel.InitiatorId == InitiatorId || usrRel.InitiatorId == DesiderId) && (usrRel.InitiatorId == DesiderId || usrRel.DesiderId == DesiderId)).FirstOrDefault();
            if (userRelSelect == null)
                return Json(new { success = false });

            facebookDataContext.UserRelations.Remove(userRelSelect);
            facebookDataContext.SaveChanges();

            return Json(new { success = true });

        }

        [HttpPost]
        [AuthorizedAction]
        public IActionResult RequestFriendAction(int InitiatorId, int DesiderId)
        {
            UserRelation newUserRel = new UserRelation()
            {
                CreatedAt = DateTime.Now,
                DesiderId = DesiderId,
                InitiatorId = InitiatorId,
                IsDeleted = false,
                SocialStatusId = (int)SocialStatuses.Request
            };
            facebookDataContext.UserRelations.Add(newUserRel);
            facebookDataContext.SaveChanges();
            return Json(new { success = true });
        }
    }
}