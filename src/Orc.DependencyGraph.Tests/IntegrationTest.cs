using NUnit.Framework;
using Orc.DependencyGraph.GraphB;
using Orc.DependencyGraph.GraphD;
using Orc.DependencyGraph.Tests.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Orc.DependencyGraph.Tests.TestSandbox;

namespace Orc.DependencyGraph.Tests
{


    public static class TestSandbox
    {
        private static Random _rand = new Random();

        public static Random Rand
        {
            get
            {
                return _rand;
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_rand.Next(s.Length)]).ToArray());
        }

        public static IEnumerable<dynamic> GetPositionBag()
        {
            var positions = GetPositionBagInternal();
            var totalMarketValue = positions.Sum(pose => (double)pose.MarketValue);
            foreach (var position in positions)
            {
                position.WeightInFund = position.MarketValue / totalMarketValue;
            }

            return positions;
        }

        private static IEnumerable<dynamic> GetPositionBagInternal()
        {
            for (var i = 0; i <= 15000; i++)
            {
                var position = new ExpandoObject() as dynamic;
                position.Asset = RandomString(10);
                position.Quantity = Rand.Next(500, 10000);
                position.Price = Rand.NextDouble() * 10;
                position.MarketValue = (double)position.Quantity * (double)position.Price;
                position.MarketValue = (double)position.Quantity * (double)position.Price;

                yield return position;
            }
        }



     

  
        public class FundNavIndicator : IndicatorBase<dynamic, IEnumerable<dynamic>>
        {
            public FundNavIndicator() : base("FundNav")
            {
            }

            public override bool Accept(IEvent ev)
            {
                return ev.Name == "MarketValue";
            }

            public override void Update(IEvent ev, dynamic position, IEnumerable<dynamic> context)
            {
                var nav = context.Sum(pose => (double)pose.MarketValue);

                foreach (var pose in context)
                {
                    pose.FundNav = nav;
                }

               
            }
        }

        public class MarketValueIndicator : IndicatorBase<dynamic, IEnumerable<dynamic>>
        {
            public MarketValueIndicator() : base("MarketValue")
            {
            }

            public override bool Accept(IEvent ev)
            {
                return ev.Name == "Price" || ev.Name == "Quantity";
            }

            public override void Update(IEvent ev, dynamic position, IEnumerable<dynamic> context)
            {
                if (ev.Name == "Price") position.MarketValue = (double)position.Quantity * (double)ev.Value;
                if (ev.Name == "Quantity") position.MarketValue = (double)position.Price * (double)ev.Value;

            }
        }

        public class WeightInFundIndicator : IndicatorBase<dynamic, IEnumerable<dynamic>>
        {
            public WeightInFundIndicator() : base("WeightInFund")
            {
            }

            public override bool Accept(IEvent ev)
            {
                return ev.Name == "MarketValue" || ev.Name == "YieldToMaturity";
            }

            public override void Update(IEvent ev, dynamic position, IEnumerable<dynamic> context)
            {
                var totalMarketValue = context.Sum(pose => (double)pose.MarketValue);

                position.WeightInFund = (double)ev.Value / totalMarketValue;
            }

        }

        public class AccruedCouponIndicator : IndicatorBase<dynamic, IEnumerable<dynamic>>
        {
            public AccruedCouponIndicator() : base("AccruedCoupon")
            {
            }

            public override bool Accept(IEvent ev)
            {
                return ev.Name == "Price";
            }

            public override void Update(IEvent ev, dynamic position, IEnumerable<dynamic> context)
            {
                var totalMarketValue = context.Sum(pose => (double)pose.MarketValue);

                position.WeightInFund = (double)ev.Value / totalMarketValue;
            }

        }

        public class YieldToMaturityIndicator : IndicatorBase<dynamic, IEnumerable<dynamic>>
        {
            public YieldToMaturityIndicator() : base("YieldToMaturity")
            {
            }

            public override bool Accept(IEvent ev)
            {
                return ev.Name == "AccruedCoupon";
            }

            public override void Update(IEvent ev, dynamic position, IEnumerable<dynamic> context)
            {
                var totalMarketValue = context.Sum(pose => (double)pose.MarketValue);

                position.WeightInFund = (double)ev.Value / totalMarketValue;
            }

        }

        public class CreditRiskIndicator : IndicatorBase<dynamic, IEnumerable<dynamic>>
        {
            public CreditRiskIndicator() : base("CreditRisk")
            {
            }

            public override bool Accept(IEvent ev)
            {
                return ev.Name == "YieldToMaturity";
            }

            public override void Update(IEvent ev, dynamic position, IEnumerable<dynamic> context)
            {
                var totalMarketValue = context.Sum(pose => (double)pose.MarketValue);

                position.WeightInFund = (double)ev.Value / totalMarketValue;
            }

        }

    }

    [TestFixture]
    public class TestIntegration
    {
        private String GetPadding(int level)
        {
            var padding = "     ";

            var actual = "";

            for(var i=0; i< level; i++)
            {
                actual += padding;
            }

            return actual;
        }

        private void ProcessNode(INode<IIndicator<dynamic, IEnumerable<dynamic>>> node, IEvent ev, List<dynamic> impactedPositions, List<dynamic> allPositions)
        {
            foreach (var pose in impactedPositions)
            {
                node.Value.Update(ev, pose, allPositions);
            }
        }

        [Test]
        public void TestPropagation()
        {
            var positions = GetPositionBag().ToList();

            var graph = new Graph<IIndicator<dynamic, IEnumerable<dynamic>>>();

            var marketValueIndicator = new MarketValueIndicator();
            var weightInFundIndicator = new WeightInFundIndicator();
            var fundNavIndicator = new FundNavIndicator();

            var accruedCoupon = new AccruedCouponIndicator();
            var yieldToMaturity = new YieldToMaturityIndicator();
            var creditRisk = new CreditRiskIndicator();

            graph.AddSequences(
                new[]
                {
                    new List<IIndicator<dynamic, IEnumerable<dynamic>>> { marketValueIndicator, weightInFundIndicator },
                    new List<IIndicator<dynamic, IEnumerable<dynamic>>> { marketValueIndicator, fundNavIndicator },
                    new List<IIndicator<dynamic, IEnumerable<dynamic>>> { fundNavIndicator, creditRisk },
                    new List<IIndicator<dynamic, IEnumerable<dynamic>>> { creditRisk, weightInFundIndicator },
                    new List<IIndicator<dynamic, IEnumerable<dynamic>>> { yieldToMaturity, weightInFundIndicator },
                    new List<IIndicator<dynamic, IEnumerable<dynamic>>> { yieldToMaturity, creditRisk },
                    new List<IIndicator<dynamic, IEnumerable<dynamic>>> { accruedCoupon, yieldToMaturity }
                });

            var sortedGraph = graph.Sort().ToList();
     
            var target = positions.First();

            var priceEvent = new Event("Price", target.Asset, (double)target.Price + 10);


            var impactedPositions = positions.Where(pose => pose.Asset == priceEvent.Subject).ToList();

            var nodes = sortedGraph
                  .Where(n => n.Value.Accept(priceEvent));

           
            foreach (var node in nodes)
            {
                Trace.WriteLine(String.Format("{0}", node.Value.Label));

                ProcessNode(node, priceEvent, impactedPositions, positions);

                foreach (var descendant in node.UniqueDescendants())
                {
                    Trace.WriteLine(String.Format("{0}{1}", GetPadding(descendant.Level), descendant.Value.Label));
                    ProcessNode(descendant, priceEvent, impactedPositions, positions);
                }

            }

        }

    }
}
