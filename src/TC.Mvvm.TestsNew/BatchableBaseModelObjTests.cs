using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TC.Mvvm.Tests
{

    [TestClass]
    public class BatchableBaseModelObjTests
    {

        private class HistoricalPerson : BatchableBaseModelObj
        {

            private string firstName;
            private string lastName;
            private DateTime birthDate;
            private DateTime deathDate;

            public HistoricalPerson()
            {
            }

            public string FirstName
            {
                get { return firstName; }
                set { SetValue(ref firstName, value); }
            }

            public string LastName
            {
                get { return lastName; }
                set { SetValue(ref lastName, value); }
            }

            public DateTime BirthDate
            {
                get { return birthDate; }
                set
                {
                    if(SetValue(ref birthDate, value))
                    {
                        OnPropertyValueChanged(nameof(DiedAtAge));
                        OnPropertyChanged(nameof(DiedAtAge));
                    }
                }
            }

            public DateTime DeathDate
            {
                get { return deathDate; }
                set
                {
                    if(SetValue(ref deathDate, value))
                    {
                        OnPropertyValueChanged(nameof(DiedAtAge));
                        OnPropertyChanged(nameof(DiedAtAge));
                    }
                }
            }

            public double? DiedAtAge
            {
                get { return (deathDate - birthDate).TotalDays / 365.0; }
            }

        }

        [TestMethod]
        public void UpdateBatch_Works()
        {
            var person = new HistoricalPerson
            {
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1920, 3, 1),
                DeathDate = new DateTime(1978, 10, 5),
            };

            int numberOfPropertyChangedEvents = 0;
            int numberOfPropertyValueChangedEvents = 0;

            person.PropertyChanged += (s, e) =>
            {
                numberOfPropertyChangedEvents++;
            };
            person.PropertyValueChanged += (s, e) => numberOfPropertyValueChangedEvents++;

            using(var updateBatch = new UpdateBatch())
            {
                updateBatch.Register(person);

                person.FirstName = "Jane";
                person.FirstName = "Jack";
                person.BirthDate = new DateTime(1919, 3, 2);
                person.DeathDate = new DateTime(1991, 5, 6);
                person.DeathDate = new DateTime(1986, 7, 12);
            }

            Assert.AreEqual(4, numberOfPropertyChangedEvents);
            Assert.AreEqual(8, numberOfPropertyValueChangedEvents);
        }

    }

}
