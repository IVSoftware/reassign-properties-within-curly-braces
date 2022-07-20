// MIT License
// Copyright (c) 2022 IVSoftware, LLC and Thomas C. Gregor

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;



namespace winforms_curly_brace_initialzer
{
    public partial class MainForm : Form, ICloneable
    {
        public MainForm() => InitializeComponent();

        static int _tstCount = 0;
        private void buttonTest_Click(object sender, EventArgs e)
        {
            // TEST SYNTAX
            var myForm = this;
            var myFormB4 = myForm;
            Debug.Assert(ReferenceEquals(myForm, myFormB4));
            myForm = myForm.With(new
            {
                // Left = 123,  // This WILL change the position. We don't actually want that.
                Text = "foo",
                AllowDrop = false
            });
            Debug.Assert(ReferenceEquals(myForm, myFormB4));
            // Debug.Assert(myForm.Left == 123);
            Debug.Assert(myForm.Text == "foo");
            Debug.Assert(!myForm.AllowDrop);

            // ALTERNATE SYNTAX
            _ = myForm.With(new
            {
                Text = "oof"
            });
            Debug.Assert(myForm.Text == "oof");


            // Generate some test values
            var point = new Point(Location.X + 25, Location.Y);
            Color color;
            switch (_tstCount % 3)
            {
                case 0:
                    color = Color.LightBlue;
                    break;
                case 1:
                    color = Color.LightGreen;
                    break;
                default:
                    color = SystemColors.Control;
                    break;
            }
            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            // TEST
            var foo = this;

            // Make sure the properties don't change as a result of the creation itself
            var locB4 = Location;
            var n = new { Location = point };
            Debug.Assert(Location.Equals(locB4));
            Debug.Assert(!Location.Equals(point));

            doTask(foo.With(new { Location = point, Text = $"Fee {_tstCount++}", BackColor = color }));
        }

        private void doTask(MainForm form, bool isClone = false)
        {
            Debug.WriteLine(form.Location);
            Debug.WriteLine(form.Text);
            Debug.WriteLine(form.BackColor);
            Debug.WriteLine(String.Empty);

            if (isClone)
            {
                if (!form.Visible) form.Show(owner: this);
                Debug.Assert(
                    !ReferenceEquals(this, form),
                    "Expecting CLONED instance with NEW values.");
            }
            else
            {
                Debug.Assert(
                    ReferenceEquals(this, form),
                    "Expecting SAME instance with NEW values.");
            }
        }

        public MainForm Clone()
        {
            var clone = new MainForm();
            _clones.Add(clone);
            clone.Refresh();
            return clone;
        }
        private List<MainForm> _clones = new List<MainForm>();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                foreach (var clone in _clones)
                {
                    if (!clone.IsDisposed)
                    {
                        clone.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }
        object ICloneable.Clone() => Clone();

        private void buttonTestClone_Click(object sender, EventArgs e)
        {
            // Generate some test values
            var point = new Point(Location.X + 25, Location.Y);
            Color color;
            switch (_tstCount % 3)
            {
                case 0:
                    color = Color.LightBlue;
                    break;
                case 1:
                    color = Color.LightGreen;
                    break;
                default:
                    color = SystemColors.Control;
                    break;
            }
            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            // TEST
            var foo = this;
            doTask(foo.Clone().With(new { Location = point, Text = $"Fee {_tstCount++}", BackColor = color }), isClone: true);
        }
    }

    static class Extensions
    {
        public static T With<T>(this T @this, object anon)
        {
            var type = typeof(T);
            foreach (var propSrce in anon.GetType().GetProperties())
            {
                var propDest = type.GetProperty(propSrce.Name);
                if (propDest == null)
                {
                    Debug.Assert(
                        false,
                        $"Property '{propSrce.Name}' not found in {type.Name}");
                }
                else
                {
                    propDest.SetValue(@this, propSrce.GetValue(anon));
                }
            }
            return @this;
        }
    }
}
