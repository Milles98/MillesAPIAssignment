﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AdvertisementAPI.Models
{
    /// <summary>
    /// Ad model
    /// </summary>
    public class Ad
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        [StringLength(50, MinimumLength = 2)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Description
        /// </summary>
        [StringLength(500, MinimumLength = 2)]
        public string Description { get; set; } = null!;
    }
}
