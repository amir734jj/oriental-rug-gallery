using System;
using System.Threading.Tasks;
using Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Enums;
using Models.Models;

namespace API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserLogic _userLogic;
        
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor dependency injection
        /// </summary>
        /// <param name="userLogic"></param>
        /// <param name="userManager"></param>
        public UserController(IUserLogic userLogic, UserManager<User> userManager)
        {
            _userLogic = userLogic;
            _userManager = userManager;
        }

        /// <summary>
        /// Returns user view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            return View(await _userLogic.GetAll());
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userLogic.Delete(id);

            return RedirectToAction("Index");
        }
         
        /// <summary>
        /// Update User Role
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UpdateUserRole/{id}/{userRoleEnum}")]
        public async Task<IActionResult> UpdateUserRole(int id, UserRoleEnum userRoleEnum)
        {;
            var userEntity = await _userLogic.Update(id, x => x.Role = userRoleEnum);

            switch (userRoleEnum)
            {
                case UserRoleEnum.Basic:
                    await _userManager.AddToRoleAsync(userEntity, UserRoleEnum.Basic.ToString());
                    await _userManager.RemoveFromRoleAsync(userEntity, UserRoleEnum.Admin.ToString());
                    break;
                case UserRoleEnum.Admin:
                    await _userManager.AddToRoleAsync(userEntity, UserRoleEnum.Admin.ToString());
                    await _userManager.AddToRoleAsync(userEntity, UserRoleEnum.Basic.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(userRoleEnum), userRoleEnum, null);
            }
            
            return RedirectToAction("Index");
        }
    }
}