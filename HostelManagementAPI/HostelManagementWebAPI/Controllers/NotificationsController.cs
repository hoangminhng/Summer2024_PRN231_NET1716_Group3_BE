using DTOs;
using HostelManagementWebAPI.MessageStatusResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Service.Exceptions;
using Service.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HostelManagementWebAPI.Controllers
{

    public class NotificationsController : ODataController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/<NotificationController>
        [Authorize]
        [EnableQuery]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> Get()
        {
            try
            {
                var list = await _notificationService.GetAllNotifications();

                return Ok(list);
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }



        // POST api/<NotificationController>
        public async Task<ActionResult> Post([FromBody] NotificationDto value)
        {
            try
            {
                await _notificationService.CreateNotification(value);
                return Ok();
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        // PUT api/<NotificationController>/5
        [HttpPut("notification")]
        public async Task<ActionResult> Put([FromODataUri] int key)
        {
            try
            {
                await _notificationService.MarkNotificationAsRead(key);
                return Ok();
            }
            catch (ServiceException ex)
            {
                return BadRequest(new ApiResponseStatus(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseStatus(500, ex.Message));
            }
        }

        //// DELETE api/<NotificationController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
