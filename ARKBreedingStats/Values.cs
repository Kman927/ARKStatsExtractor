﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace ARKBreedingStats
{
    [DataContract]
    public class Values
    {
        private static Values _V;
        [DataMember]
        public int version = 0;
        [DataMember]
        public List<Species> species = new List<Species>();
        public List<string> speciesNames = new List<string>();
        [DataMember]
        public double[][] statMultipliers = new double[8][]; // official server stats-multipliers
        [DataMember]
        public double imprintingMultiplier = 1;
        [DataMember]
        public double tamingMultiplier = 1;
        [DataMember]
        public Dictionary<string, TamingFood> foodData = new Dictionary<string, TamingFood>();

        public static Values V
        {
            get
            {
                if (_V == null)
                    _V = new Values();
                return _V;
            }
        }

        public bool loadValues()
        {
            bool loadedSuccessful = true;

            string filename = "values.json";

            // check if file exists
            if (!File.Exists(filename))
            {
                MessageBox.Show("Values-File '" + filename + "' not found. This tool will not work properly without that file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            _V.version = 0;

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Values));
            System.IO.FileStream file = System.IO.File.OpenRead(filename);

            try
            {
                _V = (Values)ser.ReadObject(file);
                _V.speciesNames = new List<string>();
                foreach (Species sp in _V.species)
                {
                    sp.initialize();
                    _V.speciesNames.Add(sp.name);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("File Couldn't be opened.\nErrormessage:\n\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                loadedSuccessful = false;
            }
            file.Close();

            //saveJSON();
            return loadedSuccessful;
        }

        public void saveJSON()
        {
            // to create minified json of current values
            DataContractJsonSerializer writer = new DataContractJsonSerializer(typeof(Values));
            try
            {
                System.IO.FileStream file = System.IO.File.Create("values.json");
                writer.WriteObject(file, _V);
                file.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error during serialization.\nErrormessage:\n\n" + e.Message, "Serialization-Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void applyMultipliersToStats(double[][] multipliers)
        {
            for (int sp = 0; sp < species.Count; sp++)
            {
                for (int s = 0; s < 8; s++)
                {
                    species[sp].stats[s].BaseValue = species[sp].statsRaw[s].BaseValue;
                    // don't apply the multiplier if AddWhenTamed is negative (currently the only case is the Giganotosaurus, which does not get the subtraction multiplied)
                    species[sp].stats[s].AddWhenTamed = species[sp].statsRaw[s].AddWhenTamed * (species[sp].statsRaw[s].AddWhenTamed > 0 ? multipliers[s][0] : 1);
                    species[sp].stats[s].MultAffinity = species[sp].statsRaw[s].MultAffinity * multipliers[s][1];
                    species[sp].stats[s].IncPerTamedLevel = species[sp].statsRaw[s].IncPerTamedLevel * multipliers[s][2];
                    species[sp].stats[s].IncPerWildLevel = species[sp].statsRaw[s].IncPerWildLevel * multipliers[s][3];
                }
            }
        }

        public void applyMultipliersToBreedingTimes(double[] multipliers)
        {
            for (int sp = 0; sp < species.Count; sp++)
            {
                for (int k = 0; k < 3; k++)
                    species[sp].breedingTimes[k] = (int)Math.Ceiling(species[sp].breedingTimesRaw[k] / multipliers[k == 2 ? 1 : 0]);
            }
        }

    }
}
