﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LK.GeoUtils;
using LK.GeoUtils.Geometry;
using LK.GPXUtils;

namespace LK.MatchGPX2OSM {
	public class STMatching {
		public static int MaxCandidatesCount = 5;

		RoadGraph _graph;
		CandidatesGraph _candidatesGraph;

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
			double sigma = 20;
			double distance = Calculations.GetDistance2D(original, candidate);
			return 0.5 * Math.Exp(-distance * distance / (2 * sigma * sigma)) / (sigma * Math.Sqrt(Math.PI * 2));
		}

		/// <summary>
		/// Calculates transmission probability for connection
		/// </summary>
		/// <param name="c">Connection</param>
		/// <returns>double value representing transmission probability</returns>
		public double CalculateTransmissionProbability(CandidatesConection c) {
			double gcd = Calculations.GetDistance2D(c.From, c.To);
			double shortestPath = ComputeShortestPath(c.From, c.To);
			if (gcd == 0 && shortestPath == 0)
				return 1;
			else 
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
			else {
				Astar pathfinder = new Astar(_graph);
				double length = double.PositiveInfinity;
				pathfinder.FindPath(from, to, ref length);
				return length;
			}
		}
		
		/// <summary>
		/// Matches the given GPX track to the map
		/// </summary>
		/// <param name="gpx"></param>
		/// <returns>The list of candidates points that matched the gpx track</returns>
		public List<CandidatePoint> Match(GPXTrackSegment gpx) {
			_candidatesGraph = new CandidatesGraph();

			//Find candidate points + ObservationProbability
			foreach (var gpxPoint in gpx.Nodes) {
				var candidates = FindCandidatePoints(gpxPoint).OrderByDescending(p => p.ObservationProbability);

				CandidateGraphLayer layer = new CandidateGraphLayer() { TrackPoint = gpxPoint };
				layer.Candidates.AddRange(candidates.Take(Math.Min(candidates.Count(), 5)));
				_candidatesGraph.Layers.Add(layer);
			}
			_candidatesGraph.ConnectLayers();

			//TODO calculate transmission probability

			// FInd matched sequence
			foreach (var candidate in _candidatesGraph.Layers[0].Candidates) {
				candidate.HighestProbability = candidate.ObservationProbability;
			}

			for (int i = 0; i < _candidatesGraph.Layers.Count - 1; i++) {
				foreach (var candidate in _candidatesGraph.Layers[i + 1].Candidates) {
					foreach (var connection in candidate.IncomingConnections) {
						double score = connection.From.HighestProbability + candidate.ObservationProbability * connection.TransmissionProbability;
						if (score > candidate.HighestProbability) {
							candidate.HighestProbability = score;
							candidate.HighesScoreParent = connection.From;
						}
					}
				}
			}

			List<CandidatePoint> result = new List<CandidatePoint>();
			CandidatePoint current = new CandidatePoint() { HighestProbability = double.NegativeInfinity };
			foreach (var point in _candidatesGraph.Layers[_candidatesGraph.Layers.Count - 1].Candidates) {
				if (point.HighestProbability > current.HighestProbability) {
					current = point;
				}
			}

			while (current != null) {
				result.Add(current);
				current = current.HighesScoreParent;
			}

			return result;
		}
	}
}
