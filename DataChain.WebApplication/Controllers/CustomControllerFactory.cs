using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using DataChain.DataProvider;
using DataChain.Abstractions.Interfaces;
using DataChain.WebApi.Models;
using DataChain.Infrastructure;
using DataChain.WebApplication.Models;

namespace DataChain.WebApplication.Controllers
{
    public class CustomControllerFactory : IControllerFactory
    {

        public  IController CreateController(System.Web.Routing.RequestContext requestContext, string controllerName)
        {


            Type targetType = typeof(MainController);
            UnitOfWork work = new UnitOfWork();

            switch (controllerName)
            {
                case "Main":
                    targetType = typeof(MainController);
                    break;
                case "Home":
                    targetType = typeof(HomeController);
                    break;
                case "Chain":
                    targetType = typeof(ChainController);
                    break;
                default:
                    requestContext.RouteData.Values["controller"] = "Main";
                    targetType = typeof(MainController);
                    break;
            }
            return targetType == null ? null :
                (IController)DependencyResolver.Current.GetService(targetType);
        }

        public System.Web.SessionState.SessionStateBehavior GetControllerSessionBehavior(
           System.Web.Routing.RequestContext requestContext, string controllerName)
        {
            return System.Web.SessionState.SessionStateBehavior.Default;
        }

        public  void ReleaseController(IController controller)
        {
            if (controller is IDisposable disposable)
                disposable.Dispose();
        }

    }
}