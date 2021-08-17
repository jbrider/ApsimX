using System;
using Models.Core;
using Models.PMF.Organs;
using Models.PMF;
using Models.PMF.Struct;
using Newtonsoft.Json;

namespace Models.Functions
{
    /// <summary>
    /// # [Name]
    /// Calculates the maximum leaf size (mm2/leaf) given its node position 
    /// C4 has dynamically created culms (tillers) where they have their own LeafNo separate to the main culm
    /// Also has differently named parameters
    /// </summary>
    [Serializable]
    public class BellCurveC4Function : Model, IFunction
    {
        /// <summary>The largest leaf position parameter</summary>
        [Link(Type = LinkType.Child, ByName = true)] IFunction LargestLeafParameter = null; 
        /// <summary>The largest leaf position</summary>
        [Link(Type = LinkType.Child, ByName = true)] IFunction LargestLeafPosition = null; // Node position where the largest leaf occurs (e.g. 10 is the 10th leaf from bottom to top)
        /// <summary>The area maximum</summary>
        [Link(Type = LinkType.Child, ByName = true)] IFunction AreaMax = null;             // Area of the largest leaf of a plant (mm2)
        /// <summary>The breadth</summary>
        [Link(Type = LinkType.Child, ByName = true)] IFunction Breadth = null;
        /// <summary>The skewness</summary>
        [Link(Type = LinkType.Child, ByName = true)] IFunction Skewness = null;
        /// <summary>The skewness</summary>
        [Link(Type = LinkType.Child, ByName = true)] IFunction EffectiveLeafNo1 = null;
        /// <summary>The skewness</summary>
        [Link(Type = LinkType.Child, ByName = true)] IFunction EffectiveFinalLeafNo1 = null;
        /// <summary>The culm</summary>
        [JsonIgnore] 
        public Culm CurrentCulm { get; set; } = null;

        /// <summary>Gets the value.</summary>
        public double Value(int arrayIndex = -1)
        {
            double chkleafNo = CurrentCulm.CurrentLeafNo;
            double leafNo = EffectiveLeafNo1.Value();
            double finalLeafNo = EffectiveFinalLeafNo1.Value(arrayIndex);
            var tmpFinal = (LargestLeafPosition as Model).Children[1];
            double chkFinal = (tmpFinal as IFunction).Value(arrayIndex);
            if(finalLeafNo != chkFinal)
            {
                int tmp = 0;
                tmp += 0;
            }
            double largestLeafPosition = finalLeafNo * LargestLeafParameter.Value(arrayIndex);
            double chkLargestLeafPosition = LargestLeafPosition.Value(arrayIndex);
            if (largestLeafPosition != chkLargestLeafPosition)
            {
                int tmp = 0;
                tmp += 0;
            }

            return AreaMax.Value(arrayIndex) * Math.Exp(Breadth.Value(arrayIndex) * Math.Pow(leafNo - largestLeafPosition, 2.0)
                                + Skewness.Value(arrayIndex) * (Math.Pow(leafNo - largestLeafPosition, 3.0))) * 100.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual BellCurveC4Function Clone()
        {
            BellCurveC4Function newFunction = (BellCurveC4Function)MemberwiseClone();
            newFunction.UpdateParents();//  Children.ForEach(f => f.Parent = newFunction);
            if (newFunction.Children[0].Parent != newFunction)
            {
                int tmpo = 0;
                tmpo += 1;
            }
            return newFunction;
        }

        void UpdateParents()
        {
            //Links aren't being updated - see if this will work
            Children.ForEach(f => f.Parent = this);

            (LargestLeafParameter as Model).Parent = this;
            (LargestLeafPosition as Model).Parent = this;
            (AreaMax as Model).Parent = this; 
            (Breadth as Model).Parent = this; 
            (Skewness as Model).Parent = this; 
            (EffectiveLeafNo1 as Model).Parent = this; 
            (EffectiveFinalLeafNo1 as Model).Parent = this; 
        }
    }
}
