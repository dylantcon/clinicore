using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace CLI.CliniCore.Service.Editor
{
    /// <summary>
    /// Manages multiple rendering zones to enable efficient partial screen updates.
    /// Coordinates zone dependencies and handles cascading layout invalidation.
    /// </summary>
    public class ZonedRenderer
    {
        private readonly ThreadSafeConsoleManager _console;
        private readonly ConcurrentDictionary<string, RenderZone> _zones;
        private readonly ConcurrentDictionary<string, Action<RenderZone, object?>> _zoneRenderers;
        private readonly ConcurrentDictionary<string, HashSet<string>> _dependencies;
        private volatile bool _isRendering = false;
        
        /// <summary>
        /// Event fired when a zone's layout changes and affects other zones
        /// </summary>
        public event Action<string, Region, Region>? ZoneLayoutChanged;

        /// <summary>
        /// Creates a new zoned renderer for the specified console
        /// </summary>
        /// <param name="console">The console manager for output operations</param>
        public ZonedRenderer(ThreadSafeConsoleManager console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _zones = new ConcurrentDictionary<string, RenderZone>();
            _zoneRenderers = new ConcurrentDictionary<string, Action<RenderZone, object?>>();
            _dependencies = new ConcurrentDictionary<string, HashSet<string>>();
        }

        /// <summary>
        /// Registers a new rendering zone with its render callback
        /// </summary>
        /// <param name="zoneId">Unique identifier for the zone</param>
        /// <param name="bounds">Initial spatial boundaries</param>
        /// <param name="renderer">Callback function to render this zone's content</param>
        /// <param name="renderPriority">Render order priority (lower renders first)</param>
        public void RegisterZone(string zoneId, Region bounds, Action<RenderZone, object?> renderer, int renderPriority = 100)
        {
            if (string.IsNullOrEmpty(zoneId))
                throw new ArgumentException("Zone ID cannot be null or empty", nameof(zoneId));
            if (bounds == null)
                throw new ArgumentNullException(nameof(bounds));
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            var zone = new RenderZone(zoneId, bounds, renderPriority);
            _zones[zoneId] = zone;
            _zoneRenderers[zoneId] = renderer;
            _dependencies[zoneId] = new HashSet<string>();
        }

        /// <summary>
        /// Adds a layout dependency between two zones
        /// </summary>
        /// <param name="dependentZone">Zone that depends on another's layout</param>
        /// <param name="dependsOnZone">Zone that the dependent zone relies on</param>
        public void AddDependency(string dependentZone, string dependsOnZone)
        {
            if (!_zones.ContainsKey(dependentZone))
                throw new ArgumentException($"Zone '{dependentZone}' not registered");
            if (!_zones.ContainsKey(dependsOnZone))
                throw new ArgumentException($"Zone '{dependsOnZone}' not registered");

            if (_dependencies.TryGetValue(dependentZone, out var deps))
            {
                lock (deps) // Only lock the specific dependency set
                {
                    deps.Add(dependsOnZone);
                }
                _zones[dependentZone].HasLayoutDependencies = true;
            }
        }

        /// <summary>
        /// Updates a zone's boundaries and triggers dependency cascades
        /// </summary>
        /// <param name="zoneId">The zone to update</param>
        /// <param name="newBounds">New spatial boundaries</param>
        public void UpdateZoneBounds(string zoneId, Region newBounds)
        {
            if (!_zones.ContainsKey(zoneId))
                throw new ArgumentException($"Zone '{zoneId}' not registered");

            if (_zones.TryGetValue(zoneId, out var zone))
            {
                var oldBounds = zone.Bounds;
                
                zone.UpdateBounds(newBounds);
                
                // If bounds actually changed, notify listeners and invalidate dependents
                if (!oldBounds.Equals(newBounds))
                {
                    ZoneLayoutChanged?.Invoke(zoneId, oldBounds, newBounds);
                    InvalidateDependentZones(zoneId);
                }
            }
        }

        /// <summary>
        /// Marks a specific zone as needing redraw
        /// </summary>
        /// <param name="zoneId">The zone to invalidate</param>
        public void InvalidateZone(string zoneId)
        {
            if (_zones.TryGetValue(zoneId, out var zone))
            {
                zone.Invalidate();
            }
        }

        /// <summary>
        /// Marks all zones as needing redraw
        /// </summary>
        /// <summary>
        /// Marks all zones as needing redraw - LOCK FREE
        /// </summary>
        public void InvalidateAll()
        {
            foreach (var zone in _zones.Values)
            {
                zone.Invalidate();
            }
        }

        /// <summary>
        /// Renders all dirty zones in priority order
        /// </summary>
        /// <param name="renderData">Optional data to pass to zone renderers</param>
        /// <summary>
        /// Renders all dirty zones - ONLY CALL FROM MAIN THREAD
        /// </summary>
        public void RenderDirtyZones(object? renderData = null)
        {
            if (_isRendering) return; // Prevent nested rendering
            
            _isRendering = true;
            try
            {
                var dirtyZones = _zones.Values
                    .Where(z => z.IsDirty)
                    .OrderBy(z => z.RenderPriority)
                    .ToList();

                foreach (var zone in dirtyZones)
                {
                    RenderZone(zone, renderData);
                }
            }
            finally
            {
                _isRendering = false;
            }
        }

        /// <summary>
        /// Renders specific zones regardless of dirty state
        /// </summary>
        /// <param name="zoneIds">The zones to render</param>
        /// <param name="renderData">Optional data to pass to zone renderers</param>
        /// <summary>
        /// Renders specific zones - ONLY CALL FROM MAIN THREAD
        /// </summary>
        public void RenderZones(IEnumerable<string> zoneIds, object? renderData = null)
        {
            if (_isRendering) return; // Prevent nested rendering
            
            _isRendering = true;
            try
            {
                var zonesToRender = zoneIds
                    .Where(id => _zones.ContainsKey(id))
                    .Select(id => _zones[id])
                    .OrderBy(z => z.RenderPriority)
                    .ToList();

                foreach (var zone in zonesToRender)
                {
                    RenderZone(zone, renderData);
                }
            }
            finally
            {
                _isRendering = false;
            }
        }

        /// <summary>
        /// Clears a specific zone's content area
        /// </summary>
        /// <param name="zoneId">The zone to clear</param>
        public void ClearZone(string zoneId)
        {
            if (!_zones.TryGetValue(zoneId, out var zone))
                return;

            ClearRegion(zone.Bounds);
        }

        /// <summary>
        /// Gets the current boundaries of a zone
        /// </summary>
        /// <param name="zoneId">The zone identifier</param>
        /// <returns>The zone's current bounds, or null if not found</returns>
        public Region? GetZoneBounds(string zoneId)
        {
            return _zones.TryGetValue(zoneId, out var zone) ? zone.Bounds : null;
        }

        /// <summary>
        /// Gets all registered zone identifiers
        /// </summary>
        /// <returns>Collection of zone IDs</returns>
        public IEnumerable<string> GetZoneIds()
        {
            return _zones.Keys;
        }

        /// <summary>
        /// Checks if a zone exists and is dirty
        /// </summary>
        /// <param name="zoneId">The zone to check</param>
        /// <returns>True if the zone exists and needs rendering</returns>
        public bool IsZoneDirty(string zoneId)
        {
            return _zones.TryGetValue(zoneId, out var zone) && zone.IsDirty;
        }

        private void RenderZone(RenderZone zone, object? renderData)
        {
            if (_zoneRenderers.TryGetValue(zone.ZoneId, out var renderer))
            {
                try
                {
                    renderer(zone, renderData);
                    zone.MarkRendered();
                }
                catch (Exception ex)
                {
                    // Log error but continue rendering other zones
                    System.Diagnostics.Debug.WriteLine($"Error rendering zone {zone.ZoneId}: {ex.Message}");
                }
            }
        }

        private void InvalidateDependentZones(string changedZoneId)
        {
            foreach (var kvp in _dependencies)
            {
                bool containsZone;
                lock (kvp.Value) // Lock individual dependency set
                {
                    containsZone = kvp.Value.Contains(changedZoneId);
                }
                
                if (containsZone && _zones.TryGetValue(kvp.Key, out var dependentZone))
                {
                    dependentZone.Invalidate();
                }
            }
        }

        private void ClearRegion(Region region)
        {
            for (int y = 0; y < region.Height; y++)
            {
                _console.SetCursorPosition(region.Left, region.Top + y);
                _console.Write(new string(' ', region.Width));
            }
        }
    }
}