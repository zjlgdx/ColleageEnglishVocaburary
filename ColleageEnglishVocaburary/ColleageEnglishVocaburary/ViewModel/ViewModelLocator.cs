/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ColleageEnglishVocaburary"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Cimbalino.Phone.Toolkit.Services;
using ColleageEnglishVocaburary.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace ColleageEnglishVocaburary.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (!SimpleIoc.Default.IsRegistered<INavigationService>())
            {
                SimpleIoc.Default.Register<INavigationService, NavigationService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IDataService>())
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }

            

            SimpleIoc.Default.Register<BookViewModel>();

            SimpleIoc.Default.Register<CourseViewModel>();

            SimpleIoc.Default.Register<WordViewModel>();
            

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

           // SimpleIoc.Default.Register<MainViewModel>();
        }

        public BookViewModel BookViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BookViewModel>();
            }
        }

        public CourseViewModel CourseViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CourseViewModel>();
            }
        }

        //public MainViewModel Main
        //{
        //    get
        //    {
        //        return ServiceLocator.Current.GetInstance<MainViewModel>();
        //    }
        //}
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}