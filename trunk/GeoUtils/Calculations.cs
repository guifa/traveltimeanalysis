﻿using System;
using System.Collections.Generic;
using System.Text;

using LK.GeoUtils.Geometry;
using System.Collections;

namespace LK.GeoUtils {
	/// <summary>
	/// Encapsulates various geographic calculations
	/// </summary>
	public static class Calculations {
		public const double EarthRadius = 6371010.0;
		public const double EpsLength = 0.01;

		static IDistanceCalculator _distanceCalculator;

		/// <summary>
		/// Initializes static class
		/// </summary>
		static Calculations() {
			_distanceCalculator = new GreatCircleDistanceCalculator();
		}

		/// <summary>
		/// Calculates distance between 2 points on geoid surface (ignores points elevations)
		/// </summary>
		/// <param name="pt1">First point</param>
		/// <param name="pt2">Second point</param>
		/// <returns>distance between given points in meters</returns>
		public static double GetDistance2D(IPointGeo pt1, IPointGeo pt2) {
			return _distanceCalculator.Calculate2D(pt1, pt2);
		}

		/// <summary>
		/// Calculates distance of the point from the segment
		/// </summary>
		/// <param name="point">The point</param>
		/// <param name="segment">The segment</param>
		/// <returns>the distance of point from the segment in meters</returns>
		public static double GetDistance2D(IPointGeo point, Segment<IPointGeo> segment) {
			IPointGeo projectedPoint = Topology.ProjectPoint(point, segment);
			return _distanceCalculator.Calculate2D(point, projectedPoint);
		}

		/// <summary>
		/// Calculates distance of the point from the line
		/// </summary>
		/// <param name="point">The point</param>
		/// <param name="line">The line</param>
		/// <returns>the distane of the point from the line in meters</returns>
		public static double GetDistance2D(IPointGeo point, IPolyline<IPointGeo> line) {
			double minDistance = double.PositiveInfinity;

			for (int i = 0; i < line.NodesCount -1; i++) {
				Segment<IPointGeo> segment = new Segment<IPointGeo>(line.Nodes[i], line.Nodes[i + 1]);
				IPointGeo projectedPoint = Topology.ProjectPoint(point, segment);
				minDistance = Math.Min(minDistance, _distanceCalculator.Calculate2D(point, projectedPoint));
			}

			return minDistance;
		}

		/// <summary>
		/// Calculates length of the pathe between two points along the specific segment
		/// </summary>
		/// <param name="from">The point where path starts</param>
		/// <param name="to">The point where path ends</param>
		/// <param name="path">Segment that defines path geometry</param>
		/// <returns>The length of the path between points from and to along the segment</returns>
		public static double GetPathLength(IPointGeo from, IPointGeo to, Segment<IPointGeo> path) {
			return Calculations.GetDistance2D(from, to);
		}


		/// <summary>
		/// Calculates length of the pathe between two points along the specific segment
		/// </summary>
		/// <param name="from">The point where path starts</param>
		/// <param name="to">The point where path ends</param>
		/// <param name="path">Polyline that defines path geometry</param>
		/// <returns>The length of the path between points from and to along the polyline</returns>
		public static double GetPathLength(IPointGeo from, IPointGeo to, IPolyline<IPointGeo> path) {
			var segments = path.Segments;

			int pointFound = 0;
			int pointFoundLast = 0;
			bool fromFound = false; bool toFound = false;
			double distance = 0;
			foreach (var segment in path.Segments) {
				pointFoundLast = pointFound;
				IPointGeo[] points = new IPointGeo[] { segment.StartPoint, segment.EndPoint };

				if (!fromFound && Calculations.GetDistance2D(from, segment) < EpsLength) {
					fromFound = true;
					points[pointFound++] = from;
				}

				if (!toFound && Calculations.GetDistance2D(to, segment) < EpsLength) {
					toFound = true;
					points[pointFound++] = to;
				}

				if (pointFound > 0) {
					if(pointFound == pointFoundLast) 
						distance += segment.Length;
					else
						distance += Calculations.GetDistance2D(points[0], points[1]);

					if (pointFound == 2)
						return distance;
				}
			}

			throw new ArgumentException("One or more points do not lie on the given path");
		}

		/// <summary>
		/// Calculates length of the specific polyline
		/// </summary>
		/// <param name="line">The line to be measured</param>
		/// <returns>length of the line in meters</returns>
		public static double GetLength(IPolyline<IPointGeo> line) {
			double length = 0;

			foreach (var segment in line.Segments) {
				length += GetLength(segment);
			}

			return length;
		}

		/// <summary>
		/// Calculates length of the specific line segment
		/// </summary>
		/// <param name="segment">The line segment to be measured</param>
		/// <returns>length of the line segment in meters</returns>
		public static double GetLength(Segment<IPointGeo> segment) {
			return Calculations.GetDistance2D(segment.StartPoint, segment.EndPoint);
		}

		/// <summary>
		/// Calculate average position of the group of points
		/// </summary>
		/// <param name="point">The collection of points used to compute average position</param>
		/// <returns>point with average position of group of points</returns>
		public static PointGeo Averaging(IEnumerable points) {
			int count = 0;
			double lat = 0, lon = 0;

			foreach (IPointGeo point in points) {
				lat += point.Latitude;
				lon += point.Longitude;
				count++;
			}

			return new PointGeo(lat / count, lon / count);
		}

		/// <summary>
		/// Converts angle in degrees to radians
		/// </summary>
		/// <param name="angle">The angle in degrees</param>
		/// <returns>The angle in radians</returns>
		public static double ToRadians(double angle) {
			return angle * Math.PI / 180;
		}

		/// <summary>
		/// Convevrts angle in radians to degrees
		/// </summary>
		/// <param name="angle">The angle in radians</param>
		/// <returns>The angle in radians</returns>
		public static double ToDegrees(double angle) {
			return angle * 180 / Math.PI;
		}
	}
}
