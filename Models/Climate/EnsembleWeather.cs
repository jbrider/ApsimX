using APSIM.Numerics;
using APSIM.Shared.Utilities;
using CommandLine;
using Models.Core;
using Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Models.Climate;

///<summary>
/// Reads in weather data and makes it available to other models.
///</summary>
[Serializable]
[ViewName("UserInterface.Views.PropertyView")]
[PresenterName("UserInterface.Presenters.PropertyPresenter")]
[ValidParent(ParentType = typeof(Simulation))]
[ValidParent(ParentType = typeof(Zone))]
public class EnsembleWeather : Model, IWeather
{
    /// <summary>
    /// A link to the clock model.
    /// </summary>
    [Link]
    private IClock clock = null;

    /// <summary>/// List of all of the met data - set from outside</summary>
    public List<DailyMetDataFromFile> MetData { get; set; } = new List<DailyMetDataFromFile>();

    /// <summary>
    /// Gets the start date of the weather
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets the end date of the weather
    /// </summary>
    public DateTime EndDate { get; set; }


    /// <summary>
    /// Gets or sets the maximum temperature (oC)
    /// </summary>
    [Units("°C")]
    [JsonIgnore]
    public double MaxT { get; set; }

    /// <summary>
    /// Gets or sets the minimum temperature (oC)
    /// </summary>
    [JsonIgnore]
    [Units("°C")]
    public double MinT { get; set; }

    /// <summary>
    /// Daily Mean temperature (oC)
    /// </summary>
    [Units("°C")]
    [JsonIgnore]
    public double MeanT { get { return (MaxT + MinT) / 2; } }

    /// <summary>
    /// Daily mean VPD (hPa)
    /// </summary>
    [Units("hPa")]
    [JsonIgnore]
    public double VPD
    {
        get
        {
            const double SVPfrac = 0.66;
            double VPDmint = MetUtilities.svp((float)MinT) - VP;
            VPDmint = Math.Max(VPDmint, 0.0);

            double VPDmaxt = MetUtilities.svp((float)MaxT) - VP;
            VPDmaxt = Math.Max(VPDmaxt, 0.0);

            return SVPfrac * VPDmaxt + (1 - SVPfrac) * VPDmint;
        }
    }
    /// <summary>
    /// Gets or sets the rainfall (mm)
    /// </summary>
    [Units("mm")]
    [JsonIgnore]
    public double Rain { get; set; }

    /// <summary>
    /// Gets or sets the solar radiation. MJ/m2/day
    /// </summary>
    [Units("MJ/m^2/d")]
    [JsonIgnore]
    public double Radn { get; set; }

    /// <summary>
    /// Gets or sets the Pan Evaporation (mm) (Class A pan)
    /// </summary>
    [Units("mm")]
    [JsonIgnore]
    public double PanEvap { get; set; }

    /// <summary>
    /// Gets or sets the vapor pressure (hPa)
    /// </summary>
    [Units("hPa")]
    [JsonIgnore]
    public double VP { get; set; }

    /// <summary>
    /// Gets or sets the wind value found in weather file or zero if not specified. (code says 3.0 not zero)
    /// </summary>
    [JsonIgnore]
    public double Wind { get; set; }

    /// <summary>
    /// Gets or sets the DF value found in weather file or zero if not specified
    /// </summary>
    [Units("0-1")]
    [JsonIgnore]
    public double DiffuseFraction { get; set; }

    /// <summary>
    /// Gets or sets the CO2 level. If not specified in the weather file the default is 350.
    /// </summary>
    [JsonIgnore]
    public double CO2 { get; set; } = 350;

    /// <summary>
    /// Gets or sets the atmospheric air pressure. If not specified in the weather file the default is 1010 hPa.
    /// </summary>
    [Units("hPa")]
    [JsonIgnore]
    public double AirPressure { get; set; } = 1010;

    /// <summary>
    /// Gets the latitude
    /// </summary>
    [JsonIgnore]
    public double Latitude { get; set; }

    /// <summary>
    /// Gets the longitude
    /// </summary>
    [JsonIgnore]
    public double Longitude { get; set; }

    /// <summary>
    /// Gets the average temperature
    /// </summary>
    [Units("°C")]
    [JsonIgnore]
    public double Tav { get; set; }

    /// <summary>
    /// Gets the temperature amplitude.
    /// </summary>
    [JsonIgnore]
    public double Amp { get; set; }

    /// <summary>
    /// Gets or sets the file name. Used to keep track of the shire/station names
    /// </summary>
    [Summary]
    [Description("Weather file name")]
    public string FileName { get; set; }


    /// <summary>Met Data from yesterday</summary>
    public DailyMetDataFromFile TomorrowsMetData => throw new NotImplementedException();

    /// <summary>Met Data from yesterday</summary>
    public DailyMetDataFromFile YesterdaysMetData => throw new NotImplementedException();

    /// <summary>
    /// Gets the duration of the day in hours.
    /// </summary>
    public double CalculateDayLength(double Twilight)
    {
        return MathUtilities.DayLength(this.clock.Today.DayOfYear, Twilight, this.Latitude);
        //return this.DayLength;
    }

    /// <summary> calculate the time of sun rise</summary>
    /// <returns>Sun rise time</returns>
    public double CalculateSunRise()
    {
        return 12 - CalculateDayLength(-6) / 2;
    }

    /// <summary> calculate the time of sun set</summary>
    /// <returns>Sun set time</returns>
    public double CalculateSunSet()
    {
        return 12 + CalculateDayLength(-6) / 2;
    }

    /// <summary> Called during the OnDoWeather event</summary>
    public DailyMetDataFromFile GetMetData(DateTime date)
    {
        int offset = (date - StartDate).Days;
        if(offset >= MetData.Count || offset < 0)
        {
            string start = StartDate.ToString("dd/MM/yyyy");
            string thisdate = date.ToString("dd/MM/yyyy");
            string endDate = EndDate.ToString("dd/MM/yyyy");
            string msg = $"Error: Invalid Index ({offset}). MaxIndex: {MetData.Count}. StartDate: {start}, End Date: {endDate}, This Date: {thisdate}. ";
            Console.WriteLine(msg);
            throw new Exception(msg);
        }
        return MetData[offset];
    }

    /// <summary>
    /// An event handler for the daily DoWeather event.
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The arguments of the event</param>
    [EventSubscribe("DoWeather")]
    private void OnDoWeather(object sender, EventArgs e)
    {
        var TodaysMetData = GetMetData(this.clock.Today); //Read first date to get todays data

        this.Radn = TodaysMetData.Radn;
        this.MaxT = TodaysMetData.MaxT;
        this.MinT = TodaysMetData.MinT;
        this.Rain = TodaysMetData.Rain;
        this.PanEvap = TodaysMetData.PanEvap;
        //this.RainfallHours = TodaysMetData.RainfallHours;
        this.VP = TodaysMetData.VP;
        this.Wind = TodaysMetData.Wind;

        //VapourPressure calculated each day
        this.VP = Math.Max(0, MetUtilities.svp(this.MinT));

        // Estimate Diffuse Fraction using the Approach of Bristow and Campbell
        double Qmax = MetUtilities.QMax(clock.Today.DayOfYear + 1, Latitude, MetUtilities.Taz, MetUtilities.Alpha, 0.0); // Radiation for clear and dry sky (ie low humidity)
        double Q0 = MetUtilities.Q0(clock.Today.DayOfYear + 1, Latitude);
        double B = Qmax / Q0;
        double Tt = MathUtilities.Bound(this.Radn / Q0, 0, 1);
        if (Tt > B) Tt = B;
        this.DiffuseFraction = (1 - Math.Exp(0.6 * (1 - B / Tt) / (B - 0.4)));
        if (Tt > 0.5 && this.DiffuseFraction < 0.1) this.DiffuseFraction = 0.1;


        //this.DayLength = TodaysMetData.DayLength;
        //this.CO2 = TodaysMetData.CO2;
    }
}
