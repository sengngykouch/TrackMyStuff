﻿using EFDataAccessLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackMyStuff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : BaseController
    {
        private readonly ILogger _logger;

        public ItemController(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger(typeof(ItemController));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> Get()
        {
            var items = await _dbContext.Item.ToListAsync();
            return items;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> Get(int id)
        {
            var item = await _dbContext.Item.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        [HttpPost]
        public async Task<ActionResult<Item>> Post(Item item)
        {
            try
            {
                _dbContext.Item.Add(item);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, Item item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            var originalItem = await _dbContext.Item.FindAsync(id);
            if (originalItem == null)
            {
                return NotFound();
            }

            originalItem.Name = item.Name;
            originalItem.Location = item.Location;
            originalItem.Description = item.Description;
            originalItem.ExpirationDate = item.ExpirationDate;
            originalItem.Image = item.Image;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException dbEx) when (!ItemExists(id))
            {
                _logger.LogError(dbEx, dbEx.Message);
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await _dbContext.Item.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            _dbContext.Item.Remove(item);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        private bool ItemExists(int id) => _dbContext.Item.Any(e => e.Id == id);
    }
}