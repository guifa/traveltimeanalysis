﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LK.OSMUtils.OSMDatabase;
using LK.OSMUtils.OSMDataSource;
using LK.GeoUtils.Geometry;

namespace LK.MatchGPX2OSM {
	public class RoadGraph {
		private Dictionary<int, Node> _nodes;
		/// <summary>
		/// Gets collection of all nodes in this graph
		/// </summary>
		public IEnumerable<Node> Nodes {
			get {
				return _nodes.Values;
			}
		}

		private List<Connection> _connections;
		/// <summary>
		/// Gets collection of all enges in this graph
		/// </summary>
		public IEnumerable<Connection> Connections {
			get {
				return _connections;
			}
		}

		/// <summary>
		/// Creates a new RoadGraph
		/// </summary>
		public RoadGraph() {
			_nodes = new Dictionary<int, Node>();
			_connections = new List<Connection>();
		}

		/// <summary>
		/// Builds road graph from map data
		/// </summary>
		/// <param name="map">OSMDB with preprocessed map data from OSM2Routing utility</param>
		public void BuildGraph(OSMDB map) {
			foreach (var segment in map.Ways) {
				Node start = GetOrCreateNode(segment.Nodes[0]);
				Node end = GetOrCreateNode(segment.Nodes[segment.Nodes.Count - 1]);

				double speed = double.Parse(segment.Tags["speed"].Value, System.Globalization.CultureInfo.InvariantCulture);
				int wayId = int.Parse(segment.Tags["way-id"].Value, System.Globalization.CultureInfo.InvariantCulture);

				Polyline<IPointGeo> geometry = new Polyline<IPointGeo>();
				foreach (var n in segment.Nodes) {
					OSMNode mapPoint = map.Nodes[n];
					geometry.Nodes.Add(new PointGeo(mapPoint.Latitude, mapPoint.Longitude));
				}

				if (segment.Tags["accessible"].Value == "yes") {
					Connection sc = new Connection(start, end) { Speed = speed, Geometry = geometry, ID = wayId };
					start.AddConnection(sc);
					end.AddConnection(sc);

					_connections.Add(sc);
				}

				if (segment.Tags["accessible-reverse"].Value == "yes") {
					Connection sc = new Connection(end, start) { Speed = speed, Geometry = geometry, ID = wayId };
					start.AddConnection(sc);
					end.AddConnection(sc);

					_connections.Add(sc);
				}
			}
		}

		/// <summary>
		/// Gets Node with specific ID from the internal storage if available or creates a new one
		/// </summary>
		/// <param name="nodeId">The ID of the node</param>
		/// <returns>The node with specific ID</returns>
		private Node GetOrCreateNode(int nodeId) {
			if (_nodes.ContainsKey(nodeId) == false) {
				Node n = new Node() { ID = nodeId };
				_nodes.Add(nodeId, n);
				return n;
			}
			else {
				return _nodes[nodeId];
			}
		}
	}
}
