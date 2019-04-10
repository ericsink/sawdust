
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using EricSinkMultiCoreLib;

namespace sd
{
    public enum Action
    {
        INTRO,
        DRILL,
        TENON,
        MORTISE,
        DADO,
        CHAMFER,
        ROUNDOVER,
        RABBET,
        CROSSCUT,
        NEW_BOARD,
        JOIN,
        JOIN_MT,
        DOVETAIL_TAILS,
        DOVETAIL_PINS,
        DOVETAIL_JOIN,
        RIP,
    }

    internal class piecerec
    {
        public Step st;
        public List<string> subs;
    }

    public class VariableDefinition : INotifyPropertyChanged
    {
        public string name;

        public bool user;

        // Valid only for user variables
        public double min;
        public double max;
        public double val;
        public int prec;
        public string help;

        // Valid only for non-user variables, expression variables
        internal string expr;

        public VariableDefinition(string _name, string _help, double _min, double _max, double _val, int _prec)
        {
            user = true;

            Debug.Assert(_max > _min);
            Debug.Assert(_prec > 0);
            Debug.Assert(_val >= _min);
            Debug.Assert(_val <= _max);

            name = _name;
            help = _help;
            min = _min;
            max = _max;
            val = _val;

            prec = _prec;
        }

        public VariableDefinition(string _name, string _expr)
        {
            user = false;

            name = _name;
            expr = _expr;
        }

        public double RoundToPrecision(double d)
        {
            double d2 = d * prec + 0.5;
            int units = (int)d2;
            return units / ((double)prec);
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public double GetValue(IGetVariable gv)
        {
            if (user)
            {
                return val;
            }
            else
            {
                return Expr.Evaluate(expr, gv);
            }
        }

        public double Value
        {
            get
            {
                //Debug.Assert(user);
                return val;
            }
            set
            {
                Debug.Assert(user);
                val = RoundToPrecision(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public bool InRange(double v)
        {
            Debug.Assert(user);
            if (v < min)
            {
                return false;
            }
            if (v > max)
            {
                return false;
            }
            return true;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    internal class FieldDefinition
    {
        public Step step;
        public string name;
        public string type;
        public string help;
        private string val;
        public string def;


        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Value
        {
            get
            {
                return val;
            }

            set
            {
                val = value;
            }
        }

        public string ValueOrDefault
        {
            get
            {
                if (val != null)
                {
                    return val;
                }
                else
                {
                    return def;
                }
            }
        }

        public string EvaluatedValue
        {
            get
            {
                string v = ValueOrDefault;
                if (IsExpressionType(type))
                {
                    return Expr.Evaluate(v, step.plan).ToString();
                }
                else
                {
                    return v;
                }
            }
        }

        public FieldDefinition(Step st, string _name, string _type, string _def, string _help)
        {
            step = st;
            name = _name;
            type = _type;
            help = _help;
            def = _def;
            val = _def;
        }

        public string BaseType
        {
            get
            {
                return type.Split(':')[0];
            }
        }

        public static bool IsExpressionType(string t)
        {
            switch (t)
            {
                case "Angle":
                case "Coord":
                case "Dimension":
                case "Integer":
                    {
                        return true;
                    }
            }
            return false;
        }

        public bool Validate(string newval)
        {
            // if it's an expression type, evaluate it.  if the expr eval fails, return false.
            // if it succeeds, check the resulting value against constraints for that type.

            if (newval == null)
            {
                newval = def;
            }

            if (IsExpressionType(type))
            {
                double v;

                try
                {
                    v = Expr.Evaluate(newval, step.plan);
                }
                catch
                {
                    return false;
                }

                // TODO validate for range

                return true;
            }
            else
            {
                // TODO can we do anything here?
                return true;
            }
        }
    }

    public enum MechanicalStrength
    {
        None,
        Low,
        Medium,
        High,
        VeryHigh
    }

    public class GlueJointScore
    {
        // TODO strength, based on glue surface area and the graingrain
        // TODO stability, based on crossgrain checks
        // TODO overall score or estimate of the quality of the joint

        public GlueJointScore(Step st)
        {
            step = st;
        }

        public double TotalSurfaceArea()
        {
            double d = 0;
            foreach (GlueJoint gj in faces)
            {
                d += gj.Area;
            }
            return d;
        }

        public double SurfaceArea(GrainGrain gg)
        {
            double d = 0;
            foreach (GlueJoint gj in faces)
            {
                if (gj.Grains == gg)
                {
                    d += gj.Area;
                }
            }
            return d;
        }

        public string Score()
        {
            return "X"; // TODO
        }

        public Step step;
        public List<GlueJoint> faces = new List<GlueJoint>();

        class dircounts
        {
            public int neg;
            public int opp;
            public int perp;
            public int other;

            public dircounts(List<xyz> directions)
            {
                for (int i = 0; i < directions.Count; i++)
                {
                    for (int j = i + 1; j < directions.Count; j++)
                    {
                        double d = xyz.dot(directions[i], directions[j]);
                        if (fp.eq_dot_unit(d, -1))
                        {
                            opp++;
                        }
                        else if (fp.eq_dot_unit(d, 0))
                        {
                            perp++;
                        }
                        else if (fp.lt_dot_unit(d, 0))
                        {
                            neg++;
                        }
                        else
                        {
                            other++;
                        }
                    }
                }
            }
        }

        public MechanicalStrength GetMechanicalStrength()
        {
#if not
            if (step.action == Action.DOVETAIL_JOIN)
            {
                return MechanicalStrength.VeryHigh;
            }
            else if (step.action == Action.JOIN_MT)
            {
                return MechanicalStrength.High;
            }
#endif
            if (faces.Count == 1)
            {
                return MechanicalStrength.None;
            }

            List<xyz> directions = new List<xyz>();
            foreach (GlueJoint gj in faces)
            {
                xyz n = gj.f1.UnitNormal();
                directions.Add(n);
            }
            List<xyz> unique = new List<xyz>();
            foreach (GlueJoint gj in faces)
            {
                xyz n = gj.f1.UnitNormal();
                bool bFound = false;
                foreach (xyz d in unique)
                {
                    if (fp.eq_unitvec(d, n))
                    {
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    unique.Add(n);
                }
            }

            if (unique.Count == 1)
            {
                return MechanicalStrength.None;
            }

            dircounts dc_all = new dircounts(directions);
            dircounts dc_unique = new dircounts(unique);

            if (dc_all.neg > 10)
            {
                return MechanicalStrength.VeryHigh;
            }

            if (dc_unique.opp >= 2)
            {
                return MechanicalStrength.High;
            }

            if (dc_unique.opp >= 1)
            {
                return MechanicalStrength.Medium;
            }

            if (dc_unique.neg > 0)
            {
                return MechanicalStrength.Low;
            }

            return MechanicalStrength.None;
        }
    }

    public class Annotation_FaceToFace
    {
        public string path1;
        public string path2;
        public double offset;
        public double size;

        public Annotation_FaceToFace(string p1, string p2, double off, double sz)
        {
            path1 = p1;
            path2 = p2;
            offset = off;
            size = sz;
        }
    }

    public class Annotation_PointToPoint
    {
        public xyz pt1;
        public xyz pt2;
        public xyz dir;
        public double dist;
        public double size;

        public Annotation_PointToPoint(xyz _pt1, xyz _pt2, xyz _dir, double _dist)
        {
            pt1 = _pt1;
            pt2 = _pt2;
            dir = _dir;
            dist = _dist;
            size = 0.5;
        }

        public Annotation_PointToPoint(xyz _pt1, xyz _pt2, xyz _dir, double _dist, double _size)
        {
            pt1 = _pt1;
            pt2 = _pt2;
            dir = _dir;
            dist = _dist;
            size = _size;
        }
    }

    public class Plan : IGetVariable
    {
        public List<Step> Steps = new List<Step>();
        public Dictionary<string, VariableDefinition> Variables = new Dictionary<string, VariableDefinition>();
        internal Dictionary<string, Dovetail> Dovetails = new Dictionary<string, Dovetail>();

        public string name;
        public string guid;
        public string document; // the format is a FlowDocument

        internal void BeforeExecute()
        {
            Dovetails.Clear();
            foreach (Step st in Steps)
            {
                st.Result = null;
                st.CalcDependencies();
            }
            foreach (Step st in Steps)
            {
                st.FixDependingOnMeList();
            }
        }

        internal bool SortSteps(List<Step> ready, List<Step> error, List<Step> later, List<Step> done)
        {
            foreach (Step st in Steps)
            {
                if (st.Errors.Count > 0)
                {
                    error.Add(st);
                }
                else if (st.Done)
                {
                    done.Add(st);
                }
                else if (st.ReadyToExecute)
                {
                    ready.Add(st);
                }
                else
                {
                    later.Add(st);
                }
            }
            return (ready.Count == 0);
        }

        /// <summary>
        /// Execute this step and all steps which depend on it (and only it).
        /// </summary>
        /// <param name="st"></param>
        private void ExecuteChain(Step st)
        {
            st.Execute();
            foreach (Step next in st.dependingOnMe)
            {
                if (
                    (next.ReadyToExecute)
                    && (next.dependsOn.Count == 1)
                    )
                {
                    Debug.Assert(next.dependsOn.ContainsValue(st));
                    ExecuteChain(next);
                }
            }
        }

        public CompoundSolid Execute()
        {
            return Execute(false);
        }

        public CompoundSolid Execute(bool bUseThreads)
        {
            this.BeforeExecute();

            int passes = 0;
            while (true)
            {
                List<Step> ready = new List<Step>();
                List<Step> error = new List<Step>();
                List<Step> later = new List<Step>();
                List<Step> done = new List<Step>();
                if (SortSteps(ready, error, later, done))
                {
                    break;
                }
                passes++;

                if (bUseThreads)
                {
                    multicore.Map_Void(ready,
                        delegate (Step dost)
                        {
                            ExecuteChain(dost);
                        }
                    );
                }
                else
                {

                    foreach (Step st in ready)
                    {
                        st.Execute();
                    }
                }
            }

            foreach (Step st in Steps)
            {
                if (
                    (st.Result == null)
                    && (st.Errors.Count == 0)
                    )

                {
                    st.Errors.Add("Unable to execute this step because a previous step failed");
                }
            }

#if true
            foreach (Step st in Steps)
            {
                st.GuessPreferredView();
            }
#endif

            return LastStep.Result;
        }

        public int ErrorCount
        {
            get
            {
                int c = 0;
                foreach (Step st in Steps)
                {
                    c += st.Errors.Count;
                }
                return c;
            }
        }

        internal void Write(XmlWriter xw)
        {
            xw.WriteStartDocument();
            xw.WriteStartElement("sawdust");
            // TODO version of sawdust
            // TODO version of the sawdust file format?
            // TODO author
            // TODO copyright
            // TODO how will we write photographs into an XML file?  or will we just not have that feature?
            xw.WriteElementString("guid", this.guid);
            xw.WriteElementString("name", this.name);
            if (this.document != null)
            {
                xw.WriteElementString("intro", this.document); // we apparently don't need base64 here?
            }
            else
            {
                xw.WriteElementString("intro", "");
            }
            xw.WriteStartElement("variables");
            foreach (VariableDefinition vd in this.Variables.Values)
            {
                xw.WriteStartElement("variable");
                xw.WriteAttributeString("name", vd.name);
                xw.WriteAttributeString("user", vd.user.ToString());
                if (vd.user)
                {
                    xw.WriteAttributeString("help", vd.help);
                    xw.WriteAttributeString("min", vd.min.ToString());
                    xw.WriteAttributeString("max", vd.max.ToString());
                    xw.WriteAttributeString("prec", vd.prec.ToString());
                    xw.WriteAttributeString("value", vd.Value.ToString());
                }
                else
                {
                    xw.WriteAttributeString("value", vd.expr);
                }
                xw.WriteFullEndElement();
            }
            xw.WriteFullEndElement();
            xw.WriteStartElement("steps");
            foreach (Step st in Steps)
            {
                st.Write(xw);
            }
            xw.WriteFullEndElement();
            xw.WriteFullEndElement();
            xw.WriteEndDocument();
        }

#if DEBUG
        internal void WriteXML(StringBuilder sb)
        {
            TextWriter tw = new StringWriter(sb);
            XmlTextWriter xw = new XmlTextWriter(tw);
            xw.Formatting = Formatting.Indented;
            this.Write(xw);
            xw.Close();
        }

        internal void WriteXML(string filename)
        {
            TextWriter tw = new StreamWriter(filename);
            XmlTextWriter xw = new XmlTextWriter(tw);
            xw.Formatting = Formatting.Indented;
            this.Write(xw);
            xw.Close();
        }
#endif

        public Plan(string sname, string sguid)
        {
            name = sname;
            guid = sguid;
        }

        public Plan(string sname)
        {
            name = sname;
            guid = Guid.NewGuid().ToString();
        }

        public CompoundSolid Result
        {
            get
            {
                return LastStep.Result;
            }
        }

        public Step LastStep
        {
            get
            {
                return this[Steps.Count - 1];
            }
        }

        public Step this[int i]
        {
            get
            {
                return Steps[i];
            }
        }

        public Step AddStep(Action action, string name, Dictionary<string, string> parms)
        {
#if DEBUG
            if (action == Action.INTRO)
            {
                Debug.Assert(Steps.Count == 0);
            }
            else
            {
                Debug.Assert(Steps.Count != 0);
            }
#endif

            if (name == null)
            {
                name = Step.GetActionString(action); // TODO
            }

            Step st = new Step(name, action);
            st.Set(parms);
            Add(st);

#if DEBUG
            st.AssertAllFieldsHaveValues();
#endif

            return st;
        }

        private void Add(Step st)
        {
            st.plan = this;
            Steps.Add(st);
        }

        public void DefineVariable(VariableDefinition vd)
        {
            Variables[vd.name] = vd;
        }

        public VariableDefinition FindVariable(string name)
        {
            return Variables[name];
        }

        public void SetVariable(string name, double val)
        {
            VariableDefinition vd = FindVariable(name);
            SetVariable(vd, val);
        }

        public void SetVariable(VariableDefinition vd, double val)
        {
            if (!vd.InRange(val))
            {
                throw new Exception("Variable value out of range");
            }

            vd.Value = val;
        }

        public double GetVariable(string name)
        {
            VariableDefinition vd = Variables[name];
            return vd.GetValue(this);
        }

        internal void ReadVariables(XmlNode top)
        {
            foreach (XmlNode nodv in top.ChildNodes)
            {
                string user = nodv.Attributes["user"].Value;
                bool bUser = ut.ParseBool(user, false);

                string vname = nodv.Attributes["name"].Value;
                string vval = nodv.Attributes["value"].Value;
                if (bUser)
                {
                    string vhelp = nodv.Attributes["help"].Value;
                    string vmin = nodv.Attributes["min"].Value;
                    string vmax = nodv.Attributes["max"].Value;
                    string prec = nodv.Attributes["prec"].Value;
                    this.DefineVariable(new VariableDefinition(vname, vhelp, double.Parse(vmin), double.Parse(vmax), double.Parse(vval), int.Parse(prec)));
                }
                else
                {
                    this.DefineVariable(new VariableDefinition(vname, vval));
                }
            }
        }

        internal void ReadSteps(XmlNode top)
        {
            foreach (XmlNode nodstep in top.ChildNodes)
            {
                Step st = new Step(nodstep);

                st.ReadPreferredView(nodstep["view"]);

                st.ReadAnnotations(nodstep["annotations"]);

                Add(st);
                st.Set(nodstep);
#if DEBUG
                st.AssertAllFieldsHaveValues();
#endif
            }
        }

#if DEBUG
        internal static Plan FromXML(XmlDocument xd)
        {
            XmlNode node_sawdust = xd.GetElementsByTagName("sawdust")[0];
            string plan_name = node_sawdust["name"].InnerText;
            string plan_guid = node_sawdust["guid"].InnerText;
            string plan_intro = node_sawdust["intro"].InnerText;

            Plan p = new Plan(plan_name, plan_guid);

            p.document = plan_intro; // we apparently don't need base64 here?

            p.ReadVariables(node_sawdust["variables"]);
            p.ReadSteps(node_sawdust["steps"]);

            return p;
        }
#endif

        public List<Step> FindAllNewBoards()
        {
            List<Step> result = new List<Step>();

            foreach (Step st in this.Steps)
            {
                if (st.action == Action.NEW_BOARD)
                {
                    result.Add(st);
                }
            }

            return result;
        }

        public double TotalBoardFeet()
        {
            List<Step> boards = FindAllNewBoards();
            double d = 0;
            foreach (Step st in boards)
            {
                Inches width = st.Get_Eval("width");
                Inches length = st.Get_Eval("length");
                Inches thickness = st.Get_Eval("thickness");
                double bf = width * length * thickness / 144.0;
                d += bf;
            }
            return d;
        }
    }
}
