using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TC.Mvvm.Tests
{

    [TestClass]
    public class ObservableCollectionSyncHelperTests
    {

        private class Model
        {
            public string SomeProperty;
        }

        private class ViewModel
        {

            private Model model;

            public ViewModel(Model model)
            {
                this.model = model;
            }

            public Model Model
            {
                get { return model; }
            }
        }

        [TestMethod]
        public void Move_Sync_IsCorrect()
        {
            ObservableCollection<Model> models = new ObservableCollection<Model>();
            models.Add(new Model { SomeProperty = "A", });
            models.Add(new Model { SomeProperty = "B", });
            models.Add(new Model { SomeProperty = "C", });

            ObservableCollection<ViewModel> viewModels = new ObservableCollection<ViewModel>();
            foreach(var model in models)
                viewModels.Add(new ViewModel(model));

            models.CollectionChanged += (s, e) => ObservableCollectionSyncHelper.Sync(models, viewModels, e, (m) => new ViewModel(m), (vm) => vm.Model);

            Assert.AreEqual("A", viewModels[0].Model.SomeProperty);
            Assert.AreEqual("B", viewModels[1].Model.SomeProperty);
            Assert.AreEqual("C", viewModels[2].Model.SomeProperty);

            models.Move(2, 0);

            Assert.AreEqual("C", viewModels[0].Model.SomeProperty);
            Assert.AreEqual("A", viewModels[1].Model.SomeProperty);
            Assert.AreEqual("B", viewModels[2].Model.SomeProperty);

            models.Move(0, 2);

            Assert.AreEqual("A", viewModels[0].Model.SomeProperty);
            Assert.AreEqual("B", viewModels[1].Model.SomeProperty);
            Assert.AreEqual("C", viewModels[2].Model.SomeProperty);
        }

    }

}
