﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LK.GeoUtils;
using LK.GeoUtils.Geometry;
using LK.OSMUtils.OSMDatabase;

namespace LK.Analyzer {
	public class TTAnalyzer {
		OSMDB _map;

		public TTAnalyzer(OSMDB map) {
			_map = map;
		}

		public Model Analyze(IEnumerable<TravelTime> travelTimes, SegmentInfo segment) {

			List<TravelTime> filteredTravelTimes = new List<TravelTime>();
			foreach (var tt in travelTimes) {
				if (tt.Stops.Where(stop => stop.Length.TotalSeconds > 5 * 60).Count() > 0)
					continue;

				filteredTravelTimes.Add(tt);
			}

			Model result = new Model();
			result.Segment = segment;
			if (filteredTravelTimes.Count == 0)
				return result;

			result.FreeFlowTravelTime = EstimateFreeFlowTime(filteredTravelTimes);
			if (_map.Nodes[segment.NodeToID].Tags.ContainsTag("highway") && _map.Nodes[segment.NodeToID].Tags["highway"].Value == "traffic_signals") {
				result.TrafficSignalsDelay = EstimateTafficSignalsDelay(filteredTravelTimes, segment);
			}

			AnalyzeTrafficDelay(filteredTravelTimes, result);

			return result;
		}

		double EstimateFreeFlowTime(IEnumerable<TravelTime> travelTimes) {
			double PercentageFastest = 10.0;
			int MinimalCount = 3;

			int desiredCount = (int)Math.Max(travelTimes.Count() * PercentageFastest / 100.0, MinimalCount);
			int count = Math.Min(travelTimes.Count(), desiredCount);

			var toEstimate = travelTimes.OrderBy(tt => tt.TotalTravelTime).Take(count);

			return toEstimate.Sum(tt => tt.TotalTravelTime.TotalSeconds - tt.Stops.Sum(stop => stop.Length.TotalSeconds)) / count;
		}

		TrafficSignalsDelayInfo EstimateTafficSignalsDelay(IEnumerable<TravelTime> travelTimes, SegmentInfo segment) {
			int totalStops = travelTimes.Where(tt => tt.Stops.Count > 0).Count();
			double totalStopsLength = travelTimes.Where(tt => tt.Stops.Count > 0).Sum(tt => tt.Stops.Last().Length.TotalSeconds);

			if (totalStops > 0)
				return new TrafficSignalsDelayInfo() { Probability = (double)totalStops / travelTimes.Count(), Length = totalStopsLength / totalStops };
			else
				return new TrafficSignalsDelayInfo() { Probability = 0, Length = 0 };
		}

		class TravelTimeDelay {
			public TravelTime TravelTime { get; set; }
			public double Delay { get; set; }
		}

		void AnalyzeTrafficDelay(IEnumerable<TravelTime> travelTimes, Model model) {
			List<TravelTimeDelay> delays = new List<TravelTimeDelay>();
			foreach (var traveltime in travelTimes) {
				double delay = 0;
				if (model.TrafficSignalsDelay.Probability > 0 && traveltime.Stops.Count > 0)
					delay = traveltime.TotalTravelTime.TotalSeconds - model.FreeFlowTravelTime - traveltime.Stops.Last().Length.TotalSeconds;
				else
					delay = traveltime.TotalTravelTime.TotalSeconds - model.FreeFlowTravelTime;

				delay = Math.Max(0, delay);
				delays.Add(new TravelTimeDelay() { TravelTime = traveltime, Delay = delay });
			}
			delays.Sort(new Comparison<TravelTimeDelay>((TravelTimeDelay td1, TravelTimeDelay td2) => td1.Delay.CompareTo(td2.Delay)));

			//model.AvgDelay = delays.Sum(delay => delay.Delay) / delays.Count;
			model.AvgDelay = delays[delays.Count / 2].Delay;

			List<List<TravelTimeDelay>> travelTimeClusters = null;
			DBScan<TravelTimeDelay> clusterAnalyzer = new DBScan<TravelTimeDelay>(new DBScan<TravelTimeDelay>.FindNeighbours(FindNeighbours));
			for (int i = 0; i < resolutions.Length; i++) {
				resolutionIndex = i;
				travelTimeClusters = clusterAnalyzer.ClusterAnalysis(delays, 2);

				if (travelTimeClusters.Sum(cluster => cluster.Count) > 0.75 * delays.Count)
					break;
			}

			foreach (var cluster in travelTimeClusters) {
				TrafficDelayInfo delayInfo = new TrafficDelayInfo();
				if(resolutions[resolutionIndex].Dates == DatesHandling.Any)
					delayInfo.AppliesTo = DayOfWeek.Any;
				else if(resolutions[resolutionIndex].Dates == DatesHandling.WeekendWorkdays)
					delayInfo.AppliesTo = (DayOfWeek.Workday & DayOfWeekFactory.FromDate(cluster[0].TravelTime.TimeStart)) > 0 ? DayOfWeek.Workday : DayOfWeek.Weekend;
				else
					delayInfo.AppliesTo = DayOfWeekFactory.FromDate(cluster[0].TravelTime.TimeStart);

				cluster.Sort(new Comparison<TravelTimeDelay>((TravelTimeDelay td1, TravelTimeDelay td2) => td1.Delay.CompareTo(td2.Delay)));

				delayInfo.Delay = cluster.Sum(tt => tt.Delay) / cluster.Count;
				//delayInfo.Delay = cluster[cluster.Count / 2].Delay;
				delayInfo.From = cluster.Min(tt => tt.TravelTime.TimeStart.TimeOfDay);
				delayInfo.To = cluster.Max(tt => tt.TravelTime.TimeEnd.TimeOfDay);

				model.TrafficDelay.Add(delayInfo);
			}
		}

		TimeResolution[] resolutions = new TimeResolution[] {
			new TimeResolution() {Dates = DatesHandling.Days, EpsMinutes = 15},
			new TimeResolution() {Dates = DatesHandling.Days, EpsMinutes = 30},
			new TimeResolution() {Dates = DatesHandling.WeekendWorkdays, EpsMinutes = 30},
			new TimeResolution() {Dates = DatesHandling.Days, EpsMinutes = 60},
			new TimeResolution() {Dates = DatesHandling.WeekendWorkdays, EpsMinutes = 60},
			new TimeResolution() {Dates = DatesHandling.Days, EpsMinutes = 120},
			new TimeResolution() {Dates = DatesHandling.WeekendWorkdays, EpsMinutes = 120},
			new TimeResolution() {Dates = DatesHandling.Any, EpsMinutes = 30},
			new TimeResolution() {Dates = DatesHandling.Any, EpsMinutes = 60},
			new TimeResolution() {Dates = DatesHandling.Any, EpsMinutes = 120},
		};
		int resolutionIndex = 0;

		List<TravelTimeDelay> FindNeighbours(TravelTimeDelay target, IList<TravelTimeDelay> items) {
			double eps = 0.10 * target.TravelTime.TotalTravelTime.TotalSeconds;
			List<TravelTimeDelay> result = new List<TravelTimeDelay>();
			for (int i = 0; i < items.Count; i++) {
				if (items[i] != target && Math.Abs(target.Delay - items[i].Delay) < eps && resolutions[resolutionIndex].AreClose(target.TravelTime.TimeStart, items[i].TravelTime.TimeStart))
					result.Add(items[i]);
			}

			return result;
		}

	}
}
