﻿using LK.GeoUtils.Geometry;
using System;
using System.Collections.Generic;
using Xunit;

namespace GeoUtils.Tests {
    /// <summary>
    ///This is a test class for PolylineTest and is intended
    ///to contain all PolylineTest Unit Tests
    ///</summary>
	
	public class PolylineTest {
		[Fact()]
		public void PolylineConstructorInitializesInteralFields() {
			Polyline<PointGeo> target = new Polyline<PointGeo>();

			Assert.NotNull(target.Nodes);
		}

		[Fact()]
		public void PolylineNodesGetsListOfNodes() {
			PointGeo p1 = new PointGeo(1, 2);
			PointGeo p2 = new PointGeo(3, 4);

			Polyline<PointGeo> target = new Polyline<PointGeo>();
			target.Nodes.Add(p1);
			target.Nodes.Add(p2);

			Assert.Equal(p1, target.Nodes[0]);
			Assert.Equal(p2, target.Nodes[1]);
		}

		[Fact()]
		public void PolylineNodesCountReturnsCorrectValue() {
			PointGeo p1 = new PointGeo(1, 2);
			PointGeo p2 = new PointGeo(3, 4);

			Polyline<PointGeo> target = new Polyline<PointGeo>();
			target.Nodes.Add(p1);
			target.Nodes.Add(p2);

			Assert.Equal(2, target.NodesCount);
		}
	}
}