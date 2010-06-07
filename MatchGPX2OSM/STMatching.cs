﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LK.GeoUtils;
using LK.GeoUtils.Geometry;
using LK.GPXUtils;

namespace LK.MatchGPX2OSM {
	public class STMatching {
		protected RoadGraph _graph;
		List<CandidateGraphLayer> _layers;

		public STMatching(RoadGraph graph) {
			_graph = graph;
		}

		/// <summary>
		/// Finds all candidates points for given GPS track point
		/// </summary>
		/// <param name="gpxPt">GPS point</param>
		/// <returns>Collection of points candidate points on road segments</returns>
		public IEnumerable<CandidatePoint> FindCandidatePoints(GPXPoint gpxPt) {
			List<CandidatePoint> result = new List<CandidatePoint>();
			BBox gpxBbox = new BBox(new IPointGeo[] { gpxPt });
			gpxBbox.Inflate(0.0007, 0.0011);
			
			foreach (var road in _graph.ConnectionGeometries) {
				if (Topology.Intersects(gpxBbox, road.BBox)) {
					PointGeo projectedPoint = Topology.ProjectPoint(gpxPt, road);
					result.Add(new CandidatePoint() { Latitude = projectedPoint.Latitude, Longitude = projectedPoint.Longitude, Road = road, ObservationProbability = CalculateObservationProbability(gpxPt, projectedPoint) });
				}
			}

			return result;
		}

		/// <summary>
		/// Calculates observation probability
		/// </summary>
		/// <param name="original">GPS track point</param>
		/// <param name="candidate">Candidate point</param>
		/// <returns>double representing probability that GPS track point corresponds with Candidate point</returns>
		public double CalculateObservationProbability(GPXPoint original, PointGeo candidate) {
			double distance = Calculations.GetDistance2D(original, candidate);
			return Math.Exp(-distance * distance / (2 * 20 * 20)) / (20 * Math.Sqrt(Math.PI * 2));
		}

		/// <summary>
		/// Calculates transmission probability for connection
		/// </summary>
		/// <param name="c">Connection</param>
		/// <returns>double value representing transmission probability</returns>
		public double CalculateTransmissionProbability(CandidatesConection c) {
			double gcd = Calculations.GetDistance2D(c.From, c.To);
			double shortestPath = ComputeShortestPath(c.From, c.To);

			return gcd / shortestPath;
		}

		/// <summary>
		/// Finds shortest path between two points along routes
		/// </summary>
		/// <param name="from">Start point</param>
		/// <param name="to">Destination point</param>
		/// <returns>length of the path in meters</returns>
		public double ComputeShortestPath(CandidatePoint from, CandidatePoint to) {
			if (from.Road == to.Road) {
				return Calculations.GetPathLength(from, to, from.Road);
			}
			return 0;
		}

		public void AStar(CandidatePoint from, CandidatePoint to) {
			
		}
		
		public void Match(GPXTrackSegment gpx) {
			_layers = new List<CandidateGraphLayer>();

			//Find candidate points + ObservationProbability
			foreach (var gpxPoint in gpx.Nodes) {
				var candidates = FindCandidatePoints(gpxPoint).OrderByDescending(p => p.ObservationProbability);

				CandidateGraphLayer layer = new CandidateGraphLayer() { TrackPoint = gpxPoint };
				layer.Candidates.AddRange(candidates.Take(Math.Min(candidates.Count(), 5)));
				_layers.Add(layer);
			}

			// Transmission probability
			ConnectLayers();
		}

		/// <summary>
		/// Creates connections among candidate points in subsequent layers
		/// </summary>
		void ConnectLayers() {
			for (int l = 0; l < _layers.Count-1; l++) {
				for (int i = 0; i < _layers[l].Candidates.Count; i++) {
					for (int j = 0; j < _layers[l+1].Candidates.Count; j++) {
						AddConnection(_layers[l].Candidates[i], _layers[l + 1].Candidates[j]);
					}
				}
			}
		}

		/// <summary>
		/// Adds connection between two candidate points
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		void AddConnection(CandidatePoint from, CandidatePoint to) {
			CandidatesConection c = new CandidatesConection() { From = from, To = to };
			from.OutgoingConnections.Add(c);
			to.IncomingConnections.Add(c);

			c.TransmissionProbability = CalculateTransmissionProbability(c);
		}
	}
}
