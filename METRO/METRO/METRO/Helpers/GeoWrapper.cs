using System;
using System.Device.Location;
using System.Diagnostics;
using METRO.GeocodeService;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Reactive;

namespace METRO.Helpers
{
    public sealed class GeoWrapper
    {
        public IObservable<GeoCoordinate> Adresse(string adress)
        {
            return Observable.Create<GeoCoordinate>(obs =>
            {
                LocationImpl(adress)
                    .DistinctUntilChanged()
                    .Subscribe(obs.OnNext, obs.OnError, obs.OnCompleted);

                return () => { };
            });
        }


        private static IObservable<GeoCoordinate> LocationImpl(string adresse)
        {
            var subject = new Subject<GeoCoordinate>();
            var geoCodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            geoCodeService.GeocodeCompleted += (s, evt) =>
            {
                if (evt.Error != null || evt.Cancelled)
                {
                    subject.OnCompleted();
                    return;
                }
                if (evt.Result.Results != null && evt.Result.Results.Count > 0)
                {
                    subject.OnNext(new GeoCoordinate(evt.Result.Results[0].Locations[0].Latitude, evt.Result.Results[0].Locations[0].Longitude));
                }else
                {
                    //subject.OnError(new Exception("Aucune localisation trouvée pour cette adresse."));
                    subject.OnCompleted();
                }
             

            };
            Scheduler.ThreadPool.Schedule(() => geoCodeService.GeocodeAsync(new GeocodeRequest
                                                                                {
                                                                                    Query = adresse,
                                                                                    //Address =  new Address{ 

                                                                                    //    AddressLine = TBSearchAddress.Text,
                                                                                    //    CountryRegion = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                                                                                    //},
                                                                                    Credentials = new Credentials
                                                                                                      {
                                                                                                          ApplicationId = App.MapId
                                                                                                      }
                                                                                }));

            return subject.AsObservable().Finally(() =>
            {
                Debug.WriteLine("Geocode Service Client : Closing Async...");
                geoCodeService.CloseAsync();
            });
        }


        public IObservable<GeoCoordinate> Location()
        {
            return Observable.Create<GeoCoordinate>(obs =>
            {
                LocationImpl(GeoPositionAccuracy.High, TimeSpan.FromSeconds(20))
                    .DistinctUntilChanged()
                    .Subscribe(obs.OnNext, obs.OnError, obs.OnCompleted);

                return () => { };
            });
            
        }

        private static IObservable<GeoCoordinate> LocationImpl(GeoPositionAccuracy accuracy, TimeSpan locationTimeout)
        {
            var subject = new Subject<GeoCoordinate>();
            var watcher = new GeoCoordinateWatcher(accuracy);
            watcher.PositionChanged += (o, args) =>
                                           {
                                               var newLocation = args.Position.Location;
                                               Debug.WriteLine("Location service : new Location : {0}: {1},{2}", args.Position.Timestamp, args.Position.Location.Latitude, args.Position.Location.Longitude);
                                               if((DateTime.Now - args.Position.Timestamp.DateTime)< locationTimeout)
                                               {
                                                   subject.OnNext(newLocation);
                                                   subject.OnCompleted();
                                               }
                                           };
            Scheduler.ThreadPool.Schedule(() =>
                                              {
                                                  Debug.WriteLine("Location service : Starting the watcher...");
                                                  var started = watcher.TryStart(true, TimeSpan.FromSeconds(5));
                                                  if (!started)
                                                  {
                                                      subject.OnCompleted();
                                                      //subject.OnError(new Exception("Impossible de démarrer le GPS."));
                                                  }
                                              });

            return subject.AsObservable().Where(c => !c.IsUnknown).Finally(() =>
                                                                               {
                                                                                   Debug.WriteLine("Location service : Shutting down the watcher...");
                                                                                   watcher.Stop();
                                                                               });
        }
    }
}
