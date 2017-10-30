﻿using System;
using System.Collections.Generic;
using System.Linq;
using Models.Core;

namespace Models.PMF.Photosynthesis
{
    /// <summary>
    /// Entry point for Photosynthesis model
    /// </summary>
    public class C3PhotoLink : Model
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DOY">Day of year</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="maxT">Maximum temp</param>
        /// <param name="minT">Minimum temp</param>
        /// <param name="radn">Radiation</param>
        /// <param name="lai">LAI</param>
        /// <param name="SLN">SLN</param>
        /// <param name="soilWaterAvail">Soil Water Supply</param>
        /// <param name="B">Unknown. Set to 1 for now.</param>
        /// <param name="RootShootRatio">Root Shoot Ratio</param>
        /// <param name="LeafAngle">Leaf Angle 0 horizontal, 90 vertical</param>
        /// <param name="SLNRatioTop">The ratio of SLN concentration at the top as a multiplier on avg SLN from Apsim</param>
        /// <param name="psiVc">Slope of linear relationship between Vmax per leaf are at 25°C and N, μmol CO2 mmol-1 N s-1</param>
        /// <param name="psiJ">Slope of linear relationship between Jmax per leaf are at 25°C and N, μmol CO2 mmol-1 N s-1</param>
        /// <param name="psiRd">Slope of linear relationship between Rd per leaf are at 25°C and N, μmol CO2 mmol-1 N s-1</param>
        /// <param name="psiFactor">Psi reduction factor that applies to all psi values. Can use as a genetic factor</param>
        /// <param name="Ca">Air CO2 partial pressure</param>
        /// <param name="CiCaRatio">Ratio of Ci to Ca, μbar</param>
        /// <param name="gm25">Mesophyll conductance for CO2 @ 25degrees, mol CO2 m-2 ground s-1 bar-1</param>
        /// <param name="structuralN">Amount of N needed to retain structure. Below this photosynthesis does not occur</param>
        /// <returns></returns>
        public double[] calc(int DOY, double latitude, double maxT, double minT, double radn, double lai, double SLN, double soilWaterAvail,
            double RootShootRatio, double LeafAngle, double B = 1, double SLNRatioTop = 1.32, double psiVc = 1.16, double psiJ = 2.4, double psiRd = 0.0116,
            double psiFactor = 1, double Ca = 400, double CiCaRatio = 0.7, double gm25 = 0.55, double structuralN = 25)
        {
            PhotosynthesisModelC3 PM = new PhotosynthesisModelC3();
            PM.initialised = false;
            PM.photoPathway = PhotosynthesisModel.PhotoPathway.C3;

            PM.conductanceModel = PhotosynthesisModel.ConductanceModel.SIMPLE;
            PM.electronTransportModel = PhotosynthesisModel.ElectronTransportModel.EMPIRICAL;

            PM.canopy.nLayers = 1;

            PM.envModel.latitudeD = latitude;
            PM.envModel.DOY = DOY;
            PM.envModel.maxT = maxT;
            PM.envModel.minT = minT;
            PM.envModel.radn = radn;  // Check that this changes ratio
            PM.envModel.ATM = 1.013;

            PM.canopy.LAI = lai;
            PM.canopy.leafAngle = LeafAngle;
            PM.canopy.leafWidth = 0.05;
            PM.canopy.u0 = 1;
            PM.canopy.ku = 0.5;

            PM.canopy.CPath.SLNAv = SLN;
            PM.canopy.CPath.SLNRatioTop = SLNRatioTop;
            PM.canopy.CPath.structuralN = structuralN;

            PM.canopy.CPath.psiVc = psiVc * psiFactor;
            PM.canopy.CPath.psiJ = psiJ * psiFactor;
            PM.canopy.CPath.psiRd = psiRd * psiFactor;

            PM.canopy.rcp = 1200;
            PM.canopy.g = 0.066;
            PM.canopy.sigma = 5.668E-08;
            PM.canopy.lambda = 2447000;

            PM.canopy.θ = 0.7;
            PM.canopy.f = 0.15;
            PM.canopy.oxygenPartialPressure = 210000;
            PM.canopy.Ca = Ca;
            PM.canopy.CPath.CiCaRatio = CiCaRatio;

            PM.canopy.gbs_CO2 = 0.003;
            PM.canopy.alpha = 0.1;
            PM.canopy.x = 0.4;

            PM.canopy.diffuseExtCoeff = 0.8;
            PM.canopy.leafScatteringCoeff = 0.2;
            PM.canopy.diffuseReflectionCoeff = 0.057;

            PM.canopy.diffuseExtCoeffNIR = 0.8;
            PM.canopy.leafScatteringCoeffNIR = 0.8;
            PM.canopy.diffuseReflectionCoeffNIR = 0.389;

            PM.canopy.CPath.Kc_P25 = 272.38;
            PM.canopy.CPath.Kc_c = 32.689;
            PM.canopy.CPath.Kc_b = 9741.4;

            PM.canopy.CPath.Ko_P25 = 165820;
            PM.canopy.CPath.Ko_c = 9.574;
            PM.canopy.CPath.Ko_b = 2853.019;

            PM.canopy.CPath.VcMax_VoMax_P25 = 4.58;
            PM.canopy.CPath.VcMax_VoMax_c = 13.241;
            PM.canopy.CPath.VcMax_VoMax_b = 3945.722;

            PM.canopy.CPath.VcMax_c = 26.355;
            PM.canopy.CPath.VcMax_b = 7857.83;
            PM.canopy.CPath.Rd_c = 18.715;
            PM.canopy.CPath.Rd_b = 5579.745;

            PM.canopy.CPath.JMax_TOpt = 28.796;
            PM.canopy.CPath.JMax_Omega = 15.536;
            PM.canopy.CPath.gm_P25 = gm25;
            PM.canopy.CPath.gm_TOpt = 34.309;
            PM.canopy.CPath.gm_Omega = 20.791;

            PM.envModel.initilised = true;
            PM.envModel.run();

            PM.initialised = true;

            List<double> sunlitWaterDemands = new List<double>();
            List<double> shadedWaterDemands = new List<double>();
            List<double> hourlyWaterDemandsmm = new List<double>();
            List<double> hourlyWaterSuppliesmm = new List<double>();
            List<double> sunlitAssimilations = new List<double>();
            List<double> shadedAssimilations = new List<double>();
            List<double> interceptedRadn = new List<double>();

            int startHour = 6;
            int endHour = 18;

            for (int time = startHour; time <= endHour; time++)
            {
                //This run is to get potential water use

                if (time > PM.envModel.sunrise && time < PM.envModel.sunset)
                {
                    PM.run(time, soilWaterAvail);
                    double sunlitWaterDemand = Math.Min(PM.sunlitAC1.Elambda_[0], PM.sunlitAJ.Elambda_[0]);
                    double shadedWaterDamand = Math.Min(PM.shadedAC1.Elambda_[0], PM.shadedAJ.Elambda_[0]);

                    if (double.IsNaN(sunlitWaterDemand))
                    {
                        sunlitWaterDemand = 0;
                    }

                    if (double.IsNaN(shadedWaterDamand))
                    {
                        shadedWaterDamand = 0;
                    }

                    sunlitWaterDemands.Add(sunlitWaterDemand);
                    shadedWaterDemands.Add(shadedWaterDamand);

                    sunlitWaterDemands[sunlitWaterDemands.Count - 1] = Math.Max(sunlitWaterDemands.Last(), 0);
                    shadedWaterDemands[shadedWaterDemands.Count - 1] = Math.Max(shadedWaterDemands.Last(), 0);

                    hourlyWaterDemandsmm.Add((sunlitWaterDemands.Last() + shadedWaterDemands.Last()) / PM.canopy.lambda * 1000 * 0.001 * 3600);
                    hourlyWaterSuppliesmm.Add(hourlyWaterDemandsmm.Last());
                }
                else
                {
                    sunlitWaterDemands.Add(0);
                    shadedWaterDemands.Add(0);
                    hourlyWaterDemandsmm.Add(0);
                    hourlyWaterSuppliesmm.Add(0);
                }
                try
                {
                    sunlitAssimilations.Add(Math.Min(PM.sunlitAC1.A[0], PM.sunlitAJ.A[0]));
                    shadedAssimilations.Add(Math.Min(PM.shadedAC1.A[0], PM.shadedAJ.A[0]));
                }
                catch (Exception)
                {
                    sunlitAssimilations.Add(0);
                    shadedAssimilations.Add(0);
                }
            }

            double maxHourlyT = hourlyWaterSuppliesmm.Max();

            while (hourlyWaterSuppliesmm.Sum() > soilWaterAvail)
            {
                maxHourlyT *= 0.99;
                for (int i = 0; i < hourlyWaterSuppliesmm.Count; i++)
                {
                    if (hourlyWaterSuppliesmm[i] > maxHourlyT)
                    {
                        hourlyWaterSuppliesmm[i] = maxHourlyT;
                    }
                }
            }

            sunlitAssimilations.Clear();
            shadedAssimilations.Clear();


            //Now that we have our hourly supplies we can calculate again
            for (int time = startHour; time <= endHour; time++)
            {
                double TSupply = hourlyWaterSuppliesmm[time - startHour];
                double sunlitWaterDemand = sunlitWaterDemands[time - startHour];
                double shadedWaterDemand = shadedWaterDemands[time - startHour];

                double totalWaterDemand = sunlitWaterDemand + shadedWaterDemand;

                if (time > PM.envModel.sunrise && time < PM.envModel.sunset)
                {
                    PM.run(time, soilWaterAvail, hourlyWaterSuppliesmm[time - startHour], sunlitWaterDemand / totalWaterDemand, shadedWaterDemand / totalWaterDemand);
                    double sunlitAssimilation = Math.Min(PM.sunlitAC1.A[0], PM.sunlitAJ.A[0]);
                    double shadedAssimilation = Math.Min(PM.shadedAC1.A[0], PM.shadedAJ.A[0]);

                    if (double.IsNaN(sunlitAssimilation))
                    {
                        sunlitAssimilation = 0;
                    }

                    if (double.IsNaN(shadedAssimilation))
                    {
                        shadedAssimilation = 0;
                    }
                    sunlitAssimilations.Add(sunlitAssimilation);
                    shadedAssimilations.Add(shadedAssimilation);


                    sunlitAssimilations[sunlitAssimilations.Count - 1] = Math.Max(sunlitAssimilations.Last(), 0);
                    shadedAssimilations[shadedAssimilations.Count - 1] = Math.Max(shadedAssimilations.Last(), 0);

                    double propIntRadn = PM.canopy.propnInterceptedRadns.Sum();
                    interceptedRadn.Add(PM.envModel.totalIncidentRadiation * propIntRadn * 3600);
                    interceptedRadn[interceptedRadn.Count - 1] = Math.Max(interceptedRadn.Last(), 0);
                }
                else
                {
                    sunlitAssimilations.Add(0);
                    shadedAssimilations.Add(0);
                    interceptedRadn.Add(0);
                }
            }
            double[] results = new double[4];

            results[0] = (sunlitAssimilations.Sum() + shadedAssimilations.Sum()) * 3600 / 1000000 * 44 * B * 100 / ((1 + RootShootRatio) * 100);
            //results[0] = (sunlitAssimilations.Sum() + shadedAssimilations.Sum()) * 3600 / 1000000 * 44 * B * 100 / ((1) * 100);
            results[1] = hourlyWaterDemandsmm.Sum();
            results[2] = hourlyWaterSuppliesmm.Sum();
            results[3] = interceptedRadn.Sum();

            return results;
        }
    }
}