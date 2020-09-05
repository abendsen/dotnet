// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Mvvm
{
    [TestClass]
    public class Test_ObservableValidator
    {
        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_ObservableValidator_HasErrors()
        {
            var model = new Person();

            Assert.IsFalse(model.HasErrors);

            model.Name = "No";

            Assert.IsTrue(model.HasErrors);

            model.Name = "Valid";

            Assert.IsFalse(model.HasErrors);
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_ObservableValidator_ErrorsChanged()
        {
            var model = new Person();

            List<(object Sender, DataErrorsChangedEventArgs Args)> errors = new List<(object, DataErrorsChangedEventArgs)>();

            model.ErrorsChanged += (s, e) => errors.Add((s, e));

            model.Name = "Foo";

            Assert.AreEqual(errors.Count, 1);
            Assert.AreSame(errors[0].Sender, model);
            Assert.AreEqual(errors[0].Args.PropertyName, nameof(Person.Name));

            errors.Clear();

            model.Name = "Bar";

            Assert.AreEqual(errors.Count, 1);
            Assert.AreSame(errors[0].Sender, model);
            Assert.AreEqual(errors[0].Args.PropertyName, nameof(Person.Name));

            errors.Clear();

            model.Name = "Valid";

            Assert.AreEqual(errors.Count, 1);
            Assert.AreSame(errors[0].Sender, model);
            Assert.AreEqual(errors[0].Args.PropertyName, nameof(Person.Name));

            errors.Clear();

            model.Name = "This is fine";

            Assert.AreEqual(errors.Count, 0);
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_ObservableValidator_GetErrors()
        {
            var model = new Person();

            Assert.AreEqual(model.GetErrors(null).Cast<object>().Count(), 0);
            Assert.AreEqual(model.GetErrors(string.Empty).Cast<object>().Count(), 0);
            Assert.AreEqual(model.GetErrors("ThereIsntAPropertyWithThisName").Cast<object>().Count(), 0);
            Assert.AreEqual(model.GetErrors(nameof(Person.Name)).Cast<object>().Count(), 0);

            model.Name = "Foo";

            var errors = model.GetErrors(nameof(Person.Name)).Cast<ValidationResult>().ToArray();

            Assert.AreEqual(errors.Length, 1);
            Assert.AreEqual(errors[0].MemberNames.First(), nameof(Person.Name));

            Assert.AreEqual(model.GetErrors("ThereIsntAPropertyWithThisName").Cast<object>().Count(), 0);

            errors = model.GetErrors(null).Cast<ValidationResult>().ToArray();

            Assert.AreEqual(errors.Length, 1);
            Assert.AreEqual(errors[0].MemberNames.First(), nameof(Person.Name));

            errors = model.GetErrors(string.Empty).Cast<ValidationResult>().ToArray();

            Assert.AreEqual(errors.Length, 1);
            Assert.AreEqual(errors[0].MemberNames.First(), nameof(Person.Name));

            model.Age = -1;

            errors = model.GetErrors(null).Cast<ValidationResult>().ToArray();

            Assert.AreEqual(errors.Length, 2);
            Assert.IsTrue(errors.Any(e => e.MemberNames.First().Equals(nameof(Person.Name))));
            Assert.IsTrue(errors.Any(e => e.MemberNames.First().Equals(nameof(Person.Age))));

            model.Age = 26;

            errors = model.GetErrors(null).Cast<ValidationResult>().ToArray();

            Assert.AreEqual(errors.Length, 1);
            Assert.IsTrue(errors.Any(e => e.MemberNames.First().Equals(nameof(Person.Name))));
            Assert.IsFalse(errors.Any(e => e.MemberNames.First().Equals(nameof(Person.Age))));
        }

        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_ObservableValidator_ValidateReturn()
        {
            var model = new Person();

            Assert.IsFalse(model.ValidateName(null));
            Assert.IsFalse(model.ValidateName(string.Empty));
            Assert.IsFalse(model.ValidateName("No"));
            Assert.IsFalse(model.ValidateName("This text is really, really too long for the target property"));
            Assert.IsTrue(model.ValidateName("1234"));
            Assert.IsTrue(model.ValidateName("01234567890123456789"));
            Assert.IsTrue(model.ValidateName("Hello world"));
        }

        public class Person : ObservableValidator
        {
            private string name;

            [MinLength(4)]
            [MaxLength(20)]
            [Required]
            public string Name
            {
                get => this.name;
                set
                {
                    ValidateProperty(value);

                    SetProperty(ref this.name, value);
                }
            }

            public bool ValidateName(string value)
            {
                return ValidateProperty(value, nameof(Name));
            }

            private int age;

            [Range(0, 100)]
            public int Age
            {
                get => this.age;
                set
                {
                    ValidateProperty(value);

                    SetProperty(ref this.age, value);
                }
            }
        }
    }
}
