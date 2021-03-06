﻿using Blazor.Diagrams.Core.Models.Core;
using Microsoft.AspNetCore.Components.Web;
using System;

namespace Blazor.Diagrams.Core.Default
{
    public class ZoomSubManager : DiagramSubManager
    {
        private const float _scaleBy = 1.05f;

        public ZoomSubManager(DiagramManager diagramManager) : base(diagramManager)
        {
            DiagramManager.Wheel += DiagramManager_Wheel;
        }

        private void DiagramManager_Wheel(WheelEventArgs e)
        {
            if (!DiagramManager.Options.Zoom.Enabled)
                return;

            var oldZoom = DiagramManager.Zoom;
            var deltaY = DiagramManager.Options.Zoom.Inverse ? e.DeltaY * -1 : e.DeltaY;
            var newZoom = deltaY > 0 ? oldZoom * _scaleBy : oldZoom / _scaleBy;

            if (newZoom < 0)
                return;

            // Other algorithms (based only on the changes in the zoom) don't work for our case
            // This solution is taken as is from react-diagrams (ZoomCanvasAction)
            var clientWidth = DiagramManager.Container.Width;
            var clientHeight = DiagramManager.Container.Height;
            var widthDiff = clientWidth * newZoom - clientWidth * oldZoom;
            var heightDiff = clientHeight * newZoom - clientHeight * oldZoom;
            var clientX = e.ClientX - DiagramManager.Container.Left;
            var clientY = e.ClientY - DiagramManager.Container.Top;
            var xFactor = (clientX - DiagramManager.Pan.X) / oldZoom / clientWidth;
            var yFactor = (clientY - DiagramManager.Pan.Y) / oldZoom / clientHeight;
            var newPanX = DiagramManager.Pan.X - widthDiff * xFactor;
            var newPanY = DiagramManager.Pan.Y - heightDiff * yFactor;

            newZoom = Math.Clamp(newZoom, DiagramManager.Options.Zoom.Minimum, DiagramManager.Options.Zoom.Maximum);
            if (newZoom == DiagramManager.Zoom)
                return;

            DiagramManager.Pan = new Point(newPanX, newPanY);
            DiagramManager.SetZoom(newZoom);
        }

        public override void Dispose()
        {
            DiagramManager.Wheel -= DiagramManager_Wheel;
        }
    }
}
