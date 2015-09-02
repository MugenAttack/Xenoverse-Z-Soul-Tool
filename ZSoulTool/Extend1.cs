using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Msgfile;

namespace ZSoulTool
{
    struct Effect
    {
        public int ID;
        public string Description;  
    }

    struct Activation
    {
        public int ID;
        public string Description;   
    }

    struct idbItem
    {
        public int msgIndexName;
        public int msgIndexDesc;
        public byte[] Data;
    }

    class EffectList
    {
        public Effect[] effects;
        public void ConstructList(XmlNodeList effectlist)
        {
            effects = new Effect[effectlist.Count];
            for (int i = 0; i < effectlist.Count; i++)
            {
                effects[i].ID = int.Parse(effectlist[i].Attributes["id"].Value);
                effects[i].Description = effectlist[i].InnerText;
            }
        }

        public void ConstructFromUnknown(ref idbItem[] items)
        {
            List<int> IDs = new List<int>();
            for (int i = 0; i < items.Length; i++)
            {
                if (!IDs.Contains(BitConverter.ToInt32(items[i].Data, 160)))
                    IDs.Add(BitConverter.ToInt32(items[i].Data, 160));

                if (!IDs.Contains(BitConverter.ToInt32(items[i].Data, 384)))
                    IDs.Add(BitConverter.ToInt32(items[i].Data, 384));
            }

            IDs.Sort();

            effects = new Effect[IDs.Count];
            for (int i = 0; i < IDs.Count; i++)
            {
                effects[i].ID = IDs[i];
                effects[i].Description = "Undetermined";
            }
        }

        public int FindIndex(int ID)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].ID == ID)
                    return i;
            }
            return 0;
        }
    }

    class ActivationList
    {
        public Activation[] activations;
        public void ConstructList(XmlNodeList activationlist)
        {
            activations = new Activation[activationlist.Count];
            for (int i = 0; i < activationlist.Count; i++)
            {
                activations[i].ID = int.Parse(activationlist[i].Attributes["id"].Value);
                activations[i].Description = activationlist[i].InnerText;
            }
        }

        public void ConstructFromUnknown(ref idbItem[] items)
        {
            List<int> IDs = new List<int>();
            for (int i = 0; i < items.Length; i++)
            {
                if (!IDs.Contains(BitConverter.ToInt32(items[i].Data, 164)))
                    IDs.Add(BitConverter.ToInt32(items[i].Data, 164));

                if (!IDs.Contains(BitConverter.ToInt32(items[i].Data, 388)))
                    IDs.Add(BitConverter.ToInt32(items[i].Data, 388));
            }

            IDs.Sort();

            activations = new Activation[IDs.Count];
            for (int i = 0; i < IDs.Count; i++)
            {
                activations[i].ID = IDs[i];
                activations[i].Description = "Undetermined";
            }
        }

        public int FindIndex(int ID)
        {
            for (int i = 0; i < activations.Length; i++)
            {
                if (activations[i].ID == ID)
                    return i;
            }
            return 0;
        }

    }
}
