using System;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Represents a rectangular rendering zone within the console interface.
    /// Manages dirty state tracking and provides coordinate boundaries for partial screen updates.
    /// </summary>
    public class RenderZone
    {
        /// <summary>
        /// The spatial boundaries of this render zone in console coordinates
        /// </summary>
        public Region Bounds { get; set; }
        
        /// <summary>
        /// Indicates whether this zone needs to be redrawn on the next render cycle
        /// </summary>
        public bool IsDirty { get; set; }
        
        /// <summary>
        /// Timestamp of the last successful render operation for this zone
        /// </summary>
        public DateTime LastUpdate { get; private set; }
        
        /// <summary>
        /// Unique identifier for this zone within the renderer
        /// </summary>
        public string ZoneId { get; }
        
        /// <summary>
        /// Priority level for rendering order (lower numbers render first)
        /// </summary>
        public int RenderPriority { get; set; }
        
        /// <summary>
        /// Whether this zone's content depends on other zones' layouts
        /// </summary>
        public bool HasLayoutDependencies { get; set; }

        /// <summary>
        /// Creates a new render zone with the specified identifier and bounds
        /// </summary>
        /// <param name="zoneId">Unique identifier for this zone</param>
        /// <param name="bounds">Initial spatial boundaries</param>
        /// <param name="renderPriority">Render order priority (default: 100)</param>
        public RenderZone(string zoneId, Region bounds, int renderPriority = 100)
        {
            ZoneId = zoneId ?? throw new ArgumentNullException(nameof(zoneId));
            Bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            RenderPriority = renderPriority;
            IsDirty = true; // New zones always need initial render
            LastUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Marks this zone as needing a redraw
        /// </summary>
        public void Invalidate()
        {
            IsDirty = true;
        }

        /// <summary>
        /// Marks this zone as clean and updates the last render timestamp
        /// </summary>
        public void MarkRendered()
        {
            IsDirty = false;
            LastUpdate = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the zone boundaries and invalidates if they changed
        /// </summary>
        /// <param name="newBounds">The new spatial boundaries</param>
        public void UpdateBounds(Region newBounds)
        {
            if (newBounds == null)
                throw new ArgumentNullException(nameof(newBounds));

            if (!Bounds.Equals(newBounds))
            {
                Bounds = newBounds;
                Invalidate();
            }
        }

        /// <summary>
        /// Checks if this zone intersects with another zone's boundaries
        /// </summary>
        /// <param name="other">The zone to check intersection with</param>
        /// <returns>True if the zones overlap spatially</returns>
        public bool IntersectsWith(RenderZone other)
        {
            if (other == null) return false;
            
            return Bounds.Left < other.Bounds.Left + other.Bounds.Width &&
                   Bounds.Left + Bounds.Width > other.Bounds.Left &&
                   Bounds.Top < other.Bounds.Top + other.Bounds.Height &&
                   Bounds.Top + Bounds.Height > other.Bounds.Top;
        }

        /// <summary>
        /// Calculates the area of intersection with another zone
        /// </summary>
        /// <param name="other">The zone to calculate intersection with</param>
        /// <returns>The intersecting region, or null if no intersection</returns>
        public Region? GetIntersection(RenderZone other)
        {
            if (other == null || !IntersectsWith(other))
                return null;

            int left = Math.Max(Bounds.Left, other.Bounds.Left);
            int top = Math.Max(Bounds.Top, other.Bounds.Top);
            int right = Math.Min(Bounds.Left + Bounds.Width, other.Bounds.Left + other.Bounds.Width);
            int bottom = Math.Min(Bounds.Top + Bounds.Height, other.Bounds.Top + other.Bounds.Height);

            return new Region(left, top, right - left, bottom - top);
        }

        public override string ToString()
        {
            return $"RenderZone[{ZoneId}] {Bounds} Dirty:{IsDirty} Priority:{RenderPriority}";
        }
    }
}