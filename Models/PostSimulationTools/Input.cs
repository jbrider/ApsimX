﻿namespace Models.PostSimulationTools
{
    using APSIM.Shared.Utilities;
    using Models.Core;
    using Models.Core.Run;
    using Models.Storage;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Threading;
    using System.Xml.Serialization;

    /// <summary>
    /// # [Name]
    /// Reads the contents of a file (in apsim format) and stores into the DataStore. 
    /// If the file has a column name of 'SimulationName' then this model will only input data for those rows
    /// where the data in column 'SimulationName' matches the name of the simulation under which
    /// this input model sits. 
    /// 
    /// If the file does NOT have a 'SimulationName' column then all data will be input.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.InputView")]
    [PresenterName("UserInterface.Presenters.InputPresenter")]
    [ValidParent(ParentType=typeof(DataStore))]
    public class Input : Model, IRunnable, IReferenceExternalFiles
    {
        /// <summary>
        /// The DataStore.
        /// </summary>
        [Link]
        private IDataStore storage = null;

        /// <summary>
        /// Gets or sets the file name to read from.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the full file name (with path). The user interface uses this. 
        /// </summary>
        [XmlIgnore]
        [Description("EXCEL file name")]
        public string FullFileName
        {
            get
            {
                Simulations simulations = Apsim.Parent(this, typeof(Simulations)) as Simulations;
                if (simulations != null && simulations.FileName != null && this.FileName != null)
                    return PathUtilities.GetAbsolutePath(this.FileName, simulations.FileName);
                return null;
            }

            set
            {
                Simulations simulations = Apsim.Parent(this, typeof(Simulations)) as Simulations;
                this.FileName = PathUtilities.GetRelativePath(value, simulations.FileName);
            }
        }

        /// <summary>Return our input filenames</summary>
        public IEnumerable<string> GetReferencedFileNames()
        {
            return new string[] { FileName };
        }

        /// <summary>
        /// Gets the parent simulation or null if not found
        /// </summary>
        private IStorageWriter StorageWriter
        {
            get
            {
                // The JobRunnerAsync will not resolve links, so we need to
                // go looking for the data store ourselves. This is an ugly
                // hack, no doubt about it, but this infrastructure is about to
                // be changed/refactored anyway, so hopefully this won't stay
                // here for too long.
                if (storage == null)
                    storage = Apsim.Find(this, typeof(IDataStore)) as IDataStore;
                if (storage == null)
                    throw new Exception("Cannot find a datastore");
                return storage.Writer;
            }
        }

        /// <summary>
        /// Main run method for performing our calculations and storing data.
        /// </summary>
        /// <param name="cancelToken">The cancel token.</param>
        public void Run(CancellationTokenSource cancelToken)
        {
            string fullFileName = FullFileName;
            if (fullFileName != null)
            {
                Simulations simulations = Apsim.Parent(this, typeof(Simulations)) as Simulations;

                DataTable data = GetTable();
                if (data != null)
                {
                    data.TableName = this.Name;
                    StorageWriter.WriteTable(data);
                }
            }
        }

        /// <summary>
        /// Provides an error message to display if something is wrong.
        /// </summary>
        public string ErrorMessage = string.Empty;

        /// <summary>
        /// Return a datatable for this input file. Returns null if no data.
        /// </summary>
        /// <returns></returns>
        public DataTable GetTable()
        {
            DataTable returnDataTable = null;
            string fullFileName = FullFileName;
            if (fullFileName != null)
            {
                if (File.Exists(fullFileName))
                {
                    ApsimTextFile textFile = new ApsimTextFile();
                    try
                    {
                        textFile.Open(fullFileName);
                    }
                    catch (Exception err)
                    {
                        ErrorMessage = err.Message;
                        return null;
                    }
                    DataTable table = textFile.ToTable();
                    textFile.Close();

                    if (returnDataTable == null)
                        returnDataTable = table;
                    else
                        returnDataTable.Merge(table);
                }
                else
                {
                    ErrorMessage = "The specified file does not exist.";
                }
            }
            else
            {
                ErrorMessage = "Please select a file to use.";
            }

            return returnDataTable;
        }       
    }
}
